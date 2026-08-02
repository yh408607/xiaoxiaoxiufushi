using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level_item : BaseUI
{
    public string Item_level_Name;
    public ItemStaue ItemStaue; 
    public List<Sprite> sprites;

    [Header("状态图片")]
    private Image staueImage;
    
    
    public override void Init()
    {
        base.Init();

        var btn = this.GetComponent<Button>();
        btn.onClick.AddListener(() => loadGameScene(Item_level_Name));

        staueImage = m_UiUitil.Get("imge_staue")._image;
    }

    public override void Show()
    {
        UpdateStaue();
    }

    public void UpdateStaue()
    {
        if (staueImage != null)
        {
            var levelName = GameManager.Instance.lagerd_level_name + "_" + Item_level_Name;
            var score = LevelStarSystem.Instance.GetLevelStarsOrDefault(levelName);
            if (score != 0)
            {
                int index = score + 1;
                staueImage.sprite = sprites[index];
            }
            else
            {

            }
        }
    }


    private void loadGameScene(string levelName)
    {
        GameManager.Instance.LoadLevel(levelName);
    }
}

public enum ItemStaue
{
    WEIKAISHI,
    ING,
    START_1,
    START_2,
    START_3
}
