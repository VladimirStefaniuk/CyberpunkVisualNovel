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
            Debug.LogWarning("Cannot play music: assign a track with an AudioClip.", this);
            return;
        }

        if (audioSource == null || !audioSource.isActiveAndEnabled || !isActiveAndEnabled)
        {
            Debug.LogWarning("Cannot play music: the service and its assigned AudioSource must be active and enabled.", this);
            return;
        }

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
            return;

        audioSource.Pause();
        _isPaused = true;
    }

    public void Resume()
    {
        if (!IsPaused || !isActiveAndEnabled || !audioSource.isActiveAndEnabled)
            return;

        // UnPause preserves the source's sample position instead of starting a new play.
        audioSource.UnPause();
        _isPaused = false;
    }

    /// <summary>Stops and rewinds the selected track. Resume only applies to paused playback.</summary>
    public void Stop()
    {
        if (audioSource != null)
            audioSource.Stop();

        _isPaused = false;
    }

    private void OnDisable()
    {
        Stop();
    }
}
