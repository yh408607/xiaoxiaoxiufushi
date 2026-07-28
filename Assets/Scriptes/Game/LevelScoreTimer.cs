using UnityEngine;
using System;

public class LevelScoreTimer : MonoBehaviour
{
    [Header("评分时间")]
    [SerializeField] private float threeStarTime = 60f;
    [SerializeField] private float twoStarTime = 120f;

    private float startTime;
    private float finishTime;
    private bool isRunning;
    private bool isFinished;

    public float ElapsedTime
    {
        get
        {
            if (isRunning)
            {
                return Time.time - startTime;
            }

            if (isFinished)
            {
                return finishTime - startTime;
            }

            return 0f;
        }
    }

    public int CurrentStars
    {
        get
        {
            return CalculateStars(ElapsedTime);
        }
    }

    public void Init(float threeStar, float twoStar)
    {
        threeStarTime = threeStar;
        twoStarTime = twoStar;

        if (twoStarTime < threeStarTime)
        {
            Debug.LogWarning("2星时间小于3星时间，已自动修正");
            twoStarTime = threeStarTime;
        }
    }

    public void StartTimer()
    {
        startTime = Time.time;
        finishTime = 0f;
        isRunning = true;
        isFinished = false;
    }

    public LevelScoreResult StopTimer()
    {
        if (!isRunning && isFinished)
        {
            return GetResult();
        }

        finishTime = Time.time;
        isRunning = false;
        isFinished = true;

        return GetResult();
    }

    public LevelScoreResult GetResult()
    {
        float elapsed = ElapsedTime;
        int stars = CalculateStars(elapsed);

        return new LevelScoreResult(
            elapsed,
            stars,
            threeStarTime,
            twoStarTime
        );
    }

    private int CalculateStars(float elapsed)
    {
        if (elapsed <= threeStarTime)
        {
            return 3;
        }

        if (elapsed <= twoStarTime)
        {
            return 2;
        }

        return 1;
    }

    public string GetFormattedTime()
    {
        return FormatTime(ElapsedTime);
    }

    public static string FormatTime(float seconds)
    {
        int minute = Mathf.FloorToInt(seconds / 60f);
        int second = Mathf.FloorToInt(seconds % 60f);

        return $"{minute:00}:{second:00}";
    }
}

[Serializable]
public struct LevelScoreResult
{
    public float elapsedTime;
    public int stars;
    public float threeStarTime;
    public float twoStarTime;

    public LevelScoreResult(
        float elapsed,
        int starCount,
        float threeStar,
        float twoStar
    )
    {
        elapsedTime = elapsed;
        stars = starCount;
        threeStarTime = threeStar;
        twoStarTime = twoStar;
    }

    public string FormattedTime
    {
        get
        {
            return LevelScoreTimer.FormatTime(elapsedTime);
        }
    }
}
