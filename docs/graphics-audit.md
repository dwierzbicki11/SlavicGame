# Graphics audit log

Persistent findings for renderer/performance iterations. Re-check entries only when the relevant code changes.

## 2026-10-06

- **Menu buffer-growth stall — fixed and merged in PR #357.** `MenuRenderer` now retires superseded dynamic vertex buffers instead of calling device-wide `GraphicsDevice.WaitForIdle()` during normal-frame capacity growth. Retired buffers are released at renderer teardown, preserving GPU lifetime safety while removing the growth hitch.
- **HUD upload allocates and copies a fresh managed array every visible frame — open.** `VeldridRenderer.Render` uploads `_hudVertices.ToArray()` even though the list is already the authoritative contiguous CPU-side geometry. This creates a per-frame allocation and full vertex copy. Replace it with a span upload (`CollectionsMarshal.AsSpan(_hudVertices)`) as already proven by `MenuRenderer`; add regression coverage that prevents `ToArray()` from returning to the HUD upload path.
- **HUD buffer growth still forces a device-wide idle — open.** When `_hudVertices.Count > _hudVertexCapacity`, the renderer calls `GraphicsDevice.WaitForIdle()`, disposes the old buffer immediately, and allocates a larger one. Use the same deferred-retirement ownership model proven by PR #357 rather than deleting the wait and risking use-after-dispose.
- **Dynamic-motion and actor buffer growth still force device-wide idle — open.** `EnsureDynamicMotionCapacity` and both actor vertex/index growth paths still wait for the whole device before replacing buffers. These are ordinary gameplay growth paths, unlike resize/quality recreation. They should move to safe deferred retirement after the HUD path is green.
- **Resize/quality waits remain lower priority.** Bloom, shadow-map, resolution-scaler, reactive-mask and motion-vector recreation waits occur at resource-shape boundaries. They remain optimization candidates but are intentionally ranked below normal-frame HUD/dynamic/actor growth stalls.

## 2026-10-05

- **Menu buffer-growth stall root cause and safe first fix pinned.** `MenuRenderer.EnsureCapacity` called device-wide `GraphicsDevice.WaitForIdle()` before disposing the old dynamic vertex buffer. The safe implementation was to create the larger buffer immediately, move the old buffer into a bounded retired-buffer collection, and dispose retired buffers only when `MenuRenderer` itself is disposed. This was implemented and merged in PR #357 on 2026-10-06.

## 2026-10-04

- **Menu renderer per-frame vertex-array allocation — fixed on `perf/menu-span-upload-v3`.** `MenuRenderer.Render` still rebuilds its reusable `List<UiVertex>`, but uploads its active contents through `CollectionsMarshal.AsSpan(_vertices)` instead of `_vertices.ToArray()`. This removes the fresh managed array allocation and full vertex copy from every rendered menu frame without changing vertex count, draw calls, UI output or CPU->GPU upload size.
- **Dynamic GPU-buffer growth still forces a device-wide idle — partially fixed.** Menu growth is fixed by PR #357. HUD, actor and dynamic-motion growth remain open. Do not simply delete the waits: old buffers may still be referenced by submitted command lists. Replace them with safe deferred retirement/frame-owned buffers or make normal-frame growth unnecessary, with regression coverage where practical.
- **Resize/quality recreation waits classified separately — accepted for now.** Bloom, shadow-map, resolution-scaler, reactive-mask and motion-vector target recreation also contain `WaitForIdle`, but those are tied to resize/quality/resource-shape changes rather than ordinary steady-state frame submission.
- **FSR 2/3 temporal foundation added before content scale-up.** The renderer owns deterministic camera history/jitter, exposes sampleable single-sample scene depth and generates an RG16F camera-motion field by depth reconstruction. Jitter stays disabled on the shipping spatial path until the real temporal backend consumes it.
- **Vulkan FSR1 RCAS bypass — fixed on `render/fsr1-vulkan-full-rcas-clean`.** Vulkan follows the EASU -> intermediate target -> RCAS -> swapchain chain. Regression coverage requires Vulkan to retain RCAS while preserving the upright fullscreen UV convention.
- **Shadow light matrix was uploaded twice per frame — fixed on `perf/skip-disabled-shadow-depth-pass`.** Depth and sampling resource sets share one dynamic uniform buffer, removing the duplicate upload and one GPU buffer allocation.
- **Terrain disabled shadow work used eager `mix` — fixed on `perf/terrain-shadow-lazy-eval-clean`.** Expensive cloud/shadow work now executes only inside enabled branches.
- **Sun-shadow PCF cost was disproportionately high — fixed on `perf/shadow-pcf-four-tap`.** The filter uses four symmetric taps instead of a 3x3 nine-fetch kernel.
- **Disabled shadow depth work — fixed on `perf/skip-disabled-shadow-depth-pass`.** Shadow matrix upload, depth clear and caster draws are skipped when effective shadows are disabled.

## 2026-10-03

- **Disabled PBR shadow features still executed their full fragment cost — fixed on `perf/skip-disabled-shadow-work`.** Frame-uniform branches now skip expensive cloud-noise/PCF work when disabled.
- **MSAA-only frames ran a redundant full-resolution post-process pass — fixed.** `PostProcessRenderer.IsNeeded` now selects FXAA specifically; MSAA-only frames can bypass the redundant fullscreen pass.
- **Bloom low quality did unnecessary blur work — fixed.** Low keeps the quarter-resolution threshold/downsample result; Medium retains one separable blur pair and High retains two.
- **Bloom shader/resource layout checked.** `BloomParameters` is two vec4 values (32 bytes), matching the 32-byte C# uniform buffer and offsets 0/16. Texture/sampler bindings match the resource layout.
- **Bloom viewport/scissor checked.** Each pass uses the target framebuffer followed by full target viewport/scissor.
- **FSR/upscaling settings present.** Settings expose Bilinear vs FSR1 and FSR quality/sharpness. Continue validating that the FSR path is genuine AMD FSR1 EASU+RCAS and that license notices remain present before modifying it.
- **LOD/terrain controls present.** Model LOD distances, vegetation impostor start, terrain mesh step and render/vegetation/clutter distances are wired in the quality catalog.

## Measured performance context

User measurements: ~45 FPS at 960x540 and ~17 FPS at 1920x1080, with an earlier ~22–25 FPS state. Treat pixel fill-rate/bandwidth as a primary bottleneck until profiling disproves it. Prefer resolution scaling/FSR, culling, LOD and reduced fullscreen-pass cost over removing visual systems.
