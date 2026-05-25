# ADR-0004 — Print structural parts in PPA-CF on Bambu X1C

- **Status:** Accepted
- **Date:** 2026-05-25

## Context

The upstream antweight reference platform specifies PLA at 100% infill, 5+ shells, no cooling fan. PLA is brittle
under impact — fine for the antweight reference's intended testing role, marginal for any actual combat exposure.

The Bambu X1C supports CF-filled engineering filaments with its hardened steel hotend and hardened extruder gears.
PPA-CF (polyphthalamide + carbon fiber) is on hand.

PPA-CF has substantially higher impact strength, stiffness, and temperature resistance than PLA, at the cost of:
print difficulty (hygroscopic, requires drying), surface finish, and dimensional behavior (CF-filled prints have
less compliance and may need tolerance adjustments).

## Decision

Print structural parts (shell, lid, weapon, weapon_pin) in PPA-CF on the X1C.

## Consequences

- Shell durability improved significantly over the PLA reference.
- Print process more involved: filament drying (~80°C for 8+ hours), hardened nozzle confirmation, slicer profile
  tuning, potential dimensional re-checks vs the upstream STLs.
- Self-tap screw behavior in PPA-CF differs from PLA — pilot thread engagement may need testing; heat-set inserts
  are a fallback.
- Validate with a small test print (e.g., a corner of the shell or a fit-check coupon) before committing the full
  shell print.
- PLA backup print recommended in case PPA-CF fit doesn't pan out under the 12-day clock.
