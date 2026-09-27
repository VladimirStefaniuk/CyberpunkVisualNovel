using UnityEngine;

[DisallowMultipleComponent]
public sealed class AudioVisualizerView : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Analyzer that supplies normalized values from the active music AudioSource.")]
    [SerializeField] private AudioSpectrumAnalyzer analyzer;

    [Tooltip("Existing UI area that contains the generated bars. It should be a RectTransform inside MusicPlayer.")]
    [SerializeField] private RectTransform barsRoot;

    [Header("Bar Appearance")]
    [Tooltip("Optional UI bar prefab. Its root must include an Image component.")]
    [SerializeField] private RectTransform barPrefab;

    [Tooltip("Optional Sprite asset used by the generated Image when no bar prefab is assigned.")]
    [SerializeField] private Sprite barSprite;

    [Tooltip("Gap between generated bars in UI units.")]
    [Min(0f)]
    [SerializeField] private float barSpacing = 3f;

    [Tooltip("Fallback color used when no bar template is supplied.")]
    [SerializeField] private Color fallbackBarColor = new Color(0.96f, 0.27f, 0.43f, 0.92f);

    private RectTransform[] _bars;
    private int _generatedBarCount;
    private float _lastRootWidth = -1f;
    private bool _reportedInvalidBarPrefab;

    private void Awake()
    {
        RebuildBarsIfNeeded();
    }

    private void Update()
    {
        RebuildBarsIfNeeded();
        if (_bars == null)
            return;

        UpdateBarLayoutIfNeeded();

        float availableHeight = Mathf.Max(0f, barsRoot.rect.height);

        for (int index = 0; index < _bars.Length; index++)
        {
            float normalizedValue = analyzer != null ? analyzer.GetBandValue(index) : 0f;
            _bars[index].SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                availableHeight * normalizedValue);
        }
    }

    private void RebuildBarsIfNeeded()
    {
        if (analyzer == null || barsRoot == null)
            return;

        int requestedBarCount = analyzer.BandCount;
        if (requestedBarCount == 0 || _generatedBarCount == requestedBarCount)
            return;

        ClearGeneratedBars();
        _bars = new RectTransform[requestedBarCount];
        _generatedBarCount = requestedBarCount;

        for (int index = 0; index < requestedBarCount; index++)
            _bars[index] = CreateBar(index);

        _lastRootWidth = -1f;
    }

    private RectTransform CreateBar(int index)
    {
        UnityEngine.UI.Image image = null;
        if (barPrefab != null)
        {
            RectTransform barInstance = Instantiate(barPrefab, barsRoot);
            image = barInstance.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                Destroy(barInstance.gameObject);
                if (!_reportedInvalidBarPrefab)
                {
                    Debug.LogWarning("Audio visualizer bar prefab requires an Image component on its root.", this);
                    _reportedInvalidBarPrefab = true;
                }
            }
        }

        if (image == null)
        {
            GameObject barObject = new GameObject(
                $"VisualizerBar_{index + 1:00}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(UnityEngine.UI.Image));
            barObject.transform.SetParent(barsRoot, false);
            image = barObject.GetComponent<UnityEngine.UI.Image>();
            image.sprite = barSprite;
            image.color = fallbackBarColor;
            image.raycastTarget = false;
        }

        RectTransform bar = image.rectTransform;
        bar.anchorMin = new Vector2(0f, 0f);
        bar.anchorMax = new Vector2(0f, 0f);
        bar.pivot = new Vector2(0.5f, 0f);
        bar.sizeDelta = new Vector2(1f, 0f);
        return bar;
    }

    private void UpdateBarLayoutIfNeeded()
    {
        float rootWidth = barsRoot.rect.width;
        if (Mathf.Approximately(rootWidth, _lastRootWidth))
            return;

        float barWidth = (rootWidth - barSpacing * (_bars.Length - 1)) / _bars.Length;
        barWidth = Mathf.Max(1f, barWidth);

        for (int index = 0; index < _bars.Length; index++)
        {
            _bars[index].anchoredPosition = new Vector2(barWidth * (index + 0.5f) + barSpacing * index, 0f);
            _bars[index].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barWidth);
        }

        _lastRootWidth = rootWidth;
    }

    private void ClearGeneratedBars()
    {
        if (_bars == null)
            return;

        for (int index = 0; index < _bars.Length; index++)
        {
            if (_bars[index] != null)
                Destroy(_bars[index].gameObject);
        }

        _bars = null;
        _generatedBarCount = 0;
        _lastRootWidth = -1f;
    }

    private void OnDestroy()
    {
        ClearGeneratedBars();
    }

    private void OnValidate()
    {
        barSpacing = Mathf.Max(0f, barSpacing);
    }
}
