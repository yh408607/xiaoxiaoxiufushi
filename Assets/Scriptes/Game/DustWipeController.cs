using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class DustWipeController : MonoBehaviour
{
    [Header("≤ƒ÷ ")]
    [SerializeField] private Material dustWipeMaterial;

    [Header("≤¡ √…Ë÷√")]
    [SerializeField] private int maskTextureSize = 512;
    [SerializeField] private float brushSize = 0.08f;
    [SerializeField] private float completePercent = 0.9f;

    [Header("ºÏ≤‚…Ë÷√")]
    [SerializeField] private float checkInterval = 0.2f;

    private SpriteRenderer spriteRenderer;
    private RenderTexture maskRenderTexture;
    private Texture2D readableMaskTexture;
    private Material runtimeMaterial;

    private bool isWipingEnabled;
    private bool isCompleted;
    private float checkTimer;

    public event Action OnWipeCompleted;

    public bool IsCompleted => isCompleted;
    public bool IsWipingEnabled => isWipingEnabled;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitMask();
    }

    private void OnDestroy()
    {
        if (maskRenderTexture != null)
        {
            maskRenderTexture.Release();
        }
    }

    public void InitConfig( Material material,  float brush,  float percent)
    {
        dustWipeMaterial = material;
        brushSize = brush;
        completePercent = percent;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        InitMask();
    }

    private void InitMask()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (dustWipeMaterial == null)
        {
            Shader shader = Shader.Find("Custom/DustWipe");

            if (shader == null)
            {
                Debug.LogError("’“≤ªµΩ Shader£∫Custom/DustWipe");
                return;
            }

            dustWipeMaterial = new Material(shader);
        }

        runtimeMaterial = new Material(dustWipeMaterial);

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

        DisableWiping();
    }

    public void EnableWiping()
    {
        if (isCompleted) return;

        isWipingEnabled = true;
        SetDustVisible(true);
    }

    public void DisableWiping()
    {
        isWipingEnabled = false;
        SetDustVisible(false);
    }

    public void WipeAtWorldPosition(Vector3 worldPosition)
    {
        if (!isWipingEnabled) return;
        if (isCompleted) return;
        if (spriteRenderer == null) return;
        if (spriteRenderer.sprite == null) return;

        Vector2 uv;

        if (!WorldToSpriteUV(worldPosition, out uv))
        {
            return;
        }

        DrawBrushToMask(uv);
    }

    private bool WorldToSpriteUV(Vector3 worldPosition, out Vector2 uv)
    {
        uv = Vector2.zero;

        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        Bounds bounds = spriteRenderer.sprite.bounds;

        float normalizedX = Mathf.InverseLerp(
            bounds.min.x,
            bounds.max.x,
            localPos.x
        );

        float normalizedY = Mathf.InverseLerp(
            bounds.min.y,
            bounds.max.y,
            localPos.y
        );

        if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
        {
            return false;
        }

        uv = new Vector2(normalizedX, normalizedY);
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

        Texture2D brushTexture = GetBrushTexture();

        Rect rect = new Rect(
            centerX - radius,
            centerY - radius,
            radius * 2f,
            radius * 2f
        );

        Graphics.DrawTexture(rect, brushTexture);

        GL.PopMatrix();

        RenderTexture.active = previous;
    }

    private Texture2D brushTexture;

    private Texture2D GetBrushTexture()
    {
        if (brushTexture != null)
        {
            return brushTexture;
        }

        int size = 128;
        brushTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);

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

                // ∞◊…´¥˙±Ì≤¡µÙª“≥æ
                Color color = new Color(1f, 1f, 1f, alpha);
                brushTexture.SetPixel(x, y, color);
            }
        }

        brushTexture.Apply();

        return brushTexture;
    }

    private void Update()
    {
        if (!isWipingEnabled) return;
        if (isCompleted) return;

        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckWipeProgress();
        }
    }

    private void CheckWipeProgress()
    {
        float percent = GetWipedPercent();

        if (percent >= completePercent)
        {
            CompleteWipe();
        }
    }

    private float GetWipedPercent()
    {
        if (maskRenderTexture == null) return 0f;

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

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 20)
            {
                wipedCount++;
            }
        }

        return (float)wipedCount / pixels.Length;
    }

    private void CompleteWipe()
    {
        isCompleted = true;
        isWipingEnabled = false;

        //gameObject.SetActive(false);
        SetDustVisible(false);

        OnWipeCompleted?.Invoke();
    }

    private void ClearMask()
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;

        GL.Clear(true, true, Color.black);

        RenderTexture.active = previous;
    }

    private void SetDustVisible(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
    }

}
