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

        m_UiUitil.Get("btn_return").AddListenrforBtn(onbtnReturnCallBack);
    }

    public override void Show()
    {
        base.Show();
        if(level_items.Count > 0)
        {
            foreach (var item in level_items)
            {
                item.Show();
            }
        }
    }

    private void onbtnReturnCallBack()
    {
        UIPanelManager.Instance.ShownPanel("UIPanel/select_level_panel");
        Hide();
    }

    private void OnDestroy()
    {
        UIPanelManager.Instance.uipanelPool.Remove("UIPanel/level_panel");
    }



}

