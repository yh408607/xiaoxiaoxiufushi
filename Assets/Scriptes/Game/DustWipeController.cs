using UnityEngine;
using System;
using System.Data.SqlTypes;

[RequireComponent(typeof(SpriteRenderer))]
public class DustWipeController : MonoBehaviour
{
    [Header("材质")]
    [SerializeField] private Material dustWipeMaterial;

    [Header("擦拭设置")]
    [SerializeField] private int maskTextureSize = 512;
    [SerializeField] private float brushSize = 0.08f;
    [SerializeField] private float completePercent = 0.9f;

    [Header("检测设置")]
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
                Debug.LogError("找不到 Shader：Custom/DustWipe");
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
            // 不在擦拭区域 -> 停止（这里是停止触发）
            SfxManager.Instance.StopIfPlaying(SfxId.DustWipe);
            return;
        }
        ////// 2) 再判断是否有效擦拭（是否还有灰可擦）
        //if (!CanWipeAtUV(uv))
        //{
        //    // 在区域内但无有效擦拭 -> 停止（这里是停止触发）
        //    SfxManager.Instance.StopIfPlaying(SfxId.DustWipe);
        //    return;
        //}
        DrawBrushToMask(uv);

        // 3) 播放音效：未播完不可重复播
        SfxManager.Instance?.PlayIfNotPlaying(SfxId.DustWipe, 0.8f);
    }

    private bool WorldToSpriteUV(Vector3 worldPosition, out Vector2 uv)
    {
        uv = Vector2.zero;

        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        Bounds b = spriteRenderer.sprite.bounds;

        // 先做范围判断（不要依赖InverseLerp后的结果）
        if (localPos.x < b.min.x || localPos.x > b.max.x ||
            localPos.y < b.min.y || localPos.y > b.max.y)
        {
            return false;
        }

        // 手算归一化，避免InverseLerp的clamp掩盖越界
        float u = (localPos.x - b.min.x) / b.size.x;
        float v = (localPos.y - b.min.y) / b.size.y;

        uv = new Vector2(u, v);
        //uv = new Vector2(normalizedX, normalizedY);
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

                // 白色代表擦掉灰尘
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

    [Header("音效")]
    [SerializeField] private float wipeSfxInterval = 2.3f; // 每80ms最多播一次
    [SerializeField] private float wipeSfxVolume = 0.8f;

    private float nextWipeSfxTime;
    private void PlayWipeSfxThrottled()
    {
        if (Time.time < nextWipeSfxTime) return;

        nextWipeSfxTime = Time.time + wipeSfxInterval;
        SfxManager.Instance?.Play(SfxId.DustWipe, wipeSfxVolume);
    }

    private bool CanWipeAtUV(Vector2 uv)
    {
        if (maskRenderTexture == null) return false;

        int centerX = Mathf.RoundToInt(uv.x * maskTextureSize);
        int centerY = Mathf.RoundToInt((1f - uv.y) * maskTextureSize);
        int radius = Mathf.Max(1, Mathf.RoundToInt(brushSize * maskTextureSize));

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;

        readableMaskTexture.ReadPixels(
            new Rect(0, 0, maskTextureSize, maskTextureSize),
            0,
            0
        );
        readableMaskTexture.Apply();

        RenderTexture.active = previous;

        int minX = Mathf.Max(0, centerX - radius);
        int maxX = Mathf.Min(maskTextureSize - 1, centerX + radius);
        int minY = Mathf.Max(0, centerY - radius);
        int maxY = Mathf.Min(maskTextureSize - 1, centerY + radius);

        int rr = radius * radius;

        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - centerY;
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - centerX;
                if (dx * dx + dy * dy > rr) continue;

                // 黑色区域=未擦，说明可擦
                Color32 c = readableMaskTexture.GetPixel(x, y);
                if (c.r < 20) return true;
            }
        }

        return false;
    }


}
