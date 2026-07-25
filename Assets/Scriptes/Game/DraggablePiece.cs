using UnityEditor;
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

    [Header("高亮")]
    [SerializeField] private SpriteOutlineHighlighter outlineHighlighter;
    [SerializeField] private bool highlightWhenDragging = true;


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

        if (outlineHighlighter == null)
        {
            outlineHighlighter = GetComponent<SpriteOutlineHighlighter>();
        }

        originPosition = transform.position;
    }

    public void Init(RepairSlot slot, Camera camera = null, SpriteOutlineHighlighter highlighter = null)
    {
        targetSlot = slot;

        if (camera != null)
        {
            dragCamera = camera;
        }

        if (highlighter != null)
        {
            outlineHighlighter = highlighter;
        }
    }

    private void OnMouseDown()
    {
        if (isCompleted) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
        isDragging = true;


        if (highlightWhenDragging && outlineHighlighter != null)
        {
            outlineHighlighter.Show();
        }

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

        if (highlightWhenDragging && outlineHighlighter != null)
        {
            outlineHighlighter.Hide();
        }

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

        if (highlightWhenDragging && outlineHighlighter != null)
        {
            outlineHighlighter.Hide();
        }

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
