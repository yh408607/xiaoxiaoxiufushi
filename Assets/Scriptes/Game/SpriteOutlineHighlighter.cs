using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineHighlighter : MonoBehaviour
{
    [Header("描边设置")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private float outlineSize = 0.05f;
    [SerializeField] private int outlineSortingOrderOffset = -1;

    [Header("是否默认隐藏")]
    [SerializeField] private bool hideOnAwake = true;

    private SpriteRenderer mainRenderer;
    private readonly List<SpriteRenderer> outlineRenderers = new List<SpriteRenderer>();
    private bool isInitialized;

    private readonly Vector2[] directions =
    {
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(1, 1),
        new Vector2(1, -1),
        new Vector2(-1, 1),
        new Vector2(-1, -1)
    };

    private void Awake()
    {
        Init();

        if (hideOnAwake)
        {
            Hide();
        }
    }

    public void Init()
    {
        if (isInitialized) return;

        mainRenderer = GetComponent<SpriteRenderer>();

        for (int i = 0; i < directions.Length; i++)
        {
            GameObject outlineObj = new GameObject("Outline_" + i);
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = directions[i].normalized * outlineSize;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;

            SpriteRenderer sr = outlineObj.AddComponent<SpriteRenderer>();
            sr.sprite = mainRenderer.sprite;
            sr.color = outlineColor;
            sr.sortingLayerID = mainRenderer.sortingLayerID;
            sr.sortingOrder = mainRenderer.sortingOrder + outlineSortingOrderOffset;
            sr.flipX = mainRenderer.flipX;
            sr.flipY = mainRenderer.flipY;
            sr.drawMode = mainRenderer.drawMode;
            sr.maskInteraction = mainRenderer.maskInteraction;

            outlineRenderers.Add(sr);
        }

        isInitialized = true;
    }


    public void InitConfig(Color color, float size)
    {
        outlineColor = color;
        outlineSize = size;

        Init();
        SetColor(color);
        SetSize(size);
        Hide();
    }
    public void Show()
    {
        Init();
        Refresh();

        for (int i = 0; i < outlineRenderers.Count; i++)
        {
            if (outlineRenderers[i] != null)
            {
                outlineRenderers[i].gameObject.SetActive(true);
            }
        }
    }

    public void Hide()
    {
        for (int i = 0; i < outlineRenderers.Count; i++)
        {
            if (outlineRenderers[i] != null)
            {
                outlineRenderers[i].gameObject.SetActive(false);
            }
        }
    }

    public void Refresh()
    {
        if (mainRenderer == null)
        {
            mainRenderer = GetComponent<SpriteRenderer>();
        }

        for (int i = 0; i < outlineRenderers.Count; i++)
        {
            SpriteRenderer sr = outlineRenderers[i];

            if (sr == null) continue;

            sr.sprite = mainRenderer.sprite;
            sr.color = outlineColor;
            sr.sortingLayerID = mainRenderer.sortingLayerID;
            sr.sortingOrder = mainRenderer.sortingOrder + outlineSortingOrderOffset;
            sr.flipX = mainRenderer.flipX;
            sr.flipY = mainRenderer.flipY;
            sr.drawMode = mainRenderer.drawMode;
            sr.maskInteraction = mainRenderer.maskInteraction;
        }
    }

    public void SetColor(Color color)
    {
        outlineColor = color;
        Refresh();
    }

    public void SetSize(float size)
    {
        outlineSize = size;

        for (int i = 0; i < outlineRenderers.Count; i++)
        {
            if (outlineRenderers[i] != null)
            {
                outlineRenderers[i].transform.localPosition =
                    directions[i].normalized * outlineSize;
            }
        }
    }
}
