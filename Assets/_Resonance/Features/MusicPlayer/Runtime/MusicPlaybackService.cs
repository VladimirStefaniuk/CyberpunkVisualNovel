using UnityEngine;

[DisallowMultipleComponent]
public sealed class MusicPlaybackService : MonoBehaviour
{
    [Tooltip("Dedicated music source, explicitly assigned. Disable Play On Awake and let this service exclusively control its playback.")]
    [SerializeField] private AudioSource audioSource;

    private MusicTrackDefinition _currentTrack;
    private bool _isPaused;

    public MusicTrackDefinition CurrentTrack => _currentTrack;
    public bool IsPlaying => HasLoadedTrack && audioSource.isPlaying;
    public bool IsPaused => HasLoadedTrack && _isPaused;
    public float PlaybackTime => HasLoadedTrack ? audioSource.time : 0f;
    public float Duration => HasLoadedTrack ? audioSource.clip.length : 0f;

    private bool HasLoadedTrack => audioSource != null &&
        _currentTrack != null && audioSource.clip != null &&
        audioSource.clip == _currentTrack.AudioClip;

    /// <summary>Starts a valid track from the beginning, including an already selected track.</summary>
    public void PlayTrack(MusicTrackDefinition track)
    {
        if (track == null || track.AudioClip == null)
        {
            LogDiagnostic($"PlayTrack ignored track={(track != null ? track.name : "<null>")} reason=MissingTrackOrClip");
            Debug.LogWarning("Cannot play music: assign a track with an AudioClip.", this);
            return;
        }

        if (audioSource == null || !audioSource.isActiveAndEnabled || !isActiveAndEnabled)
        {
            LogDiagnostic($"PlayTrack ignored track={track.name} reason=InactiveServiceOrAudioSource");
            Debug.LogWarning("Cannot play music: the service and its assigned AudioSource must be active and enabled.", this);
            return;
        }

        LogDiagnostic($"PlayTrack track={track.name} action=StartFromBeginning");
        audioSource.Stop();
        audioSource.clip = track.AudioClip;
        audioSource.loop = false;
        _currentTrack = track;
        _isPaused = false;
        audioSource.Play();
    }

    public void Pause()
    {
        if (!IsPlaying || _isPaused)
        {
            LogDiagnostic($"Pause ignored {GetPlaybackSnapshot()}");
            return;
        }

        audioSource.Pause();
        _isPaused = true;
        LogDiagnostic($"Pause {GetPlaybackSnapshot()}");
    }

    public void Resume()
    {
        if (!IsPaused || !isActiveAndEnabled || !audioSource.isActiveAndEnabled)
        {
            LogDiagnostic($"Resume ignored {GetPlaybackSnapshot()}");
            return;
        }

        // UnPause preserves the source's sample position instead of starting a new play.
        audioSource.UnPause();
        _isPaused = false;
        LogDiagnostic($"Resume {GetPlaybackSnapshot()}");
    }

    /// <summary>Stops and rewinds the selected track. Resume only applies to paused playback.</summary>
    public void Stop()
    {
        bool hadLoadedTrack = HasLoadedTrack;

        if (audioSource != null)
            audioSource.Stop();

        _isPaused = false;
        LogDiagnostic($"Stop hadLoadedTrack={hadLoadedTrack} current={GetTrackName(_currentTrack)}");
    }

    private void OnDisable()
    {
        Stop();
    }

    private string GetPlaybackSnapshot()
    {
        return $"current={GetTrackName(_currentTrack)} isPlaying={IsPlaying} isPaused={IsPaused} " +
            $"time={PlaybackTime:F2} duration={Duration:F2}";
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
        Debug.Log($"[MusicPlayback] {message}", this);
#endif
    }
}
