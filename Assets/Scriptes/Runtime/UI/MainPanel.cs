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
            SfxManager.Instance?.Play(SfxId.ButtonClick);
            showPanel("GuoBaoTuJian");
        });

        //声音开关按钮
        m_UiUitil.Get("btn_sykg").AddListenrforBtn(() =>
        {
            SfxManager.Instance?.Play(SfxId.ButtonClick);
            showPanel("Sources");
        });

        //家长中心按钮
        m_UiUitil.Get("btn_jzzx").AddListenrforBtn(() =>
        {
            SfxManager.Instance?.Play(SfxId.ButtonClick);
            showPanel("JiaZhangZhongXing");
        });

        //购买中心按钮
        m_UiUitil.Get("btn_hfgm").AddListenrforBtn(() =>
        {
            SfxManager.Instance?.Play(SfxId.ButtonClick);
            showPanel("GouMaiZhongXing");
        });

        //购买中心按钮
        m_UiUitil.Get("btn_ksxf").AddListenrforBtn(() =>
        {
            SfxManager.Instance?.Play(SfxId.ButtonClick);
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
                UIPanelManager.Instance.ShownPanel("UIPanel/ParentVerify_Panel");

                break;
            case "GouMaiZhongXing":

                UIPanelManager.Instance.ShownPanel("UIPanel/goumai_panel");
                break;


            case "StarGame":

                UIPanelManager.Instance.ShownPanel("UIPanel/select_level_panel");
                break;

            default:
                break;
        }

       // this.Hide();
    }

    private void OnDestroy()
    {
        UIPanelManager.Instance.uipanelPool.Remove("UIPanel/main_Panal");
    }
}
