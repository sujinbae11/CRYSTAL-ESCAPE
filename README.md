# Crystal Escape

**AME 294 — Portfolio Game 3: Unreal or Unity, Built with AI**

Public source repository: <https://github.com/sujinbae11/CRYSTAL-ESCAPE>

Playable WebGL share page: <https://cloud.unity.com/public-share/build-automation/share?shareId=a9qMVVcWENDKb2uGS4CLSqv4CH1xo6W7PDsbow1O4nw>  
The current Unity share link expires on **November 12, 2026**.

Crystal Escape is a compact third-person 3D collection game. The player has three health points and must collect five crystals, survive hazardous spike fields, and enter the exit portal. Both victory and defeat end in clear terminal screens with an immediate restart option.

![Crystal Escape key art](Trailer/KeyArt_CrystalEscape.png)

## Verified development status

The Unity source generates its level automatically when Play begins. It includes:

- responsive WASD movement, mouse look, and jumping;
- five functional reward collectibles;
- three functional damage hazards with one-second damage cooldown;
- health and crystal HUD;
- locked portal until every crystal is collected;
- win, loss, restart, and quit flows;
- start screen and stranger-readable controls;
- procedural runtime BGM and event cues, plus support for imported audio;
- runtime loading and functional sockets for three mandatory Tripo assets.

All three bespoke Tripo assets were generated, optimized to roughly 10K quads, exported as FBX with 2K textures, and wired to the functional Reward, Damage, and End sockets. Unity Build Automation build #1 successfully checked out the public repository, compiled the C# project, completed the WebGL/IL2CPP build, and published an approximately 30.7 MiB artifact. The public player was opened and the `CRYSTAL ESCAPE` / `START RUN` screen rendered successfully.

| Review area | Evidence-based status |
|---|---|
| Public GitHub source | **PASS** — repository is public and Unity Cloud checkout succeeded |
| WebGL package | **PASS** — Unity Build Automation build #1 finished successfully |
| Browser launch | **PASS** — Unity player initialized and displayed the start screen |
| Reward, Damage, End, restart | **SOURCE VERIFIED** — implemented in C#; a full manual playthrough is still required |
| Tripo assets | **SOURCE VERIFIED** — all three FBX files, textures, Resources paths, and load calls are present |
| Audio | **SOURCE VERIFIED** — five procedural fallback clips are generated at runtime; listening and course-policy checks remain |
| Trailer | **NOT YET PRODUCED** — key art and shot plan exist, but the required 30–60 second final video and gameplay capture do not |
| Personal reflection | **STUDENT AUTHORSHIP REQUIRED** — the worksheet must be replaced with a truthful 200–300 word reflection after playtesting |

### V2 visual redesign prepared

The gameplay source has been redesigned around the key art's floating-ruin composition. It replaces the camera-blocking boxed arena with a rising path of tiled stone plazas, bridges, stairs, side islands, and an elevated portal dais. The source also adds violet fog and void light, moon and stars, distant ruined silhouettes, cyan route lighting, a stylized explorer, animated pickups, damage recoil, an open locked/ready portal, and a clearer timer/objective HUD.

These V2 changes require a new Unity Build Automation run and complete browser playthrough. The public share page linked above continues to identify itself as build #1 until that replacement build is published and validated.

## Open and play

Recommended editor: Unity `6000.0` or a compatible later Unity 6 release.

For the quickest browser test, open the **Playable WebGL share page** above and select **Play**. The observed validation reached the title/start screen; complete the manual control, pickup, damage, win, loss, and restart checks in `Documentation/TESTING.md` before describing the entire loop as playtested.

To open the source in Unity:

1. Open this repository folder as a Unity project.
2. Wait for package and script import to finish.
3. Choose **Crystal Escape > Prepare Project** if the scene does not open automatically.
4. Press Play.
5. Select **Start Run**.

The repository includes `Assets/CrystalEscape/Scenes/MainGame.unity` in the build list. The runtime bootstrap creates the complete game in that scene when Play begins.

## Controls

| Input | Action |
|---|---|
| WASD | Move |
| Mouse | Look |
| Space | Jump |
| Escape | Release the mouse cursor |
| Left click | Capture the cursor again |

## Required gameplay loop

- **Reward:** collect five glowing crystals; each gives positive visual and audio feedback.
- **Damage:** spike zones remove one health point, with a one-second invulnerability window.
- **End — win:** collect all five crystals and enter the portal.
- **End — loss:** health reaches zero.
- **Restart:** available from either terminal state without restarting the editor.

## Build

From the Unity menu, select:

- **Crystal Escape > Build Windows**, or
- **Crystal Escape > Build WebGL**.

Build outputs go to the ignored `Builds/` directory. Upload the resulting ZIP to a GitHub Release or itch.io; do not commit packaged builds into the repository.

The repository is connected to Unity Build Automation and WebGL build #1 completed successfully from the `main` branch. The service detected the editor version from `ProjectSettings/ProjectVersion.txt`.

The successful build used Unity `6000.0.24f1`. Its cloud log reports a known security issue in that patch; rebuild with Unity `6000.0.58f2` or later before long-term public distribution.

## Tripo assets

The three mandatory functional assets are:

1. Crystal reward collectible
2. Spike damage hazard
3. Exit portal

Store the functional runtime files under `Assets/CrystalEscape/Resources/Tripo/` with these exact paths:

- `Crystal/Crystal.fbx`
- `SpikeTrap/SpikeTrap.fbx`
- `ExitPortal/ExitPortal.fbx`

The game loads these assets automatically and falls back to primitive placeholders only when a file is absent. Keep generation prompts, route, original format, optimization steps, and final placement in `Documentation/PROMPT_LOG.md`.

## Audio

Put generated audio in `Assets/CrystalEscape/Resources/Audio/` using the exact names described in that folder's README. The game will automatically replace its synthesized development tones with those clips.

## Documentation

- [Prompt log](Documentation/PROMPT_LOG.md)
- [Asset attribution](Documentation/ASSET_ATTRIBUTION.md)
- [Reflection worksheet — student-authored final required](Documentation/REFLECTION.md)
- [Testing checklist](Documentation/TESTING.md)
- [Trailer plan](Trailer/TRAILER_PLAN.md)

## Generative AI inventory

This list records the tools and outputs that are currently evidenced in the repository.

| Area | Tool | Use |
|---|---|---|
| Planning and code | OpenAI Codex | Project scaffold, Unity C# systems, documentation structure |
| 3D | Tripo Studio H3.1 / Smart Mesh | Crystal, spike trap, and portal; FBX export and topology optimization |
| Audio | OpenAI Codex + Unity runtime synthesis | Procedural BGM, pickup, hurt, win, and loss fallback clips; no external Week 6 audio files |
| Image | OpenAI built-in image generation | 16:9 key art and trailer opening frame |
| Video | No video model used | Key art and a shot plan exist; the final trailer and gameplay capture have not been produced |

## Repository hygiene

The Unity `.gitignore` excludes `Library/`, `Temp/`, `Logs/`, `Build/`, `Builds/`, and IDE-generated files. Packaged games belong in Releases or itch.io.

