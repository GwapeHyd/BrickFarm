using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTreeCameraUI : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler
{
    public RectTransform target; // Le parent du skill tree (panel à déplacer/zoomer)
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.5f;
    public float dragSpeed = 1f;

    private Vector2 lastDragPosition;

    void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        float newScale = Mathf.Clamp(target.localScale.x + scroll * zoomSpeed, minZoom, maxZoom);
        target.localScale = new Vector3(newScale, newScale, 1f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = (eventData.position - lastDragPosition) / target.localScale.x;
        target.anchoredPosition += delta * dragSpeed;
        lastDragPosition = eventData.position;
    }
}
