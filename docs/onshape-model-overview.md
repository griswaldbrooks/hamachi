---
title: Onshape model overview
sidebar_position: 1
---

# Onshape model overview

> **Status:** Stub. To be expanded with embedded Onshape renders of each Part Studio, Variable Studio reference table, and feature-by-feature tour.

Until this page is filled out, the long-form record of how the model was built is in [Onshape migration log](./onshape-migration.md).

## Quick links

- **Document:** `hamachi` (Onshape) — id `3662b787ba73917e77c5a543` / workspace `64e642b76ea639b57af58924`
- **Source-of-truth:** Onshape model for all printable geometry; SCAD reference (`antweight_reference_platform/spinbot_ant.scad`) is frozen.
- **Migration log:** [onshape-migration.md](./onshape-migration.md) (long-form)
- **Agent quick-start:** the `hamachi-onshape` skill at `.claude/skills/hamachi-onshape/SKILL.md`

## Part Studios

| Element | Purpose | Features | Status |
|---|---|---|---|
| `variables` (Variable Studio) | All parametric variables | 34 base + 5 derived | populated |
| `lid` | Top lid with LED/power holes + motor clearance | 11 | built |
| `shell_no_weapon` | Main shell, detachable-weapon variant | 25 | built |
| `shell_with_weapon` | Main shell, integrated-weapon variant | 25 | built |
| `weapon` | Detachable weapon bar | 8 | built |
| `weapon_pin` | Press-fit pin | 2 | built |

## Coordinate system

- **Origin:** center of bot, at the bottom outer face of the shell (the arena-touching surface).
- **+Z** up (Onshape's Top plane normal).
- **+Y** away from the wheel side. SCAD places the wheel at `-Y`.
- **+X** is the long axis of the weapon slot.

The main shell base is sketched on the **Top plane** as a circle of diameter `#botDiameter`, extruded `+Z` for `#botHeight`.

## TODO

- Iso/top/front renders of each Part Studio embedded inline. The Onshape-MCP renders live in an in-process cache (no disk path exposed) — to mirror them to `docs-site/static/img/onshape/` for embedding, set `ONSHAPE_MCP_MIRROR_IMAGES_DIR=$(pwd)/docs-site/static/img/onshape` in the MCP server's environment before starting Claude Code (or add it to `.claude/settings.json` env block). After that, every `render_*` call writes `<image_id>.png` to that dir; reference them in markdown as `![iso](/img/onshape/img_xxxxxxxx.png)`.
- Variable Studio reference table grouped by topic (shell / motor / weapon / fastener).
- Per-Part-Studio feature tour with annotated screenshots.
- Cross-reference each printed STL to the Part Studio that generates it.
