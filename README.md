# Resonance — Music Player & Audio Visualizer

A Unity visual-novel UI prototype created for the Kangyroo Creative / Resonance developer test. The project combines the supplied dialogue presentation with a responsive music player, real-time audio visualizer, and animated dialogue/player layout states.

## Highlights

- Real music playback with pause/resume, track navigation, and progress scrubbing.
- Audio-reactive visualizer driven by the active `AudioSource` spectrum data, with configurable smoothing.
- Animated transitions between dialogue and music-player visibility states.
- Responsive bottom UI composition designed for 16:9, 16:10, and ultrawide displays.
- Designer-editable track, playlist, and player configuration through ScriptableObjects.
- Focused runtime responsibilities: playback, spectrum analysis, UI presentation, and layout state are kept separate.

## Setup

- **Unity:** 6000.0.84f1
- **Render pipeline:** Universal Render Pipeline (URP)
- **Main scene:** `Assets/_Resonance/Scenes/MainScene.unity`

Open the project in the Unity version above, then open the main scene and enter Play Mode.

## Project Structure

```text
Assets/_Resonance/
├── Art/                 # Supplied and presentation assets
├── Audio/Music/         # Music clips
├── Data/Music/          # Track, playlist, and player-config assets
├── Features/
│   ├── Dialogue/        # Dialogue presentation
│   ├── MusicPlayer/     # Playback, analysis, and player UI
│   └── Presentation/    # Dialogue/music layout transitions
└── Scenes/MainScene     # Test scene
```

## AI-Assisted Development

AI was used as a development aid, with architecture decisions, visual implementation, scene wiring, and final review kept under direct human control.

### Tooling and models

- Unity CLI with the official Unity Skill.
- GPT-6 Astra for the initial `AGENTS.md` guidance and implementation planning.
- GPT-5.6 Terra / Sol for code implementation, ScriptableObject data creation, initial scene hierarchy, and UI scaffolding.

### Development log

1. I defined the scope and architecture, using ChatGPT for a small number of early design questions. I then used the developer-test brief to create the final `AGENTS.md` instructions.
2. I used ChatGPT 6 (Astra) to track progress step by step against the assignment requirements.
3. I used Codex to establish the initial folder structure, scene hierarchy, and UI foundation.
4. I manually completed the music-player prefab and scene setup because this was faster and more reliable for visual iteration.
5. AI created the track and playlist ScriptableObjects; I assigned the audio tracks and connected the player in the scene.
6. AI implemented the playback and visualizer code; I completed the player UI and connected the scripts.
7. I used a combination of AI-assisted review and manual checking against the test brief to verify feature completeness.

Detailed contemporaneous notes, including AI corrections and validation observations, are available in [AI_NOTES.md](AI_NOTES.md).

## What I Would Do Next

- Expand the visualizer toward the full richness of the supplied prototype. This was the part of the project I most enjoyed and would be excited to refine further.
- Add automated tests around playback, layout state transitions, and configuration data to protect against regressions.
- With character art separated from the background, develop more expressive character scaling, parallax, and layout adjustments for the narrative presentation.
