using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "RepairLevelData",
    menuName = "Repair Game/Repair Level Data"
)]
public class RepairLevelData : ScriptableObject
{
    [Header("关卡基础信息")]
    public string levelName = "New Repair Level";

    [Header("底图公共位置")]
    public Vector3 backgroundPosition = Vector3.zero;
    public Vector3 backgroundScale = Vector3.one;

    [Header("修复阶段底图")]
    public Sprite repairBackgroundSprite;
    public int repairBackgroundSortingOrder = -10;

    [Header("干净底图")]
    public Sprite cleanBackgroundSprite;
    public int cleanBackgroundSortingOrder = -10;

    [Header("灰尘擦拭")]
    public Sprite dustSprite;
    public int dustSortingOrder = 10;


    [Range(0f, 1f)]
    public float wipeCompletePercent = 0.9f;

    public float wipeBrushSize = 0.08f;

    [Header("评分时间")]
    public float threeStarTime = 60f;
    public float twoStarTime = 120f;


    [Header("修复点列表")]
    public List<RepairPointData> repairPoints = new List<RepairPointData>();


}

public enum RepairColliderType
{
    None,
    Box,
    Polygon
}

[Serializable]
public class RepairPointData
{
    [Header("基础")]
    public string id = "piece_01";

    [Header("图片")]
    public Sprite graySlotSprite;
    public Sprite dragPieceSprite;

    [Header("位置")]
    public Vector3 slotPosition;
    public Vector3 dragStartPosition;

    [Header("显示层级")]
    public int slotSortingOrder = 0;
    public int dragSortingOrder = 5;

    [Header("吸附")]
    public float snapDistance = 0.5f;

    [Header("碰撞器")]
    public RepairColliderType colliderType = RepairColliderType.Polygon;

    [Header("拖拽高亮")]
    public bool enableOutline = true;
    public Color outlineColor = Color.yellow;
    public float outlineSize = 0.05f;
}
