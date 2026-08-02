using System;
using System.Collections.Generic;

[Serializable]
public class LevelStarEntry
{
    public string levelName;

    public float bestElapsedTime;   // 最快通关时间（越小越好）
    public int bestStars;           // 最高星级（越大越好）

    public float threeStarTime;     // 该关三星阈值（用于展示）
    public float twoStarTime;       // 该关二星阈值（用于展示）

    public long updateTime;
}

[Serializable]
public class LevelStarSaveData
{
    public List<LevelStarEntry> entries = new List<LevelStarEntry>();
}
