# Asset Attribution Sheet

This sheet lists only assets and tools that are evidenced in the repository or
in the dated production record. “Not created” means that no such deliverable is
being claimed.

| Asset | Source/tool | Prompt or origin | Terms basis | Functional use | Verified status |
|---|---|---|---|---|---|
| Unity gameplay code | OpenAI Codex-assisted | Detailed reconstructed briefs in the prompt log | Use subject to applicable OpenAI terms; student remains responsible for review and submission | Entire game scaffold | Source included; cloud compilation passed |
| Crystal model | Tripo Studio H3.1 + Retopology P1.0 | Exact prompt and model ID in prompt log | Free plan; public/non-commercial use shown by Tripo pricing; Tripo rights described in Terms §5.2.1 | Reward collectible | Optimized FBX and textures included; functional load path verified |
| Spike trap model | Tripo Studio H3.1 + Retopology P1.0 | Exact prompt and model ID in prompt log | Free plan; public/non-commercial use shown by Tripo pricing; Tripo rights described in Terms §5.2.1 | Damage hazard | Optimized FBX and textures included; functional load path verified |
| Exit portal model | Tripo Studio H3.1 + Retopology P1.0 | Exact prompt and model ID in prompt log | Free plan; public/non-commercial use shown by Tripo pricing; Tripo rights described in Terms §5.2.1 | End/win condition | Optimized FBX and textures included; functional load path verified |
| Background music | OpenAI Codex-assisted Unity C# synthesis | Mathematically generated at runtime by `AudioClip.Create` | Original project code; subject to applicable OpenAI and Unity terms | Looping development ambience | Included as runtime fallback; listening test required |
| Pickup sound | OpenAI Codex-assisted Unity C# synthesis | Mathematically generated at runtime by `AudioClip.Create` | Original project code; subject to applicable OpenAI and Unity terms | Reward feedback | Included as runtime fallback; listening test required |
| Hurt sound | OpenAI Codex-assisted Unity C# synthesis | Mathematically generated at runtime by `AudioClip.Create` | Original project code; subject to applicable OpenAI and Unity terms | Damage feedback | Included as runtime fallback; listening test required |
| Win sound | OpenAI Codex-assisted Unity C# synthesis | Mathematically generated at runtime by `AudioClip.Create` | Original project code; subject to applicable OpenAI and Unity terms | Win feedback | Included as runtime fallback; listening test required |
| Loss sound | OpenAI Codex-assisted Unity C# synthesis | Mathematically generated at runtime by `AudioClip.Create` | Original project code; subject to applicable OpenAI and Unity terms | Loss feedback | Included as runtime fallback; listening test required |
| UI font | Unity built-in runtime font | Unity installation | Unity engine distribution terms | Runtime UI | Included |
| Trailer key art | OpenAI built-in image generation | Exact production prompt in prompt log | Use subject to applicable OpenAI terms; student remains responsible for compliant use | README cover and planned opening frame | Included as `Trailer/KeyArt_CrystalEscape.png` |
| Trailer generated shots | No video model used | No executed video-generation prompt | Not applicable | Required final trailer component | Not created |
| Trailer gameplay footage | Original game capture | Must be recorded from Crystal Escape | Student-created when recorded | Required gameplay evidence | Not created |

## Tripo plan and disclosure

The signed-in Tripo pricing interface was checked on October 29, 2026 and
showed the **Free** plan. The generation workspace showed `공유 전용`
(share-only) access. These labels describe different things: the plan governs
account terms, while share-only describes who can open a model link.

The official sources checked on October 29, 2026 were:

- [Tripo Terms of Service](https://www.tripo3d.ai/terms)
- [Tripo Pricing and plan comparison](https://www.tripo3d.ai/pricing)
- [Tripo privacy/access explanation](https://www.tripo3d.ai/help/privacy-policy/is-my-asset-safe)

Tripo's pricing page describes Free-plan models as public and for
non-commercial use. Section 5.2.1 of the Terms describes the rights Tripo
retains in Free-user inputs and outputs. The public repository therefore
discloses the Free plan and does not claim exclusive or commercial rights to
the three models.

## Submission note

The procedural sounds make the prototype audible without external files, but
they should not be described as a separate Week 6 audio-generation pipeline
unless the instructor explicitly accepts this implementation. Likewise, the
key art and shot plan are not a finished 30–60 second trailer.
