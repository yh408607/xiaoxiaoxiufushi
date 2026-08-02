using UnityEngine;

public class FingerDragUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform fingerRect;
    [SerializeField] private Vector2 screenOffset = new Vector2(30f, -30f); //  ÷÷∏Õº”Î¥•µ„∆´“∆£¨±‹√‚’⁄µ≤
    [SerializeField] private bool hideOnStart = true;

    private Camera uiCamera;

    private void Awake()
    {
        if (fingerRect == null)
        {
            fingerRect = transform as RectTransform;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        if (hideOnStart)
        {
            Hide();
        }
    }

    public void ShowAtScreenPosition(Vector2 screenPos)
    {
        if (fingerRect == null || canvas == null) return;

        gameObject.SetActive(true);
        SetScreenPosition(screenPos);
    }

    public void FollowScreenPosition(Vector2 screenPos)
    {
        if (!gameObject.activeSelf) return;
        SetScreenPosition(screenPos);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetScreenPosition(Vector2 screenPos)
    {
        RectTransform root = canvas.transform as RectTransform;
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root,
            screenPos + screenOffset,
            uiCamera,
            out localPoint
        );

        fingerRect.anchoredPosition = localPoint;
    }

    public void ShowGuideAtWorldPosition(Vector3 worldPos, Camera worldCamera)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPos);
        ShowAtScreenPosition(screenPos);
    }

}
