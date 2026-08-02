using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCarPanel : BaseUI
{
    public override void Init()
    {
        base.Init();

        m_UiUitil.Get("btn_next").AddListenrforBtn(onbtn_nextCallBack);
    }

    private void onbtn_nextCallBack()
    {
        GameManager.Instance.LoadLevel("Level_1");
    }

    public override void Clear()
    {
        m_UiUitil.Get("btn_next").RemoveListenerForBtn();
    }
}
