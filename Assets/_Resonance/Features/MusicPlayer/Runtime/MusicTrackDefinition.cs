using UnityEngine;

[CreateAssetMenu(fileName = "MusicTrack", menuName = "Resonance/Music/Track")]
public sealed class MusicTrackDefinition : ScriptableObject
{
    [SerializeField, HideInInspector] private string stableId;

    [Header("Track")]
    [Tooltip("Track title displayed to the player.")]
    [SerializeField] private string trackTitle;

    [Tooltip("Artist name displayed to the player.")]
    [SerializeField] private string artistName;

    [Tooltip("Audio recording played for this track.")]
    [SerializeField] private AudioClip audioClip;

    public string StableId => stableId;
    public string TrackTitle => trackTitle;
    public string ArtistName => artistName;
    public AudioClip AudioClip => audioClip;

#if UNITY_EDITOR
    private void OnEnable()
    {
        QueueStableIdSynchronization();
    }

    private void OnValidate()
    {
        QueueStableIdSynchronization();
    }

    private void QueueStableIdSynchronization()
    {
        // Creation/duplication can invoke validation before the asset has its own path.
        UnityEditor.EditorApplication.delayCall -= SynchronizeStableId;
        UnityEditor.EditorApplication.delayCall += SynchronizeStableId;
    }

    private void SynchronizeStableId()
    {
        if (this == null)
            return;

        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
            return;

        string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid) || stableId == guid)
            return;

        stableId = guid;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
