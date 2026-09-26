using UnityEngine;

[DisallowMultipleComponent]
public sealed class MusicPlayerController : MonoBehaviour
{
    [Header("Content")]
    [Tooltip("Ordered collection of tracks available to this player.")]
    [SerializeField] private MusicPlaylistDefinition playlist;

    [Tooltip("Playlist entry selected when the player initializes. Out-of-range values are clamped.")]
    [SerializeField] private int initialTrackIndex;

    [Tooltip("Begin playing the selected track when the player initializes.")]
    [SerializeField] private bool playOnStart;

    [Header("Dependencies")]
    [Tooltip("Playback service that owns the music AudioSource.")]
    [SerializeField] private MusicPlaybackService playbackService;

    [Tooltip("View that presents track metadata, playback state, and user-seekable progress.")]
    [SerializeField] private MusicPlayerView view;

    private MusicTrackDefinition _selectedTrack;
    private int _currentTrackIndex = -1;
    private bool _selectedTrackStarted;
    private bool _wasPlaying;
    private bool _completedNaturally;

    public MusicTrackDefinition SelectedTrack => _selectedTrack;
    public int CurrentTrackIndex => _currentTrackIndex;

    private void Awake()
    {
        if (view != null)
            view.ProgressChanged += SeekToNormalizedPosition;
    }

    private void OnDestroy()
    {
        if (view != null)
            view.ProgressChanged -= SeekToNormalizedPosition;
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        RefreshPlaybackPresentation();
    }

    public void TogglePlayPause()
    {
        LogDiagnostic($"TogglePlayPause BEFORE {GetPlaybackSnapshot()}");

        if (!CanControlSelectedTrack())
        {
            LogDiagnostic($"TogglePlayPause branch=IgnoredInvalidState AFTER {GetPlaybackSnapshot()}");
            return;
        }

        bool selectedTrackIsLoaded = playbackService.CurrentTrack == _selectedTrack;
        string branch;

        if (selectedTrackIsLoaded && playbackService.IsPlaying)
        {
            branch = "Pause";
            playbackService.Pause();
        }
        else if (selectedTrackIsLoaded && playbackService.IsPaused)
        {
            branch = "Resume";
            playbackService.Resume();
        }
        else
        {
            branch = "PlayFromBeginning";
            PlaySelectedTrack();
        }

        RefreshPlaybackPresentation();
        LogDiagnostic($"TogglePlayPause branch={branch} AFTER {GetPlaybackSnapshot()}");
    }

    /// <summary>Selects the next playlist entry, wrapping from the end to the beginning.</summary>
    public void NextTrack()
    {
        CycleTrack(1, "NextTrack");
    }

    /// <summary>Selects the previous playlist entry, wrapping from the beginning to the end.</summary>
    public void PreviousTrack()
    {
        CycleTrack(-1, "PreviousTrack");
    }

    private void SeekToNormalizedPosition(float normalizedPosition)
    {
        if (!CanControlSelectedTrack() || playbackService.CurrentTrack != _selectedTrack)
            return;

        if (!playbackService.SeekToNormalizedPosition(normalizedPosition))
            return;

        // A completed track can be replayed after its handle is dragged away from the end.
        _completedNaturally = false;
        RefreshPlaybackPresentation();
    }

    private void Initialize()
    {
        if (view == null)
            Debug.LogError("Music player controller requires a MusicPlayerView reference.", this);
        else
            view.Clear();

        if (playbackService == null)
            Debug.LogError("Music player controller requires a MusicPlaybackService reference.", this);

        if (playlist == null)
        {
            Debug.LogWarning("Music player controller has no playlist assigned.", this);
            LogInitialization();
            return;
        }

        if (playlist.Tracks.Count == 0)
        {
            Debug.LogWarning("Music player controller cannot select a track from an empty playlist.", this);
            LogInitialization();
            return;
        }

        int clampedTrackIndex = Mathf.Clamp(initialTrackIndex, 0, playlist.Tracks.Count - 1);
        if (clampedTrackIndex != initialTrackIndex)
        {
            Debug.LogWarning(
                $"Initial track index {initialTrackIndex} is outside the playlist. Using index {clampedTrackIndex}.",
                this);
        }

        _currentTrackIndex = clampedTrackIndex;
        _selectedTrack = playlist.Tracks[_currentTrackIndex];

        if (_selectedTrack == null)
        {
            Debug.LogWarning($"Playlist entry {_currentTrackIndex} has no track assigned.", this);
            LogInitialization();
            return;
        }

        if (view != null)
            view.SetTrackInfo(_selectedTrack.TrackTitle, _selectedTrack.ArtistName);

        if (_selectedTrack.AudioClip == null)
        {
            Debug.LogWarning($"Selected track '{_selectedTrack.name}' has no AudioClip assigned.", this);
            LogInitialization();
            return;
        }

        if (playOnStart)
            PlaySelectedTrack();

        RefreshPlaybackPresentation();
        LogInitialization();
    }

    private bool CanControlSelectedTrack()
    {
        if (playbackService == null)
        {
            Debug.LogError("Cannot control music without a MusicPlaybackService reference.", this);
            return false;
        }

        if (_selectedTrack == null)
        {
            Debug.LogWarning("Cannot control music because no track is selected.", this);
            return false;
        }

        if (_selectedTrack.AudioClip == null)
        {
            Debug.LogWarning($"Cannot play '{_selectedTrack.name}' because it has no AudioClip assigned.", this);
            return false;
        }

        return true;
    }

    private void PlaySelectedTrack()
    {
        if (!CanControlSelectedTrack())
            return;

        playbackService.PlayTrack(_selectedTrack);
        _selectedTrackStarted = playbackService.CurrentTrack == _selectedTrack;
        _completedNaturally = false;
        _wasPlaying = playbackService.IsPlaying;
    }

    private void CycleTrack(int direction, string commandName)
    {
        LogDiagnostic($"{commandName} BEFORE {GetPlaybackSnapshot()}");

        if (playlist == null || playlist.Tracks.Count == 0)
        {
            LogDiagnostic($"{commandName} branch=IgnoredEmptyPlaylist AFTER {GetPlaybackSnapshot()}");
            return;
        }

        int trackCount = playlist.Tracks.Count;
        int currentIndex = _currentTrackIndex >= 0 ? _currentTrackIndex : 0;
        int nextIndex = (currentIndex + direction + trackCount) % trackCount;
        MusicTrackDefinition nextTrack = playlist.Tracks[nextIndex];

        if (nextTrack == null || nextTrack.AudioClip == null)
        {
            Debug.LogWarning($"Cannot select playlist entry {nextIndex}: assign a track with an AudioClip.", this);
            LogDiagnostic($"{commandName} branch=IgnoredInvalidTrack index={nextIndex} AFTER {GetPlaybackSnapshot()}");
            return;
        }

        bool wasPlaying = playbackService != null && playbackService.IsPlaying;
        _currentTrackIndex = nextIndex;
        _selectedTrack = nextTrack;
        _selectedTrackStarted = false;
        _completedNaturally = false;

        if (view != null)
            view.SetTrackInfo(_selectedTrack.TrackTitle, _selectedTrack.ArtistName);

        if (wasPlaying)
            PlaySelectedTrack();
        else
            RefreshPlaybackPresentation();

        LogDiagnostic($"{commandName} branch={(wasPlaying ? "PlaySelectedTrack" : "SelectOnly")} AFTER {GetPlaybackSnapshot()}");
    }

    private void RefreshPlaybackPresentation()
    {
        if (view == null)
            return;

        bool selectedTrackIsLoaded = playbackService != null &&
            playbackService.CurrentTrack == _selectedTrack;

        bool isPlaying = selectedTrackIsLoaded && playbackService.IsPlaying;
        bool isPaused = selectedTrackIsLoaded && playbackService.IsPaused;

        if (isPlaying)
        {
            _selectedTrackStarted = true;
            _completedNaturally = false;
        }
        else if (_wasPlaying && _selectedTrackStarted && !isPaused && selectedTrackIsLoaded)
        {
            _completedNaturally = true;
            LogDiagnostic($"Natural completion {GetPlaybackSnapshot()}");
        }

        view.SetPlaybackState(isPlaying);
        view.SetProgress(GetNormalizedProgress(selectedTrackIsLoaded));
        _wasPlaying = isPlaying;
    }

    private float GetNormalizedProgress(bool selectedTrackIsLoaded)
    {
        if (!selectedTrackIsLoaded || playbackService.Duration <= 0f)
            return 0f;

        if (_completedNaturally)
            return 1f;

        return Mathf.Clamp01(playbackService.PlaybackTime / playbackService.Duration);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private void LogInitialization()
    {
        int trackCount = playlist != null ? playlist.Tracks.Count : 0;
        LogDiagnostic(
            $"Initialize playlist={(playlist != null ? playlist.name : "<missing>")} " +
            $"count={trackCount} requestedIndex={initialTrackIndex} selectedIndex={_currentTrackIndex} " +
            $"selected={GetTrackName(_selectedTrack)} playOnStart={playOnStart} " +
            $"servicePresent={playbackService != null} viewPresent={view != null}");
    }

    private string GetPlaybackSnapshot()
    {
        return $"selected={GetTrackName(_selectedTrack)} " +
            $"current={GetTrackName(playbackService != null ? playbackService.CurrentTrack : null)} " +
            $"isPlaying={playbackService != null && playbackService.IsPlaying} " +
            $"isPaused={playbackService != null && playbackService.IsPaused} " +
            $"time={(playbackService != null ? playbackService.PlaybackTime : 0f):F2} " +
            $"duration={(playbackService != null ? playbackService.Duration : 0f):F2}";
    }

    private static string GetTrackName(MusicTrackDefinition track)
    {
        return track != null ? track.name : "<none>";
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private void LogDiagnostic(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[MusicPlayer] {message}", this);
#endif
    }
}
