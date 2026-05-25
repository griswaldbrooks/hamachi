# AGENTS.md

Project conventions for AI coding agents (Claude Code, etc.) working in this repo.

## Issue tracking

This repo uses **two** trackers, by design:

- **GitHub Issues** — for roadmap items, decisions, anything visible to humans or collaborators.
  See https://github.com/griswaldbrooks/hamachi/issues.
- **[beads](https://github.com/steveyegge/beads) (`bd` CLI)** — for agent working memory: subtasks, exploration
  notes, anything fine-grained that doesn't need to be public.

**Default convention:** when you discover a sub-task while working, file it in `bd`. When you make a decision worth
preserving, write an ADR in `docs/decisions/`. When something needs a human's attention or belongs on the roadmap,
open a GitHub issue.

## Documentation

- All durable project docs live under `docs/`. The tree is docusaurus-compatible.
- Architectural decisions go in `docs/decisions/NNNN-slug.md` in MADR format.
- Don't edit accepted ADRs to reverse a decision — write a superseding one.

## Code style

The firmware (`openmelt/`) is upstream Arduino C++ inherited from nothinglabs/openmelt2. Match the existing style
when editing — header-guarded `.h` files, lowercase-underscore filenames, Arduino-style `setup()/loop()` for the
main sketch. Larger structural changes (renaming the sketch folder, restructuring modules) should be ADR-tracked
before execution.

## Hardware context

- Target MCU for the 2026-06-06 competition: Arduino Micro (Atmega32u4). No firmware changes through that date.
- Target MCU for the long-term fork: ESP32. See [docs/roadmap.md](./docs/roadmap.md).
- Pinned library versions live in `arduino_library_archives/` — do not assume "latest" works (upstream readme calls
  out an I2C hang in newer SparkFun_LIS331).
