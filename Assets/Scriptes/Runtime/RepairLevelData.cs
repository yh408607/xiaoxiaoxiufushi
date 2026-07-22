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

    [Header("背景")]
    public Sprite backgroundSprite;
    public Vector3 backgroundPosition = Vector3.zero;
    public int backgroundSortingOrder = -10;

    [Header("修复点列表")]
    public List<RepairPointData> repairPoints = new List<RepairPointData>();
}

[Serializable]
public class RepairPointData
{
    [Header("基础")]
    public string id = "piece_01";

    [Header("图片")]
    public Sprite graySlotSprite;
    public Sprite repairedSprite;
    public Sprite dragPieceSprite;

    [Header("位置")]
    public Vector3 slotPosition;
    public Vector3 dragStartPosition;

    [Header("显示层级")]
    public int slotSortingOrder = 0;
    public int repairedSortingOrder = -1;
    public int dragSortingOrder = 5;

    [Header("吸附")]
    public float snapDistance = 0.5f;

    [Header("碰撞器")]
    public bool addPolygonCollider  = true;
}
