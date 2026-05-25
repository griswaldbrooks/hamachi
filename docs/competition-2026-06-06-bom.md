# BOM — 2026-06-06 Build

Exact bill of materials for the 2026-06-06 competition build, with vendor part numbers for reproducibility.
Build strategy and 13-day schedule live in [competition-2026-06-06.md](./competition-2026-06-06.md). Substitution
rationale and on-hand inventory are at the bottom.

**Orders placed 2026-05-25.**

## Adafruit

| Role | Adafruit PID | Mfg PN | Qty | Unit | Total | Notes |
|---|---|---|---|---|---|---|
| MCU | [3677](https://www.adafruit.com/product/3677) | Adafruit ItsyBitsy 32u4 5V 16MHz | 2 | $9.95 | $19.90 | Primary + spare. Drop-in for the upstream-specified Arduino Micro — same Atmega32u4. |
| Accelerometer | [4627](https://www.adafruit.com/product/4627) | Adafruit H3LIS331 ultra-high-range triple-axis breakout | 2 | $24.95 | $49.90 | Primary + spare. Breakout includes the 3V↔5V level converter — no external level shifting needed. |

**Subtotal:** $69.80

## Digi-Key

| Role | Digi-Key PN | Mfg PN | Qty | Unit | Total | Notes |
|---|---|---|---|---|---|---|
| Motor MOSFET | IRLB3813PBF-ND | Infineon **IRLB3813PBF** | 4 | $2.55 | $10.20 | N-channel logic-level, 30V / 260A, Rds(on) ≈ 3 mΩ. **Upgrade from upstream's discontinued RFP30N06LE** — ~15× lower Rds(on) means dramatically less heat dissipation. 30V Vds is adequate for a 2S system; Schottky clamps back-EMF spikes. |
| Back-EMF clamp diode | 4530-30SQ045CT-ND | Anbon **30SQ045** | 4 | $1.47 | $5.88 | 30A axial Schottky, 45V. Across motor leads. Required — without it the MOSFET dies on motor back-EMF. |
| Bus capacitor | P14408-ND | Panasonic **EEU-FR1C472L** | 2 | $2.43 | $4.86 | 4700 µF / 16 V / 20% radial, 16×25 mm. Required across 5V bus per upstream guide — keeps motor noise from causing reboots. |
| LED current-limit resistor | 13-CFR-25JR-52-100RCT-ND | YAGEO **CFR-25JR-52-100R** | 10 | $0.035 | $0.35 | 100 Ω / 5% / 1/4 W axial. Sized for blue LED on 5V. |
| Battery divider — low | 13-MFR-25FRF52-10KCT-ND | YAGEO **MFR-25FRF52-10K** | 10 | $0.04 | $0.40 | 10 kΩ / 1% / 1/4 W axial. Low side of the 10:1 voltage divider for battery monitoring. |
| Battery divider — high | CF14JT100KCT-ND | Stackpole **CF14JT100K** | 10 | $0.027 | $0.27 | 100 kΩ / 5% / 1/4 W axial. High side of the divider. |
| Heading LED | C503B-BCN-CV0Z0461-ND | Cree **C503B-BCN-CV0Z0461** | 5 | $0.25 | $1.25 | 5 mm blue clear, ultra-bright (CV bin). Upstream calls for 3 mm — shell hole needs slight enlargement. Narrow ~20–30° beam, so **mount as far outboard on shell perimeter and proud of shell edge** to compensate. |

**Subtotal:** $23.21 in parts; **$35.76 with tariff + shipping + tax**.

## Amazon

| Role | Amazon ASIN | Mfg / Description | Qty | Unit | Total | Notes |
|---|---|---|---|---|---|---|
| Radio (TX + RX kit) | [B0DMSQ6J36](https://www.amazon.com/dp/B0DMSQ6J36) | MEUS Racing **ME-10B** transmitter (orange) + bundled **M-10A** receiver | 1 | $49.99 | $49.99 | 10-channel 2.4 GHz pistol-grip. Bundled M-10A becomes the primary receiver. Configure to **Simulation servo / 50Hz** mode for `rc_handler.cpp` compatibility. See [ADR-0005](./decisions/0005-select-meus-me-8b-radio.md). |
| Spare receiver | _(see standalone M-10A listing)_ | MEUS Racing **M-10A** | 1 | $12.99 | $12.99 | Battle-damage spare. The receiver is inside the robot getting hit; this is the meaningful redundancy. |
| Battery | _(see Ovonic listing)_ | **Ovonic 2S 850 mAh 7.4V 120C** LiPo with **XT30** plug | 1 pack of 2 | $29.99 | $29.99 | Two batteries per pack. Rotate during tuning (one charges, one runs). 120 C discharge rating is overkill but ensures zero voltage sag during motor surges. |
| Drive motor | _(see INJORA listing)_ | **INJORA 540** brushed motor **55T** waterproof | 2 | $14.98 | $29.96 | Upstream specifies 45T; 55T is the available-in-time fallback. Lower top RPM (~2200–2500 vs ~3000) but easier on the electronics — see substitutions table. |

**Subtotal:** $122.93

## FingerTech Robotics (CAD)

Direct from [fingertechrobotics.com](https://www.fingertechrobotics.com/).

| Role | FingerTech SKU | Qty | Unit (CAD) | Total (CAD) | Notes |
|---|---|---|---|---|---|
| Power switch | **FT-MINI-SWITCH** (Torx) | 3 | $12.97 | $38.91 | Main power kill, key-operated. Extras for spares and future builds. |
| Wheel hubs (pair) | **FT-TWIST-HUBS**, 3 mm Thin | 2 | $27.93 | $55.86 | Each pair clamps one foam wheel. Sold without wheels — wheels fabricated in-house this build. |

**Subtotal:** $94.77 CAD (~$69 USD at recent rates).

## On-hand (no order)

| Item | Notes |
|---|---|
| 16 AWG silicone hookup wire (red / black) | Battery and motor power leads. |
| 22–26 AWG signal wire (multi-color) | RC channel and I2C wiring. _(Confirm gauge available.)_ |
| XT30 male/female connectors | Battery interface, mates with the Ovonic LiPo pigtail. |
| Copper-clad FR4 PCB blank | Cut piece used as MOSFET tab heatsink. |
| Heat shrink (assorted) | All solder joints. |
| Mounting tape (3M VHB / Scotch Extreme) | MCU, accelerometer, MOSFET, battery. |
| Zip ties (4–6") | Motor mount + general strain relief. |
| Thread-forming plastic screws | For PPA-CF lid attachment — prior-build validated. Replaces upstream's #2-28 self-tap. |
| LiPo balance charger (iMax B6 or equiv.) | Required for 2S 850 mAh charge/discharge. |
| LiPo storage bag | Safety during charging and transport. |
| Soldering iron, solder, flux | High-temp work for MOSFET-to-copper-clad heatsink. |
| Hot glue gun + sticks | LED, resistors, switch, cap mounting in chassis. |
| 3.175 mm (1/8") drill bit | Drilling out wheel hub from 3 mm to fit motor shaft. |

## Self-fabricated

| Item | Notes |
|---|---|
| Foam wheels | In-house technique (prior-build validated). Builder makes own rather than buying FingerTech foam wheels. |
| Shell (`shell_no_weapon.stl`), lid (`lid.stl`), weapon (`weapon.stl`), weapon pin (`weapon_pin.stl`) | Bambu X1C. **First pass: PLA fit-check** (in progress 5/24 evening). **Final pass: PPA-CF**, after drying filament 8+ hrs @ 80°C. See [ADR-0004](./decisions/0004-use-ppa-cf-print-material.md). Source STLs in [`antweight_reference_platform/`](../antweight_reference_platform/). Print no-weapon shell first; weapon attaches via detachable pin. |

## Cost summary

| Vendor | USD |
|---|---|
| Adafruit | $69.80 |
| Digi-Key | $35.76 |
| Amazon | $122.93 |
| FingerTech | ~$69 (CAD→USD) |
| **Total** | **~$298** |

## Substitutions from upstream BOM

| Upstream spec | Used instead | Reason |
|---|---|---|
| Arduino Micro | Adafruit ItsyBitsy 32u4 5V (PID 3677) | Same Atmega32u4. Smaller. No firmware impact. |
| RFP30N06LE MOSFET | Infineon IRLB3813PBF | Original is discontinued. IRLB3813 has ~15× lower Rds(on) — runs much cooler. 30V Vds adequate for 2S. |
| 45T 540 brushed motor | INJORA 540 55T | 45T sold out / slow ship. 55T is slower top-RPM (still > `MIN_TRANSLATION_RPM=400`) but easier on the MOSFET, motor, and Schottky. |
| 3 mm blue LED, wide-angle | Cree C503B-BCN-CV0Z0461 (5 mm, narrow-beam) | Higher brightness budget. Shell hole needs slight enlargement; mount choice compensates for narrow beam. |
| FlySky FS-i6 + iA6 radio | MEUS RACING ME-10B + M-10A | Pistol-grip preference, 10ch, peer-validated. ME-10B chosen over ME-8B for Prime shipping. |
| FingerTech 1.75"×0.5" foam wheel | In-house fabricated | Builder has prior experience; cost + time savings. |
| #2-28 × 1/4" Phillips self-tap screws | Thread-forming plastic screws | Behaves much better than self-tap in CF-filled prints; prior-build validated. |
| FlySky-style 7.4V 900 mAh LiPo | Ovonic 2S 850 mAh 120C, XT30 | Equivalent capacity, higher C-rating, XT30 connector. |

## Open items / things to verify

- M-10A receiver per-channel PWM output behavior — confirm on first power-up before wiring to MCU (jumper + scope or LED).
- ME-10B failsafe configuration — verify throttle channel returns to 0% on signal loss; re-verify before every test session.
- ME-10B pistol-grip → 3-axis melty control mapping (trigger as throttle-only, wheel as L/R, aux thumb stick as F/B) — configure via mixing menus.
- PPA-CF dimensional behavior vs PLA fit-check — anything that surprises us in the fit-check goes here.
- Foam wheel balance — test-spin before installing.
- Thread-forming screw engagement in PPA-CF — test on a scrap before committing the shell print.
