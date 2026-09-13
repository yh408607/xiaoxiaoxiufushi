using System;
using UnityEngine;

/// <summary>
/// 拼接关卡的通用擦拭层控制器。
/// 每个擦拭阶段拥有独立的遮罩、材质和完成比例。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FragmentWipeController : MonoBehaviour
{
    [SerializeField] private Material wipeMaterial;
    [SerializeField] private int maskTextureSize = 512;
    [SerializeField] private float brushSize = 0.08f;
    [SerializeField, Range(0f, 1f)] private float completePercent = 0.95f;
    [SerializeField] private float checkInterval = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Material runtimeMaterial;
    private RenderTexture maskRenderTexture;
    private Texture2D readableMaskTexture;
    private Texture2D brushTexture;
    private bool isWipingEnabled;
    private bool isCompleted;
    private float checkTimer;

    public bool IsCompleted => isCompleted;
    public bool IsWipingEnabled => isWipingEnabled;
    public event Action OnWipeCompleted;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(Material material, float size, float percent)
    {
        wipeMaterial = material;
        brushSize = Mathf.Max(0.001f, size);
        completePercent = Mathf.Clamp01(percent);
        InitializeMask();
    }

    private void InitializeMask()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (wipeMaterial == null)
        {
            Shader shader = Shader.Find("Custom/DustWipe");
            if (shader == null)
            {
                Debug.LogError(
                    "FragmentWipeController：找不到 Custom/DustWipe Shader。"
                );
                return;
            }

            wipeMaterial = new Material(shader);
        }

        ReleaseMaskResources();
        runtimeMaterial = new Material(wipeMaterial);
        maskRenderTexture = new RenderTexture(
            maskTextureSize,
            maskTextureSize,
            0,
            RenderTextureFormat.ARGB32
        );
        maskRenderTexture.Create();

        readableMaskTexture = new Texture2D(
            maskTextureSize,
            maskTextureSize,
            TextureFormat.RGBA32,
            false
        );

        ClearMask();
        runtimeMaterial.SetTexture("_MaskTex", maskRenderTexture);
        spriteRenderer.material = runtimeMaterial;
        isWipingEnabled = false;
        isCompleted = false;
        checkTimer = 0f;
    }

    public void EnableWiping()
    {
        if (isCompleted) return;
        isWipingEnabled = true;
        spriteRenderer.enabled = true;
    }

    public void DisableWiping()
    {
        isWipingEnabled = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void WipeAtWorldPosition(Vector3 worldPosition)
    {
        if (!isWipingEnabled || isCompleted || spriteRenderer.sprite == null)
        {
            return;
        }

        if (!TryWorldToSpriteUV(worldPosition, out Vector2 uv))
        {
            return;
        }

        DrawBrushToMask(uv);

        // 刷子和抹布共用旧流程中的擦拭音效，并避免拖动时重复叠加播放。
        SfxManager.Instance?.PlayIfNotPlaying(SfxId.DustWipe, 0.8f);
    }

    private void Update()
    {
        if (!isWipingEnabled || isCompleted) return;

        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return;

        checkTimer = 0f;
        if (GetWipedPercent() >= completePercent)
        {
            CompleteWipe();
        }
    }

    private bool TryWorldToSpriteUV(
        Vector3 worldPosition,
        out Vector2 uv)
    {
        uv = Vector2.zero;
        Vector3 localPosition =
            transform.InverseTransformPoint(worldPosition);
        Bounds bounds = spriteRenderer.sprite.bounds;

        if (!bounds.Contains(localPosition))
        {
            return false;
        }

        uv = new Vector2(
            Mathf.InverseLerp(bounds.min.x, bounds.max.x, localPosition.x),
            Mathf.InverseLerp(bounds.min.y, bounds.max.y, localPosition.y)
        );
        return true;
    }

    private void DrawBrushToMask(Vector2 uv)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, maskTextureSize, maskTextureSize, 0);

        int centerX = Mathf.RoundToInt(uv.x * maskTextureSize);
        int centerY = Mathf.RoundToInt((1f - uv.y) * maskTextureSize);
        float radius = brushSize * maskTextureSize;

        Graphics.DrawTexture(
            new Rect(
                centerX - radius,
                centerY - radius,
                radius * 2f,
                radius * 2f
            ),
            GetBrushTexture()
        );

        GL.PopMatrix();
        RenderTexture.active = previous;
    }

    private float GetWipedPercent()
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;
        readableMaskTexture.ReadPixels(
            new Rect(0, 0, maskTextureSize, maskTextureSize),
            0,
            0
        );
        readableMaskTexture.Apply();
        RenderTexture.active = previous;

        Color32[] pixels = readableMaskTexture.GetPixels32();
        int wipedCount = 0;
        foreach (Color32 pixel in pixels)
        {
            if (pixel.r > 20) wipedCount++;
        }

        return (float)wipedCount / pixels.Length;
    }

    private Texture2D GetBrushTexture()
    {
        if (brushTexture != null) return brushTexture;

        const int size = 128;
        brushTexture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    center
                );
                float alpha = Mathf.Clamp01(1f - distance / radius);
                brushTexture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        brushTexture.Apply();
        return brushTexture;
    }

    private void CompleteWipe()
    {
        isCompleted = true;
        isWipingEnabled = false;
        spriteRenderer.enabled = false;
        OnWipeCompleted?.Invoke();
    }

    private void ClearMask()
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = previous;
    }

    private void ReleaseMaskResources()
    {
        if (maskRenderTexture != null)
        {
            maskRenderTexture.Release();
            Destroy(maskRenderTexture);
            maskRenderTexture = null;
        }

        if (readableMaskTexture != null)
        {
            Destroy(readableMaskTexture);
            readableMaskTexture = null;
        }
    }

    private void OnDestroy()
    {
        ReleaseMaskResources();
        if (runtimeMaterial != null) Destroy(runtimeMaterial);
        if (brushTexture != null) Destroy(brushTexture);
    }
}
