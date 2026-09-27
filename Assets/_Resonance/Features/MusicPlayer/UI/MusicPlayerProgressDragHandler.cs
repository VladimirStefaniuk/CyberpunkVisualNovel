using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class MusicPlayerProgressDragHandler : MonoBehaviour, IBeginDragHandler
{
    public event Action DragStarted;

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragStarted?.Invoke();
    }
}
