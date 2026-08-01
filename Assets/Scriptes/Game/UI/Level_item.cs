using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level_item : BaseUI
{
    public string ItemName;
    public ItemStaue ItemStaue; 
    
    public override void Init()
    {
        base.Init();

        var btn = this.GetComponent<Button>();
        btn.onClick.AddListener(() => loadGameScene(ItemName));
    }


    private void loadGameScene(string levelName)
    {
        //SceneManager.LoadScene("LevelScene");
        //GameManager.Instance.small_level = levelName;
        GameManager.Instance.LoadLevel(levelName);
    }
}

public enum ItemStaue
{
    WEIKAISHI,
    START_1,
    START_2,
    START_3
}
