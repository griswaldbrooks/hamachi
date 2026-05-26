---
title: Build & assembly guide
sidebar_position: 1
---

# Build & assembly guide

> **Status:** Stub. To be expanded with photographed step-by-step assembly.

Step-by-step assembly from the four STL prints + electronics is the goal of this page. Until it's filled out:

- **For the 2026-06-06 build:** see [competition-2026-06-06.md](./competition-2026-06-06.md) and the [Build Log](./competition-2026-06-06-build-log.md).
- **For the parts list:** [BOM](./competition-2026-06-06-bom.md).
- **For Onshape model context:** [Onshape model overview](./onshape-model-overview.md).

## Print files

Four printable STLs (exported from the Onshape Part Studios; SCAD reference STLs are at `antweight_reference_platform/`):

| STL | Onshape source | Part name |
|---|---|---|
| `lid.stl` | `lid` Part Studio | Lid (138 × 138 × 1 mm) |
| `shell_no_weapon.stl` | `shell_no_weapon` Part Studio | Shell, detachable-weapon variant |
| `shell_with_weapon.stl` | `shell_with_weapon` Part Studio | Shell, integrated-weapon variant |
| `weapon.stl` | `weapon` Part Studio | Detachable weapon bar |
| `weapon_pin.stl` | `weapon_pin` Part Studio | Press-fit pin |

Choose **either** `shell_no_weapon` + `weapon` + `weapon_pin`, **or** `shell_with_weapon`, depending on whether you want the weapon swappable.

## Print material

PPA-CF on Bambu X1C. See [ADR-0004](./decisions/0004-use-ppa-cf-print-material.md) for material rationale; pre-print filament drying procedure is tracked under `bd show hamachi-obd`.

## TODO

- Photographed assembly steps (orient & insert motor, route wires, mount Arduino, etc.).
- Screw torque + thread engagement notes (PPA-CF can crack at the tap holes if over-torqued; M3 self-tap pilots are 2.5mm in the Onshape model).
- Battery installation + retention.
- Lid alignment + screw pattern.
- Wheel hub press-fit procedure (drill 3 → 3.175mm per `bd show hamachi-t8l`).
- Weapon-pin install for the detachable-weapon variant.
