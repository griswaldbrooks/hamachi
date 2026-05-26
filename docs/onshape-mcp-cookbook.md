---
title: Onshape-MCP cookbook for agents
sidebar_position: 3
---

# Onshape-MCP cookbook for agents

Long-form companion to the `hamachi-onshape` skill (`.claude/skills/hamachi-onshape/SKILL.md`). The skill is the fast-load reference dropped into context at session start; this page is the teaching material with worked examples.

> **Audience:** AI coding agents (Claude Code, etc.) and the humans reviewing their work. If you're a human looking for the model itself, start with [Onshape model overview](./onshape-model-overview.md).

## When to read this

- You're about to make non-trivial changes to the hamachi Onshape doc.
- You hit an Onshape MCP error you don't recognize.
- You're writing a FeatureScript custom feature for the first time.
- You're checking STL parity vs the SCAD reference.

## Workflow

### 1. Load context first, then act

In order:

1. Read this page's quick-links section and the skill (auto-loaded if registered).
2. Read `feedback_onshape_mcp_gotchas` auto-memory — refinements may have landed since this doc was written.
3. Verify the current state with `describe_part_studio` on the Part Studio you're about to modify. Read the FEATURE TREE block before assuming anything.
4. Only then start writing features.

### 2. The verification loop after every mutation

`describe_part_studio(documentId, workspaceId, elementId, views=["iso"])` is the canonical state check. It returns:

- **FEATURE TREE** — every feature with its build status (`OK` / `INFO` / `WARNING` / `ERROR`).
- **BODIES** — every face/edge with deterministic ID + human-readable description.
- **PHYSICAL SUMMARY** — body volume, bbox, face/edge counts.
- **VIEWS RENDERED** — embedded image(s) you can visually inspect.

Use this **once per mutation**. Do not chain `get_features` + `list_entities` + `render_part_studio_views` separately — slower, less informative, and `get_features` overflows on Part Studios with 20+ features.

### 3. Overflowing `get_features` responses

On a Part Studio with 20+ features, `get_features` returns 128KB+ of JSON that exceeds the MCP response cap. The runtime auto-saves the response to `tool-results/*.txt` and returns the path.

The file is **Python repr, not JSON**. Parse with `ast.literal_eval`:

```python
import ast

with open(path) as f:
    content = f.read().removeprefix("Features data: ")
data = ast.literal_eval(content)

for i, feat in enumerate(data['features']):
    name = feat.get('name')
    bt = feat.get('btType')                  # BTMSketch-151 / BTMFeature-134
    ftype = feat.get('featureType')          # extrude / arduinoShelf / etc.
    for p in feat.get('parameters', []):
        # quantity params: p['expression']
        # enum/bool params: p['value']
        # Query params: deeper inspection needed
        print(p.get('parameterId'), p.get('expression') or p.get('value'))
```

This pattern was validated on `hamachi-v5c` (shell_with_weapon replay). Extracting `feature.name`, `feature.btType`, `feature.featureType`, and `feature.parameters[].expression` per feature gives a clean per-feature spec to replay in a new Part Studio.

## Worked example: replaying a Part Studio (Option A)

This is the procedure followed to build `shell_with_weapon` from `shell_no_weapon`.

### Step 1 — read the source Part Studio

```python
features = get_features(doc, ws, source_partstudio_id)
# OR if it overflows:
# features = ast.literal_eval(open(saved_path).read().removeprefix("Features data: "))
```

Identify which features to skip (e.g. the weapon-mount-hole sketch + cut in our case) and what new features to add (e.g. the integrated weapon bar).

### Step 2 — create the new Part Studio

```python
result = create_part_studio(doc, ws, name="shell_with_weapon")
new_elementId = result['element_id']
```

### Step 3 — replay each feature in order

For each feature in the source list (skipping the ones you're omitting):

- **For sketches:** re-construct from your knowledge of the geometry. The cached BT representation is dense (`BTMSketchCurveSegment-155`) and usually not faster to parse than re-deriving entities + constraints from the original SCAD or design intent.
- **For extrudes (and other features with parameters):** copy the parameter values verbatim — `operationType`, `depth`/`variableDepth`, `oppositeDirection`, etc.

### Step 4 — for any sketch on a face, find the equivalent face ID

Face IDs are deterministic-but-element-local. `shell_no_weapon`'s top face `JXO` exists at `(0, 0, 33) mm` with `outward +Z` — in `shell_with_weapon` after the same feature sequence, find that face with `list_entities` filtered by `outward_axis="+Z"` and `at_z_mm=33`.

In practice many face IDs match across Part Studios that share an identical feature prefix (because the deterministic ID hash depends on the feature history). But don't rely on this — always verify with `list_entities`.

### Step 5 — verify parity at each stage

After each feature: `describe_part_studio` + check the `volume_after_mm3` delta vs your prediction. Bbox should change in expected ways. If something looks off, fix it before adding more features on top.

### Step 6 — final STL parity check

```python
export_part_studio(doc, ws, elementId, format="STL")  # path returned
# Onshape STLs come out in METERS — scale ×1000 to mm.
# Compare body volume from describe_part_studio (exact)
# vs SCAD STL volume (parsed manually).
# Acceptable delta: ≤1% after accounting for known divergences.
```

## Worked example: tilted-plane FeatureScript custom feature

The Arduino shelf was built as an FS custom feature because the tilted plane couldn't be constructed via Onshape primitives (`create_offset_plane` doesn't support arbitrary rotation; only parallel offset). The full source is archived to [`docs/fs/arduino-shelf.fs`](https://github.com/griswaldbrooks/hamachi/blob/main/docs/fs/arduino-shelf.fs).

### The pattern

1. **Compute the rotated local frame in FS.** For SCAD-style `rotate([rx, ry, rz])` (composed as `Rz · Ry · Rx`), the local X / Y axes in world coords are:

   ```fs
   const cosRx = cos(rxAngle); const sinRx = sin(rxAngle);
   const cosRz = cos(rzAngle); const sinRz = sin(rzAngle);
   const localX = vector(cosRz, sinRz, 0);
   const localY = vector(-cosRx * sinRz, cosRx * cosRz, sinRx);
   const localN = cross(localX, localY);  // automatically unit if X⊥Y are unit
   ```

2. **Build the tilted plane.** No `opPlane` needed:

   ```fs
   var sk = newSketchOnPlane(context, id + "sk", {
       "sketchPlane" : plane(origin, localN, localX)
   });
   skRectangle(sk, "r", {
       "firstCorner"  : vector(0, 0)   * millimeter,
       "secondCorner" : vector(20, 38) * millimeter
   });
   skSolve(sk);
   ```

3. **Use `extrude(...)`, NOT `opExtrude(...)`.** The user-facing wrapper does default-scope plumbing that `opExtrude` needs as explicit args. Same inputs to `opExtrude` failed with opaque `EXTRUDE_FAILED`; `extrude` worked.

   ```fs
   extrude(context, id + "ext", {
       "entities"      : qSketchRegion(id + "sk"),
       "endBound"      : BoundingType.BLIND,
       "depth"         : 2 * millimeter,
       "operationType" : NewBodyOperationType.NEW
   });
   ```

4. **Union with `qAllNonMeshSolidBodies()`, NOT `qEverything(EntityType.BODY)`.** The latter picks up non-solid entities (sketch regions, planar surfaces) and fails `BOOLEAN_INPUTS_NOT_SOLID`.

   ```fs
   opBoolean(context, id + "merge", {
       "tools"         : qAllNonMeshSolidBodies(),
       "operationType" : BooleanOperationType.UNION
   });
   ```

## STL parity check recipe

```python
import struct

def stl_bbox_vol_binary(path, scale=1000.0):
    """Bbox + signed volume of binary STL. scale=1000 converts Onshape's meters to mm."""
    with open(path, 'rb') as f:
        f.read(80)
        n = struct.unpack('<I', f.read(4))[0]
        xmin = ymin = zmin = float('inf')
        xmax = ymax = zmax = float('-inf')
        vol = 0.0
        for _ in range(n):
            f.read(12)
            v = struct.unpack('<9f', f.read(36))
            f.read(2)
            x1, y1, z1, x2, y2, z2, x3, y3, z3 = v
            for x, y, z in [(x1, y1, z1), (x2, y2, z2), (x3, y3, z3)]:
                xmin = min(xmin, x); xmax = max(xmax, x)
                ymin = min(ymin, y); ymax = max(ymax, y)
                zmin = min(zmin, z); zmax = max(zmax, z)
            vol += (x1*(y2*z3 - y3*z2) + x2*(y3*z1 - y1*z3) + x3*(y1*z2 - y2*z1)) / 6.0
        return ((xmin*scale, ymin*scale, zmin*scale,
                 xmax*scale, ymax*scale, zmax*scale),
                abs(vol) * scale**3, n)

# Onshape STL exports use METERS; SCAD STLs are mm-native.
ons = stl_bbox_vol_binary('/tmp/onshape-mcp-exports/<your-export>', scale=1000.0)
scad = stl_bbox_vol_binary('antweight_reference_platform/shell_with_weapon.stl', scale=1.0)
```

The Onshape STL export uses Onshape's default tessellation, which can produce a coarser mesh than SCAD's `$fn=50`. The exported STL's volume will be _under_-approximated; **compare body volume from `describe_part_studio` / `get_mass_properties` (exact) against the SCAD STL volume instead**.

For `shell_with_weapon` the final parity was +0.09% (133344 mm³ Onshape exact vs 133229 mm³ SCAD STL) — within tessellation noise.

If your STL is ASCII (some Onshape exports vary), use a `vertex`-line parser:

```python
def stl_bbox_vol_ascii(path, scale=1000.0):
    xmin = ymin = zmin = float('inf')
    xmax = ymax = zmax = float('-inf')
    vol = 0.0
    n_tri = 0
    verts = []
    with open(path) as f:
        for line in f:
            line = line.strip()
            if line.startswith('vertex'):
                _, x, y, z = line.split()
                x, y, z = float(x), float(y), float(z)
                verts.append((x, y, z))
                xmin = min(xmin, x); xmax = max(xmax, x)
                ymin = min(ymin, y); ymax = max(ymax, y)
                zmin = min(zmin, z); zmax = max(zmax, z)
            elif line == 'endfacet':
                if len(verts) == 3:
                    (x1,y1,z1),(x2,y2,z2),(x3,y3,z3) = verts
                    vol += (x1*(y2*z3-y3*z2) + x2*(y3*z1-y1*z3) + x3*(y1*z2-y2*z1)) / 6.0
                    n_tri += 1
                verts = []
    return ((xmin*scale, ymin*scale, zmin*scale,
             xmax*scale, ymax*scale, zmax*scale),
            abs(vol) * scale**3, n_tri)
```

## Gotcha taxonomy

These are the patterns that have surfaced repeatedly. The auto-memory `feedback_onshape_mcp_gotchas` holds the latest refinements.

### Sketch solver: DISTANCE collisions

- **TWO `DISTANCE direction=HORIZONTAL` with the same anchor and same value:** entities silently collapse to the same side. Workaround: replace one with a pair-distance (`DISTANCE` between the two entities, value = `2 × offset`).
- **3+ `DISTANCE direction=MINIMUM` with the same value and same anchor:** the solver picks ONE branch and applies it to all matching entities. Seed positions are ignored when there are infinitely many (x, y) at distance `R` from the anchor. Workaround: drop the MINIMUM constraints on those entities; rely on seed coordinates.
- `inspect_sketch` returns SEED positions, NOT solved positions. To verify a fix worked, call `list_entities` on the resulting body and count.

### Variable Studio: derived vars don't propagate

Base variables (`#botDiameter`, `#botHeight`, etc.) resolve everywhere — constraints, `variableDepth`, inline arithmetic. Derived variables (`#botRadius = #botDiameter / 2`, `#motorWallWidth = sqrt(...)`) ONLY resolve as expression text inside the Variable Studio itself.

Errors when you try to use a derived var:
- In a constraint VALUE: `SKETCH_UNSOLVABLE_CONSTRAINT (WARNING)` (misleading; not the clearer "MISSING_PARAMETER" variant).
- In `variableRadius` / `variableDepth`: `SKETCH_DIMENSION_MISSING_PARAMETER`.

Workaround: paste the inline expression verbatim. `sqrt()` and `^` work fine in constraint VALUES — just not `#derivedVar` references.

### create_extrude depth parsing

The `depth` arg rejects `#var` syntax. Use the separate `variableDepth` slot for variable references (base var name only, no `#` prefix). `update_feature`'s `expression` field DOES accept `#var` arithmetic for the `depth` parameterId, but only after the fact.

### REMOVE-on-face auto-flip

When `operationType=REMOVE` is set on a sketch placed on a picked face (any non-standard plane), `create_extrude` auto-sets `oppositeDirection=true` so the cut goes INTO the face's body's material. The auto-flip notes itself in the response.

Override with `forceOppositeDirection=false` when you want the cut to go in the OTHER direction — e.g. cutting INTO a feature that sits ABOVE the sketch face (the battery wall zip-tie gap case). Pass both `oppositeDirection=false` AND `forceOppositeDirection=false`.

### offset planes can't be sketched on

`create_offset_plane` returns a feature_id, but using it as `faceId` on `create_sketch` fails with `SKETCH_NO_PLANE`. Workaround: sketch on a standard plane and use two opposite-direction extrudes for asymmetric ranges (the lid motor cut pattern: REMOVE 28mm `oppositeDirection=false`, REMOVE 23mm `oppositeDirection=true`, both from one Front-plane sketch).

### FeatureScript inside custom features

- Prefer `extrude(...)` (user-facing wrapper) over `opExtrude(...)`. `opExtrude` failed with opaque `EXTRUDE_FAILED` on a clean rectangle.
- For boolean unions across an unknown body set, use `qAllNonMeshSolidBodies()`. `qEverything(EntityType.BODY)` includes non-solids (sketch regions, surfaces) and fails `BOOLEAN_INPUTS_NOT_SOLID`.

### Unit handling

- Onshape's API is internally meters. Pass lengths as strings (`"30 mm"`) or as `vector(...) * millimeter` in FS. Bare numbers are interpreted as mm.
- **STL exports from Onshape are in METERS.** Scale ×1000 to compare with mm-native SCAD STLs.

### MCP capability gaps

- **No `delete_element`:** the MCP can't delete a Part Studio, Assembly, or Feature Studio. The Onshape web UI is the only way to clean up.
- **No cross-element FS reference:** `write_featurescript_feature` always creates a NEW Feature Studio. To re-use an existing FS feature in a new Part Studio, either re-upload the source (creates a parallel Feature Studio — clutters the doc) or insert it manually in the web UI.

## See also

- The `hamachi-onshape` skill: `.claude/skills/hamachi-onshape/SKILL.md`.
- The `feedback_onshape_mcp_gotchas` auto-memory at `~/.claude/projects/-media-griswald-wd-black-2tb-personal-hamachi/memory/`.
- The plugin-level skill `jarvis-onshape-mcp:onshape` — general Onshape protocols (units, planes, sketch surfaces).
- The FS cookbook at `~/.claude/plugins/cache/jarvis-onshape-mcp/.../docs/fs-cookbook/helix.fs` — the helical-sweep recipe was the starting point for our Arduino-shelf FS.
- [Onshape migration log](./onshape-migration.md) — long-form record of how each Part Studio was built, with rationale.
