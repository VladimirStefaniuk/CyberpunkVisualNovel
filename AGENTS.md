# AGENTS.md

# Resonance Developer Test

## Project Goal

This is a Unity take-home test for Kangyroo Creative / Resonance.

The assignment is to implement a music player and real-time audio visualizer
that integrates with the supplied visual-novel dialogue UI.

The implementation should demonstrate:

- strong visual fidelity
- responsive Unity UI
- clean scene hierarchy
- clear code responsibilities
- designer-friendly data configuration
- disciplined use of AI-assisted development

This is a small scoped assignment.
Prefer a few polished systems over unnecessary complexity.

---

# Source of Truth

Use the following priority:

1. Developer Test.pdf
2. Supplied visual / HTML prototype
3. Supplied Unity assets
4. Existing project conventions
5. This AGENTS.md

The HTML prototype is a visual and behavioral reference.

DO NOT mechanically translate or port HTML/JavaScript implementation code
into C#.

Understand the intended behavior and implement it appropriately in Unity.

---

# Unity

Use Unity 6 LTS / 6000.x LTS as required by the assignment.

Do not upgrade Unity, packages, or project dependencies unless required.

Use standard Unity uGUI / Canvas for the test UI unless the supplied project
already establishes another approach.

Avoid unnecessary third-party dependencies.

---

# Scope Priority

Required functionality must be completed and polished before optional features.

## Required

- Song playback
- Song pause / resume
- Real-time audio-reactive visualizer
- Music player hide
- Music player show
- Animated UI transitions
- Dialogue visible + Music Player visible
- Dialogue hidden + Music Player visible
- Dialogue visible + Music Player hidden
- Correct transitions between UI states

## Optional

Only implement these after all required functionality is polished:

- Song scrubbing
- Next track
- Previous track

Never sacrifice visual polish or required behavior for optional features.

---

# Design Philosophy

Use a simple, feature-oriented, data-driven architecture.

"Data-driven" in this project means:

- designer-editable configuration is stored in ScriptableObjects
- behavior operates on that configuration
- designers should be able to add and tune content without editing C#

It does NOT mean DOTS or ECS.

Do not introduce DOTS/ECS for this assignment.

Prefer the simplest architecture that:

- has clear responsibilities
- supports designer iteration
- can grow naturally if the production project adopts Naninovel
- is easy for another Unity developer to inspect

Avoid architecture for architecture's sake.

---

# Designer Experience

A designer should be able to:

- create a new music track
- assign an AudioClip
- change track title
- change artist name
- add tracks to a playlist
- reorder tracks
- tune visualizer responsiveness
- tune visualizer appearance
- tune UI animation speed

without modifying C#.

Designer-facing assets should be clear and minimal.

Preferred designer-facing asset types:

- MusicTrackDefinition
- MusicPlaylistDefinition
- MusicPlayerConfig

Do not create many small ScriptableObject types unless there is a clear
designer-facing reason.

Use Inspector attributes where useful:

- Header
- Tooltip
- Range
- Min

Expose meaningful design terminology.

Prefer:

- Sensitivity
- Attack
- Release
- Bar Count
- Maximum Height
- Transition Duration

over low-level implementation terminology unless necessary.

---

# ScriptableObject Rules

Use ScriptableObjects for static authorable configuration.

Examples:

## MusicTrackDefinition

Contains:

- stable track ID
- display title
- artist
- AudioClip

## MusicPlaylistDefinition

Contains:

- ordered list of MusicTrackDefinition assets

## MusicPlayerConfig

Contains grouped configuration for:

- visualizer
- transitions
- layout tuning where appropriate

ScriptableObjects MUST NOT contain mutable playback state.

Do not store runtime values such as:

- current playback time
- IsPlaying
- selected track index
- current FFT spectrum
- current UI visibility

inside ScriptableObjects.

Do not use ScriptableObjects as global mutable state.

---

# Runtime State

Runtime state should remain normal runtime C# data.

Examples:

- current track
- current track index
- playback position
- playback duration
- playing / paused state
- UI visibility
- visualizer spectrum values

Keep runtime state straightforward.

Do not build an unnecessary generic state framework.

---

# Architecture

Keep responsibilities focused.

Target architecture:

MusicTrackDefinition
MusicPlaylistDefinition
MusicPlayerConfig

MusicPlaybackService
AudioSpectrumAnalyzer

MusicPlayerController
MusicPlayerView
AudioVisualizerView
DialogueMusicLayoutController

A small composition root / installer may connect these systems.

Do not create unnecessary managers.

Avoid:

- giant manager classes
- generic ServiceLocator
- dependency injection framework
- global static state
- event bus framework
- generic UI framework
- MVVM framework
- unnecessary abstraction layers

---

# Services

Use lightweight services only where they provide a meaningful boundary.

## Music Playback

Music playback behavior should be separate from UI presentation.

A playback service may own:

- AudioSource interaction
- Play
- Pause
- track selection
- playback state

The UI should not directly contain playback business logic.

## Audio Analysis

Audio analysis should be separate from playback and UI rendering.

AudioSpectrumAnalyzer should:

- inspect the AudioSource that is actually playing
- extract spectrum/audio data
- perform normalization
- perform smoothing
- expose useful visualizer values

The visualizer must react to actual playing audio.

Do not use:

- random bar animation
- looping AnimationClips
- predefined waveform animation
- fake audio visualization

---

# Narrative Integration

Do not make the Music Player depend directly on Naninovel.

The production project may adopt Naninovel later.

Keep the integration boundary thin so a future adapter can be introduced
without rewriting the music feature.

For example, narrative/dialogue visibility may be exposed through a small
abstraction or explicit controller API.

Dependency direction should be:

Naninovel Integration
↓
Resonance Music / UI Systems

NOT:

Resonance Music System
↓
Naninovel

Do not install Naninovel for this test unless explicitly requested.

---

# UI State Model

Treat dialogue visibility and music-player visibility as explicit UI state.

Suggested states:

- BothVisible
- DialogueOnly
- MusicOnly
- Hidden

Do not scatter independent positioning logic across multiple views.

DialogueMusicLayoutController owns transitions between layout states.

---

# Layout Requirements

The dialogue and music player form one bottom UI composition.

## Both Visible

Dialogue is on the left.

Music Player is on the right.

The combined composition is centered.

## Dialogue Hidden

The Music Player remains in the EXACT position it occupied in the
two-panel layout.

It must NOT recenter.

## Music Player Hidden

The dialogue panel animates into the centered position.

The Music Player animates away.

It must not disappear instantly.

## Music Player Shown

The Music Player animates back into its two-panel position.

At the same time, Dialogue moves from centered into the two-panel position.

Neither panel should snap.

---

# Responsive UI

The UI must work correctly across:

- 16:9
- 16:10
- 21:9

Test sizes include approximately:

- 1280x720
- 1920x1080
- 1920x1200
- 2560x1080
- 3840x2160

Nothing should:

- overlap
- clip
- leave the visible screen
- drift incorrectly

Use:

- anchors
- RectTransform relationships
- CanvasScaler
- shared layout calculations

Do not base responsive behavior on hard-coded absolute screen coordinates.

---

# Visualizer

The visualizer is one of the most important parts of the assignment.

Use real audio analysis from the active AudioSource.

AudioSource.GetSpectrumData or an equivalent real-time Unity audio analysis
approach is appropriate.

The pipeline should conceptually be:

AudioSource
↓
Spectrum Analysis
↓
Frequency Mapping
↓
Normalization / Sensitivity
↓
Attack / Release Smoothing
↓
Visualizer Bars

Visualizer behavior should feel intentional and stable.

Avoid raw noisy FFT output.

Use different rise and fall behavior where useful:

- fast enough attack to feel responsive
- smoother release to avoid flicker

When music is paused:

- preserve playback position
- smoothly decay visualizer values toward rest
- do not instantly zero all bars

Avoid per-frame allocations.

---

# UI Presentation

Views are responsible for presentation.

## MusicPlayerView

May present:

- title
- artist
- playback progress
- playback time
- play / pause state
- buttons

## AudioVisualizerView

Responsible for rendering visualizer values.

It should not own audio playback logic.

## DialogueMusicLayoutController

Responsible for:

- layout state
- panel target positions
- show/hide transitions
- responsive positioning

---

# Animation

Keep transitions lightweight.

Prefer scripted RectTransform / CanvasGroup transitions.

Serialized values should control:

- duration
- easing

Do not add a tweening dependency solely for this test.

Transitions must animate rather than snap.

---

# Scene Hierarchy

Keep scene hierarchy understandable to another Unity developer.

Suggested structure:

DeveloperTest
|
|-- Systems
|   `-- MusicSystem
|       |-- AudioSource
|       `-- MusicPlayerInstaller
|
|-- Environment
|   `-- TestBackground
|
`-- UI
`-- Canvas
    |
    |-- DialogueMusicLayout
    |   |-- DialoguePanel
    |   `-- MusicPlayer
|       |-- Background
|       |-- Header
|       |-- TrackInfo
|       |-- Visualizer
|       |-- Progress
|       `-- Controls
        |
        |-- MusicPlayerShowButton
        |
        `-- DebugControls

Do not create unnecessary empty GameObjects.

Avoid meaningless hierarchy names such as:

- Container1
- Holder
- Wrapper2
- GroupNew

Use names that communicate purpose.

---

# Project Folder Structure

Use feature-oriented organization.

Preferred structure:

Assets/
`-- _Resonance/
    |
    |-- Art/
    |   |-- Backgrounds/
    |   |-- Fonts/
    |   `-- UI/
|
|-- Audio/
|   `-- Music/
    |
    |-- Data/
    |   `-- Music/
|       |-- Tracks/
|       |-- Playlists/
|       `-- Config/
    |
    |-- Features/
    |   |
    |   |-- MusicPlayer/
    |   |   |-- Runtime/
    |   |   |-- UI/
    |   |   `-- Prefabs/
|   |
|   `-- Dialogue/
    |       |-- Runtime/
    |       `-- UI/
|
|-- Integration/
|   `-- Narrative/
    |
    |-- Scenes/
    |
    `-- Tests/

Data/ contains primarily designer-authored project data.

Features/ contains implementation.

Do not create deep folder nesting unless it improves discoverability.

---

# Assemblies

Prefer one runtime assembly definition:

Resonance.Runtime.asmdef

Tests may use separate EditMode / PlayMode assemblies if tests are added.

Do not create one assembly per small feature.

A future Naninovel integration could live in a separate assembly that depends
on Resonance.Runtime.

Resonance.Runtime must not depend on Naninovel.

---

# Code Style

Prefer:

- clear descriptive names
- private serialized fields
- explicit dependencies
- small focused methods
- cached component references

Avoid:

- FindObjectOfType for normal dependency wiring
- magic scene paths
- magic screen coordinates
- unnecessary singletons
- per-frame allocations
- unnecessary Update methods
- public fields solely for Inspector access

Comments should explain WHY something works a certain way.

Do not comment obvious code.

---

# Prefabs

MusicPlayer should preferably exist as a reusable prefab.

Prefab hierarchy should remain easy for designers to inspect and modify.

Do not put gameplay state inside the prefab asset.

Runtime systems provide state to the view.

---

# Validation

A code change is not considered complete merely because it compiles.

After meaningful implementation work:

1. Compile the Unity project.
2. Check Unity Console for errors and warnings.
3. Enter Play Mode.
4. Verify the changed behavior.
5. Validate important UI states.
6. Validate relevant resolutions / aspect ratios.

Use UnityCLI and Unity Skills for validation where helpful.

When possible, validate at:

- 1280x720
- 1920x1080
- 1920x1200
- 2560x1080
- 3840x2160

---

# Agent Workflow

When asked to implement a substantial feature:

1. Inspect the relevant project files first.
2. Understand existing architecture and assets.
3. State the intended change briefly.
4. Make the smallest coherent implementation.
5. Compile.
6. Validate behavior in Unity.
7. Report what changed and any issues.

Do not blindly generate large amounts of code.

Do not restructure unrelated systems without a clear reason.

Do not overwrite supplied assets unnecessarily.

Do not implement optional features until required functionality is working
and visually polished.

---

# AI Workflow Notes

The assignment requires a short description of how AI was used.

Maintain:

AI_NOTES.md

During development, record:

- what tasks were delegated to AI
- what was implemented manually
- mistakes made by AI
- how those mistakes were discovered
- important human design / architecture decisions

Do not manufacture an AI mistake.

Record real issues as they occur.

---

# Definition of Done

Required functionality is done when:

- actual music plays
- pause preserves playback position
- visualizer reacts to actual playing audio
- visualizer settles smoothly on pause
- player hide/show is animated
- dialogue/player layout transitions are animated
- Music Player does not recenter when Dialogue is hidden
- Dialogue centers when Music Player is hidden
- required aspect ratios work correctly
- scene hierarchy is clean
- code responsibilities are understandable
- designer configuration does not require code changes
- Unity Console is clean of relevant errors