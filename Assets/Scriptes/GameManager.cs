using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourInstanceExample<GameManager>
{
    /// <summary>
    /// 大关卡的名称
    /// </summary>
    public string lagerd_level_name;
    /// <summary>
    /// 小关卡的名称
    /// </summary>
    public string small_level_Name;

    public LevelLoader levelLoader;

    private string currenSceneName;


    // Start is called before the first frame update
    void Start()
    {
        // 先注册“场景加载完成”回调
        SceneManager.sceneLoaded += OnSceneLoaded;
        currenSceneName = "MainScene";
        SceneManager.LoadScene(currenSceneName);

        //加载评分数据
        LevelStarSystem.Instance.Init();
    }

   
    public void LoadLevel(string levelName)
    {
        small_level_Name = levelName;


        ////判断当前场景名称是否是需要加载的场景
        //if( SceneManager.GetActiveScene().name == "LevelScene")
        //{
        //    if (levelLoader == null)
        //    {
        //        levelLoader = new LevelLoader();
        //        levelLoader.Init();
        //        levelLoader.RegisterLevelCompletedCallback(OnLevelComplete);
        //    }

        //    levelLoader.LoadLevel(small_level_Name);
        //    UIPanelManager.Instance.HideAllPanel();
        //}
        //else
        //{


        //}

        // 先注册“场景加载完成”回调
        SceneManager.sceneLoaded += OnSceneLoaded;
        currenSceneName = "LevelScene";
        SceneManager.LoadScene(currenSceneName);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {

        // 用完立刻反注册，避免重复触发
        SceneManager.sceneLoaded -= OnSceneLoaded;

       // UIPanelManager.Instance.ClearUIPool();

        // 只处理目标场景
        if (scene.name != currenSceneName)
        {
            return;
        }

        switch (currenSceneName)
        {
            case "LevelScene":

                if (levelLoader == null)
                {
                    levelLoader = new LevelLoader();
                }

                levelLoader.Init();
                levelLoader.LoadLevel(small_level_Name);
                levelLoader.RegisterLevelCompletedCallback(OnLevelComplete);

                break;
            case "MainScene":

                //UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/shouyePanel");
                UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/main_Panal");
                UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/select_level_panel");
                UIPanelManager.Instance.addUIPanelInCuretnSceneAndInit("UIPanel/level_panel");
                UIPanelManager.Instance.HideAllPanel();

               // UIPanelManager.Instance.ShownPanel("UIPanel/shouyePanel");
                break;
            default:
                break;
        }
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
        StartCoroutine(ShowLevelCompletePanelAfterDelay(scoreResult, 1f));

        //保存关卡评分
        var levelName = lagerd_level_name + "_" + small_level_Name;
        LevelStarSystem.Instance.SaveLevelResult(levelName, scoreResult, true);
    }

    IEnumerator ShowLevelCompletePanelAfterDelay(LevelScoreResult scoreResult, float v)
    {
        yield return new WaitForSeconds(v);
        var ui = UIPanelManager.Instance.ShownPanel("UIPanel/complete_panel") as LevelCompleteUIPanel;
        yield return  ui.Show(scoreResult);

    }
}
