using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FragmentWipeController : MonoBehaviour
{
    [SerializeField, Min(32)]
    private int maskTextureSize = 512;

    [SerializeField, Min(0.01f)]
    private float checkInterval = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material runtimeMaterial;
    private Texture2D maskTexture;

    private Color32[] maskPixels;
    private bool[] validPixels;

    private int validPixelCount;
    private int wipedPixelCount;
    private int resolution;

    private float brushSize;
    private float completePercent;
    private float checkTimer;

    private bool initialized;
    private bool isWipingEnabled;
    private bool isCompleted;
    private bool maskDirty;

    public bool IsCompleted => isCompleted;
    public bool IsWipingEnabled => isWipingEnabled;

    public float WipedPercent => validPixelCount > 0
        ? (float)wipedPixelCount / validPixelCount
        : 0f;

    public event Action OnWipeCompleted;

    /// <summary>
    /// 为同一张完整图初始化一次擦拭资源。
    /// </summary>
    public bool Init(Material material)
    {
        ReleaseResources();

        spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite sprite = spriteRenderer.sprite;

        if (sprite == null)
        {
            Debug.LogError("擦拭完整图未配置。", this);
            return false;
        }

        if (sprite.packed)
        {
            Debug.LogError(
                "擦拭完整图请使用独立 Sprite，不要加入 Sprite Atlas。",
                this
            );
            return false;
        }

        if (material == null ||
            material.shader == null ||
            material.shader.name != "Custom/FragmentAssemblyWipe")
        {
            Debug.LogError(
                "请配置使用 Custom/FragmentAssemblyWipe 的材质。",
                this
            );
            return false;
        }

        resolution = Mathf.Max(32, maskTextureSize);

        maskPixels = new Color32[resolution * resolution];
        validPixels = new bool[maskPixels.Length];

        BuildValidPixels(sprite);

        if (validPixelCount == 0)
        {
            Debug.LogError("待清洁完整图没有有效的非透明区域。", this);
            return false;
        }

        originalMaterial = spriteRenderer.sharedMaterial;
        runtimeMaterial = new Material(material);

        maskTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false,
            true
        );
        maskTexture.wrapMode = TextureWrapMode.Clamp;
        maskTexture.filterMode = FilterMode.Bilinear;

        Bounds bounds = sprite.bounds;

        runtimeMaterial.SetVector(
            "_LocalBounds",
            new Vector4(
                bounds.min.x,
                bounds.min.y,
                bounds.size.x,
                bounds.size.y
            )
        );

        runtimeMaterial.SetTexture("_MaskTex", maskTexture);
        spriteRenderer.sharedMaterial = runtimeMaterial;

        initialized = true;
        isCompleted = false;

        SetUniformAlpha(1f);
        DisableWiping();

        return true;
    }

    /// <summary>
    /// 只在初始化时读取原图 Alpha，无需打开图片 Read/Write。
    /// </summary>
    private void BuildValidPixels(Sprite sprite)
    {
        Texture2D source = sprite.texture;
        Rect rect = sprite.rect;

        Vector2 scale = new Vector2(
            rect.width / source.width,
            rect.height / source.height
        );

        Vector2 offset = new Vector2(
            rect.x / source.width,
            rect.y / source.height
        );

        RenderTexture temporary = RenderTexture.GetTemporary(
            resolution,
            resolution,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        );

        RenderTexture previous = RenderTexture.active;
        Texture2D readable = null;

        try
        {
            Graphics.Blit(source, temporary, scale, offset);
            RenderTexture.active = temporary;

            readable = new Texture2D(
                resolution,
                resolution,
                TextureFormat.RGBA32,
                false,
                true
            );

            readable.ReadPixels(
                new Rect(0, 0, resolution, resolution),
                0,
                0
            );
            readable.Apply();

            Color32[] sourcePixels = readable.GetPixels32();

            validPixelCount = 0;

            for (int i = 0; i < sourcePixels.Length; i++)
            {
                // 排除透明背景和极淡的边缘。
                validPixels[i] = sourcePixels[i].a > 20;

                if (validPixels[i])
                {
                    validPixelCount++;
                }
            }
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);

            if (readable != null)
            {
                DestroyResource(readable);
            }
        }
    }

    /// <summary>
    /// 每次进入一个阶段，都重置遮罩和进度。
    /// </summary>
    public void BeginStage(
        float size,
        float percent,
        float startAlpha,
        float eraseStrength)
    {
        if (!initialized) return;

        brushSize = Mathf.Max(0.001f, size);
        completePercent = Mathf.Clamp01(percent);

        isCompleted = false;
        isWipingEnabled = true;
        checkTimer = 0f;

        ClearMask();

        runtimeMaterial.SetFloat(
            "_StageAlpha",
            Mathf.Clamp01(startAlpha)
        );

        runtimeMaterial.SetFloat(
            "_EraseStrength",
            Mathf.Clamp01(eraseStrength)
        );

        spriteRenderer.enabled = true;
    }

    /// <summary>
    /// 收尾到统一透明度，同时关闭擦拭。
    /// </summary>
    public void SetUniformAlpha(float alpha)
    {
        if (!initialized) return;

        isWipingEnabled = false;
        ClearMask();

        runtimeMaterial.SetFloat("_StageAlpha", Mathf.Clamp01(alpha));
        runtimeMaterial.SetFloat("_EraseStrength", 0f);

        spriteRenderer.enabled = alpha > 0f;
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
        if (!initialized || !isWipingEnabled || isCompleted) return;

        Vector3 localPosition =
            transform.InverseTransformPoint(worldPosition);

        Bounds bounds = spriteRenderer.sprite.bounds;

        // 只检查 XY，避免 Z 浮点误差影响擦拭。
        if (localPosition.x < bounds.min.x ||
            localPosition.x > bounds.max.x ||
            localPosition.y < bounds.min.y ||
            localPosition.y > bounds.max.y)
        {
            return;
        }

        float u = Mathf.InverseLerp(
            bounds.min.x, bounds.max.x, localPosition.x
        );
        float v = Mathf.InverseLerp(
            bounds.min.y, bounds.max.y, localPosition.y
        );

        if (PaintMask(new Vector2(u, v)))
        {
            SfxManager.Instance?.PlayIfNotPlaying(
                SfxId.DustWipe,
                0.8f
            );
        }
    }

    private bool PaintMask(Vector2 uv)
    {
        float centerX = uv.x * (resolution - 1);
        float centerY = uv.y * (resolution - 1);
        float radius = Mathf.Max(1f, brushSize * resolution);
        float radiusSquared = radius * radius;

        int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - radius));
        int maxX = Mathf.Min(
            resolution - 1,
            Mathf.CeilToInt(centerX + radius)
        );
        int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - radius));
        int maxY = Mathf.Min(
            resolution - 1,
            Mathf.CeilToInt(centerY + radius)
        );

        bool changed = false;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;

                if (dx * dx + dy * dy > radiusSquared) continue;

                int index = y * resolution + x;

                // 重复擦过的像素不重复累计，也不继续降低 Alpha。
                if (maskPixels[index].r == 255) continue;

                maskPixels[index] = new Color32(255, 255, 255, 255);

                if (validPixels[index])
                {
                    wipedPixelCount++;
                }

                changed = true;
            }
        }

        maskDirty |= changed;
        return changed;
    }

    private void Update()
    {
        if (!initialized || !isWipingEnabled || isCompleted) return;

        checkTimer += Time.deltaTime;

        if (checkTimer < checkInterval) return;

        checkTimer = 0f;

        if (WipedPercent < completePercent) return;

        isCompleted = true;
        isWipingEnabled = false;

        // 下一阶段的显示和 Alpha 由阶段控制器处理。
        OnWipeCompleted?.Invoke();
    }

    private void LateUpdate()
    {
        if (initialized && maskDirty)
        {
            UploadMask();
        }
    }

    private void ClearMask()
    {
        Array.Clear(maskPixels, 0, maskPixels.Length);
        wipedPixelCount = 0;
        UploadMask();
    }

    private void UploadMask()
    {
        maskTexture.SetPixels32(maskPixels);
        maskTexture.Apply(false, false);
        maskDirty = false;
    }

    private void ReleaseResources()
    {
        initialized = false;
        isWipingEnabled = false;

        if (spriteRenderer != null &&
            runtimeMaterial != null &&
            spriteRenderer.sharedMaterial == runtimeMaterial)
        {
            spriteRenderer.sharedMaterial = originalMaterial;
        }

        if (runtimeMaterial != null)
        {
            DestroyResource(runtimeMaterial);
        }

        if (maskTexture != null)
        {
            DestroyResource(maskTexture);
        }

        runtimeMaterial = null;
        maskTexture = null;
        originalMaterial = null;
        maskPixels = null;
        validPixels = null;

        validPixelCount = 0;
        wipedPixelCount = 0;
        maskDirty = false;
    }

    private static void DestroyResource(UnityEngine.Object resource)
    {
        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(resource);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(resource);
        }
    }

    private void OnDestroy()
    {
        ReleaseResources();
    }
}