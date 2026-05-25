# ADR-0001 — Hard fork from nothinglabs/openmelt2

- **Status:** Accepted
- **Date:** 2026-05-24

## Context

[nothinglabs/openmelt2](https://github.com/nothinglabs/openmelt2) is an open-source Arduino-based melty-brain combat
robot controller, originally authored by Rich Olson. It is well-architected for what it is — a teaching/reference
design for plastic antweight (16 oz) class.

This project will be worked on for years and will need to evolve in directions that diverge significantly from
upstream: MCU port to ESP32, beetleweight (1kg) and larger mechanical designs, telemetry, receiver protocol
support, possibly a Rust rewrite. Upstream is a single-maintainer hobby project with an Atmega32u4-only scope.

## Decision

Hard fork. Rename the project to `hamachi`. No tracking of upstream commits. Origin is
`github.com/griswaldbrooks/hamachi`.

## Consequences

- Free to rename modules, restructure folders, change build system, swap MCU, etc.
- Upstream bug fixes are not auto-inherited — must be cherry-picked manually if relevant.
- License (CC BY-NC-SA 4.0) carries forward. Attribution to Rich Olson / nothinglabs preserved in the top-level
  readme. Share-alike means downstream forks of `hamachi` must also be CC BY-NC-SA.
- Non-commercial clause limits monetization options for any future productized version of this work.
