# Implementation Plan

## Scope and basis

This plan is based only on the task requirements and the complete root AGENTS.md. The supplied HTML prototype was not opened, inspected, analyzed, or used. Project implementation files were not inspected; paths below are proposed locations, not claims about existing files.

This task updates only this document. All implementation, Unity validation, and development documentation updates described below are future work.

Use Unity 6 LTS / 6000.x, standard uGUI / Canvas unless an established project approach requires otherwise, and the feature-oriented structure from AGENTS.md. Preserve supplied assets and avoid dependency upgrades, third-party frameworks, DOTS/ECS, and Naninovel installation. Prefer one runtime assembly at `Assets/_Resonance/Features/Resonance.Runtime.asmdef` and explicit Inspector wiring; add a small installer only if needed to connect the listed components.

## Responsibility boundaries

| Component | Responsibility |
| --- | --- |
| MusicTrackDefinition | Authorable stable ID, title, artist, and AudioClip. |
| MusicPlaylistDefinition | Authorable ordered track references. |
| MusicPlayerConfig | Authorable visualizer, transition, and relevant layout settings. |
| MusicPlaybackService | Own AudioSource interaction, selected track, playback position, and playback state. |
| AudioSpectrumAnalyzer | Read the playing source, map frequency bands, normalize, and smooth values. |
| MusicPlayerController | Connect user commands and playback state to presentation; delegate visibility to the layout controller. |
| MusicPlayerView | Present metadata, playback status, and controls; expose user interactions. |
| AudioVisualizerView | Render analyzer values inside the player; own no playback logic. |
| DialogueMusicLayoutController | Own explicit visibility state, shared responsive geometry, and all panel transitions. |

Runtime selection, playback state, spectrum buffers, and visibility state remain in runtime C# objects/components, never ScriptableObjects. Expose dialogue visibility through the layout controller's explicit API so a future narrative adapter can call it without a music-system dependency on Naninovel.

## Ordered milestones

### 1. Construct the static visual scene and MusicPlayer prefab

**Purpose:** Establish visual fidelity and a responsive composition before writing playback code. No gameplay behavior is required in this milestone.

**Likely files/components:**

- `Assets/_Resonance/Scenes/`: assignment scene with Environment/Test_BG and UI/Canvas.
- Canvas/CanvasScaler and DialogueMusicLayout with a Dialogue UI stub and MusicPlayer.
- `Assets/_Resonance/Features/MusicPlayer/Prefabs/`: reusable MusicPlayer prefab with Background, Header, TrackInfo, Visualizer, Progress, and Controls children.
- Supplied fonts and sprites, reused from their existing locations without unnecessary changes.

**Acceptance criteria:**

- Test_BG, the Dialogue UI stub, and the MusicPlayer visual hierarchy form an inspectable static scene with meaningful object names.
- Supplied fonts and sprites establish consistent typography, imagery, spacing, alignment, and visual fidelity without using the HTML prototype.
- Dialogue is on the left and MusicPlayer on the right; their combined bounds, including the gap, are centered.
- CanvasScaler, anchors, RectTransform relationships, and available-width constraints keep the static composition responsive without hard-coded screen coordinates.
- Inspect the static composition at 1280x720, 1920x1080, 1920x1200, 2560x1080, and 3840x2160. Visible elements do not overlap, clip, drift off-screen, or become incorrectly aligned.
- Placeholder metadata and resting visualizer bars are sufficient. Controls need no behavior, and no playback or data-definition code is required yet.

### 2. Add designer data and working play, pause, and resume

**Purpose:** Connect the established visual player to designer-authored content and correct music playback.

**Likely files/components:**

- `Assets/_Resonance/Features/MusicPlayer/Runtime/`: MusicTrackDefinition, MusicPlaylistDefinition, MusicPlayerConfig, MusicPlaybackService, MusicPlayerController; a small MusicPlayerInstaller only if needed.
- `Assets/_Resonance/Features/MusicPlayer/UI/`: MusicPlayerView.
- `Assets/_Resonance/Data/Music/Tracks/`, `Playlists/`, and `Config/`: example authoring assets.
- Systems/MusicSystem/AudioSource and wiring to the existing scene and MusicPlayer prefab.
- `Assets/_Resonance/Features/Resonance.Runtime.asmdef`.

**Acceptance criteria:**

- An Inspector-configured selection from the playlist plays the assigned AudioClip, and the player displays its title and artist. Runtime track navigation is not required.
- Track IDs remain stable when display metadata changes; IDs and required references have clear authoring validation.
- Designers can create tracks, assign clips, edit metadata, and reorder the playlist without C# changes.
- MusicPlayerConfig groups meaningful settings such as Sensitivity, Attack, Release, Bar Count, Maximum Height, Transition Duration, easing, panel sizes, gap, and margins; implement settings as their milestones need them.
- Missing clips or an empty playlist leave controls in a safe, understandable state without exceptions.
- Playback logic remains in the service; the view contains presentation and input bindings only.
- Pause retains the selected clip and playback position; position stops advancing while paused.
- Resume continues from that position instead of starting the track again. Initial play and resume are distinct service operations.
- Repeated pause/resume cycles remain correct and the control presentation matches the actual playback state.
- Natural track completion returns to a consistent non-playing presentation; no automatic next-track behavior is required.
- Compile, check the Console, and verify audible playback and playback position before pause, during pause, and after resume in Play Mode.

### 3. Add the real-time audio visualizer

**Purpose:** Produce stable, visibly audio-reactive bars with smooth decay on pause.

**Likely files/components:** AudioSpectrumAnalyzer in MusicPlayer/Runtime, AudioVisualizerView in MusicPlayer/UI, MusicPlayerConfig, and the player's Visualizer child.

**Acceptance criteria:**

- The analyzer samples the same AudioSource owned by MusicPlaybackService using GetSpectrumData or equivalent actual audio analysis.
- The pipeline maps frequency bands, applies normalization/sensitivity, then separate attack and release smoothing before rendering.
- Audible changes produce corresponding bar changes; no prerecorded, random, looping, or synthetic idle animation drives the bars.
- Pause drives target values toward rest while smoothing continues; bars are not cleared instantly and do not remain frozen. Resume responds to the resumed audio.
- Designers can tune responsiveness, bar count, and maximum height through MusicPlayerConfig with useful Inspector labels, tooltips, and bounds.
- Reuse buffers and cached bar references to avoid obvious per-frame allocations in analysis and rendering.
- Compile, inspect the Console, and validate playback, silence, pause decay, and resume in Play Mode.

### 4. Connect the required responsive layout states

**Purpose:** Connect the static layout to centralized state handling and verify destinations before layering on transitions. Prioritize BothVisible, MusicOnly, and DialogueOnly; retain BothHidden as a useful internal state.

**Likely files/components:** DialogueMusicLayoutController, MusicPlayerConfig, Canvas/CanvasScaler, DialogueMusicLayout with DialoguePanel and MusicPlayer children, and minimal state debug controls.

**State targets:**

| State | Dialogue target | Music Player target |
| --- | --- | --- |
| BothVisible | Left side of centered combined composition | Right side of the same composition |
| MusicOnly | Hidden | Exactly the BothVisible player position for the current canvas size |
| DialogueOnly | Centered horizontally | Hidden/out of view |
| BothHidden | Hidden | Hidden/out of view |

**Acceptance criteria:**

- One controller computes all targets from the available Canvas/RectTransform area, anchors, panel dimensions, gap, and margins; no absolute screen-coordinate positioning.
- BothVisible includes both panel widths and the gap when centering the composition.
- MusicOnly reuses the BothVisible music target without recalculating a single-panel centered target.
- DialogueOnly centers the dialogue independently; BothHidden leaves no invisible panels intercepting input.
- Layout fits 1280x720, 1920x1080, 1920x1200, 2560x1080, and 3840x2160, with deliberate scaling or width constraints when available space is limited.
- Validate the three required visible layouts in Play Mode at all five sizes, with Console inspection after compilation. Smoke-test BothHidden internally without prioritizing it over visible layouts. Animated state changes are completed next.

### 5. Animate hide/show transitions

**Purpose:** Animate the required visible layout changes and connect hide plus a reachable temporary/test show affordance. The assignment leaves the final show affordance unspecified.

**Likely files/components:** DialogueMusicLayoutController, MusicPlayerConfig, MusicPlayerController/View, panel RectTransforms/CanvasGroups, and a reachable temporary/test show affordance outside the hidden player hierarchy.

**Acceptance criteria:**

- Hiding the player while dialogue is visible simultaneously moves the player out of view and moves dialogue to center. Showing reverses both motions together.
- Hiding dialogue while the player remains visible leaves the player's position unchanged.
- With dialogue hidden, hiding/showing the player transitions between MusicOnly and BothHidden without revealing dialogue.
- Scripted RectTransform/CanvasGroup transitions use configured duration and easing, without a tweening dependency.
- Hidden endpoints derive from current canvas and panel bounds so the player fully leaves view at every required resolution. Visible states remain inside the viewport.
- Hidden controls do not receive input. The temporary/test show affordance remains reachable when the player is hidden, including for internal BothHidden testing; its final design is not prescribed.
- UI visibility changes preserve playback state and position; hiding the player does not implicitly pause or restart audio.
- Compile, check the Console, and verify smooth, simultaneous hide/show motion and dialogue visibility changes among the required visible layouts in Play Mode. No snapping during these transitions. Smoke-test entry to and recovery from BothHidden internally.

### 6. Polish, validate, and prepare the handoff

**Purpose:** Finish a small, inspectable feature and prove the required behavior across the target display sizes.

**Likely files/components:** MusicPlayer prefab in `Assets/_Resonance/Features/MusicPlayer/Prefabs/`, assignment scene, configuration assets, the listed runtime/UI components, targeted tests under `Assets/_Resonance/Tests/` if useful, and AI_NOTES.md during implementation.

**Acceptance criteria:**

- Review and polish the MusicPlayer prefab constructed in milestone 1. Runtime systems supply state; prefab assets do not persist gameplay state.
- Hierarchy and dependencies are easy to inspect; remove redundant objects and keep designer configuration focused on the three specified ScriptableObject types.
- At each of 1280x720, 1920x1080, 1920x1200, 2560x1080, and 3840x2160, verify the three required visible layouts, hide/show motion, dialogue toggling, play/pause/resume, and visualizer decay. Keep BothHidden validation to an internal smoke test.
- Visible UI has no overlap, clipping, unintended off-screen placement, or alignment drift. Intentional hide motion exits the viewport completely.
- Compare the settled player's position in BothVisible and MusicOnly at each size: they must match exactly. Confirm centered dialogue in DialogueOnly and centered combined bounds in BothVisible.
- Verify transitions during playback and pause at each fixed target resolution; hidden panels cannot intercept clicks.
- Perform a designer walkthrough: add a track, assign its clip, edit title/artist, reorder the playlist, and tune visualizer and transition settings without changing C#.
- Complete a final Unity compile, Console review, and Play Mode pass. Resolve relevant errors and warnings; record any unrelated pre-existing issues accurately.
- Maintain AI_NOTES.md during future implementation with actual AI work, manual work, human decisions, and real mistakes and how they were discovered. Do not invent entries. This planning task does not modify that file.
- Record validation evidence and remaining issues accurately; compilation alone is not completion.

## Polish if time permits

After the required visible layouts and behavior are complete, improve robust interrupted-transition retargeting and resizing during an active transition, then validate those edge cases. Explicit Profiler allocation validation is also an if-time-permits check. These are not required acceptance criteria; the implementation should still reuse buffers and references and avoid obvious per-frame allocations.

## Deferred optional work

Scrubbing, next track, and previous track are excluded from the required milestone sequence. Consider them only after all required behavior, visual polish, and resolution validation are complete. Do not add speculative abstractions or controls for them now.
