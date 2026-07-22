using UnityEngine;
using System;

public class RepairSlot : MonoBehaviour
{
    [Header("匹配配置")]
    [SerializeField] private string requiredPieceId;
    [SerializeField] private float snapDistance = 0.5f;

    [Header("显示对象")]
    [SerializeField] private GameObject graySlotObject;
    [SerializeField] private GameObject repairedImageObject;

    [Header("吸附设置")]
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

        if (repairedImageObject != null)
        {
            repairedImageObject.SetActive(false);
        }
    }

    public void Init(string requiredId,float distance,GameObject grayObject,GameObject repairedObject,Transform point)
    {
        requiredPieceId = requiredId;
        snapDistance = distance;
        graySlotObject = grayObject;
        repairedImageObject = repairedObject;
        snapPoint = point;

        if (repairedImageObject != null)
        {
            repairedImageObject.SetActive(false);
        }
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

        // 先吸附到指定位置
        piece.transform.position = new Vector3(
            snapPoint.position.x,
            snapPoint.position.y,
            piece.transform.position.z
        );

        // 隐藏拖拽物
        piece.HidePiece();

        // 隐藏灰色缺口
        if (graySlotObject != null)
        {
            graySlotObject.SetActive(false);
        }

        // 显示修复后的图片
        if (repairedImageObject != null)
        {
            repairedImageObject.SetActive(true);
        }

        OnRepaired?.Invoke(this);
    }
}
