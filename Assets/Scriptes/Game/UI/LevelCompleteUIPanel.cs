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

    [Header("星星入场动画")]
    [SerializeField] private float starInterval = 0.18f;         // 每颗星间隔
    [SerializeField] private float flyDuration = 0.45f;          // 飞行时长
    [SerializeField] private float punchDuration = 0.18f;        // 到位弹一下
    [SerializeField] private float startScale = 2.2f;            // 起始缩放（远处感）
    [SerializeField] private float endScale = 1f;                // 目标缩放
    [SerializeField] private Vector2 flyOffset = new Vector2(420f, 220f); // 从目标右上方飞入

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
        starSeq = DOTween.Sequence();

        // 先重置所有星星到 off，并保存目标位置
        Vector2[] targetAnchors = new Vector2[starImages.Length];

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            RectTransform rt = starImages[i].rectTransform;
            targetAnchors[i] = rt.anchoredPosition;

            starImages[i].sprite = starOffSprite;
            starImages[i].color = Color.white;
            rt.localScale = Vector3.one * endScale;
        }

        // 播放获得星（飞入 + 缩放 + 落点弹性）
        int winStars = Mathf.Clamp(result.stars, 0, starImages.Length);
        for (int i = 0; i < winStars; i++)
        {
            if (starImages[i] == null) continue;

            Image img = starImages[i];
            RectTransform rt = img.rectTransform;

            Vector2 target = targetAnchors[i];
            Vector2 start = target + flyOffset; // 从“远处”来（你可改成左上、右侧等）

           // float insertTime = i * starInterval;
            float insertTime = i* starInterval;
            starSeq.InsertCallback(insertTime, () =>
            {
                img.sprite = starOnSprite; // 开始飞入时点亮
                rt.anchoredPosition = start;
                rt.localScale = Vector3.one * startScale;
            });

            // 飞行 + 缩放
            starSeq.Insert(insertTime,
                rt.DOAnchorPos(target, flyDuration).SetEase(Ease.OutCubic));

            starSeq.Insert(insertTime,
                rt.DOScale(endScale, flyDuration).SetEase(Ease.OutCubic));

            // 到位“打击镶嵌”感
            starSeq.Insert(insertTime + flyDuration - 0.02f,
                rt.DOPunchScale(Vector3.one * 0.22f, punchDuration, 8, 0.7f));
        }

        // 未获得星：稍后再显示 off（避免一开始抢视觉）
        for (int i = winStars; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            int index = i;

            starSeq.InsertCallback(winStars * starInterval + 0.08f, () =>
            {
                starImages[index].sprite = starOffSprite;
                starImages[index].rectTransform.localScale = Vector3.one;
            });
        }

        starSeq.Play();

        // 等待动画播完
        float total = (winStars > 0 ? (winStars - 1) * starInterval + flyDuration + punchDuration : 0.1f);
        yield return new WaitForSeconds(total);
    }

    private void OnDestroy()
    {
        starSeq?.Kill();
        UIPanelManager.Instance.uipanelPool.Remove("UIPanel/complete_panel");
    }
}
