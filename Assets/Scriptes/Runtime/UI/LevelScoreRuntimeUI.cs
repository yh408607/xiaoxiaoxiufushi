using UnityEngine;
using UnityEngine.UI;

public class LevelScoreRuntimeUI : MonoBehaviour
{
    [Header("运行中星星，从左到右配置")]
    [SerializeField] private Image[] starImages;

    [Header("星星图片")]
    [SerializeField] private Sprite starOnSprite;
    [SerializeField] private Sprite starOffSprite;

    private LevelScoreTimer scoreTimer;

    public void Bind(LevelScoreTimer timer)
    {
        Unbind();

        scoreTimer = timer;

        if (scoreTimer == null)
        {
            Debug.LogError("LevelScoreRuntimeUI：绑定的 LevelScoreTimer 为空");
            return;
        }

        scoreTimer.OnStarsChanged += RefreshStars;
        RefreshStars(scoreTimer.CurrentStars);
    }

    public void Unbind()
    {
        if (scoreTimer == null)
        {
            return;
        }

        scoreTimer.OnStarsChanged -= RefreshStars;
        scoreTimer = null;
    }

    private void RefreshStars(int currentStars)
    {
        if (starImages == null || starImages.Length == 0)
        {
            Debug.LogWarning("LevelScoreRuntimeUI：没有配置星星 Image");
            return;
        }

        int starCount = Mathf.Clamp(currentStars, 0, starImages.Length);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                continue;
            }

            starImages[i].sprite = i < starCount
                ? starOnSprite
                : starOffSprite;
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
