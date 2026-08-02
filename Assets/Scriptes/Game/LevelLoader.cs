using System;
using UnityEngine;

public class LevelLoader 
{
    [Header("生成器")]
    [SerializeField] private RepairLevelBuilder builder;

    [Header("默认关卡名")]
    [SerializeField] private string defaultLevelName = "Level_1";

    [Header("Resources 路径")]
    [SerializeField] private string resourcesFolder = "RepairLevels";


    public void Init()
    {
        builder = GameObject.FindObjectOfType<RepairLevelBuilder>();

    }

    public void LoadLevel(string levelName)
    {
        string path = resourcesFolder + "/" + levelName;

        RepairLevelData data = Resources.Load<RepairLevelData>(path);

        if (data == null)
        {
            Debug.LogError("关卡数据加载失败：" + path);
            return;
        }

        builder.SetLevelData(data);
        builder.BuildLevel();

       // Debug.Log("关卡加载成功：" + levelName);
    }


    public void RegisterLevelCompletedCallback(Action<LevelScoreResult> callback)
    {
        if (builder.CurrentManager != null)
        {
            builder.CurrentManager.OnLevelCompletedWithScore -= callback;
            builder.CurrentManager.OnLevelCompletedWithScore += callback;
        }
        else
        {
            Debug.LogError("builder.CurrentManager 为null");
        }
    }
}
