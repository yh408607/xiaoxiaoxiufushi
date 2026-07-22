using UnityEngine;
using System.Collections.Generic;

public class RepairManager : MonoBehaviour
{
    [Header("所有修复点")]
    [SerializeField] private List<RepairSlot> slots = new List<RepairSlot>();

    [Header("是否自动查找场景中的修复点")]
    [SerializeField] private bool autoFindSlots = true;

    private int repairedCount;

    private void Awake()
    {
        if (autoFindSlots)
        {
            slots.Clear();
            slots.AddRange(FindObjectsOfType<RepairSlot>());
        }
    }

    private void OnEnable()
    {
        foreach (RepairSlot slot in slots)
        {
            if (slot != null)
            {
                slot.OnRepaired += HandleSlotRepaired;
            }
        }
    }

    private void OnDisable()
    {
        foreach (RepairSlot slot in slots)
        {
            if (slot != null)
            {
                slot.OnRepaired -= HandleSlotRepaired;
            }
        }
    }

    private void Start()
    {
        repairedCount = 0;

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
            Debug.Log("所有缺口修复完成！");
        }
    }
}
