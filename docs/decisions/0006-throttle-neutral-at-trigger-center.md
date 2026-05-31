# ADR-0006 — Move throttle neutral to the pistol trigger's spring-center

- **Status:** Accepted
- **Date:** 2026-05-31

## Context

The melty firmware reads throttle as a servo pulse width: `rc_get_throttle_percent()`
(`openmelt/rc_handler.cpp`) maps `IDLE_THROTTLE_PULSE_LENGTH` → 0% and
`FULL_THROTTLE_PULSE_LENGTH` → 100%. Upstream defaults are **1250 µs / 1850 µs**, which
assume a throttle control that **rests at the bottom** (a thumb stick or slider).

The chosen radio (MEUS pistol-grip, see [ADR-0005](./0005-select-meus-me-8b-radio.md)) drives
throttle from the **trigger**, which is **spring-centered**: released, it outputs ~**1507 µs**
(the channel center, ~1500), with a few µs of jitter. Against the upstream 1250 µs idle point a
released trigger reads **~43% throttle**. Two failures follow:

- `VERIFY_RC_THROTTLE_ZERO_AT_BOOT` requires throttle = 0% to arm — at 43% the bot **never arms**.
- If it did arm, it would **spin at idle** the instant the trigger is released.

Radio-side correction was attempted first and is insufficient: **SUB TR** on the throttle channel
caps at roughly −120 µs (floor ~1380 µs ≈ 22% throttle), and stacking **TH Trim** hits the same
cap. The ME-8B has no mechanical throttle trim, and remapping throttle to a non-centering knob was
rejected on ergonomics. The radio cannot get the released-trigger pulse at or below 1250 µs.

## Decision

Move the throttle zero-point into firmware. In `openmelt/rc_handler.h`:

```c
#define IDLE_THROTTLE_PULSE_LENGTH 1550   // was 1250
```

`FULL_THROTTLE_PULSE_LENGTH` stays **1850**. 1550 sits ~40 µs above the trigger's ~1507 µs resting
jitter band — a deadzone so a released trigger **reliably** reads 0%, while a full squeeze (~2000 µs)
still clears 1850 → 100%.

This deliberately **breaks the firmware freeze** in [AGENTS.md](../../AGENTS.md) /
[ADR-0002](./0002-keep-arduino-micro-for-comp.md) for the 2026-06-06 build. The freeze exists to keep
tested logic stable; this is a one-line calibration constant required to make the selected radio
usable, with no change to control logic.

## Consequences

- **Verified on the bench 2026-05-31** (diagnostic build, USB serial): released trigger → `RC Throttle: 0`
  (16/16 samples), full squeeze → `RC Throttle: 100` (16/16). Compiles clean (45% flash / 23% RAM).
- Proportional band is now **1550–1850 µs** (300 µs). 100% is reached before full trigger travel — fine,
  full spin power stays easily reachable; the top of trigger travel is saturated at 100%.
- **Single source of truth:** the threshold is defined once (`rc_handler.h`) and consumed only in
  `rc_get_throttle_percent()`. Downstream consumers (`spin_control.cpp`, `motor_driver.cpp`, the boot-arm
  and RPM-flash gates in `openmelt.ino`) use the abstract 0–100% value and inherit the new zero point.
- **Untouched look-alikes:** `CENTER_LEFTRIGHT_PULSE_LENGTH` and `CENTER_FORBACK_PULSE_LENGTH` are also
  `1500` but are unrelated (wheel / 3-way centers) and were left alone.
- **Revisit if the throttle control changes.** If a future build uses a non-centering throttle (knob/slider
  that rests low), the 1550 deadzone wastes the bottom of its range — revert toward 1250.

## Related

- [ADR-0005 — Radio selection](./0005-select-meus-me-8b-radio.md) (the pistol-grip ergonomics that forced this)
- [ADR-0002 — Keep Arduino Micro / firmware freeze](./0002-keep-arduino-micro-for-comp.md) (the freeze this overrides)
- Build log: [competition-2026-06-06-build-log.md](../competition-2026-06-06-build-log.md) (2026-05-31 session)
