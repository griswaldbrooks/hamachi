---
name: hamachi-onshape
description: Project-local quick-start for driving the hamachi Onshape document via the jarvis-onshape-mcp plugin. Loads doc IDs, Variable Studio map, Part Studio layout, validated workflow + gotchas, and cookbook recipes (feature replay from cached get_features, STL parity check, tilted-plane FS pattern). Load FIRST when starting any Onshape work in this repo — saves 5–15 turns of rediscovery on every session.
---

# Hamachi × Onshape — agent quick-start

This skill is **project-local** to `hamachi/`. It complements (does not replace) the plugin-level `jarvis-onshape-mcp:onshape` skill — that one covers general protocols (units, planes, sketch surfaces, regen-check); this one is the hamachi-specific overlay.

**Always pair with:** the auto-memory `feedback_onshape_mcp_gotchas` (latest gotcha refinements) and `docs/onshape-migration.md` (long-form rationale + lessons learned). Read those second if you're doing anything non-trivial.

## Document IDs — drop straight into tool args

```
documentId          = 3662b787ba73917e77c5a543
workspaceId         = 64e642b76ea639b57af58924

# Variable Studio (all parametric vars live here)
variables           = 8ad42fc0569446006c975dba

# Part Studios (one per printed part)
lid                 = 4184aeece3d6787270371115   # 11 features
shell_no_weapon     = 9e9326c01379985fe1982ee6   # 25 features (incl. arduinoShelf)
shell_with_weapon   = a28ec3540e926aba920d88b6   # 25 features (incl. arduinoShelf)
weapon              = 109b30fd7fdba9ecc8543a6e   #  8 features
weapon_pin          = d8b341a8f7fcb9bbee7d33a3   #  2 features

# Feature Studios (carry custom FS source — keep referenced ones)
ClaudeFS_arduinoShelf (shell_no_weapon ref)  = 8745567717fd9a71f2816bbf
ArduinoShelfFS_v2     (shell_with_weapon ref) = 661e2d122314a6cc0e1c2311
```

The Variable Studio + every Part Studio are stable; the FS Studio IDs may change if either is rebuilt. Re-verify with `get_elements` if a tool call returns "namespace not found."

## Variable Studio — common references

Base variables (resolve everywhere — constraints, `variableDepth`, inline expressions):

```
#botHeight = 33 mm                  #botDiameter = 138 mm
#botShellThickness = 5 mm           #floorThickness = 3 mm
#lidThickness = 1 mm                #lidScrewPilotDiameter = 2.5 mm   # M3 self-tap
#lidScrewHoleDepth = 10 mm          #ledHoleDiameter = 5 mm           # Cree C503B size

#motorWallThickness = 3 mm          #motorWallOffset = 28 mm           # +Y motor side
#motorDiameter = 37 mm              #motorMountHeight = 7 mm
#motorMountOffset = -2 mm           #motorBushingDiameter = 13 mm
#motorBushingHeight = 2 mm          #motorLength = 51 mm
#motorDepthBelowFloor = 5 mm

#zipTieHoleDiameter = 5.25 mm       #zipTieHoleOffset = 6 mm

#weaponMountHoleDiameter = 12 mm    #weaponPinExtraLength = 5 mm
#weaponPinKerf = 1 mm               #weaponThickness = 19 mm
#weaponWidth = 40 mm                #weaponInset = 2 mm

#weaponBottomVerticalExtension = 1.5 mm    # detachable weapon extends below shell
#weaponTopVerticalExtension    = 4 mm      # detachable weapon extends above shell
#weaponSlotExtraWidth          = 1 mm      # 3D-print kerf widening on slot
#batteryWallZipTieGap          = 3 mm      # gap below battery wall at Z=3..6

#powerHoleAngle = 140 deg           #ledHoleAngle = -24 deg
```

Derived variables (resolve as expression text **in the Variable Studio only** — DO NOT reference them in sketch constraint VALUES or in `variableRadius`/`variableDepth`):

```
#botRadius          = #botDiameter / 2
#motorMountLength   = #motorLength + 4 mm
#motorMountWidth    = #motorDiameter + 4 mm
#motorWallWidth     = 2 * sqrt((#botDiameter/2)^2 - (#motorWallOffset + #motorWallThickness)^2) - #botShellThickness
```

Workaround for derived var refs in constraints: paste the inline expression verbatim. `sqrt()` and `^` work fine in constraint VALUES, just not `#derivedVar` references.

## Part Studio layout — what's in each

- **`lid`**: 138mm ⌀ × 1mm tall disk. Six M3 tap holes (one per 60° around bolt circle), 2 LED/power holes (5mm), D-shaped wheel-well cut at radius 64 + chord Y=-31, motor clearance cut (split into negY + posY extrudes — single offset-plane sketch path is broken, see gotchas).
- **`shell_no_weapon`**: main shell, hollow, motor wall + bushing relief, motor mount + ziptie holes, 4 lid taps (NE/NW silently slipped — DISTANCE-MINIMUM bug), Ø12 weapon mount hole, battery wall + ziptie gap, arduino shelf (FS).
- **`shell_with_weapon`**: same as shell_no_weapon MINUS the weapon mount hole, PLUS a parametric weapon bar slab at the front (X=±20, Y=[-76.5, -57.5], Z=[0, 33]). **6** lid taps (E/NE/NW/W/mwL/mwR all cut — radial MINIMUM constraints dropped). Arduino shelf is a freshly-built FS feature (source archived to `docs/fs/arduino-shelf.fs`).
- **`weapon`**: detachable bar with slot for shell wall + Ø12 mount hole. 8 features. Volume parity vs SCAD STL: 0.11%.
- **`weapon_pin`**: trivial press-fit pin, 2 features.

## Validated workflow patterns

### Verification loop after every mutation

```
describe_part_studio(doc, ws, partStudioId, views=["iso"])
```

One call → feature tree + status flags + all body topology + multi-view renders. **Do not** chain `get_features` + `list_entities` + `render_part_studio_views` separately; the consolidated tool is faster AND its text catches more errors.

`get_features` alone returns a 128KB+ blob that exceeds the MCP response cap on Part Studios with 20+ features — the runtime auto-saves to `tool-results/*.txt`. Parse with `ast.literal_eval` (Python repr, not JSON).

### Sketch + extrude: parametric circles

Constraint-first with `DIAMETER` + base-variable expressions:
```python
create_sketch(plane="Top", entities=[
    {"id": "origin", "type": "point", "at": [0, 0]},
    {"id": "outer", "type": "circle", "center": [0, 0], "radius": 69}
], constraints=[
    {"type": "COINCIDENT", "entities": ["outer.center", "origin"]},
    {"type": "DIAMETER", "entity": "outer", "value": "#botDiameter"}
])
create_extrude(sketchFeatureId=..., depth=33, variableDepth="botHeight", operationType="NEW")
```

### REMOVE-on-face cuts: auto-flip is usually correct

REMOVE extrudes on a picked face auto-set `oppositeDirection=true` so the cut goes INTO the body's material. Override only when cutting INTO a feature sitting ON TOP of the sketch face (see battery_wall_zip_tie_gap):
```
create_extrude(operationType="REMOVE", depth=3, oppositeDirection=false, forceOppositeDirection=false)
```

### Multiple BLIND REMOVE from one sketch (asymmetric ranges)

The lid motor cut spans Y=-28 to +23. Sketch ONCE on Front plane, then:
- First REMOVE 28 (default direction = -Y)
- Second REMOVE 23 with `oppositeDirection=true` (+Y)

Cleaner than offset planes (which have `SKETCH_NO_PLANE` bugs).

## Cookbook recipes

### Recipe 1: Replay a Part Studio's features from a cached get_features dump

When `get_features` overflows the MCP response cap, the runtime saves to a file. Parse it like this:

```python
import ast
path = "/path/to/tool-results/mcp-...-get_features-...txt"
content = open(path).read().removeprefix("Features data: ")
data = ast.literal_eval(content)  # Python repr, NOT JSON

for i, feat in enumerate(data['features']):
    name = feat.get('name')
    bt = feat.get('btType')         # BTMSketch-151 / BTMFeature-134
    ftype = feat.get('featureType') # extrude / arduinoShelf / etc.
    # For extrudes, the operative params:
    for p in feat.get('parameters', []):
        pid = p.get('parameterId')  # depth / operationType / oppositeDirection / etc.
        # quantity params have .expression
        # enum/bool params have .value
```

Replay by re-deriving each feature's sketch entities + constraints from your knowledge (usually faster than parsing the raw BT representation), and pasting the extrude params verbatim. Validated on `hamachi-v5c` (shell_with_weapon replay).

### Recipe 2: STL parity check vs SCAD reference

Onshape exports STL in METERS (not mm). The mesh-diff tool needs to scale ×1000.

```python
import struct
def stl_bbox_vol(path, scale=1000.0):  # scale=1.0 for SCAD STLs (mm-native)
    # ... parses binary or ASCII STL, returns (bbox, volume)
```

Compare body volume (exact, from `get_mass_properties` or `describe_part_studio`) — NOT the Onshape STL volume (coarse tessellation). SCAD STLs are usually mm-native and high-tessellation. Target ≤1% volume delta after accounting for known deltas (e.g. missing FS features).

### Recipe 3: Tilted-plane FS custom feature (arduinoShelf pattern)

See `docs/fs/arduino-shelf.fs` for a complete example. Key elements:

```fs
FeatureScript 2909;
import(path : "onshape/std/geometry.fs", version : "2909.0");

annotation { "Feature Type Name" : "<name>" }
export const myFeat = defineFeature(function(context is Context, id is Id, definition is map)
    precondition {}
    {
        // 1. Compute the rotated local frame in FS:
        const cosRx = cos(rxAngle); const sinRx = sin(rxAngle);
        const cosRz = cos(rzAngle); const sinRz = sin(rzAngle);
        const localX = vector(cosRz, sinRz, 0);
        const localY = vector(-cosRx*sinRz, cosRx*cosRz, sinRx);
        const localN = cross(localX, localY);
        const origin = vector(46, 19, 15) * millimeter;

        // 2. Build the tilted plane (NO opPlane needed):
        var sk = newSketchOnPlane(context, id + "sk", {
            "sketchPlane" : plane(origin, localN, localX)
        });
        skRectangle(sk, "r", { "firstCorner" : vector(0,0)*millimeter,
                               "secondCorner": vector(20, 38)*millimeter });
        skSolve(sk);

        // 3. Use extrude(...), NOT opExtrude(...):
        extrude(context, id + "ext", {
            "entities"      : qSketchRegion(id + "sk"),
            "endBound"      : BoundingType.BLIND,
            "depth"         : 2 * millimeter,
            "operationType" : NewBodyOperationType.NEW
        });

        // 4. opBoolean UNION with qAllNonMeshSolidBodies, NOT qEverything(EntityType.BODY):
        opBoolean(context, id + "merge", {
            "tools"         : qAllNonMeshSolidBodies(),
            "operationType" : BooleanOperationType.UNION
        });
    });
```

## Known gotchas — hamachi-specific summary

(Full details in `feedback_onshape_mcp_gotchas` auto-memory and `docs/onshape-migration.md` lessons-learned sections.)

- **`DISTANCE direction=MINIMUM`** with the same value on 3+ entities silently collapses positions. Workaround: drop the MINIMUM constraints on those entities and use seed coords. The lid_screw_holes sketch hit this — `shell_no_weapon` lost 2 of 6 perimeter taps; `shell_with_weapon` got all 6 after the workaround. Same applies to TWO DISTANCE-H/V with same anchor + same value.
- **Derived Variable Studio vars** fail in constraint VALUES (silent `SKETCH_UNSOLVABLE_CONSTRAINT`) — paste inline expression instead.
- **`create_extrude` `depth` arg** rejects `#var` syntax — use the separate `variableDepth` (base var name, no `#`).
- **`create_offset_plane` faceId** fails on a sketch with `SKETCH_NO_PLANE` — sketch on standard planes + use opposite-direction extrudes for asymmetric ranges.
- **Inside FS:** prefer `extrude(...)` over `opExtrude(...)`. Use `qAllNonMeshSolidBodies()` for boolean tools, not `qEverything(EntityType.BODY)`.
- **STL exports come out in METERS** — scale ×1000 to compare with mm-native SCAD STLs.
- **MCP doesn't expose `delete_element` or cross-element FS reference** — extra Feature Studio elements accumulate; clean up via Onshape web UI when they bother you (`hamachi-m18` filed for the current set).

## Deliberate divergences from SCAD reference

When STL parity comes up short, check whether the gap is one of these intentional Onshape choices:
- All lid screw pilots use 2.5mm (M3) instead of SCAD's 2.2mm (#2-28).
- LED/power holes use 5mm instead of SCAD's accidentally-cut 8mm (SCAD bug: positional `cylinder(h, r1, r2)` interpreted Diameter as Radius).
- Weapon bar sits at z=0..38.5 instead of SCAD's z=-1.5..37 (`hamachi-kaj` deferred).
- Lid retains the 5mm outer rim crescent below the wheel-well chord (`hamachi-5fc` resolved: keep for stiffness; SCAD's cube chord-cut form is preserved in the frozen reference only).

## When to load this skill

- Starting any new Onshape work on hamachi.
- A user mentions "Onshape" or one of the Part Studio names.
- You're about to call `create_sketch`, `create_extrude`, or `write_featurescript_feature` against the hamachi doc.

Skip if the user is asking about firmware, OpenSCAD, or non-Onshape topics.
