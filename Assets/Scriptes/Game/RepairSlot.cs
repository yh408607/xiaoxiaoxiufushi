using UnityEngine;
using System;

public class RepairSlot : MonoBehaviour
{
    [Header("匹配配置")]
    [SerializeField] private string requiredPieceId;
    [SerializeField] private float snapDistance = 0.5f;

    [Header("灰色遮挡对象")]
    [SerializeField] private GameObject graySlotObject;

    [Header("吸附点")]
    [SerializeField] private Transform snapPoint;

    private bool isRepaired;

    public bool IsRepaired => isRepaired;

    public event Action<RepairSlot> OnRepaired;

    private void Awake()
    {
        if (graySlotObject == null)
        {
            graySlotObject = gameObject;
        }

        if (snapPoint == null)
        {
            snapPoint = transform;
        }
    }

    public void Init(
        string requiredId,
        float distance,
        GameObject grayObject,
        Transform point
    )
    {
        requiredPieceId = requiredId;
        snapDistance = distance;
        graySlotObject = grayObject;
        snapPoint = point;
    }

    public bool CanRepair(DraggablePiece piece)
    {
        if (isRepaired) return false;
        if (piece == null) return false;

        PieceIdentity identity = piece.GetComponent<PieceIdentity>();

        if (!string.IsNullOrEmpty(requiredPieceId))
        {
            if (identity == null) return false;
            if (identity.PieceId != requiredPieceId) return false;
        }

        float distance = Vector2.Distance(piece.transform.position, snapPoint.position);

        return distance <= snapDistance;
    }

    public void Repair(DraggablePiece piece)
    {
        if (isRepaired) return;
        if (!CanRepair(piece)) return;

        isRepaired = true;

        // 先把拖拽图吸附到灰色底图位置
        piece.transform.position = new Vector3(
            snapPoint.position.x,
            snapPoint.position.y,
            piece.transform.position.z
        );

        // 隐藏拖拽图
        piece.HidePiece();

        // 隐藏灰色遮挡图，让下面完整底图露出来
        if (graySlotObject != null)
        {
            graySlotObject.SetActive(false);
        }

        OnRepaired?.Invoke(this);
    }
}
