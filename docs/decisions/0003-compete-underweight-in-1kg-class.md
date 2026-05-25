# ADR-0003 — Compete underweight in 1kg class for 2026-06-06

- **Status:** Accepted
- **Date:** 2026-05-25

## Context

The competition on 2026-06-06 is a 1kg (beetleweight-class) event. The upstream antweight reference platform
targets ~440g (plastic antweight, 16 oz). A true beetleweight design differs in motor sizing (brushless inrunner +
ESC), battery (3S/4S vs 2S), driver topology (brushless ESC vs MOSFET), shell durability material, wheel/hub class,
and accel placement.

Upstream notes that a Hobbypower 30A brushless ESC running SimonK firmware has been verified to work — but no build
plans, BOM, or schematic for a brushless-driven build exist in the repo.

Available time: 12 days. Designing, sourcing, building, and tuning a real beetleweight from scratch in that window
carries high risk of not having a working robot on competition day.

## Decision

Build the upstream antweight reference platform as-specified and enter underweight (~440g in a 1kg class).

## Consequences

- We field a working robot on 6/6. Lose the mass advantage; mass disadvantage in any pushing/durability matchup.
- The upstream BOM and proven firmware mean low bringup risk and a survivable schedule.
- The "real beetleweight" build becomes Phase 3 of the [roadmap](../roadmap.md), with the SimonK brushless build
  plans as a sub-deliverable.
- Future events in this class will need the beetleweight build to be competitive.
