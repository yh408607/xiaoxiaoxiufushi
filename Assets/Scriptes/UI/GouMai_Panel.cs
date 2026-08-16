using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GouMai_Panel : BaseUI
{
    public override void Init()
    {
        m_UiUitil.Get("btn_buy").AddListenrforBtn(BuyItem);
        m_UiUitil.Get("btn_return").AddListenrforBtn(btn_return);
    }

    /// <summary>
    /// 这里是接家长中心与苹果账号购买内容
    /// </summary>
    private void BuyItem()
    {
        SfxManager.Instance.Play(SfxId.ButtonClick);
    }

    private void btn_return()
    {
        SfxManager.Instance.Play(SfxId.ButtonClick);
        Hide();

    }
    
}
