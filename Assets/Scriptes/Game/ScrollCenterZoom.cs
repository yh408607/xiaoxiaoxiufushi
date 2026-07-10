using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollCenterZoom : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;
    public Transform itemRoot; // 可选：不填则用 content

    [Header("Scale Settings")]
    [Range(0.2f, 1f)] public float minScale = 0.8f;
    [Range(1f, 2f)] public float maxScale = 1.2f;
    public float scaleDistance = 300f; // 距离中心多少像素内开始明显放大
    public float lerpSpeed = 12f;

    [Header("Snap Settings")]
    public bool snapToCenter = true;
    public float snapSpeed = 10f;
    public float snapThreshold = 0.001f; // 停止阈值（anchoredPosition 差值）

    private readonly List<RectTransform> items = new List<RectTransform>();
    private bool isDragging = false;
    private int targetIndex = -1;

    void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            content = scrollRect.content;
            if (scrollRect.viewport != null) viewport = scrollRect.viewport;
        }
    }

    void Start()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (content == null && scrollRect != null) content = scrollRect.content;
        if (viewport == null && scrollRect != null) viewport = scrollRect.viewport;

        CollectItems();
        UpdateScaleImmediate();
    }

    void Update()
    {
        if (items.Count == 0 || content == null || viewport == null) return;

        UpdateScaleLerp();

        if (snapToCenter && !isDragging)
        {
            if (targetIndex < 0) targetIndex = GetNearestToCenterIndex();
            SnapToItem(targetIndex);
        }
    }

    public void RefreshItems()
    {
        CollectItems();
        targetIndex = -1;
        UpdateScaleImmediate();
    }

    private void CollectItems()
    {
        items.Clear();
        Transform root = itemRoot != null ? itemRoot : content;

        for (int i = 0; i < root.childCount; i++)
        {
            var rt = root.GetChild(i) as RectTransform;
            if (rt != null && rt.gameObject.activeInHierarchy)
                items.Add(rt);
        }
    }

    private float GetViewportCenterXInWorld()
    {
        Vector3[] corners = new Vector3[4];
        viewport.GetWorldCorners(corners);
        return (corners[0].x + corners[3].x) * 0.5f;
    }

    private float GetItemCenterXInWorld(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0].x + corners[3].x) * 0.5f;
    }

    private void UpdateScaleImmediate()
    {
        float centerX = GetViewportCenterXInWorld();

        foreach (var item in items)
        {
            float dist = Mathf.Abs(GetItemCenterXInWorld(item) - centerX);
            float t = Mathf.Clamp01(dist / scaleDistance);
            float scale = Mathf.Lerp(maxScale, minScale, t);
            item.localScale = Vector3.one * scale;
        }
    }

    private void UpdateScaleLerp()
    {
        float centerX = GetViewportCenterXInWorld();

        foreach (var item in items)
        {
            float dist = Mathf.Abs(GetItemCenterXInWorld(item) - centerX);
            float t = Mathf.Clamp01(dist / scaleDistance);
            float targetScale = Mathf.Lerp(maxScale, minScale, t);
            Vector3 target = Vector3.one * targetScale;
            item.localScale = Vector3.Lerp(item.localScale, target, Time.deltaTime * lerpSpeed);
        }
    }

    private int GetNearestToCenterIndex()
    {
        float centerX = GetViewportCenterXInWorld();
        float minDist = float.MaxValue;
        int idx = 0;

        for (int i = 0; i < items.Count; i++)
        {
            float dist = Mathf.Abs(GetItemCenterXInWorld(items[i]) - centerX);
            if (dist < minDist)
            {
                minDist = dist;
                idx = i;
            }
        }
        return idx;
    }

    private void SnapToItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        // 计算 item 中心与 viewport 中心的世界坐标偏移，再转成 content 的 anchoredPosition.x 偏移
        float viewportCenterX = GetViewportCenterXInWorld();
        float itemCenterX = GetItemCenterXInWorld(items[index]);
        float deltaWorld = viewportCenterX - itemCenterX;

        Vector2 pos = content.anchoredPosition;
        Vector2 targetPos = new Vector2(pos.x + deltaWorld, pos.y);

        // 可选边界限制（防止超出 content）
        targetPos = ClampContentPos(targetPos);

        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPos, Time.deltaTime * snapSpeed);

        if (Vector2.SqrMagnitude(content.anchoredPosition - targetPos) < snapThreshold * snapThreshold)
        {
            content.anchoredPosition = targetPos;
        }
    }

    private Vector2 ClampContentPos(Vector2 pos)
    {
        // 对于大部分水平 ScrollRect（pivot 一般在左上/中），这个限制可避免吸附越界。
        // 若你的锚点/pivot 特殊，可自行调整此函数。
        if (!scrollRect.horizontal) return pos;

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        // content 比 viewport 小时，不需要滚动
        if (contentWidth <= viewportWidth) return new Vector2(0f, pos.y);

        // 下面是常见设置下的范围估算：
        // 左边界：0，右边界：contentWidth - viewportWidth（取决于锚点方向，必要时取反）
        float minX = -(contentWidth - viewportWidth);
        float maxX = 0f;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        return pos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        targetIndex = -1;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        targetIndex = GetNearestToCenterIndex();
    }
}
