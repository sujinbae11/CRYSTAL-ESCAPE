# Prompt and Process Log

## Documentation note

This document is a chronological record of the AI-assisted development of
*Crystal Escape*. Text labeled **Exact prompt** is reproduced from the
generation interface or development session. Short conversational requests
are followed by a detailed **Reconstructed execution brief** so that the
design goal, constraints, and acceptance criteria are clear to a reviewer.
These reconstructed briefs describe what the request meant in practice; they
are not presented as verbatim user quotations.

Passwords, access tokens, and other credentials are intentionally excluded.
Build results, paths, model IDs, credit changes, and errors are included only
when they were directly observed.

## 1. Assignment analysis and scope selection

**Date:** October 29, 2026  
**Tool and route:** OpenAI Codex desktop chat  
**Prompt status:** Original request plus reconstructed brief

**Original request (Korean):** “이거 어떻게 하면 되는지 차례대로 자세히 알려줘”  
**English translation:** “Explain in detail, step by step, how to complete this.”

**Reconstructed execution brief:**

```text
Analyze the AME 294 Portfolio Game 3 brief and propose a Unity project that
satisfies the required Reward–Damage–End loop, responsive controls, three
functional Tripo assets, terminal win and loss states, restart without an
Editor reset, prompt documentation, AI-tool attribution, and a playable
build. Keep the scope small enough to complete without a locally installed
Unity Editor, and identify every item that must still be personally tested,
recorded, or reflected on before submission.
```

**Result:** A compact five-crystal escape game was selected. The player has
three health points, collects five rewards, avoids three spike hazards, and
uses an exit portal only after all rewards have been collected. This scope
gives each required Tripo asset a functional role instead of using AI models
as decoration.

## 2. Unity gameplay scaffold

**Date:** October 29, 2026  
**Tool and route:** OpenAI Codex desktop chat  
**Prompt status:** Original request plus reconstructed brief

**Original request (Korean):** “너가 만들어주면 안되니”  
**English translation:** “Could you make it for me?”

**Reconstructed execution brief:**

```text
Create a Unity 6 project named Crystal Escape. Implement a compact
third-person 3D arena with WASD movement, mouse look, grounded jumping, five
collectible crystals, three damage zones, three health points, a one-second
damage cooldown, and a portal that unlocks only after all rewards are
collected. Include separate win and loss screens, restart without restarting
the Editor, a start screen, readable controls, HUD feedback, development
audio, a Unity .gitignore, build utilities, and documentation templates.
Bootstrap the game from a minimal scene so the project can compile through
Unity Build Automation.
```

**Agent contribution:** Codex authored the Unity C# scaffold, scene/bootstrap
files, repository structure, documentation framework, build menu, and runtime
fallback assets.

**Files produced:**

- `Assets/CrystalEscape/Scripts/CrystalEscapeGame.cs`
- `Assets/CrystalEscape/Editor/CrystalEscapeProjectSetup.cs`
- `Assets/CrystalEscape/Scenes/MainGame.unity`
- `ProjectSettings/EditorBuildSettings.asset`
- `Packages/manifest.json`

**Validation at this stage:** Static source inspection only; a local Unity
Editor was not available.

## 3. Local Unity installation constraint

**Date:** October 29, 2026  
**Prompt status:** Original report plus reconstructed recovery brief

**Original report (Korean):** “내 컴퓨터에 unity 다운로드가 안돼”  
**English translation:** “I cannot download Unity on my computer.”

**Reconstructed recovery brief:**

```text
Continue without requiring a local Unity installation. Prepare the repository
so Unity Build Automation can detect the Editor version, restore packages,
import the project, compile C#, build WebGL from the main branch, and publish
a browser-testable artifact. Preserve a route for later testing on an ASU lab
computer if a full manual playthrough is still required.
```

**Resolution:** Development moved to a public GitHub repository plus Unity
Build Automation. The project records Unity `6000.0.24f1` in
`ProjectSettings/ProjectVersion.txt`.

## 4. Tripo generation authorization and functional plan

**Date:** October 29, 2026  
**Tool and route:** Signed-in Tripo Studio, Text-to-3D  
**Prompt status:** Original authorization plus reconstructed brief

**Original authorization (Korean):** “로그인했고 있는 크레딧 다 써도 돼”  
**English translation:** “I am signed in, and you may use the available credits.”

**Reconstructed execution brief:**

```text
Use Tripo Studio to make three game-ready assets with distinct functional
roles: a cyan crystal for Reward, an iron spike trap for Damage, and a stone
portal for End. Prefer clear silhouettes, isolated single objects, and FBX
exports with textures. Retopologize high-density results toward a 10,000-quad
target. Record the exact prompts, model IDs, credit changes, export settings,
errors, and final Unity Resources paths.
```

**Observed account state:** The interface displayed a 3,200-credit starting
balance and share-only access. A later account check showed the Free plan.

## 5. Crystal generation: failed first result and recovery

**Date:** October 29, 2026  
**Tool/model:** Tripo Smart Mesh P2.0 trial, then Tripo H3.1 HD  
**Prompt status:** Exact Tripo prompts

**Initial exact prompt:**

```text
A stylized low-poly magical crystal collectible, glowing cyan core,
faceted gemstone shape, game-ready 3D asset, clean quad topology,
centered single object, symmetrical, no pedestal, no background, no text
```

**Failure:** The 5,490-face Smart Mesh result looked like a rounded rock with
a base, not a readable collectible.

**Recovery decision:** Reject the result, change to H3.1 HD, and constrain the
silhouette with explicit geometric and negative terms.

**Recovery exact prompt:**

```text
Single isolated upright magical cyan gemstone collectible for a video
game, one elongated six-sided hexagonal crystal prism with a sharp
pointed tip, clean faceted surfaces, stylized low-poly, symmetrical,
bright cyan and blue material, no rock, no cluster, no pedestal,
no base, no text
```

**Result and validation:** H3.1 produced a clear hexagonal crystal. Generation
cost 55 credits (3,200 → 3,145). The 1,958,242-triangle HD mesh was sent to
Retopology P1.0 for a 10,000-polygon quad target at 40 credits. The job paused
at 99% during the first check, then completed with 11,383 faces and 10,898
vertices.

**Model ID:** `bd96e0c7-c049-4af8-affd-2e1b2322ffcc`  
**Export:** FBX, Blender orientation, 2K textures  
**Unity path:** `Assets/CrystalEscape/Resources/Tripo/Crystal/Crystal.fbx`  
**Functional placement:** Loaded into all five Reward sockets; the parent
trigger increments the crystal counter.

## 6. Spike-trap generation

**Date:** October 29, 2026  
**Tool/model:** Tripo H3.1 HD + Retopology P1.0  
**Prompt status:** Exact Tripo prompt

```text
Single compact square floor spike trap for a video game, dark iron base
plate with nine sharp triangular metal spikes pointing straight upward,
red glowing cracks, stylized low-poly fantasy prop, clean silhouette,
one isolated object, no character, no environment, no pedestal, no text
```

**Result:** Generation cost 55 credits (3,105 → 3,050). Retopology cost 40
credits and completed with 10,771 quad faces and 10,596 vertices.

**Model ID:** `76c0140d-dadb-44fc-9e2a-7a99907fd0af`  
**Export:** FBX, Blender orientation, 2K textures  
**Unity path:** `Assets/CrystalEscape/Resources/Tripo/SpikeTrap/SpikeTrap.fbx`  
**Functional placement:** Loaded into three Damage sockets; the parent trigger
removes one health point and applies a one-second cooldown.

## 7. Exit-portal generation

**Date:** October 29, 2026  
**Tool/model:** Tripo H3.1 HD + Retopology P1.0  
**Prompt status:** Exact Tripo prompt

```text
Single freestanding circular ancient stone escape portal frame for a
video game, complete thick ring with an open empty center, stable stone
base feet, carved cyan glowing runes, stylized low-poly fantasy prop,
clean silhouette, front view, isolated one object, no wall, no landscape,
no character, no text
```

**Result:** Generation cost 55 credits (3,050 → 2,995). Retopology cost 40
credits and completed with 11,242 quad faces and 10,159 vertices. The final
displayed balance after generation and retopology was 2,915 credits.

**Model ID:** `a50a108c-2bf0-4aa1-81c1-c7f481c2f3c6`  
**Export:** FBX, Blender orientation, 2K textures  
**Unity path:** `Assets/CrystalEscape/Resources/Tripo/ExitPortal/ExitPortal.fbx`  
**Functional placement:** Loaded into the End socket; its parent trigger wins
only after all five crystals have been collected.

## 8. Functional Tripo integration

**Date:** October 29, 2026  
**Tool:** OpenAI Codex  
**Prompt status:** Reconstructed execution brief

```text
Integrate the three optimized FBX assets through Resources.Load. Use the
crystal in all five Reward sockets, the spike trap in all three Damage
sockets, and the portal in the End socket. Keep triggers and gameplay logic
on parent objects so imported mesh scale or collider details cannot disable
the required mechanics. Fall back to primitive geometry only when an FBX
cannot be loaded.
```

**Validation evidence:** The three FBX files and textures exist at the exact
Resources paths, and source inspection confirms these load calls:

```csharp
Resources.Load<GameObject>("Tripo/Crystal/Crystal")
Resources.Load<GameObject>("Tripo/SpikeTrap/SpikeTrap")
Resources.Load<GameObject>("Tripo/ExitPortal/ExitPortal")
```

The files were included in the successful cloud-build workspace. In-game
visual placement after pressing Start still requires a manual visual check.

## 9. Key art generation

**Date:** October 29, 2026  
**Tool:** OpenAI built-in image generation  
**Prompt status:** Exact production prompt

```text
Use case: stylized-concept
Asset type: 16:9 game key art and trailer opening frame
Primary request: cinematic key art for the small 3D game Crystal Escape
Scene/backdrop: a compact dark midnight-blue and violet low-poly arena
at night, with simple walls and platforms
Subject: a lone small white capsule-like explorer running toward a large
freestanding circular ancient stone portal; five tall cyan hexagonal
crystal prisms float as collectibles; dangerous square iron spike traps
with upward spikes glow faintly red along the route
Style/medium: polished stylized low-poly 3D game render, readable
silhouettes, professional indie game key art
Composition/framing: landscape 16:9, low wide cinematic angle, explorer
in lower-left foreground, cyan portal as dominant focal point in
upper-right distance, crystals forming a path, clear depth and motion
Lighting/mood: cyan portal rim light, violet ambient shadows, restrained
magenta danger accents, adventurous and urgent
Color palette: midnight navy, deep violet, luminous cyan, small
red-magenta accents, white player
Materials/textures: faceted translucent cyan crystals, chunky gray portal
stones with cyan runes, dark iron spike plates
Constraints: exactly one explorer, exactly five cyan crystals, one circular
open-center portal, visible gameplay arena, no logos, no watermark, no UI
Avoid: photorealism, text, title lettering, extra characters, fantasy
forest, city skyline, guns, gore, solid portal center
```

**Result:** `Trailer/KeyArt_CrystalEscape.png` shows the player, five rewards,
red hazards, and an open portal. It is used as the README cover and trailer
planning frame; it is not represented as a completed trailer.

## 10. Public repository and Unity Cloud connection

**Date:** October 29, 2026  
**Tools:** GitHub and Unity Build Automation  
**Prompt status:** Reconstructed execution brief

```text
Create a public GitHub repository containing the Unity source, Packages,
ProjectSettings, documentation, Tripo assets, key art, and Unity .gitignore.
Connect it to Unity Build Automation with the minimum repository access
needed for manual builds. Do not enable automatic builds, and never store
credentials in the repository or prompt log.
```

**Result:** The source was committed to the public `sujinbae11/CRYSTAL-ESCAPE`
repository and connected to the Unity Cloud project.

## 11. Source-control authentication errors and recovery

**Date:** October 29, 2026  
**Prompt status:** Directly observed errors and reconstructed recovery action

**Error 1:** Unity reported that the repository was private or inaccessible
when the public HTTPS URL was used.  
**Error 2:** GitHub rejected Unity's generated deploy key as too short and
recommended an RSA key of at least 3,072 bits.

**Recovery brief:**

```text
Do not place the rejected short SSH key in GitHub. Use a repository-scoped,
fine-grained GitHub access token with read-only Contents permission so Unity
Build Automation can clone only this repository. Keep the token secret and
exclude its value from every committed file and screenshot.
```

**Resolution:** Repository checkout subsequently succeeded. No credential is
stored in this repository.

## 12. WebGL target and first cloud build

**Date:** October 29, 2026  
**Tool:** Unity Build Automation  
**Prompt status:** Reconstructed execution brief

```text
Create a manual WebGL build target for the main branch. Detect the Unity
version from ProjectSettings/ProjectVersion.txt, use a free-tier-eligible
Windows Micro machine, leave the project subfolder empty, and keep automatic
scheduling disabled. Run build #1 and monitor checkout, package restoration,
C# compilation, IL2CPP conversion, WebAssembly linking, and artifact
publication. Preserve warnings in the record.
```

**Observed configuration:** Target `Default WebGL`; branch `main`; Unity
`6000.0.24f1`; Windows Micro; no automatic schedule.

**Build evidence:**

- GitHub checkout succeeded at revision
  `361904ee1044428524827bbfe4ce5cdd49cdc30e`.
- C# compilation succeeded with three obsolete-API warnings for
  `FindObjectOfType<T>()`.
- WebGL/IL2CPP compilation and artifact publication succeeded.
- Artifact size was approximately 30.7 MiB.
- Final build status was `SUCCESS`.

The build log also warned that Unity `6000.0.24f1` is affected by a known
security issue; a final rebuild with `6000.0.58f2` or later is recommended.

## 13. Public WebGL delivery validation

**Date:** October 29, 2026  
**Tool:** Unity Build Automation public share page  
**Prompt status:** Reconstructed validation brief

```text
Create a public share page for the successful WebGL artifact, open the page,
press Play, and confirm that the Unity runtime downloads its data,
initializes, and reaches the Crystal Escape title/start screen. Do not report
a complete gameplay playthrough unless movement, pickups, damage, both ending
states, and restart are manually exercised.
```

**Result:** The public share page displayed a Play button and QR code. After
Play, the WebGL player loaded and rendered the `CRYSTAL ESCAPE` title and
`START RUN` screen. The share link expires November 12, 2026. A direct internal
player URL failed outside its parent share-page context, so the public share
page is the correct test route.

**Runtime warning:** A `CapsuleCollider` type-stripping warning appeared. The
start screen remained functional, but a full manual playthrough is still
needed to verify all interactions.

## 14. Procedural development audio

**Date:** October 29, 2026  
**Tool:** OpenAI Codex / Unity C#  
**Prompt status:** Reconstructed execution brief

```text
Implement temporary in-engine audio feedback so the prototype remains
audible without external files: looping synthesized ambience, a bright
pickup cue, a damage pulse, and separate win and loss cues. Keep Resources/
Audio override paths available so Week 6 generated clips can replace the
procedural sounds later without changing gameplay code.
```

**Result:** `CrystalEscapeGame.cs` uses `AudioClip.Create` to generate runtime
BGM, Pickup, Hurt, Win, and Lose fallback clips when matching external audio
files are absent. Source integration is verified; an actual listening test
has not been documented. No external audio-generation prompt is represented
as executed.

## 15. Trailer production status

**Date:** October 29, 2026  
**Tools:** OpenAI image generation and planning documentation  
**Prompt status:** Evidence-based status record

The key art and `Trailer/TRAILER_PLAN.md` were created. A final 30–60 second
trailer containing generated shots and real gameplay capture was not produced
during this documented session. No unexecuted video-generation prompt is
listed as though it had been run.

## 16. Key-art-directed V2 gameplay redesign

**Date:** October 29, 2026  
**Tool and route:** OpenAI Codex, local Unity source revision  
**Prompt status:** Original request plus reconstructed execution brief

**Original request (Korean):** “게임 실행해봤는데 너무 미완성된느낌이야. 다시 디벨롭가능할까? 너가 만들었던 사진 그대로의 느낌이었으면 좋겠어”  
**English translation:** “I tried the game and it feels too unfinished. Can
you develop it again? I want it to feel like the image you created.”

**Reconstructed execution brief:**

```text
Redesign the playable game so its composition and atmosphere match the key
art rather than a flat prototype arena. Build a forward-moving route through
floating midnight-blue ruins, guide the player with five cyan crystals, place
red spike hazards on readable alternate lanes, and make a large elevated
portal the destination. Use violet fog and void light, a moon, stars, distant
monoliths, cyan route accents, and a white bean-like explorer. Add movement
acceleration, running animation, camera bob and landing response, pickup
bursts, damage recoil and shake, a visibly locked/unlocked open portal, timer,
reticle, portal status, and clearer start/end screens. Keep the implementation
compatible with Unity WebGL and the Built-in Render Pipeline, with no new
packages.
```

**Diagnosis:** The original camera began behind the south wall, outside the
boxed arena. After Start, that wall could fill the entire camera view with a
flat violet surface. The level was also a single square floor, so its spatial
composition did not match the key art's floating path, height changes, gaps,
and distant portal.

**Source changes prepared:**

- replaced the boxed arena with connected plazas, bridges, stairs, side
  islands, and an elevated portal dais;
- added runtime fog, violet void, moon, stars, ruined skyline silhouettes,
  tiled stone, cyan edge lighting, and limited WebGL-safe accent lights;
- rebuilt the player presentation with a stylized body, visor, limbs, running
  motion, smoother acceleration, camera feedback, knockback, and fall reset;
- added crystal halos, orbiting motes, pickup shards, red hazard underglow,
  and an open portal ring that changes from violet/locked to cyan/ready;
- upgraded the HUD with timer, portal state, reticle, feedback flashes, and
  revised title/end panels;
- replaced obsolete Unity object lookup calls and added explicit concrete
  collider references to address the earlier WebGL stripping warning.

**Validation status:** Static source review and delimiter checks passed. A new
Unity Build Automation run and full browser playthrough are still required
before the V2 changes can be described as build-verified.

## Confirmed evidence and current limitations

**Confirmed:**

- Three optimized Tripo FBX assets exist at the documented Resources paths.
- GitHub checkout, Unity C# compilation, and WebGL build #1 succeeded.
- The public share page loaded the WebGL runtime and rendered the start screen.
- Reward, Damage, End, restart, Tripo loading, and procedural audio paths are
  present in source code.

**Still requires student completion or direct observation:**

- A manual start-to-win and start-to-loss playthrough, including restart.
- A first-time-player/stranger usability test.
- Replacement or acceptance of the procedural audio under the course's Week 6
  audio requirement.
- A final 30–60 second trailer containing actual gameplay capture.
- A personal 200–300 word analytical reflection based on the student's own
  testing and decisions.
