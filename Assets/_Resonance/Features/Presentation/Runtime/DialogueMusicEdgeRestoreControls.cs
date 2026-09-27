using UnityEngine;

[DisallowMultipleComponent]
public sealed class DialogueMusicEdgeRestoreControls : MonoBehaviour
{
    private const float VisibleAlpha = 0.88f;

    [Header("Scene Controls")]
    [SerializeField] private UnityEngine.UI.Button showDialogueButton;
    [SerializeField] private CanvasGroup showDialogueGroup;
    [SerializeField] private UnityEngine.UI.Button showMusicPlayerButton;
    [SerializeField] private CanvasGroup showMusicPlayerGroup;

    [Header("Transition")]
    [Min(0f)] [SerializeField] private float fadeSpeed = 4.5f;

    private DialogueMusicLayoutController _layoutController;
    private float _dialogueTargetAlpha;
    private float _musicTargetAlpha;

    public void Initialize(DialogueMusicLayoutController layoutController)
    {
        Unsubscribe();
        _layoutController = layoutController;

        if (_layoutController == null ||
            showDialogueButton == null || showDialogueGroup == null ||
            showMusicPlayerButton == null || showMusicPlayerGroup == null)
        {
            Debug.LogError("Edge restore controls require both scene buttons, their CanvasGroups, and a layout controller.", this);
            enabled = false;
            return;
        }

        showDialogueButton.onClick.RemoveListener(_layoutController.ShowDialogue);
        showDialogueButton.onClick.AddListener(_layoutController.ShowDialogue);
        showMusicPlayerButton.onClick.RemoveListener(_layoutController.ShowMusicPlayer);
        showMusicPlayerButton.onClick.AddListener(_layoutController.ShowMusicPlayer);
        _layoutController.StateChanged += RefreshVisibility;
        RefreshVisibility();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (_layoutController == null)
            return;

        showDialogueGroup.alpha = Mathf.MoveTowards(
            showDialogueGroup.alpha, _dialogueTargetAlpha, Time.unscaledDeltaTime * fadeSpeed);
        showMusicPlayerGroup.alpha = Mathf.MoveTowards(
            showMusicPlayerGroup.alpha, _musicTargetAlpha, Time.unscaledDeltaTime * fadeSpeed);
    }

    private void RefreshVisibility()
    {
        SetVisible(showDialogueGroup, !_layoutController.DialogueVisible, out _dialogueTargetAlpha);
        SetVisible(showMusicPlayerGroup, !_layoutController.MusicPlayerVisible, out _musicTargetAlpha);
    }

    private static void SetVisible(CanvasGroup group, bool visible, out float targetAlpha)
    {
        targetAlpha = visible ? VisibleAlpha : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private void Unsubscribe()
    {
        if (_layoutController == null)
            return;

        _layoutController.StateChanged -= RefreshVisibility;
        if (showDialogueButton != null)
            showDialogueButton.onClick.RemoveListener(_layoutController.ShowDialogue);
        if (showMusicPlayerButton != null)
            showMusicPlayerButton.onClick.RemoveListener(_layoutController.ShowMusicPlayer);
    }
}
