# Core Systems

A reusable Unity foundation layer — the systems I don't want to rewrite on every project.

This is the template I start new Unity projects from. Instead of rebuilding audio management, scene loading, save/load, UI screen flow, and state machines each time, they live here as decoupled, assembly-separated modules that drop into a new project and work.

> **Unity version:** <!-- TODO: e.g. 2022.3 LTS --> · **Render pipeline:** <!-- TODO: URP / Built-in -->

---

## Why this exists

Most of my project time used to go into re-solving the same problems: a singleton audio manager here, a hand-rolled scene transition there, save code welded to gameplay code. Every project ended up with the same systems built slightly differently and none of them portable.

Core Systems is the fix. Three rules govern everything in here:

- **Nothing talks directly to anything else.** Cross-system communication goes through an event bus or ScriptableObject-based data assets, so modules can be deleted without cascading compile errors.
- **Each module is its own assembly.** Assembly definitions enforce the dependency direction and keep iteration compile times low.
- **Systems are configured in the inspector, not in code.** Designers and artists can retune behaviour without a programmer.

---

## What's in here

### Core modules

| Module | What it does |
|---|---|
| **Event Bus** | Decoupled publish/subscribe messaging. The backbone that lets every other module stay independent. |
| **SOAP** | ScriptableObject Architecture Pattern — shared runtime data and events as assets rather than static singletons. |
| **Audio** | Centralised playback, mixing, and a UI sound component for one-line hookup on buttons and screens. |
| **Screens System** | Stack-based UI screen flow — push, pop, and transition between screens without per-project glue code. |
| **Scene Management** | Async scene loading with transitions, plus a custom editor tool for wiring scene flow. |
| **Saving** | Serialisation and persistence layer, kept independent of gameplay types. |
| **State Machine** | Generic, reusable state machine used for gameplay, UI, and sequencing. |
| **Tweening System** | Lightweight in-house tweening, including `IsFrom` support for reverse-origin animations. |
| **Timers** | Managed countdown/stopwatch timers without scattered coroutines. |
| **Fading** | Screen and element fade utilities shared by scene loads and screen transitions. |
| **Selection System** | Runtime object selection and highlighting. |

### Supporting modules

| Module | What it does |
|---|---|
| **Question System** | Data-driven question/answer flow with a presenter layer — built for quiz and training-style content. |
| **Networking** | Multiplayer scaffolding on Netcode for GameObjects, including connection UI. |
| **UI** | Shared UI components and behaviours. |
| **Utilities** | `Singleton`, `Logger`, `ClientPrefs`, `ProfileManager`, `Helper`, `TextValidator`, `LayoutGroupFreezer`, `ScrollRectEvents`, `ParticleAutoDestroy`, `UILine`, `DontDestroyOnLoad`. |
| **Editor** | Custom editor tooling — a project toolbar with time-scale control and scene-loader shortcuts. |

---


## Third-party dependencies

This project builds on several Unity Asset Store packages. **These are not mine and are not covered by this repository's license** — you'll need your own licenses to use them:

<!-- TODO: confirm and trim this list, and see the licensing note below -->
Odin Inspector · Transitions Plus · Editor Console Pro · vFolders · vHierarchy · Clipboard Plus Ultimate · Selection History · Live Script Reload ·
---

