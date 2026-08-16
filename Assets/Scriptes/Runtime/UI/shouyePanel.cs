using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shouyePanel : BaseUI
{
    public override void Init()
    {
        base.Init();
        m_UiUitil.Get("btn_toucth").AddListenrforBtn(OnToucthBtnClick);
    }

    public override void Show()
    {
        base.Show();

        //this.m_Animator.StartPlayback();
    }

    public void OnToucthBtnClick()
    {
        this.m_Animator.SetBool("is Mousedown", true);
        UIPanelManager.Instance.ShownPanel("UIPanel/main_Panal");
        this.Hide();
    }

    private void OnDestroy()
    {
        UIPanelManager.Instance.uipanelPool.Remove("UIPanel/shouyePanel");
    }
}
