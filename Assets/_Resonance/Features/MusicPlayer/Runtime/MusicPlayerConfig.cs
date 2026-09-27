using UnityEngine;

[CreateAssetMenu(fileName = "MusicPlayerConfig", menuName = "Resonance/Music/Player Config")]
public sealed class MusicPlayerConfig : ScriptableObject
{
    [Header("Playback")]
    [Tooltip("When enabled, completing a track starts the following valid playlist entry. Playback stops after the last entry.")]
    [SerializeField] private bool playNextTrackOnCompletion;

    public bool PlayNextTrackOnCompletion => playNextTrackOnCompletion;
}
