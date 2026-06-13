# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Digger** is a Unity tool for creating natural caves and overhangs on terrain. This repository contains the URP (Universal Render Pipeline) version. Two product variants exist:
- **Digger**: Core cave/overhang creation (editor-only)
- **Digger PRO**: Includes runtime/in-game editing support

Documentation: https://ofux.github.io/Digger-Documentation/

## Architecture

### Module Structure

The codebase is organized into 5 main modules under `Modules/`:

| Module | Purpose |
|--------|---------|
| `Core/` | Voxel operations, mesh generation, terrain interaction |
| `Runtime/` | Runtime editing capabilities (PRO only) |
| `AdvancedOperations/` | Spline-based terrain modification |
| `PolyTerrainsIntegration/` | Blocky voxel terrain support |

Shaders are in `Shaders/` with subdirectories for URP, HDRP, and Standard pipelines.

### Core System Components

**Entry Points:**
- `DiggerMaster` (Modules/Core/Sources/) - Main inspector component for editor operations
- `DiggerSystem` (Modules/Core/Sources/) - Core runtime system managing chunks, voxels, persistence

**Voxel System:**
- Uses Signed Distance Field (SDF): Negative = inside volume, Positive = outside, Zero = surface
- `Voxel` struct packs SDF + metadata (texture indices, blend weights, alteration state) into 32-bit properties
- `VoxelChunk` stores voxel data per terrain chunk

**Key Subsystems:**
- `Sources/Jobs/` - Unity Job System (Burst-compiled) for voxel modification and mesh generation
- `Sources/Polygonizers/` - Mesh generation via Marching Cubes algorithm
- `Sources/Generators/` - Voxel population (SimpleVoxelGenerator, AdvancedVoxelGenerator)
- `Sources/TerrainInterface/` - Terrain height/normal/alphamap interaction
- `Sources/VoxelPhysics/` - Floating voxel detection and removal

### Operation System

Operations follow `IOperation<T>` interface pattern:
1. `GetAreaToModify()` - Determines affected region
2. `Do()` - Creates IJobParallelFor job
3. `Complete()` - Post-processing after job completion

`ModificationParameters` struct defines operation properties (position, brush type, action, texture).

**Action Types:** Dig, Add, Smooth, Paint, Reset, PaintHoles
**Brush Types:** Sphere, Cube, Cylinder, Custom

### Data Persistence

Voxel data stored in `Assets/DiggerData/Scenes/{sceneDataFolder}/`:
- `.vox3` - Voxel data
- `.vom` - Metadata
- `.labels` - Physics labels
- `.ver` - Version info

## Development Guidelines

### Assembly Definitions

Each module has its own `.asmdef` files. Key dependencies:
- `Unity.Burst` - Job compilation
- `Unity.Mathematics` - Math operations
- `Unity.Collections` - Native data structures
- Conditional: `JBooth.MicroSplat.Core`, `PolyTerrains`

Conditional defines used: `USING_HDRP`, `USING_URP`

### Burst-Compatible Code

All Jobs must be Burst-compatible:
- No managed allocations
- Use native collections with appropriate allocator (Persistent for long-lived data)
- All job code is in `Modules/Core/Sources/Jobs/`

### Editor vs Runtime

- Editor code uses `#if UNITY_EDITOR` conditional compilation
- Editor classes are in `Editor/` subdirectories
- Runtime module (`Modules/Runtime/`) is PRO-only feature

### Adding New Operations

1. Implement `IOperation<T>` interface
2. Create corresponding IJobParallelFor for voxel modification
3. Add `IOperationEditor` implementation for inspector UI
4. Mark editor with `[OperationAttr]` attribute

### Adding New Voxel Generators

1. Implement `IVoxelGenerator` interface
2. Create ScriptableObject asset
3. Add editor with `[VoxelGeneratorAttr]` attribute
