using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class DialogueMusicLayoutController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private RectTransform musicPlayer;
    [SerializeField] private CanvasGroup dialogueGroup;
    [SerializeField] private CanvasGroup musicPlayerGroup;
    [Tooltip("The root Canvas RectTransform, used to calculate off-screen destinations.")]
    [SerializeField] private RectTransform canvasBounds;

    [Header("Restore Controls")]
    [SerializeField] private DialogueMusicEdgeRestoreControls edgeRestoreControls;

    [Header("Layout")]
    [SerializeField] private DialogueMusicLayoutState initialState = DialogueMusicLayoutState.BothVisible;
    [Min(0f)] [SerializeField] private float panelGap = 36f;
    [Tooltip("Distance beyond the Canvas edge when hidden, in layout units.")]
    [Min(0f)] [SerializeField] private float hiddenPadding = 24f;

    [Header("Transitions")]
    [Min(0f)] [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private AnimationCurve transitionEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly Vector3[] _canvasCorners = new Vector3[4];
    private RectTransform _layoutRoot;
    private Vector2 _dialogueTarget;
    private Vector2 _musicTarget;
    private Vector2 _dialogueStart;
    private Vector2 _musicStart;
    private Vector2 _dialogueVisibleOffset;
    private Vector2 _musicVisibleOffset;
    private float _dialogueAlphaStart;
    private float _musicAlphaStart;
    private float _elapsed;
    private bool _initialized;
    private DialogueMusicEdgeRestoreControls _edgeRestoreControls;

    /// <summary>The requested layout state, including while its transition is running.</summary>
    public DialogueMusicLayoutState CurrentState { get; private set; }
    public bool IsTransitioning { get; private set; }
    public bool DialogueVisible => CurrentState == DialogueMusicLayoutState.BothVisible ||
                                   CurrentState == DialogueMusicLayoutState.DialogueOnly;
    public bool MusicPlayerVisible => CurrentState == DialogueMusicLayoutState.BothVisible ||
                                      CurrentState == DialogueMusicLayoutState.MusicOnly;
    public event Action StateChanged;

    private void OnEnable()
    {
        _layoutRoot = (RectTransform)transform;
        if (dialoguePanel == null || musicPlayer == null ||
            canvasBounds == null ||
            dialoguePanel.parent != transform || musicPlayer.parent != transform)
        {
            Debug.LogError("Dialogue/music layout requires two direct child panels and Canvas bounds.", this);
            enabled = false;
            return;
        }

        if (dialogueGroup == null)
            dialogueGroup = GetOrAddCanvasGroup(dialoguePanel);
        if (musicPlayerGroup == null)
            musicPlayerGroup = GetOrAddCanvasGroup(musicPlayer);

        ConfigureInteractionControls();

        Canvas.ForceUpdateCanvases();
        if (!_initialized)
        {
            CurrentState = initialState;
            CaptureAuthoredVisibleOffsets();
        }
        _initialized = true;
        CalculateTargets(out _dialogueTarget, out _musicTarget);
        CompleteTransition();
        StateChanged?.Invoke();
    }

    public void SetState(DialogueMusicLayoutState state)
    {
        if (!_initialized && !enabled && gameObject.activeInHierarchy)
            enabled = true;

        if (!_initialized || !gameObject.activeInHierarchy || state == CurrentState)
            return;

        CurrentState = state;
        CalculateTargets(out _dialogueTarget, out _musicTarget);
        BeginTransition();
        StateChanged?.Invoke();
    }

    public void SetDialogueVisible(bool visible)
    {
        SetState(visible
            ? (MusicPlayerVisible ? DialogueMusicLayoutState.BothVisible : DialogueMusicLayoutState.DialogueOnly)
            : (MusicPlayerVisible ? DialogueMusicLayoutState.MusicOnly : DialogueMusicLayoutState.BothHidden));
    }

    public void ShowDialogue() => SetDialogueVisible(true);
    public void HideDialogue() => SetDialogueVisible(false);
    public void ToggleDialogue() => SetDialogueVisible(!DialogueVisible);

    public void SetMusicPlayerVisible(bool visible)
    {
        SetState(visible
            ? (DialogueVisible ? DialogueMusicLayoutState.BothVisible : DialogueMusicLayoutState.MusicOnly)
            : (DialogueVisible ? DialogueMusicLayoutState.DialogueOnly : DialogueMusicLayoutState.BothHidden));
    }

    public void ShowMusicPlayer() => SetMusicPlayerVisible(true);
    public void HideMusicPlayer() => SetMusicPlayerVisible(false);
    public void ToggleMusicPlayer() => SetMusicPlayerVisible(!MusicPlayerVisible);

    private void LateUpdate()
    {
        // Dimensions can change without a state change (Canvas resize or designer tuning).
        CalculateTargets(out Vector2 dialogueTarget, out Vector2 musicTarget);
        if (dialogueTarget != _dialogueTarget || musicTarget != _musicTarget)
        {
            _dialogueTarget = dialogueTarget;
            _musicTarget = musicTarget;
            if (IsTransitioning)
                BeginTransition();
            else
                CompleteTransition();
        }

        if (!IsTransitioning)
            return;

        _elapsed += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(_elapsed / transitionDuration);
        float eased = transitionEasing == null ? progress : Mathf.Clamp01(transitionEasing.Evaluate(progress));
        dialoguePanel.anchoredPosition = Vector2.LerpUnclamped(_dialogueStart, _dialogueTarget, eased);
        musicPlayer.anchoredPosition = Vector2.LerpUnclamped(_musicStart, _musicTarget, eased);
        dialogueGroup.alpha = Mathf.Lerp(_dialogueAlphaStart, DialogueVisible ? 1f : 0f, eased);
        musicPlayerGroup.alpha = Mathf.Lerp(_musicAlphaStart, MusicPlayerVisible ? 1f : 0f, eased);

        if (progress >= 1f)
            CompleteTransition();
    }

    private void CalculateTargets(out Vector2 dialogue, out Vector2 music)
    {
        CalculateDefaultVisibleTargets(out dialogue, out music);
        dialogue += _dialogueVisibleOffset;
        music += _musicVisibleOffset;

        Vector2 center = _layoutRoot.rect.center;
        if (DialogueVisible && !MusicPlayerVisible)
        {
            // DialogueOnly must remain horizontally centered while preserving designer-authored vertical tuning.
            dialogue.x = CenterToAnchoredPosition(dialoguePanel, center).x;
        }

        float dialogueWidth = dialoguePanel.rect.width;
        float musicWidth = musicPlayer.rect.width;

        canvasBounds.GetWorldCorners(_canvasCorners);
        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        for (int i = 0; i < _canvasCorners.Length; i++)
        {
            float x = _layoutRoot.InverseTransformPoint(_canvasCorners[i]).x;
            left = Mathf.Min(left, x);
            right = Mathf.Max(right, x);
        }

        if (!DialogueVisible)
        {
            float hiddenCenter = left - hiddenPadding - dialogueWidth * 0.5f;
            dialogue.x = CenterToAnchoredPosition(dialoguePanel, new Vector2(hiddenCenter, center.y)).x;
        }

        if (!MusicPlayerVisible)
        {
            float hiddenCenter = right + hiddenPadding + musicWidth * 0.5f;
            music.x = CenterToAnchoredPosition(musicPlayer, new Vector2(hiddenCenter, center.y)).x;
        }
    }

    private void CaptureAuthoredVisibleOffsets()
    {
        CalculateDefaultVisibleTargets(out Vector2 defaultDialogue, out Vector2 defaultMusic);
        _dialogueVisibleOffset = dialoguePanel.anchoredPosition - defaultDialogue;
        _musicVisibleOffset = musicPlayer.anchoredPosition - defaultMusic;
    }

    private void CalculateDefaultVisibleTargets(out Vector2 dialogue, out Vector2 music)
    {
        float dialogueWidth = dialoguePanel.rect.width;
        float musicWidth = musicPlayer.rect.width;
        float combinedWidth = dialogueWidth + panelGap + musicWidth;
        Vector2 center = _layoutRoot.rect.center;
        float dialogueCenter = center.x - combinedWidth * 0.5f + dialogueWidth * 0.5f;
        float musicCenter = center.x + combinedWidth * 0.5f - musicWidth * 0.5f;

        dialogue = CenterToAnchoredPosition(dialoguePanel, new Vector2(dialogueCenter, center.y));
        music = CenterToAnchoredPosition(musicPlayer, new Vector2(musicCenter, center.y));
    }

    private Vector2 CenterToAnchoredPosition(RectTransform panel, Vector2 center)
    {
        Vector2 anchor = new Vector2(
            Mathf.Lerp(panel.anchorMin.x, panel.anchorMax.x, panel.pivot.x),
            Mathf.Lerp(panel.anchorMin.y, panel.anchorMax.y, panel.pivot.y));
        Vector2 anchorPosition = _layoutRoot.rect.min + Vector2.Scale(_layoutRoot.rect.size, anchor);
        return center - anchorPosition - panel.rect.center;
    }

    private void BeginTransition()
    {
        _dialogueStart = dialoguePanel.anchoredPosition;
        _musicStart = musicPlayer.anchoredPosition;
        _dialogueAlphaStart = dialogueGroup.alpha;
        _musicAlphaStart = musicPlayerGroup.alpha;
        _elapsed = 0f;
        IsTransitioning = true;
        // Outgoing panels stop intercepting input immediately; incoming panels wait until settled.
        SetInput(dialogueGroup, DialogueVisible && _dialogueAlphaStart >= 1f);
        SetInput(musicPlayerGroup, MusicPlayerVisible && _musicAlphaStart >= 1f);
        if (transitionDuration <= 0f)
            CompleteTransition();
    }

    private void CompleteTransition()
    {
        dialoguePanel.anchoredPosition = _dialogueTarget;
        musicPlayer.anchoredPosition = _musicTarget;
        dialogueGroup.alpha = DialogueVisible ? 1f : 0f;
        musicPlayerGroup.alpha = MusicPlayerVisible ? 1f : 0f;
        SetInput(dialogueGroup, DialogueVisible);
        SetInput(musicPlayerGroup, MusicPlayerVisible);
        IsTransitioning = false;
    }

    private static void SetInput(CanvasGroup group, bool visible)
    {
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private static CanvasGroup GetOrAddCanvasGroup(RectTransform panel)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        return group != null ? group : panel.gameObject.AddComponent<CanvasGroup>();
    }

    private void ConfigureInteractionControls()
    {
        DialoguePanelClickToHide dialogueClickHandler = dialoguePanel.GetComponent<DialoguePanelClickToHide>();
        if (dialogueClickHandler == null)
            dialogueClickHandler = dialoguePanel.gameObject.AddComponent<DialoguePanelClickToHide>();
        dialogueClickHandler.Initialize(this);

        if (_edgeRestoreControls == null)
            _edgeRestoreControls = edgeRestoreControls;
        if (_edgeRestoreControls == null)
        {
            Debug.LogError("Dialogue/music layout requires scene-authored edge restore controls.", this);
            return;
        }

        _edgeRestoreControls.Initialize(this);
    }
}
