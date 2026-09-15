using System;
using UnityEngine;

/// <summary>
/// 新拼接关卡加载器。
/// 根据关卡名称加载 FragmentAssemblyLevelData，
/// 然后交给 FragmentAssemblyLevelBuilder 生成关卡。
/// </summary>
public class FragmentAssemblyLevelLoader : MonoBehaviour
{
    [Header("拼接关卡生成器")]
    [SerializeField]
    private FragmentAssemblyLevelBuilder builder;

    [Header("Resources 中的关卡目录")]
    [SerializeField]
    private string resourcesFolder = "AssemblyLevels";

    private FragmentAssemblyScoreController currentScoreController;

    public FragmentAssemblyLevelData CurrentLevelData =>
        builder != null ? builder.LevelData : null;

    /// <summary>
    /// 关卡完成事件
    /// </summary>
    public event Action<LevelScoreResult> OnLevelCompletedWithScore;

    /// <summary>
    /// 加载指定名称的拼接关卡。
    /// </summary>
    public void LoadLevel(string levelName)
    {
        if (builder == null)
        {
            Debug.LogError(
                "FragmentAssemblyLevelLoader 没有绑定 FragmentAssemblyLevelBuilder"
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(levelName))
        {
            Debug.LogError(
                "FragmentAssemblyLevelLoader 关卡名称不能为空"
            );

            return;
        }

        string resourcePath = resourcesFolder + "/" + levelName;

        FragmentAssemblyLevelData data =Resources.Load<FragmentAssemblyLevelData>(resourcePath);

        if (data == null)
        {
            Debug.LogError( "FragmentAssemblyLevelLoader 加载关卡失败：" + resourcePath );

            return;
        }

        UnsubscribeScoreController();

        builder.SetLevelData(data);
        builder.BuildLevel();

        currentScoreController = builder.ScoreController;

        if (currentScoreController == null)
        {
            Debug.LogError(
                "关卡评分控制器未生成，请检查关卡生成过程中的错误。",
                this
            );
            return;
        }

        currentScoreController.OnLevelCompletedWithScore += HandleLevelCompleted;

        Debug.Log(
            "FragmentAssemblyLevelLoader 加载关卡成功：" + data.levelName
        );
    }

    internal void RegisterLevelCompletedCallback(Action<LevelScoreResult> onLevelComplete)
    {
        OnLevelCompletedWithScore -= onLevelComplete;
        OnLevelCompletedWithScore += onLevelComplete;
    }

    private void HandleLevelCompleted(LevelScoreResult result)
    {
        OnLevelCompletedWithScore?.Invoke(result);
    }

    private void UnsubscribeScoreController()
    {
        if (currentScoreController != null)
        {
            currentScoreController.OnLevelCompletedWithScore -= HandleLevelCompleted;
        }

        currentScoreController = null;
    }

    private void OnDestroy()
    {
        UnsubscribeScoreController();
    }
}
