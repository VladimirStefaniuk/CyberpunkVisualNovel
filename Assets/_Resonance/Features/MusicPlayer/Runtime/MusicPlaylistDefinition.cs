using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicPlaylist", menuName = "Resonance/Music/Playlist")]
public sealed class MusicPlaylistDefinition : ScriptableObject
{
    [Tooltip("Tracks in playback order. Add, remove, and drag entries to reorder them. An empty playlist is valid.")]
    [SerializeField] private List<MusicTrackDefinition> tracks = new List<MusicTrackDefinition>();

    private ReadOnlyCollection<MusicTrackDefinition> _readOnlyTracks;

    public IReadOnlyList<MusicTrackDefinition> Tracks
    {
        get
        {
            if (_readOnlyTracks == null)
                _readOnlyTracks = tracks.AsReadOnly();

            return _readOnlyTracks;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Inspector edits can replace the serialized list; rebuild its read-only view.
        _readOnlyTracks = null;
    }
#endif
}
