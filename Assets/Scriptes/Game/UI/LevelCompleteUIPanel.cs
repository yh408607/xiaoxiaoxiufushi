using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelCompleteUIPanel : BaseUI
{
    [SerializeField] private Text timeText;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starOnSprite;
    [SerializeField] private Sprite starOffSprite;

    //[Header("星星入场动画")]
    //[SerializeField] private float starInterval = 0.18f;         // 每颗星间隔
    //[SerializeField] private float flyDuration = 0.45f;          // 飞行时长
    //[SerializeField] private float punchDuration = 0.18f;        // 到位弹一下
    //[SerializeField] private float startScale = 2.2f;            // 起始缩放（远处感）
    //[SerializeField] private float endScale = 1f;                // 目标缩放
    //[SerializeField] private Vector2 flyOffset = new Vector2(420f, 220f); // 从目标右上方飞入


    // ====== 建议放在类字段区域 ======
    [SerializeField] private float starInterval = 0.18f;
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private float flyStartScale = 1.8f;
    [SerializeField] private float flyEndScale = 1f;

    // 三个方向偏移（相对各自目标星）
    [SerializeField] private float leftUpX = -280f;   // 第1颗：左上
    [SerializeField] private float upY = 260f;        // 第2颗：正上（x=0）
    [SerializeField] private float rightUpX = 280f;   // 第3颗：右上

    [SerializeField] private RectTransform flyStarRoot; // 可不绑，留空则用星星父节点

    private Sequence starSeq;

    public override void Init()
    {
        base.Init();
        m_UiUitil.Get("btn_showCard").AddListenrforBtn(() =>
        {
            UIPanelManager.Instance.ShownPanel("UIPanel/showCar_Panel");
        });

        m_UiUitil.Get("btn_next").AddListenrforBtn(() =>
        {
            GameManager.Instance.LoadLevel("Level_1");
        });
    }

    public void TestShowResult()
    {
        var temp_reult = new LevelScoreResult(120, 3, 4, 4);

        StartCoroutine(Show(temp_reult));
    }

    public IEnumerator Show(LevelScoreResult result)
    {
        Show();

        if (timeText != null)
            timeText.text = result.FormattedTime;
        starSeq?.Kill();
        ClearFlyStars();

        starSeq = DOTween.Sequence();

        int count = starImages != null ? starImages.Length : 0;
        if (count <= 0) yield break;

        Vector2[] targetAnchors = new Vector2[count];

        // 1) 重置暗星（每次都恢复可见，防止上一局隐藏后不回来）
        for (int i = 0; i < count; i++)
        {
            if (starImages[i] == null) continue;

            RectTransform rt = starImages[i].rectTransform;
            targetAnchors[i] = rt.anchoredPosition;

            starImages[i].enabled = true;       // 关键：恢复暗星显示
            starImages[i].sprite = starOffSprite;
            starImages[i].color = Color.white;
            rt.localScale = Vector3.one;
        }

        int winStars = Mathf.Clamp(result.stars, 0, count);

        // 2) 亮星飞入（独立对象），到位后隐藏暗星
        for (int i = 0; i < winStars; i++)
        {
            if (starImages[i] == null) continue;

            Image darkStar = starImages[i];
            RectTransform darkRt = darkStar.rectTransform;

            Vector2 target = targetAnchors[i];
            Vector2 start = target + GetStartOffsetByIndex(i);

            float insertTime = i * starInterval;

            starSeq.InsertCallback(insertTime, () =>
            {
                // 创建飞入亮星
                GameObject flyObj = new GameObject($"FlyStar_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                flyObj.transform.SetParent(flyStarRoot != null ? flyStarRoot : darkRt.parent, false);

                Image flyImg = flyObj.GetComponent<Image>();
                flyImg.sprite = starOnSprite;
                flyImg.raycastTarget = false;
                flyImg.color = Color.white;

                RectTransform flyRt = flyObj.GetComponent<RectTransform>();
                flyRt.sizeDelta = darkRt.sizeDelta;  // 与槽位星一致
                flyRt.anchoredPosition = start;
                flyRt.localScale = Vector3.one * flyStartScale;

                // 飞行 + 缩放到位
                flyRt.DOAnchorPos(target, flyDuration).SetEase(Ease.OutCubic);
                flyRt.DOScale(flyEndScale, flyDuration).SetEase(Ease.OutCubic);

                // 到位后：隐藏暗星，亮星保留
                DOVirtual.DelayedCall(flyDuration, () =>
                {
                    if (darkStar != null) darkStar.enabled = false;

                    if (flyRt != null)
                    {
                        flyRt.anchoredPosition = target;
                        flyRt.localScale = Vector3.one * flyEndScale;
                    }
                });
            });
        }

        starSeq.Play();

        float total = (winStars > 0)
            ? (winStars - 1) * starInterval + flyDuration + 0.02f
            : 0.1f;

        yield return new WaitForSeconds(total);
    }

    private Vector2 GetStartOffsetByIndex(int index)
    {
        // 第1颗：左上；第2颗：正上；第3颗：右上
        if (index == 0) return new Vector2(leftUpX, upY);
        if (index == 1) return new Vector2(0f, upY);
        if (index == 2) return new Vector2(rightUpX, upY);

        // 超过3颗兜底（交替）
        float x = (index % 2 == 0) ? leftUpX : rightUpX;
        return new Vector2(x, upY);
    }


    private void ClearFlyStars()
    {
        // 优先清理指定容器
        if (flyStarRoot != null)
        {
            for (int i = flyStarRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = flyStarRoot.GetChild(i);
                if (child != null && child.name.StartsWith("FlyStar_"))
                {
                    Destroy(child.gameObject);
                }
            }
            return;
        }

        // 未指定容器：从各星星父节点清理
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            Transform parent = starImages[i].transform.parent;
            if (parent == null) continue;

            for (int c = parent.childCount - 1; c >= 0; c--)
            {
                Transform child = parent.GetChild(c);
                if (child != null && child.name.StartsWith("FlyStar_"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void OnDestroy()
    {
        starSeq?.Kill();
        UIPanelManager.Instance.uipanelPool.Remove("UIPanel/complete_panel");
    }
}
