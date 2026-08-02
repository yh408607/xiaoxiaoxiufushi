using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class LevelStarSystem : NoramlInstanceExample<LevelStarSystem>
{
    private const string SaveKey = "LEVEL_STAR_SAVE_V1";

    private LevelStarSaveData saveData = new LevelStarSaveData();
    private Dictionary<string, LevelStarEntry> cache = new Dictionary<string, LevelStarEntry>();

    public event Action<string, LevelScoreResult> OnLevelResultUpdated;

    //private void Awake()
    //{
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    Instance = this;
    //    DontDestroyOnLoad(gameObject);

    //    Load();
    //}

    public override void Init()
    {
        Load();
    }

    /// <summary>
    /// 保存关卡结果（默认保留最好结果：星级更高优先；同星级下用时更短优先）
    /// </summary>
    public void SaveLevelResult(string levelName, LevelScoreResult result, bool keepBest = true)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("SaveLevelResult失败：levelName为空");
            return;
        }

        if (cache.TryGetValue(levelName, out var old))
        {
            if (keepBest)
            {
                bool replace = IsNewResultBetter(result, old);
                if (replace)
                {
                    old.bestElapsedTime = result.elapsedTime;
                    old.bestStars = result.stars;
                }

                // 阈值一般以本次关卡配置为准，保持最新用于展示
                old.threeStarTime = result.threeStarTime;
                old.twoStarTime = result.twoStarTime;
                old.updateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else
            {
                old.bestElapsedTime = result.elapsedTime;
                old.bestStars = result.stars;
                old.threeStarTime = result.threeStarTime;
                old.twoStarTime = result.twoStarTime;
                old.updateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            OnLevelResultUpdated?.Invoke(levelName, ToResult(old));
        }
        else
        {
            var entry = new LevelStarEntry
            {
                levelName = levelName,
                bestElapsedTime = result.elapsedTime,
                bestStars = result.stars,
                threeStarTime = result.threeStarTime,
                twoStarTime = result.twoStarTime,
                updateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            cache[levelName] = entry;
            saveData.entries.Add(entry);

            OnLevelResultUpdated?.Invoke(levelName, result);
        }

        Save();
    }

    public bool TryGetLevelResult(string levelName, out LevelScoreResult result)
    {
        result = default;

        if (string.IsNullOrEmpty(levelName)) return false;
        if (!cache.TryGetValue(levelName, out var entry)) return false;

        result = ToResult(entry);
        return true;
    }

    public int GetLevelStarsOrDefault(string levelName, int defaultValue = 0)
    {
        if (TryGetLevelResult(levelName, out var r)) return r.stars;
        return defaultValue;
    }

    public List<LevelStarEntry> GetAllEntries()
    {
        return cache.Values.OrderBy(e => e.levelName).ToList();
    }

    public void ClearAll()
    {
        saveData.entries.Clear();
        cache.Clear();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    private bool IsNewResultBetter(LevelScoreResult incoming, LevelStarEntry old)
    {
        // 1) 星级高更好
        if (incoming.stars > old.bestStars) return true;
        if (incoming.stars < old.bestStars) return false;

        // 2) 同星级，用时更短更好
        return incoming.elapsedTime < old.bestElapsedTime;
    }

    private LevelScoreResult ToResult(LevelStarEntry e)
    {
        return new LevelScoreResult(
            e.bestElapsedTime,
            e.bestStars,
            e.threeStarTime,
            e.twoStarTime
        );
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            saveData = new LevelStarSaveData();
            cache = new Dictionary<string, LevelStarEntry>();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
        {
            saveData = new LevelStarSaveData();
            cache = new Dictionary<string, LevelStarEntry>();
            return;
        }

        saveData = JsonUtility.FromJson<LevelStarSaveData>(json);
        if (saveData == null) saveData = new LevelStarSaveData();
        if (saveData.entries == null) saveData.entries = new List<LevelStarEntry>();

        cache = new Dictionary<string, LevelStarEntry>();
        foreach (var e in saveData.entries)
        {
            if (e == null || string.IsNullOrEmpty(e.levelName)) continue;
            cache[e.levelName] = e;
        }
    }
}
