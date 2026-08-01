using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : BaseUI
{
    private void Start()
    {
        
    }


    public override void Init()
    {
        //显示国宝图鉴按钮
        m_UiUitil.Get("btn_gbtj").AddListenrforBtn(() =>
        {
            showPanel("GuoBaoTuJian");
        });

        //声音开关按钮
        m_UiUitil.Get("btn_sykg").AddListenrforBtn(() =>
        {
            showPanel("Sources");
        });

        //家长中心按钮
        m_UiUitil.Get("btn_jzzx").AddListenrforBtn(() =>
        {
            showPanel("JiaZhangZhongXing");
        });

        //购买中心按钮
        m_UiUitil.Get("btn_hfgm").AddListenrforBtn(() =>
        {
            showPanel("GouMaiZhongXing");
        });

        //购买中心按钮
        m_UiUitil.Get("btn_ksxf").AddListenrforBtn(() =>
        {
            showPanel("StarGame");
        });
    }


    private void showPanel(string panelName)
    {
        switch (panelName)
        {
            case "GuoBaoTuJian":
               // m_UiUitil.ShowPanel("GuoBaoTuJian");
                break;


            case "Sources":
                break;
            case "JiaZhangZhongXing":
                break;
            case "GouMaiZhongXing":
                break;


            case "StarGame":

                UIPanelManager.Instance.ShownPanel("UIPanel/select_level_panel");
                break;

            default:
                break;
        }

        this.Hide();
    }
}
