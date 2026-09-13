using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碎片拼接关卡数据。
/// 使用 ScriptableObject 保存，便于在 Unity 编辑器中创建和配置关卡。
/// </summary>
[CreateAssetMenu(
    fileName = "FragmentAssemblyLevelData",
    menuName = "Repair Game/Fragment Assembly Level Data"
)]
public class FragmentAssemblyLevelData : ScriptableObject
{
    [Header("关卡基础信息")]
    public string levelName = "New Fragment Assembly Level";

    [Header("编辑器参考图")]
    [Tooltip("仅用于编辑器中对齐碎片，运行时默认不会显示。")]
    public Sprite backgroundSprite;

    [Header("底图变换")]
    public Vector3 backgroundPosition = Vector3.zero;
    public Vector3 backgroundScale = Vector3.one;

    [Header("底图显示层级")]
    public int backgroundSortingOrder = -10;

    [Tooltip("是否在运行时显示完整底图。正常拼接关卡应保持关闭，避免提前看到完整文物。")]
    public bool showBackgroundAtRuntime = false;

    [Header("运行时拼接线框")]
    [Tooltip("玩家拼接时显示的完整文物线框图。全部拼接完成后自动隐藏。")]
    public Sprite wireframeSprite;

    [Tooltip("线框图位置，通常与完整参考图保持一致。")]
    public Vector3 wireframePosition = Vector3.zero;

    [Tooltip("线框图缩放，通常与完整参考图保持一致。")]
    public Vector3 wireframeScale = Vector3.one;

    public int wireframeSortingOrder = -5;

    [Header("刷子除尘阶段")]
    [Tooltip("拼接完成后覆盖在文物表面的灰尘图片。")]
    public Sprite dustLayerSprite;

    [Tooltip("灰尘层的显示层级，应高于文物底图。")]
    public int dustLayerSortingOrder = 10;

    [Tooltip("刷子擦除比例达到该值后进入毛巾阶段。")]
    [Range(0f, 1f)]
    public float dustCompletePercent = 0.95f;

    [Tooltip("刷子的擦拭范围大小。")]
    public float brushSize = 0.08f;

    [Header("毛巾擦亮阶段")]
    [Tooltip("毛巾阶段覆盖在文物表面的污渍或暗层图片。")]
    public Sprite polishLayerSprite;

    [Tooltip("污渍层的显示层级，应高于文物底图和灰尘层。")]
    public int polishLayerSortingOrder = 11;

    [Tooltip("毛巾擦除比例达到该值后完成关卡。")]
    [Range(0f, 1f)]
    public float polishCompletePercent = 0.95f;

    [Tooltip("毛巾的擦拭范围大小。")]
    public float towelSize = 0.08f;

    [Header("评分时间")]
    [Tooltip("总用时不超过该值时获得三星。")]
    public float threeStarTime = 60f;

    [Tooltip("总用时不超过该值时获得二星。")]
    public float twoStarTime = 120f;

    [Header("碎片列表")]
    public List<FragmentAssemblyPieceData> pieces =
        new List<FragmentAssemblyPieceData>();
}

/// <summary>
/// 单个文物碎片的数据。
/// 每个碎片都有一个初始位置和一个最终拼接位置。
/// </summary>
[Serializable]
public class FragmentAssemblyPieceData
{
    [Header("碎片标识")]
    public string id = "fragment_01";

    [Header("碎片图片")]
    public Sprite sprite;

    [Header("碎片初始状态")]
    public Vector3 startPosition = Vector3.zero;
    public float startRotation = 0f;

    [Header("碎片目标状态")]
    public Vector3 targetPosition = Vector3.zero;
    public float targetRotation = 0f;

    [Header("显示层级")]
    public int sortingOrder = 0;

    [Header("拼接判定")]
    [Tooltip("碎片距离目标位置小于该值时，允许自动吸附。")]
    public float snapDistance = 0.35f;

    [Tooltip("碎片最终吸附时使用的目标旋转角度。")]
    public float rotationTolerance = 12f;

    [Header("碰撞器类型")]
    [Tooltip("不规则碎片使用 PolygonCollider2D，规则碎片可以使用 BoxCollider2D。")]
    public bool usePolygonCollider = true;
}

/// <summary>
/// 碎片拼接关卡的运行阶段。
/// </summary>
public enum FragmentAssemblyStage
{
    /// <summary>
    /// 玩家正在拼接文物碎片。
    /// </summary>
    Assembly,

    /// <summary>
    /// 玩家使用刷子清除灰尘。
    /// </summary>
    DustBrushing,

    /// <summary>
    /// 玩家使用毛巾擦亮文物。
    /// </summary>
    TowelPolishing,

    /// <summary>
    /// 所有阶段完成。
    /// </summary>
    Completed
}
