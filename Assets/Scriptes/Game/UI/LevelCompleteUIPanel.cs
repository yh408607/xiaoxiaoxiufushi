using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUIPanel : BaseUI
{

    [SerializeField] private Text timeText;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starOnSprite;
    [SerializeField] private Sprite starOffSprite;


    public override void Init()
    {
        base.Init();
        m_UiUitil.Get("btn_showCard").AddListenrforBtn(() =>
        {
            UIPanelManager.Instance.ShownPanel("UIPanel/showCar_Panel");
        });

        m_UiUitil.Get("btn_next").AddListenrforBtn(() =>
        {

        });
    }

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