using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class DialogueMusicDebugControls : MonoBehaviour
{
    [SerializeField] private DialogueMusicLayoutController layoutController;
    [SerializeField] private Graphic dialogueButtonGraphic;
    [SerializeField] private Graphic musicPlayerButtonGraphic;

    [Header("State Colors")]
    [SerializeField] private Color visibleColor = new(0.05f, 0.35f, 0.28f, 0.95f);
    [SerializeField] private Color hiddenColor = new(0.2f, 0.07f, 0.1f, 0.95f);

    private void OnEnable()
    {
        if (layoutController == null)
        {
            Debug.LogError("Debug layout controls require a layout controller.", this);
            enabled = false;
            return;
        }

        layoutController.StateChanged += RefreshColors;
        RefreshColors();
    }

    private void OnDisable()
    {
        if (layoutController != null)
            layoutController.StateChanged -= RefreshColors;
    }

    private void RefreshColors()
    {
        if (dialogueButtonGraphic != null)
            dialogueButtonGraphic.color = layoutController.DialogueVisible ? visibleColor : hiddenColor;

        if (musicPlayerButtonGraphic != null)
            musicPlayerButtonGraphic.color = layoutController.MusicPlayerVisible ? visibleColor : hiddenColor;
    }
}
