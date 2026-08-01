using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUIPanel : BaseUI
{

    [SerializeField] private Text timeText;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starOnSprite;
    [SerializeField] private Sprite starOffSprite;

    public IEnumerator Show(LevelScoreResult result)
    {
        Show();

        if (timeText != null)
        {
            timeText.text = result.FormattedTime;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            yield return new WaitForSeconds(0.5f); // 每颗星星之间的延迟
            starImages[i].sprite = i < result.stars
                ? starOnSprite
                : starOffSprite;
        }
    }
}