using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DraggablePiece : MonoBehaviour
{
    [Header("目标插槽")]
    [SerializeField] private RepairSlot targetSlot;

    [Header("拖拽设置")]
    [SerializeField] private Camera dragCamera;
    [SerializeField] private bool returnToOriginWhenFailed = true;
    [SerializeField] private bool disableColliderWhenCompleted = true;

    [Header("层级设置")]
    [SerializeField] private float draggingZ = -1f;

    [Header("吸附动画")]
    [SerializeField] private float snapMoveDuration = 0.22f;
    [SerializeField] private float snapScalePunch = 1.08f;
    [SerializeField] private float snapScaleDuration = 0.08f;

    [Header("回退动画")]
    [SerializeField] private float returnMoveDuration = 0.28f;

    [Header("高亮")]
    [SerializeField] private SpriteOutlineHighlighter outlineHighlighter;
    [SerializeField] private bool highlightWhenDragging = true;

    [Header("手指引导")]
    [SerializeField] private FingerDragUI fingerDragUI;
    [SerializeField] private bool showFingerGuideOnLevelStart = true;
    [SerializeField] private float showGuideDelay = 0.2f; // 可选，等一帧布局稳定


    private Collider2D selfCollider;
    private Vector3 originPosition;
    private Vector3 originalScale;
    private Vector3 dragOffset;

    private bool isDragging;
    private bool isCompleted;
    private bool isAnimating;

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
        originalScale = transform.localScale;
    }
    private void Start()
    {
        if (fingerDragUI == null)
        {
            fingerDragUI = FindObjectOfType<FingerDragUI>(true);
        }

        if (showFingerGuideOnLevelStart && !isCompleted && fingerDragUI != null)
        {
            Invoke(nameof(ShowStartGuideFinger), showGuideDelay);
        }
    }

    private void ShowStartGuideFinger()
    {
        if (isCompleted || fingerDragUI == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // 显示在当前碎片位置（你也可以换成 targetSlot 位置）
        fingerDragUI.ShowGuideAtWorldPosition(transform.position, cam);
    }

    public void Init(
        RepairSlot slot,
        Camera camera = null,
        SpriteOutlineHighlighter highlighter = null
    )
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

        originPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        if (isCompleted) return;
        if (isAnimating) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
        isDragging = true;

        if (highlightWhenDragging && outlineHighlighter != null)
        {
            outlineHighlighter.Show();
        }

        Vector3 pos = transform.position;
        pos.z = draggingZ;
        transform.position = pos;

        if (fingerDragUI != null)
        {
            fingerDragUI.ShowAtScreenPosition(GetPointerScreenPosition());
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        if (isCompleted) return;
        if (isAnimating) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 targetPos = mouseWorldPos + dragOffset;

        targetPos.z = draggingZ;
        transform.position = targetPos;

        if (fingerDragUI != null)
        {
            fingerDragUI.FollowScreenPosition(GetPointerScreenPosition());
        }
    }

    private void OnMouseUp()
    {
        if (isCompleted) return;
        if (isAnimating) return;

        isDragging = false;

        if (highlightWhenDragging && outlineHighlighter != null)
        {
            outlineHighlighter.Hide();
        }

        if (targetSlot != null && targetSlot.CanRepair(this))
        {
            StartCoroutine(SnapToSlotRoutine());
        }
        else
        {
            if (returnToOriginWhenFailed)
            {
                StartCoroutine(ReturnToOriginRoutine());
            }
        }

        if(fingerDragUI!=null)
        {
            fingerDragUI.Hide();
        }
    }

    private IEnumerator SnapToSlotRoutine()
    {
        isAnimating = true;

        if (selfCollider != null)
        {
            selfCollider.enabled = false;
        }

        Vector3 startPos = transform.position;

        Vector3 endPos = targetSlot.SnapPosition;
        endPos.z = startPos.z;

        yield return MoveRoutine(startPos, endPos, snapMoveDuration);

        // 镶嵌感：轻微放大
        yield return ScaleRoutine(originalScale, originalScale * snapScalePunch, snapScaleDuration);

        // 回到原大小
        yield return ScaleRoutine(originalScale * snapScalePunch, originalScale, snapScaleDuration);

        CompleteRepair();

        isAnimating = false;
    }

    private IEnumerator ReturnToOriginRoutine()
    {
        isAnimating = true;

        if (selfCollider != null)
        {
            selfCollider.enabled = false;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = originPosition;

        yield return MoveRoutine(startPos, endPos, returnMoveDuration);

        transform.position = originPosition;
        transform.localScale = originalScale;

        if (!isCompleted && selfCollider != null)
        {
            selfCollider.enabled = true;
        }

        isAnimating = false;
    }

    private IEnumerator MoveRoutine(Vector3 startPos, Vector3 endPos, float duration)
    {
        if (duration <= 0f)
        {
            transform.position = endPos;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            t = EaseOutCubic(t);

            transform.position = Vector3.LerpUnclamped(startPos, endPos, t);

            yield return null;
        }

        transform.position = endPos;
    }

    private IEnumerator ScaleRoutine(Vector3 startScale, Vector3 endScale, float duration)
    {
        if (duration <= 0f)
        {
            transform.localScale = endScale;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            t = EaseOutCubic(t);

            transform.localScale = Vector3.LerpUnclamped(startScale, endScale, t);

            yield return null;
        }

        transform.localScale = endScale;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private void CompleteRepair()
    {
        isCompleted = true;

        if (highlightWhenDragging && outlineHighlighter != null)
        {
            outlineHighlighter.Hide();
        }

        transform.localScale = originalScale;

        if (targetSlot != null)
        {
            targetSlot.Repair(this);
        }

        if (disableColliderWhenCompleted && selfCollider != null)
        {
            selfCollider.enabled = false;
        }

        //if (fingerDragUI != null)
        //{
        //    fingerDragUI.Hide();
        //}
    }

    public void HidePiece()
    {
        gameObject.SetActive(false);
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        Vector3 mouseScreenPos = Input.mousePosition;

        float zDistance = 0f;

        if (dragCamera != null)
        {
            zDistance = Mathf.Abs(dragCamera.transform.position.z - transform.position.z);
        }

        mouseScreenPos.z = zDistance;

        Vector3 worldPos = dragCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = transform.position.z;

        return worldPos;
    }

    private Vector2 GetPointerScreenPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition;
#else
    if (Input.touchCount > 0)
    {
        return Input.GetTouch(0).position;
    }
    return Input.mousePosition;
#endif
    }

}
