# Pinned Veldrid Vulkan creation patch

SlavicGame builds Veldrid 4.9.0 from the exact upstream release commit
`a121087cadf38755f28c397a5b3c42ff1c559a19`. `tools/prepare-veldrid.py` fetches
that source into `.cache`, checks the commit and clean worktree, then applies
narrow Vulkan changes in a separate generated directory. No upstream
repository or installed NuGet package is modified. The script rejects drift.

The patch adds the requested instance API version to `VulkanDeviceOptions`,
explicit optional storage-feature selection, and creation metadata exposed by
`BackendInfoVulkan`. It also checks instance/device creation errors in Release;
upstream's `CheckResult` is compiled out there. A typed instance error permits
retrying an incompatible old ICD with Vulkan 1.0 before any device exists.

The stage-2 scene proof also exposed an upstream RG16F mapping defect: the
backend created RGBA16F images for RG16F descriptions. The generated source
fixes that one mapping, making motion-vector views and staging copies use the
same four-byte pixel format. The compiler and Vulkan validation check this
through actual rendered motion and readback.

The presentation patch exposes a checked submit/present endpoint with a
render-finished semaphore per acquired image and host-fence acquisition. It
retains old swapchains/semaphores across recreation until a new presentation
has completed, and avoids MAILBOX while FG is active. The renderer's window
proof captures the actual images handed to WSI and validates order, spacing,
reset and resize with VSync off/on. Swapchain images include optional transfer
source usage only if the surface advertises it, for that GPU readback proof.
The semaphore-reuse and recreation rules follow the Khronos Vulkan Guide and
swapchain-recreation sample. Terminal shutdown retains the conventional
unextended-Vulkan WaitIdle path; ordinary resize uses reacquisition evidence.

Synchronization validation additionally checks buffer reuse and copies,
depth/color attachment dependencies and the native AMD-to-Veldrid memory
boundary. The patch makes copied buffer ranges visible to uniform/index and
storage consumers as well as vertices, preserves compatible pipeline/render
pass dependencies, and exposes image transitions to compute and fragment
sampling. These are GPU barriers; they do not add per-frame idle waits.

All original Vulkan, D3D11, OpenGL and Metal sources and pinned native binding
versions are retained. StartupUtilities and ImageSharp remain the official
4.9.0 packages and reference the same Veldrid assembly identity. The local
source projects target the game's .NET 11 version.

Build prerequisites are .NET 11, Git and Python 3 (`python` on Windows,
`python3` on Linux). A clean `dotnet build SlavicGame.csproj -c Release` prepares
the dependency automatically; the first build needs GitHub access. The cache
can be removed and reconstructed. `SLAVICGAME_VELDRID_SOURCE` may name a clean
local checkout of the same pinned release for an offline build.

The original MIT license is retained in `LICENSE` and copied to published game
output under `licenses/Veldrid-LICENSE.txt`.
