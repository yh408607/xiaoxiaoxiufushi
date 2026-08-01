using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLevelPanel : BaseUI
{
    public override void Init()
    {
        base.Init();

        //兵马俑
        m_UiUitil.Get("Scroll View/Viewport/Content/level_1").AddListenrforBtn(() =>
        {
            loadLevelPanel("BingMaYong");
        });

        //陶瓷
        m_UiUitil.Get("Scroll View/Viewport/Content/level_2").AddListenrforBtn(() =>
        {
            loadLevelPanel("TaoChi");
        });
    }


    public override void Show()
    {
        base.Show();

        //判断是否解锁关卡

    }



    private void loadLevelPanel(string leveName)
    {
        //需要判断是否解锁





        //解锁后才能加载关卡

        var temp = UIPanelManager.Instance.ShownPanel("UIPanel/level_panel");
        temp.Show(leveName);

        this.Hide();

    }
}
