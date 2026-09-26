using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class MusicPlayerView : MonoBehaviour
{
    [Header("Track Information")]
    [Tooltip("Text element that displays the selected track title.")]
    [SerializeField] private TextMeshProUGUI trackTitleText;

    [Tooltip("Text element that displays the selected artist name.")]
    [SerializeField] private TextMeshProUGUI artistNameText;

    [Header("Playback State")]
    [Tooltip("Visual shown when pressing the control would start or resume playback.")]
    [SerializeField] private GameObject playVisual;

    [Tooltip("Visual shown when pressing the control would pause playback.")]
    [SerializeField] private GameObject pauseVisual;

    [Header("Progress (Optional)")]
    [Tooltip("Optional Slider that displays playback progress and requests a seek when the user drags it.")]
    [SerializeField] private UnityEngine.UI.Slider progressSlider;

    [Tooltip("Optional filled Image used for read-only playback progress.")]
    [SerializeField] private UnityEngine.UI.Image progressFillImage;

    /// <summary>Raised only when the user changes the progress Slider.</summary>
    public event Action<float> ProgressChanged;

    private void Awake()
    {
        if (progressSlider != null)
            progressSlider.onValueChanged.AddListener(NotifyProgressChanged);
    }

    private void OnDestroy()
    {
        if (progressSlider != null)
            progressSlider.onValueChanged.RemoveListener(NotifyProgressChanged);
    }

    public void SetTrackInfo(string trackTitle, string artistName)
    {
        if (trackTitleText != null)
            trackTitleText.text = trackTitle ?? string.Empty;

        if (artistNameText != null)
            artistNameText.text = artistName ?? string.Empty;
    }

    public void SetPlaybackState(bool isPlaying)
    {
        SetActiveIfNeeded(playVisual, !isPlaying);
        SetActiveIfNeeded(pauseVisual, isPlaying);
    }

    public void SetProgress(float normalizedProgress)
    {
        float clampedProgress = Mathf.Clamp01(normalizedProgress);

        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(clampedProgress);

        if (progressFillImage != null)
            progressFillImage.fillAmount = clampedProgress;
    }

    public void Clear()
    {
        SetTrackInfo(string.Empty, string.Empty);
        SetPlaybackState(false);
        SetProgress(0f);
    }

    private void NotifyProgressChanged(float normalizedProgress)
    {
        ProgressChanged?.Invoke(Mathf.Clamp01(normalizedProgress));
    }

    private static void SetActiveIfNeeded(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
            target.SetActive(active);
    }
}
