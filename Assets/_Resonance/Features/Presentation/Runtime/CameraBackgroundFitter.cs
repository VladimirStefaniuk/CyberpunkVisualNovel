using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CameraBackgroundFitter : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("Framing")]
    [Tooltip(
        "Horizontal framing within the available cropped area. " +
        "-1 and +1 represent the maximum safe offsets without exposing the background edge.")]
    [Range(-1f, 1f)]
    [SerializeField] private float horizontalFraming;

    [Tooltip(
        "Vertical framing within the available cropped area. " +
        "-1 and +1 represent the maximum safe offsets without exposing the background edge.")]
    [Range(-1f, 1f)]
    [SerializeField] private float verticalFraming;

    [Tooltip("Additional zoom applied after fitting the background to the camera.")]
    [Min(1f)]
    [SerializeField] private float zoom = 1f;

    private SpriteRenderer _spriteRenderer;

    private float _lastAspect = -1f;
    private float _lastOrthographicSize = -1f;
    private float _lastHorizontalFraming;
    private float _lastVerticalFraming;
    private float _lastZoom = -1f;
    private Sprite _lastSprite;

    private void OnEnable()
    {
        CacheReferences();
        Fit();
    }

    private void Update()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (NeedsRefit())
            Fit();
    }

    private void OnValidate()
    {
        CacheReferences();
        Fit();
    }

    private void CacheReferences()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private bool NeedsRefit()
    {
        if (targetCamera == null ||
            !targetCamera.orthographic ||
            _spriteRenderer == null ||
            _spriteRenderer.sprite == null)
        {
            return false;
        }

        return
            !Mathf.Approximately(_lastAspect, targetCamera.aspect) ||
            !Mathf.Approximately(_lastOrthographicSize, targetCamera.orthographicSize) ||
            !Mathf.Approximately(_lastHorizontalFraming, horizontalFraming) ||
            !Mathf.Approximately(_lastVerticalFraming, verticalFraming) ||
            !Mathf.Approximately(_lastZoom, zoom) ||
            _lastSprite != _spriteRenderer.sprite;
    }

    private void Fit()
    {
        if (targetCamera == null ||
            !targetCamera.orthographic ||
            _spriteRenderer == null ||
            _spriteRenderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        // Minimum uniform scale required to completely cover the camera.
        float coverScale = Mathf.Max(
            cameraWidth / spriteSize.x,
            cameraHeight / spriteSize.y);

        float safeZoom = Mathf.Max(1f, zoom);
        float finalScale = coverScale * safeZoom;

        transform.localScale = new Vector3(
            finalScale,
            finalScale,
            1f);

        // Calculate how much artwork exists outside the camera after fitting.
        // This gives us the maximum distance we can safely shift the image
        // without revealing an edge.
        float scaledWidth = spriteSize.x * finalScale;
        float scaledHeight = spriteSize.y * finalScale;

        float maxOffsetX = Mathf.Max(
            0f,
            (scaledWidth - cameraWidth) * 0.5f);

        float maxOffsetY = Mathf.Max(
            0f,
            (scaledHeight - cameraHeight) * 0.5f);

        float safeHorizontalFraming =
            Mathf.Clamp(horizontalFraming, -1f, 1f);

        float safeVerticalFraming =
            Mathf.Clamp(verticalFraming, -1f, 1f);

        Vector3 localPosition = transform.localPosition;

        localPosition.x =
            safeHorizontalFraming * maxOffsetX;

        localPosition.y =
            safeVerticalFraming * maxOffsetY;

        transform.localPosition = localPosition;

        _lastAspect = targetCamera.aspect;
        _lastOrthographicSize = targetCamera.orthographicSize;
        _lastHorizontalFraming = horizontalFraming;
        _lastVerticalFraming = verticalFraming;
        _lastZoom = zoom;
        _lastSprite = _spriteRenderer.sprite;
    }
}