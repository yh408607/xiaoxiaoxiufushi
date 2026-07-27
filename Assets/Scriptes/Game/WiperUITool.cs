using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WiperUITool : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("擦灰控制器")]
    [SerializeField] private DustWipeController dustWipeController;

    [Header("相机")]
    [SerializeField] private Camera worldCamera;

    [Header("UI 设置")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform dragArea;

    [Header("擦拭点偏移")]
    [SerializeField] private Vector2 wipeScreenOffset = Vector2.zero;

    private RectTransform rectTransform;
    private bool isEnabled;
    private bool isDragging;

    public bool IsEnabled => isEnabled;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        gameObject.SetActive(false);
    }

    public void Init(DustWipeController controller, Camera camera = null)
    {
        dustWipeController = controller;

        if (camera != null)
        {
            worldCamera = camera;
        }

        DisableWiper();
    }

    public void EnableWiper()
    {
        isEnabled = true;
        isDragging = false;
        gameObject.SetActive(true);
    }

    public void DisableWiper()
    {
        isEnabled = false;
        isDragging = false;
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isEnabled) return;

        isDragging = true;
        MoveToPointer(eventData);
        WipeAtPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isEnabled) return;
        if (!isDragging) return;

        MoveToPointer(eventData);
        WipeAtPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransform parentRect = rectTransform.parent as RectTransform;

        if (parentRect == null) return;

        Vector2 localPoint;

        Camera uiCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            uiCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    private void WipeAtPointer(PointerEventData eventData)
    {
        if (dustWipeController == null) return;
        if (worldCamera == null) return;

        Vector2 screenPos = eventData.position + wipeScreenOffset;

        Vector3 worldPos = ScreenToDustWorldPosition(screenPos);

        dustWipeController.WipeAtWorldPosition(worldPos);
    }

    private Vector3 ScreenToDustWorldPosition(Vector2 screenPos)
    {
        if (dustWipeController == null)
        {
            return Vector3.zero;
        }

        float zDistance = Mathf.Abs(
            worldCamera.transform.position.z - dustWipeController.transform.position.z
        );

        Vector3 screenPoint = new Vector3(
            screenPos.x,
            screenPos.y,
            zDistance
        );

        Vector3 worldPos = worldCamera.ScreenToWorldPoint(screenPoint);
        worldPos.z = dustWipeController.transform.position.z;

        return worldPos;
    }
}
