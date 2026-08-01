using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourInstanceExample<GameManager>
{
    public string lagerd_level;
    public string small_level;

    public LevelLoader levelLoader;

    // Start is called before the first frame update
    void Start()
    {
        UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/shouyePanel");
        UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/main_Panal");
        UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/select_level_panel");
        UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/level_panel");
        UIPanelManager.Instance.HideAllPanel();

        UIPanelManager.Instance.ShownPanel("UIPanel/shouyePanel");
    }


    public void LoadLevel(string levelName)
    {
        small_level = levelName;

        // 先注册“场景加载完成”回调
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("LevelScene");


    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {

        // 只处理目标场景
        if (scene.name != "LevelScene")
        {
            return;
        }

        // 用完立刻反注册，避免重复触发
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (levelLoader == null)
        {
            levelLoader = new LevelLoader();
        }

        levelLoader.Init();
        levelLoader.LoadLevel(small_level);

        levelLoader.RegisterLevelCompletedCallback(OnLevelComplete);
    }

    private void OnDestroy()
    {
        // 防止对象销毁时残留监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnLevelComplete(LevelScoreResult scoreResult)
    {
        // 处理关卡完成后的逻辑，例如显示分数、解锁下一关等
       // UIPanelManager.Instance.ShownPanel("UIPanel/level_panel");
       //先等几秒再显示关卡完成面板
        StartCoroutine(ShowLevelCompletePanelAfterDelay(scoreResult, 2f));
    }

    IEnumerator ShowLevelCompletePanelAfterDelay(LevelScoreResult scoreResult, float v)
    {
        yield return new WaitForSeconds(v);
        var ui = UIPanelManager.Instance.ShownPanel("UIPanel/complete_panel") as LevelCompleteUIPanel;
        yield return  ui.Show(scoreResult);

    }
}
