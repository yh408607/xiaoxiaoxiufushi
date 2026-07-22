using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggablePiece : MonoBehaviour
{
    [Header("拖拽配置")]
    [SerializeField] private Camera dragCamera;
    [SerializeField] private bool returnToOriginWhenFailed = true;
    [SerializeField] private bool disableColliderWhenCompleted = true;

    [Header("目标插槽")]
    [SerializeField] private RepairSlot targetSlot;

    private Vector3 originPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private bool isCompleted;

    private Collider2D selfCollider;

    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();

        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        originPosition = transform.position;
    }

    public void Init(RepairSlot slot, Camera camera = null)
    {
        targetSlot = slot;

        if (camera != null)
        {
            dragCamera = camera;
        }
    }

    private void OnMouseDown()
    {
        if (isCompleted) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
        isDragging = true;

        // 拖拽物体显示在前面一点，避免被遮挡
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;
    }

    private void OnMouseDrag()
    {
        if (isCompleted || !isDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 targetPos = mouseWorldPos + dragOffset;

        targetPos.z = transform.position.z;
        transform.position = targetPos;
    }

    private void OnMouseUp()
    {
        if (isCompleted) return;

        isDragging = false;

        if (targetSlot != null && targetSlot.CanRepair(this))
        {
            CompleteRepair();
        }
        else
        {
            if (returnToOriginWhenFailed)
            {
                transform.position = originPosition;
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;

        float distanceToCamera = Mathf.Abs(dragCamera.transform.position.z - transform.position.z);
        mouseScreenPos.z = distanceToCamera;

        return dragCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    private void CompleteRepair()
    {
        isCompleted = true;

        if (targetSlot != null)
        {
            targetSlot.Repair(this);
        }

        if (disableColliderWhenCompleted && selfCollider != null)
        {
            selfCollider.enabled = false;
        }
    }

    public void HidePiece()
    {
        gameObject.SetActive(false);
    }
}
