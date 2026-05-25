# ADR-0005 — Use MEUS RACING ME-8B as the radio

- **Status:** Accepted
- **Date:** 2026-05-25

## Context

The upstream reference platform specifies a FlySky i6 TX + iA6 RX. We don't have one on hand. Alternatives
considered:

- **MEUS RACING ME-8B** — 8-channel 2.4GHz pistol-grip-style RC car/crawler radio with M-08A receiver. Receiver
  outputs per-channel PWM in two modes: digital servo (333Hz) and "simulation" servo (50Hz, 500–2500µs pulse). The
  50Hz mode is standard hobby servo PWM — what the existing `rc_handler.cpp` is built around.

## Decision

Use the MEUS ME-8B + M-08A. Configure the receiver to **Simulation servo / 50Hz** mode.

## Consequences

- No firmware changes required — the existing per-channel-PWM decode in `rc_handler.cpp` works as-is.
- 8 channels is overkill for a melty-brain that needs only 3 (throttle, fwd/back, left/right) — extra channels are
  available for future use (mode switch, weapon arm, etc.).
- Per-channel-PWM constraint locks us in to PWM-output receivers for now. Decoupling from PWM (adding SBUS/PPM/iBUS
  support) is tracked as Phase 4 in the [roadmap](../roadmap.md).
- **Critical safety setup:** verify the receiver's failsafe sets throttle channel to 0% (or signal-loss) on radio
  loss. Re-verify before every test session.
- Pistol-grip ergonomics differ from FlySky i6 stick layout — config-mode UX (forward/back/left/right gestures from
  upstream) needs to be tested with this transmitter; trigger/wheel mappings may surprise.

## Implementation notes (2026-05-25)

Ended up with the **MEUS RACING ME-10B** (orange, [Amazon B0DMSQ6J36](https://www.amazon.com/dp/B0DMSQ6J36))
rather than the ME-8B. Same MEUS family — the ME-10B is the 10-channel sibling of the ME-8B, and the bundled
**M-10A** receiver is the 10-channel sibling of the M-08A. PWM output behavior, simulation servo mode, binding
procedure, and firmware compatibility are all identical. Extra channels are unused for melty (3 needed).

**Why the swap:** the orange ME-8B kit listing on Amazon ships from MeusRacing-Direct with a 5/27–6/24 delivery
window — too late for 6/6. The ME-10B has Prime-shipping sellers and arrives next-day. Also peer-recommended by a
fellow competitor.

**Order specifics:**
- ME-10B kit (includes 1× M-10A receiver) ×1 — the primary radio
- Standalone M-10A receiver ×1 — battle-damage spare (the receiver is the part inside the bot getting hit)

**Setup work still required:**
- Configure receiver to **Simulation servo / 50Hz** mode (not 333Hz digital).
- Map pistol-grip controls to the firmware's expected channels: trigger → throttle-only (no reverse), wheel → L/R,
  an aux thumb stick → F/B. The MEUS supports this via mixing menus.
- Verify failsafe: throttle channel must go to 0% (or signal-loss state) on radio loss. Re-test before every
  spin-up session.
