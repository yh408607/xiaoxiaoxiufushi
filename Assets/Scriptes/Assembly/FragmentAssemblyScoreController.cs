using System;
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

        // 保存成绩和显示结算界面由外层 GameManager 统一处理。
        OnLevelCompletedWithScore?.Invoke(result);
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

}
