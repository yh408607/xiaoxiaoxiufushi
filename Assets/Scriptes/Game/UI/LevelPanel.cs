using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPanel : BaseUI
{
    public List<Level_item> level_items = new List<Level_item>();
    public override void Init()
    {
        base.Init();

       GetComponentsInChildren<Level_item>(level_items);
        if(level_items.Count > 0)
        {
            foreach (var item in level_items)
            {
                item.Init();
            }
        }
        else
        {
            Debug.LogError("No Level_item components found in children.");
        }
    }


}

