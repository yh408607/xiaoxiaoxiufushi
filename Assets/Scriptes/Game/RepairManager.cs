using UnityEngine;
using System;
using System.Collections.Generic;

public class RepairManager : MonoBehaviour
{
    [Header("所有修复点")]
    [SerializeField] private List<RepairSlot> slots = new List<RepairSlot>();

    [Header("是否自动查找场景中的修复点")]
    [SerializeField] private bool autoFindSlots = true;

    private int repairedCount;

    public event Action OnRepairStageCompleted;
    public event Action OnLevelCompleted;

    private DustWipeController dustWipeController;
    private WiperUITool wiperTool;
    private FingerDragUI fingerDragUI;

    private GameObject repairBackgroundObject;
    private GameObject cleanBackgroundObject;

    private LevelScoreTimer scoreTimer;
    private LevelScoreResult lastScoreResult;

    public LevelScoreResult LastScoreResult => lastScoreResult;

    public event Action<LevelScoreResult> OnLevelCompletedWithScore;


    private void Awake()
    {
        if (autoFindSlots)
        {
            RefreshSlots();
        }
    }

    private void OnEnable()
    {
        SubscribeSlots();
    }

    private void OnDisable()
    {
        UnsubscribeSlots();

        if (dustWipeController != null)
        {
            dustWipeController.OnWipeCompleted -= HandleWipeCompleted;
        }
    }

    private void Start()
    {
        ResetProgress();
    }

    public void Init( List<RepairSlot> repairSlots, DustWipeController dustController, WiperUITool wiper,FingerDragUI fingerDragUI, GameObject repairBackground, GameObject cleanBackground,    float threeStarTime, float twoStarTime)
    {
        UnsubscribeSlots();

        slots = repairSlots;
        dustWipeController = dustController;
        wiperTool = wiper;
        this.fingerDragUI = fingerDragUI;


        repairBackgroundObject = repairBackground;
        cleanBackgroundObject = cleanBackground;

        repairedCount = 0;

        SubscribeSlots();

        if (dustWipeController != null)
        {
            dustWipeController.OnWipeCompleted -= HandleWipeCompleted;
            dustWipeController.OnWipeCompleted += HandleWipeCompleted;
            dustWipeController.DisableWiping();
        }

        if (wiperTool != null)
        {
            wiperTool.Init(dustWipeController, Camera.main);
            wiperTool.DisableWiper();
        }

        // 初始状态：修复底图显示，干净底图隐藏
        if (repairBackgroundObject != null)
        {
            repairBackgroundObject.SetActive(true);
        }

        if (cleanBackgroundObject != null)
        {
            cleanBackgroundObject.SetActive(false);
        }

        scoreTimer = GetComponent<LevelScoreTimer>();

        if (scoreTimer == null)
        {
            scoreTimer = gameObject.AddComponent<LevelScoreTimer>();
        }

        scoreTimer.Init(threeStarTime, twoStarTime);
        scoreTimer.StartTimer();

        //todo加载UI，绑定UI事件

    }

    private void RefreshSlots()
    {
        slots.Clear();
        slots.AddRange(FindObjectsOfType<RepairSlot>());
    }

    private void SubscribeSlots()
    {
        if (slots == null) return;

        foreach (RepairSlot slot in slots)
        {
            if (slot != null)
            {
                slot.OnRepaired -= HandleSlotRepaired;
                slot.OnRepaired += HandleSlotRepaired;
            }
        }
    }

    private void UnsubscribeSlots()
    {
        if (slots == null) return;

        foreach (RepairSlot slot in slots)
        {
            if (slot != null)
            {
                slot.OnRepaired -= HandleSlotRepaired;
            }
        }
    }

    private void ResetProgress()
    {
        repairedCount = 0;

        if (slots == null) return;

        foreach (RepairSlot slot in slots)
        {
            if (slot != null && slot.IsRepaired)
            {
                repairedCount++;
            }
        }
    }

    private void HandleSlotRepaired(RepairSlot slot)
    {
        repairedCount++;

        Debug.Log($"修复完成：{repairedCount}/{slots.Count}");

        if (repairedCount >= slots.Count)
        {
            Debug.Log("所有碎片修复完成，进入擦灰阶段");

            OnRepairStageCompleted?.Invoke();

            StartWipeStage();
        }
    }

    private void StartWipeStage()
    {
        // 1. 隐藏修复阶段底图
        if (repairBackgroundObject != null)
        {
            repairBackgroundObject.SetActive(false);
        }

        // 2. 显示干净底图
        if (cleanBackgroundObject != null)
        {
            cleanBackgroundObject.SetActive(true);
        }

        // 3. 开启灰尘层
        if (dustWipeController == null)
        {
            Debug.LogWarning("没有灰尘擦拭控制器，直接完成关卡");
            OnLevelCompleted?.Invoke();
            return;
        }

        dustWipeController.EnableWiping();

        // 4. 开启 UI 抹布
        if (wiperTool != null)
        {
            wiperTool.EnableWiper();
        }
        else
        {
            Debug.LogWarning("没有找到 UI 抹布 WiperUITool");
        }

        //隐藏fingerUI
        if (fingerDragUI != null)
        {
            fingerDragUI.Hide();
        }
    }

    private void HandleWipeCompleted()
    {
        Debug.Log("擦灰完成，关卡完成");

        if (wiperTool != null)
        {
            wiperTool.DisableWiper();
        }

        if (scoreTimer != null)
        {
            lastScoreResult = scoreTimer.StopTimer();
        }

        Debug.Log(
            $"关卡用时：{lastScoreResult.FormattedTime}，评分：{lastScoreResult.stars}星"
        );

        OnLevelCompletedWithScore?.Invoke(lastScoreResult);
        OnLevelCompleted?.Invoke();
    }
}
