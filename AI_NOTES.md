# AI Workflow Notes

## 2026-09-23

### Tools
- IDE: Codex.
- Model: GPT-5.6
- Unity Official Skills. Codex plugin. Package com.unity.pipeline

### Architecture
Human:
- Defined feature-oriented architecture.
- Chose ScriptableObjects for designer-authored data.
- Chose lightweight service boundaries.
- Decided core systems must not depend on Naninovel.

AI:
- Helped review architecture and project structure.

### Project Setup
AI:
- Created initial folders and asmdef based on AGENTS.md.

Human:
- Reviewed structure before proceeding.

### Issues / Corrections
- TBD

### Third-Party Packages
- Initial structure setup added none. Human installed `com.unity.ugui` 2.0.0 for the static scene; AI did not install packages.

### Initial Structure Setup
- AI was used to create the initial project structure from the human-defined architecture in AGENTS.md.

### Static Scene Foundation (2026-09-23)
- AI created and saved `Assets/_Resonance/Scenes/DeveloperTest.unity` in the live Unity Editor. It contains Environment/Test_BG, a Main Camera, and UI/Canvas/DialogueMusicLayout with DialoguePanel and MusicPlayer.
- After the human installed uGUI, AI configured screen-space canvases and CanvasScalers at 1920x1080 with Match Width Or Height 0.5. Test_BG uses the supplied texture with an Envelope Parent aspect fitter at 16:9; its source image was not changed.
- The centered bottom layout uses a HorizontalLayoutGroup with a 36-unit gap, an 820x360 dialogue card, and a 468x336 MusicPlayer image placeholder. The dialogue stub uses the supplied Anton and Noto Sans fonts. No supplied dialogue UI stub asset was present in Assets, so AI used simple placeholder copy and styling.
- AI first tried adding RawImage alongside Image, used enum spellings rejected by Unity, and passed a screenshot path outside the project root. Unity Console responses exposed these mistakes; AI removed the conflicting Image first, used the valid enum display names, and captured under a temporary Assets folder. The task-generated Console entries and captures were cleared after verification.
- Unity 6000.0.84f1 reported no scripts needed recompilation. Play Mode captures at 1280x720, 1920x1080, 1920x1200, 2560x1080, and 3840x2160 showed the background filling with aspect-preserving crop and the panels inside the screen without overlap.
- No playback, ScriptableObjects, services, controllers, or detailed MusicPlayer hierarchy were added.

### Supplied Dialogue UI Stub (2026-09-24)
- The supplied `DialogueUI_StubIn.png` was added to the project after the initial scene setup. It now replaces the temporary dialogue card via a RawImage in DeveloperTest; the two placeholder text overlays were removed, and the source image was left unchanged. The dialogue panel keeps its 820-unit width and uses the source aspect ratio for height.

### Background and UI Scaling Correction (2026-09-24)
- Replaced `Environment/Test_BG/Test_BG_Image`'s `RawImage` with a uGUI `Image` using the existing `Test_BG` sprite. Enabled Image aspect preservation and kept the full-stretch RectTransform with `AspectRatioFitter` set to `Envelope Parent` at 16:9; the source image was not modified.
- Set the gameplay UI CanvasScaler to Scale With Screen Size, 1920x1080 reference resolution, and Match Height (1.0). Kept the separate full-screen background canvas scaler at 0.5 so background coverage remains independent of dialogue/player sizing.
- Expanded the centered layout rect to the sum of its existing panel widths and 36-unit gap, preserving panel dimensions and gap while centering the group at all tested sizes.
- Validated Play Mode Game view at 1280x720, 1920x1080, 1920x1200, 2560x1080, and 3840x2160. The background filled each view with aspect-preserving crop where needed; the dialogue/player composition stayed centered without overlap or clipping.

### Dialogue / Music Layout States (2026-09-26, in progress)
- Human constrained this step to responsive layout and animated visibility, with actual RectTransform widths as the geometry source. Playback, visualizer, data assets, HTML inspection, and package changes are excluded.
- AI read AGENTS.md and IMPLEMENTATION_PLAN.md completely and inspected the saved DeveloperTest scene. AI authored DialogueMusicLayoutState and DialogueMusicLayoutController under Presentation/Runtime. The controller uses width-derived shared targets, Canvas-bound off-screen targets, unscaled-time AnimationCurve transitions, and CanvasGroup input gating.
- After the Pipeline server became available, AI removed the HorizontalLayoutGroup, wired the controller to the existing panels and Canvas, and added four clearly named temporary state controls plus an EventSystem. The controls remain outside MusicPlayer so they are reachable while it is hidden.
- The first setup pass failed to persist the two CanvasGroup references. The failure was found from the saved scene and a controller validation error; the controller now deterministically gets or adds a CanvasGroup on each assigned panel before applying visibility or input state.
- Unity compiled the initial implementation with no compiler errors or warnings. A 1920x1080 Play Mode capture verified the initial BothVisible composition visually. Full automated state/resolution validation could not be completed after the local Pipeline connection became hidden from the sandbox again; no unsupported validation results were claimed.
- The first temporary controls appeared in Play Mode but did not respond to clicks. Static inspection confirmed the EventSystem, GraphicRaycaster, input backend, raycast targets, and button interactability were configured. The buttons' serialized Boolean callbacks were replaced with explicit parameterless Show/Hide methods to make their UnityEvent wiring straightforward and inspectable.
- Human review identified a small startup snap after manually positioning the panels. AI traced it to the controller immediately replacing scene-authored positions with width-derived targets in `OnEnable`. The layout now records each panel's authored offset from the responsive two-panel calculation and reapplies that offset to visible targets, preserving visual placement while retaining responsive centering and off-screen transitions.
- Human requested simpler state testing controls. AI replaced the separate show/hide actions with one dialogue toggle and one music-player toggle, added state-aware visible/hidden button colors, and disabled raycast interception on the full-screen reference overlay so the controls receive pointer input.

### Music Data and Core Playback (2026-09-26)
- Human limited this task to track/playlist data and core playback while manually working on the MusicPlayer UI prefab. AI read AGENTS.md and IMPLEMENTATION_PLAN.md completely, then added MusicTrackDefinition, MusicPlaylistDefinition, and MusicPlaybackService to the existing runtime assembly. No HTML reference, UI integration, scene wiring, source audio changes, packages, or persistent data assets were used or added.
- Tracks generate a serialized GUID when the ID is blank during editor validation; title/artist edits preserve it. Duplicated assets retain the ID, with an Inspector tooltip explaining how to generate a distinct one. Playlists expose a read-only collection; neither asset contains playback state.
- The service uses an explicitly Inspector-assigned, dedicated AudioSource. PlayTrack starts over; Pause/Resume use Pause/UnPause; Stop rewinds, and disabling the service stops playback. Read-only properties expose selection, playing/paused state, time, and duration. No events or per-frame Update were needed for this scope.
- Unity 6000.0.84f1 imported and compiled all three scripts; explicit recompile reported up to date and Console ground truth reported compilationFailed=false and zero errors. Reflection verified both Resonance/Music CreateAssetMenu entries.
- Temporary tooling in /tmp used an isolated Unity preview scene, in-memory data assets, and a muted generated test clip. All 15 checks passed: ID creation/preservation, empty/read-only playlist, unloaded and invalid-track operations, playback progression, repeated pause/resume, exact paused sample retention across editor ticks, resumed progression, restart, stop, and natural completion. All temporary Unity objects and the preview scene were destroyed. This was Editor preview validation, not a Play Mode or audible listening pass.
- The first validation script reused a stale SerializedObject snapshot after OnValidate generated the ID, falsely reporting ID instability. Refreshing the test snapshot with Update before editing metadata corrected the test; production code did not need a change. The installed CLI also rejected the skill's caller/skill flags; validation used the supported command arguments.
- Two expected invalid-content warnings came from null-track/null-clip tests. Other captured diagnostics concerned existing TMP/font tooling and screen-capture permissions; the final Console had zero errors and five warnings. The Console was not cleared. Concurrent prefab edits and glyph-report deletions were left untouched.

### Track Asset GUID Correction (2026-09-26)
- Human requested automatic distinct identities for duplicated tracks. AI changed only MusicTrackDefinition.cs plus this note: stableId is hidden in the Inspector and synchronized to the Unity asset GUID by Editor-only code. OnEnable/OnValidate queue a deferred synchronization so new assets have a path; SetDirty runs only when the GUID differs. Runtime reads the serialized value without UnityEditor dependencies or GUID generation.
- Unity validation passed for creation, title/artist/AudioClip edits, rename, move and move back, duplication with a different ID, preservation of the original ID, serialized persistence, and no repeated dirtying. All temporary track/clip assets and their folder were deleted. The first test used an in-memory clip whose serialized reference was not retained; using a temporary persisted clip corrected the test fixture.
- Unity compilation completed successfully. No new captured warnings/errors; final Console ground truth reported zero errors and zero warnings. Playback, playlist, scenes, prefabs, UI, audio source assets, and packages were not changed by this correction.
