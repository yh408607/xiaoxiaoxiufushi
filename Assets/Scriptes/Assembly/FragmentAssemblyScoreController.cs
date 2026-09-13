using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 新拼接流程的评分控制器。
/// 计时从进入关卡开始，覆盖拼接、除尘和擦亮三个阶段。
/// </summary>
public class FragmentAssemblyScoreController : MonoBehaviour
{
    private FragmentAssemblyWipeStageController stageController;
    private LevelScoreTimer scoreTimer;
    private LevelScoreRuntimeUI runtimeScoreUI;
    private string levelName;
    private bool resultPublished;

    public LevelScoreTimer ScoreTimer => scoreTimer;
    public event Action<LevelScoreResult> OnLevelCompletedWithScore;

    /// <summary>
    /// 初始化评分配置并立即开始计时。
    /// </summary>
    public void Init(
        FragmentAssemblyWipeStageController stage,
        string assemblyLevelName,
        float threeStarTime,
        float twoStarTime)
    {
        Unsubscribe();

        stageController = stage;
        levelName = assemblyLevelName;
        resultPublished = false;

        scoreTimer = GetComponent<LevelScoreTimer>();
        if (scoreTimer == null)
        {
            scoreTimer = gameObject.AddComponent<LevelScoreTimer>();
        }

        scoreTimer.Init(threeStarTime, twoStarTime);
        scoreTimer.StartTimer();

        runtimeScoreUI = FindObjectOfType<LevelScoreRuntimeUI>(true);
        if (runtimeScoreUI != null)
        {
            runtimeScoreUI.Bind(scoreTimer);
        }

        if (stageController != null)
        {
            stageController.OnLevelCompleted += HandleLevelCompleted;
        }
    }

    private void HandleLevelCompleted()
    {
        if (resultPublished || scoreTimer == null) return;

        resultPublished = true;
        LevelScoreResult result = scoreTimer.StopTimer();

        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError(
                "FragmentAssemblyScoreController：关卡名称为空，无法保存评分。"
            );
        }
        else if (LevelStarSystem.Instance != null)
        {
            LevelStarSystem.Instance.SaveLevelResult(
                levelName,
                result,
                true
            );
        }

        OnLevelCompletedWithScore?.Invoke(result);
        OnLevelComplete(result);
    }

    private void Unsubscribe()
    {
        if (stageController != null)
        {
            stageController.OnLevelCompleted -= HandleLevelCompleted;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (runtimeScoreUI != null)
        {
            runtimeScoreUI.Unbind();
        }
    }

    private void OnLevelComplete(LevelScoreResult scoreResult)
    {
        // 处理关卡完成后的逻辑，例如显示分数、解锁下一关等
        // UIPanelManager.Instance.ShownPanel("UIPanel/level_panel");
        //先等几秒再显示关卡完成面板
        StartCoroutine(ShowLevelCompletePanelAfterDelay(scoreResult, 1f));

        //保存关卡评分
        var levelName = 1 + "_" + 1;
        LevelStarSystem.Instance.SaveLevelResult(levelName, scoreResult, true);
    }

    IEnumerator ShowLevelCompletePanelAfterDelay(LevelScoreResult scoreResult, float v)
    {
        yield return new WaitForSeconds(v);
        var ui = UIPanelManager.Instance.ShownPanel("UIPanel/complete_panel") as LevelCompleteUIPanel;
        yield return ui.Show(scoreResult);

    }
}
