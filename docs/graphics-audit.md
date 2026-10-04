# Graphics audit log

Persistent findings for renderer/performance iterations. Re-check entries only when the relevant code changes.

## 2026-10-04

- **Terrain disabled shadow work used eager `mix` — fixed on `perf/terrain-shadow-lazy-eval-clean`.** `terrain.frag` evaluated `CloudShadowFactor(...)` and `SampleSunShadow(...)` as arguments to `mix`, so disabled Cloud/Sun Shadows could still pay procedural-noise and shadow-map sampling cost. The terrain shader now matches the PBR path: defaults to a shadow factor of 1.0 and calls each expensive function only inside a frame-uniform enabled branch. The clean retry intentionally changes only the shadow block (13 additions / 10 deletions), avoiding the unrelated shader reformat churn caught in the previous attempt.
- **Sun-shadow PCF cost was disproportionately high for a full-screen fragment path — fixed on `perf/shadow-pcf-four-tap`.** `SampleSunShadow` used a 3x3 kernel, issuing 9 point-sampled shadow-map fetches for every sun-shadowed terrain/PBR fragment. The filter now uses four symmetric sub-texel taps around the receiver, preserving a soft filtered edge while reducing shadow-map texture fetches by 55.6% (9 -> 4). This directly targets texture bandwidth/fill-rate pressure on weak integrated GPUs.
- **Shadow depth pass begins before `shadowEnabled` is tested — open.** `VeldridRenderer` calls `UpdateLight` and `BeginDepthPass` before checking `settings.SunShadows`/sun intensity. Even with shadows disabled this still updates shadow matrices, binds the 2048x2048 depth framebuffer and clears it. Move the update/begin work inside the enabled branch, while keeping a valid sample resource bound for material pipelines.

## 2026-10-03

- **Disabled PBR shadow features still executed their full fragment cost — fixed on `perf/skip-disabled-shadow-work`.** `pbr.frag` used `mix(1.0, CloudShadowFactor(...), flag)` and `mix(1.0, SampleSunShadow(...), flag)`. GLSL `mix` does not provide lazy evaluation, so disabling cloud shadows or sun shadows did not skip cloud-noise/PCF work. The shader now uses frame-uniform branches and only calls the expensive functions when their setting is enabled. This makes the existing quality switches actually reduce fragment workload, which matters especially at 1080p on fill-rate-limited GPUs.
- **MSAA-only frames ran a redundant full-resolution post-process pass — fixed.** `PostProcessRenderer.IsNeeded` previously returned true for every anti-aliasing mode, but the post-process shader only performs AA when the mode is FXAA. MSAA is already resolved by `ResolutionScalerRenderer`, so MSAA + bloom off + default brightness/gamma needlessly read and rewrote the entire resolved frame. `IsNeeded` now selects FXAA specifically; MSAA-only frames go directly to the presentation/upscaling path.
- **Bloom low quality did unnecessary blur work — fixed.** Low already renders at quarter linear resolution, but then ran horizontal + vertical 5-fetch Gaussian passes. Low now keeps the 4-sample threshold/downsample result; Medium retains one separable blur pair and High retains two.
- **Global `GraphicsDevice.WaitForIdle()` remains an audit risk.** Renderer searches show waits during dynamic HUD/actor/menu buffer growth and target recreation (bloom, shadow map, resolution scaler). Target recreation waits are acceptable as rare resize/quality events, but dynamic buffer growth can hitch if it occurs during gameplay. Future iteration should replace hot-path growth waits with deferred retirement/ring-buffer ownership where safe.
- **Bloom shader/resource layout checked.** `BloomParameters` is two vec4 values (32 bytes), matching the 32-byte C# uniform buffer and offsets 0/16. Texture/sampler bindings match the resource layout.
- **Bloom viewport/scissor checked.** Each pass uses the target framebuffer followed by full target viewport/scissor, so quarter/half-resolution bloom does not inherit scene dimensions.
- **Bloom texture-fetch cost noted.** Threshold/downsample is 4 source fetches; each Gaussian blur pass is 5 fetches. Medium therefore costs 4 + 5 + 5 samples per bloom pixel chain; High adds another blur pair.
- **FSR/upscaling settings present.** Settings expose Bilinear vs FSR1 and FSR quality/sharpness. Continue validating that the FSR path is genuine AMD FSR1 EASU+RCAS and that license notices remain present before modifying it.
- **LOD/terrain controls present.** Model LOD distances, vegetation impostor start, terrain mesh step and render/vegetation/clutter distances are wired in the quality catalog. Future audits should verify draw-time selection/culling rather than adding duplicate menu switches.

## Measured performance context

User measurements: ~45 FPS at 960x540 and ~17 FPS at 1920x1080, with an earlier ~22–25 FPS state. Treat pixel fill-rate/bandwidth as a primary bottleneck until profiling disproves it. Prefer resolution scaling/FSR, culling, LOD and reduced fullscreen-pass cost over removing visual systems.
