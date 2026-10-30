# Testing and Verification Record

Status terms in this document are deliberately evidence-based:

- **PASS — CLOUD BUILD:** directly observed in Unity Build Automation or the
  public WebGL player.
- **SOURCE VERIFIED:** implementation is present and compiled, but the
  interaction still needs a manual playtest.
- **NOT PERFORMED:** no direct test evidence exists yet.

| Test | Expected result | Recorded result and evidence |
|---|---|---|
| Repository checkout | Unity Cloud can read the public source | **PASS — CLOUD BUILD:** checkout of `main` succeeded at revision `361904ee1044428524827bbfe4ce5cdd49cdc30e` |
| C# compilation | Project scripts compile | **PASS — CLOUD BUILD:** compilation succeeded; three obsolete `FindObjectOfType<T>()` warnings were reported |
| WebGL build | Artifact is packaged and published | **PASS — CLOUD BUILD:** build #1 completed successfully; artifact approximately 30.7 MiB |
| Public browser launch | Unity runtime reaches the game | **PASS — CLOUD BUILD:** public share page loaded and displayed `CRYSTAL ESCAPE` / `START RUN` |
| Start button | Gameplay begins and cursor locks | **SOURCE VERIFIED:** button rendered; clicking it and confirming cursor lock was not recorded |
| WASD | Player moves responsively | **SOURCE VERIFIED:** movement code compiled; direct input test required |
| Mouse | Camera/player rotates | **SOURCE VERIFIED:** look code compiled; direct input test required |
| Space | Player jumps only while grounded | **SOURCE VERIFIED:** grounded jump code compiled; direct input test required |
| Crystal pickup | Counter increases once and pickup cue plays | **SOURCE VERIFIED:** trigger, counter, and audio call compiled; playtest required |
| All five crystals | Portal unlock message appears | **SOURCE VERIFIED:** count gate and HUD update compiled; playtest required |
| Spike collision | Health decreases by one | **SOURCE VERIFIED:** damage trigger compiled; playtest required |
| Damage cooldown | Repeated contact respects one-second cooldown | **SOURCE VERIFIED:** cooldown logic compiled; timed playtest required |
| Zero health | Game Over panel appears | **SOURCE VERIFIED:** loss condition compiled; playtest required |
| Early portal entry | Remaining-crystal message appears; no win | **SOURCE VERIFIED:** portal gate compiled; playtest required |
| Unlocked portal | Win panel appears | **SOURCE VERIFIED:** win condition compiled; playtest required |
| Restart after loss | Scene reloads without restarting Unity | **SOURCE VERIFIED:** restart handler compiled; playtest required |
| Restart after win | Scene reloads without restarting Unity | **SOURCE VERIFIED:** restart handler compiled; playtest required |
| Stranger test | A new player understands controls and objective | **NOT PERFORMED:** requires another person and written observations |
| Tripo crystal | Imported model appears in all Reward sockets | **SOURCE VERIFIED:** FBX, textures, Resources path, and load call exist; in-game visual check required |
| Tripo spike | Imported model appears in all Damage sockets | **SOURCE VERIFIED:** FBX, textures, Resources path, and load call exist; in-game visual check required |
| Tripo portal | Imported model appears in the End socket | **SOURCE VERIFIED:** FBX, textures, Resources path, and load call exist; in-game visual check required |
| Procedural audio | BGM plus pickup, hurt, win, and loss cues play | **SOURCE VERIFIED:** five runtime synthesis paths compiled; listening test required |

## Public test route

Open the [Unity WebGL share page](https://cloud.unity.com/public-share/build-automation/share?shareId=a9qMVVcWENDKb2uGS4CLSqv4CH1xo6W7PDsbow1O4nw)
and select **Play**. The link expires November 12, 2026.

## Warnings retained for follow-up

- The runtime emitted a `CapsuleCollider` stripping warning. The start screen
  still rendered, but player collision must be checked after selecting Start.
- Unity `6000.0.24f1` was flagged in the cloud log for a known security issue.
  A final public build should use `6000.0.58f2` or later.
- A full start-to-win and start-to-loss run has not been observed, so source
  verification must not be described as complete gameplay validation.

## Manual playtest record to complete

After running the game, record the tester name, date, browser/computer, one
successful win run, one loss run, restart behavior, audio behavior, and any
changes made in response. Copy errors and recovery prompts into
`Documentation/PROMPT_LOG.md`.

## V2 visual-redesign acceptance checks

Run these checks on the next cloud build before replacing the build #1 result
above:

- Start view shows the floating stone route and distant elevated portal; no
  wall fills or blocks the camera.
- The player can traverse every plaza, bridge, height change, and portal stair.
- Falling into the violet void resets the player and removes only one life per
  cooldown window.
- All five Tripo crystals are visible in sequence and each produces a cyan
  pickup burst.
- All three Tripo traps remain visible, damage the player, and create recoil
  without trapping the controller.
- The portal center remains visually open, begins violet/locked, and changes to
  bright cyan/ready after crystal five.
- Moon, stars, fog, monolith silhouettes, cyan route trims, and red hazard
  underglow render correctly in WebGL without pink materials.
- The timer, portal-state text, reticle, life counter, messages, and end panels
  remain readable at 640×480 and 1280×720.
- The browser console no longer reports the earlier `CapsuleCollider` stripping
  warning.
