# SlavicGame assets

This directory is intentionally versioned in Git.

## Layout

```text
assets/
├── runtime/
│   ├── models/
│   │   ├── static/
│   │   └── animated/
│   ├── lod/
│   │   ├── lod1/
│   │   └── lod2/
│   ├── colliders/
│   └── textures/
├── source/
│   ├── downloaded/
│   ├── blender/
│   └── licenses/
└── metadata/
```

### runtime
Assets consumed directly by the game/engine. Prefer `.glb` for 3D models.

### source
Original downloaded/source files and license information. Keep the source URL and license for every third-party asset.

### metadata
Asset manifests, catalogs, attribution data and import metadata.

## Third-party assets

Do not add a downloaded model without recording its license and source. CC0/public-domain assets are preferred.
