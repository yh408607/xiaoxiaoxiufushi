using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 拼接关卡的 UI 擦拭工具。
/// 刷子和毛巾都使用这个组件，通过 UI 指针事件擦除世界空间覆盖层。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FragmentAssemblyWipeUITool :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [SerializeField] private FragmentWipeController wipeController;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 wipeScreenOffset;

    private RectTransform rectTransform;
    private bool isEnabled;
    private bool isDragging;

    public bool IsEnabled => isEnabled;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (worldCamera == null) worldCamera = Camera.main;
        DisableTool();
    }

    public void Init(
        FragmentWipeController controller,
        Camera camera = null)
    {
        wipeController = controller;
        if (camera != null) worldCamera = camera;
        DisableTool();
    }

    public void EnableTool()
    {
        isDragging = false;
        gameObject.SetActive(true);
        // 对于场景初始为 inactive 的 UI，SetActive(true) 会先触发 Awake。
        // Awake 完成初始化后再设置启用状态，避免被 Awake 中的默认关闭逻辑覆盖。
        isEnabled = true;
        // Awake 中的 DisableTool 可能在首次激活期间将对象再次关闭，
        // 因此这里需要在初始化完成后再次确保工具对象处于激活状态。
        gameObject.SetActive(true);

        Debug.Log(
            "[FragmentAssemblyWipeUITool] 启用工具："
            + gameObject.name
            + "，activeSelf=" + gameObject.activeSelf,
            this
        );
    }

    public void DisableTool()
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
        if (!isEnabled || !isDragging) return;
        MoveToPointer(eventData);
        WipeAtPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (canvas == null || rectTransform.parent == null) return;

        RectTransform parent =
            rectTransform.parent as RectTransform;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            eventData.position,
            uiCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    private void WipeAtPointer(PointerEventData eventData)
    {
        if (wipeController == null || worldCamera == null) return;

        Vector2 screenPosition =
            eventData.position + wipeScreenOffset;
        float zDistance = Mathf.Abs(
            worldCamera.transform.position.z -
            wipeController.transform.position.z
        );

        Vector3 screenPoint = new Vector3(
            screenPosition.x,
            screenPosition.y,
            zDistance
        );

        Vector3 worldPosition =
            worldCamera.ScreenToWorldPoint(screenPoint);
        worldPosition.z = wipeController.transform.position.z;
        wipeController.WipeAtWorldPosition(worldPosition);
    }
}
