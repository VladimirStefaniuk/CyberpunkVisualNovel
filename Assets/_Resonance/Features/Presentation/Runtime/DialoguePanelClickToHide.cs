using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class DialoguePanelClickToHide : MonoBehaviour, IPointerClickHandler
{
    private DialogueMusicLayoutController _layoutController;

    public void Initialize(DialogueMusicLayoutController layoutController)
    {
        _layoutController = layoutController;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            _layoutController != null &&
            _layoutController.DialogueVisible &&
            !_layoutController.IsTransitioning)
        {
            _layoutController.HideDialogue();
        }
    }
}
