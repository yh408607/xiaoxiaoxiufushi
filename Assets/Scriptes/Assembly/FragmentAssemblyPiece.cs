using System;
using UnityEngine;

/// <summary>
/// 文物碎片运行时控制器。
/// 挂载在每一个可拖拽的碎片对象上。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FragmentAssemblyPiece : MonoBehaviour
{
    [Header("拖拽相机")]
    [SerializeField] private Camera dragCamera;

    [Header("手指引导")]
    [Tooltip("拖动碎片时跟随鼠标或触摸点移动的 UI 手指图标。")]
    [SerializeField] private FingerDragUI fingerDragUI;

    [Header("拖拽设置")]
    [Tooltip("拖拽时碎片使用的 Z 坐标。")]
    [SerializeField] private float draggingZ = -1f;

    [Header("失败处理")]
    [Tooltip("拼接失败后，碎片是否回到初始位置和初始旋转。")]
    [SerializeField] private bool returnToStartWhenFailed = true;

    [Header("诊断日志")]
    [Tooltip("开启后在 Console 输出碎片拖拽和吸附判定过程。")]
    [SerializeField] private bool enableDebugLogs = true;

    private string pieceId;

    // 碎片开始拖拽前的位置和旋转。
    private Vector3 startPosition;
    private float startRotation;

    // 碎片正确拼接时的位置和旋转。
    private Vector3 targetPosition;
    private float targetRotation;

    // 位置和旋转的允许误差。
    private float snapDistance;
    private float rotationTolerance;

    private Collider2D pieceCollider;
    private Vector3 dragOffset;

    private bool isDragging;
    private bool isFixed;
    private float nextDragLogTime;
    private bool cursorVisibleBeforeDrag;

    /// <summary>
    /// 碎片唯一标识。
    /// </summary>
    public string PieceId => pieceId;

    /// <summary>
    /// 当前碎片是否已经固定。
    /// </summary>
    public bool IsFixed => isFixed;

    /// <summary>
    /// 碎片固定成功时触发。
    /// </summary>
    public event Action<FragmentAssemblyPiece> OnFixed;

    private void Awake()
    {
        pieceCollider = GetComponent<Collider2D>();

        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }
    }

    /// <summary>
    /// 初始化碎片运行时数据。
    /// </summary>
    public void Init(
        string id,
        Vector3 initialPosition,
        float initialRotation,
        Vector3 correctPosition,
        float correctRotation,
        float positionTolerance,
        float angleTolerance,
        Camera camera,
        FingerDragUI finger)
    {
        pieceId = id;

        startPosition = initialPosition;
        startRotation = initialRotation;

        targetPosition = correctPosition;
        targetRotation = correctRotation;

        snapDistance = Mathf.Max(0f, positionTolerance);
        rotationTolerance = Mathf.Max(0f, angleTolerance);

        if (camera != null)
        {
            dragCamera = camera;
        }

        if (finger != null)
        {
            fingerDragUI = finger;
        }

        // 初始化时，将碎片放到配置的初始位置和角度。
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, startRotation);

        //DebugLog(
        //    "初始化完成。"
        //    + " start=" + startPosition
        //    + " target=" + targetPosition
        //    + " startRotation=" + startRotation
        //    + " targetRotation=" + targetRotation
        //    + " snapDistance=" + snapDistance
        //    + " rotationTolerance=" + rotationTolerance
        //    + " collider=" + (pieceCollider != null)
        //    + " colliderEnabled="
        //    + (pieceCollider != null && pieceCollider.enabled)
        //    + " camera=" + (dragCamera != null)
        //);
    }

    private void OnMouseDown()
    {
        if (isFixed)
        {
           // DebugLog("忽略按下：碎片已经固定。");
            return;
        }

        //DebugLog(
        //    "OnMouseDown 触发。"
        //    + " mouseScreen=" + Input.mousePosition
        //    + " world=" + GetPointerWorldPosition()
        //);

        Vector3 pointerWorldPosition = GetPointerWorldPosition();

        // 保留鼠标点击点和碎片中心之间的偏移，
        // 避免点击碎片边缘时碎片瞬间跳到鼠标中心。
        dragOffset = transform.position - pointerWorldPosition;
        isDragging = true;
        nextDragLogTime = 0f;

        // 拖拽时隐藏系统鼠标，避免与 UI 手指图标重复显示。
        cursorVisibleBeforeDrag = Cursor.visible;
        Cursor.visible = false;

        if (fingerDragUI != null)
        {
            fingerDragUI.ShowAtScreenPosition(
                GetPointerScreenPosition()
            );
        }

        PlayPickFeedback();
    }

    private void OnMouseDrag()
    {
        if (!isDragging || isFixed)
        {
            return;
        }

        Vector3 nextPosition = GetPointerWorldPosition() + dragOffset;
        nextPosition.z = draggingZ;

        transform.position = nextPosition;

        if (Time.unscaledTime >= nextDragLogTime)
        {
            //DebugLog(
            //    "OnMouseDrag 触发。"
            //    + " pointerWorld=" + GetPointerWorldPosition()
            //    + " piecePosition=" + transform.position
            //);

            nextDragLogTime = Time.unscaledTime + 0.2f;
        }

        if (fingerDragUI != null)
        {
            fingerDragUI.FollowScreenPosition(
                GetPointerScreenPosition()
            );
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging || isFixed)
        {
            //DebugLog(
            //    "忽略 OnMouseUp。"
            //    + " isDragging=" + isDragging
            //    + " isFixed=" + isFixed
            //);
            return;
        }

        isDragging = false;

        //DebugLog(
        //    "OnMouseUp 触发。"
        //    + " currentPosition=" + transform.position
        //    + " currentRotation=" + transform.eulerAngles.z
        //);

        if (fingerDragUI != null)
        {
            fingerDragUI.Hide();
        }

        Cursor.visible = cursorVisibleBeforeDrag;
        TryCompleteAssembly();
    }

    private void OnDisable()
    {
        // 场景切换或碎片被禁用时也要恢复系统鼠标。
        if (isDragging)
        {
            isDragging = false;
            Cursor.visible = cursorVisibleBeforeDrag;
        }
    }

    /// <summary>
    /// 获取当前鼠标或触摸点的屏幕坐标。
    /// Unity 的 Input.mousePosition 同时适用于鼠标和单指触摸。
    /// </summary>
    private Vector2 GetPointerScreenPosition()
    {
        return Input.mousePosition;
    }

    /// <summary>
    /// 检查碎片是否到达正确的位置和角度。
    /// </summary>
    private void TryCompleteAssembly()
    {
        float positionDistance = Vector2.Distance(
            transform.position,
            targetPosition
        );

        // 先完整输出位置和旋转误差，再分别判断失败原因。
        float angleDistance = Mathf.Abs(
            Mathf.DeltaAngle(
                transform.eulerAngles.z,
                targetRotation
            )
        );

        //DebugLog(
        //    "开始吸附判定。"
        //    + " currentPosition=" + transform.position
        //    + " targetPosition=" + targetPosition
        //    + " positionDistance=" + positionDistance
        //    + "/" + snapDistance
        //    + " currentRotation=" + transform.eulerAngles.z
        //    + " targetRotation=" + targetRotation
        //    + " angleDistance=" + angleDistance
        //    + "/" + rotationTolerance
        //);

        // 位置距离不符合时，拼接失败。
        if (positionDistance > snapDistance)
        {
           // DebugLog("吸附失败：距离目标位置过远。");
            HandleAssemblyFailed();
            return;
        }

        // DeltaAngle 可以正确处理 0 度和 360 度之间的角度差。
        // 旋转角度不符合时，拼接失败。
        if (angleDistance > rotationTolerance)
        {
           // DebugLog("吸附失败：旋转角度超出容错范围。");
            HandleAssemblyFailed();
            return;
        }

        DebugLog("吸附判定成功，开始固定碎片。");
        CompleteAssembly();
    }

    /// <summary>
    /// 拼接成功后，将碎片吸附到精确的目标位置和角度。
    /// </summary>
    private void CompleteAssembly()
    {
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            targetRotation
        );

        isFixed = true;

        // 固定后关闭碰撞器，避免碎片再次被点击拖动。
        if (pieceCollider != null)
        {
            pieceCollider.enabled = false;
        }

        //DebugLog(
        //    "碎片固定完成。"
        //    + " finalPosition=" + transform.position
        //    + " finalRotation=" + transform.eulerAngles.z
        //);

        PlayCorrectFeedback();
        OnFixed?.Invoke(this);
    }

    /// <summary>
    /// 拼接失败后的处理。
    /// </summary>
    private void HandleAssemblyFailed()
    {
        if (returnToStartWhenFailed)
        {
            transform.position = startPosition;
            transform.rotation = Quaternion.Euler(
                0f,
                0f,
                startRotation
            );

            DebugLog(
                "吸附失败，碎片已返回初始位置："
                + startPosition
            );
        }

        PlayWrongFeedback();
    }

    /// <summary>
    /// 将屏幕鼠标位置转换为世界坐标。
    /// </summary>
    private Vector3 GetPointerWorldPosition()
    {
        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        if (dragCamera == null)
        {
            return transform.position;
        }

        Vector3 screenPosition = Input.mousePosition;

        // 根据相机和碎片之间的 Z 距离计算屏幕到世界的转换深度。
        screenPosition.z = Mathf.Abs(
            dragCamera.transform.position.z - transform.position.z
        );

        Vector3 worldPosition =
            dragCamera.ScreenToWorldPoint(screenPosition);

        worldPosition.z = draggingZ;
        return worldPosition;
    }

    /// <summary>
    /// 输出带有碎片对象名称和 ID 的诊断日志，便于定位具体碎片。
    /// </summary>
    private void DebugLog(string message)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log( "[FragmentAssemblyPiece] " + gameObject.name  + " [" + pieceId + "] " + message, this );
    }

    /// <summary>
    /// 预留拾取音效或动画。
    /// </summary>
    private void PlayPickFeedback()
    {
        // 后续可以在这里播放碎片被拿起的音效或缩放动画。
    }

    /// <summary>
    /// 预留正确拼接音效或动画。
    /// </summary>
    private void PlayCorrectFeedback()
    {
        // 后续可以在这里播放拼接成功音效或吸附动画。
        SfxManager.Instance?.Play(SfxId.PieceDropCorrect);

    }

    /// <summary>
    /// 预留错误拼接音效或动画。
    /// </summary>
    private void PlayWrongFeedback()
    {
        // 后续可以在这里播放拼接失败音效。
        SfxManager.Instance?.Play(SfxId.PieceDropWrong);
    }
}
