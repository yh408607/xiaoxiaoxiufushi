using System;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("配置表")]
    [SerializeField] private SfxEntry[] sfxEntries;

    [Header("播放器")]
    [SerializeField] private AudioSource oneShotSource; // 主播放源（PlayOneShot）
    [SerializeField] private AudioSource loopSource;    // 预留：循环音效（可不用）

    [Header("全局设置")]
    [Range(0f, 1f)][SerializeField] private float masterSfxVolume = 1f;

    private Dictionary<SfxId, SfxEntry> sfxMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (oneShotSource == null)
            oneShotSource = gameObject.AddComponent<AudioSource>();

        BuildMap();
    }

    private void BuildMap()
    {
        sfxMap = new Dictionary<SfxId, SfxEntry>();
        if (sfxEntries == null) return;

        for (int i = 0; i < sfxEntries.Length; i++)
        {
            var e = sfxEntries[i];
            if (e == null || e.clip == null) continue;
            sfxMap[e.id] = e; // 后者覆盖前者
        }
    }

    public void Play(SfxId id)
    {
        Play(id, 1f);
    }

    public void Play(SfxId id, float volumeScale)
    {
        if (oneShotSource == null) return;
        if (sfxMap == null || !sfxMap.TryGetValue(id, out var e)) return;
        if (e.clip == null) return;

        float finalVol = e.volume * masterSfxVolume * Mathf.Clamp01(volumeScale);
        oneShotSource.PlayOneShot(e.clip, finalVol);
    }

    public void SetMasterVolume(float v)
    {
        masterSfxVolume = Mathf.Clamp01(v);
    }

    public float GetMasterVolume()
    {
        return masterSfxVolume;
    }
}

public enum SfxId
{
    ButtonClick,
    PiecePick,        // 可选：拿起碎片
    PieceDropCorrect, // 碎片正确
    PieceDropWrong,   // 碎片错误
    DustWipe,         // 擦拭灰尘
    LevelComplete,    // 完成关卡
    StarAppear        // 评分星星出现
}

