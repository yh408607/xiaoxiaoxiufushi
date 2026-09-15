using UnityEngine;

public class ShowCarPanel : BaseUI
{
    private AudioSource narrationSource;

    public override void Init()
    {
        base.Init();

        m_UiUitil.Get("btn_next").AddListenrforBtn(onbtn_nextCallBack);
    }

    private void onbtn_nextCallBack()
    {
        StopNarration();
        GameManager.Instance.LoadLevel("Level_1");
    }

    public override void Show()
    {
        base.Show();
        StopNarration();

        FragmentAssemblyLevelLoader loader =
            FindObjectOfType<FragmentAssemblyLevelLoader>();
        FragmentAssemblyLevelData data =
            loader != null ? loader.CurrentLevelData : null;

        if (data == null || data.narrationClip == null)
        {
            Debug.LogWarning("当前关卡未配置文物讲解音频。", this);
            return;
        }

        // 使用独立音源，按钮音效不会打断讲解。
        if (narrationSource == null)
        {
            narrationSource = gameObject.AddComponent<AudioSource>();
            narrationSource.playOnAwake = false;
            narrationSource.loop = false;
            narrationSource.spatialBlend = 0f;
        }

        narrationSource.clip = data.narrationClip;
        narrationSource.Play();
    }

    public override void Hide()
    {
        StopNarration();
        base.Hide();
    }

    private void StopNarration()
    {
        if (narrationSource == null) return;

        narrationSource.Stop();
        narrationSource.clip = null;
    }

    private void OnDisable()
    {
        StopNarration();
    }

    public override void Clear()
    {
        StopNarration();
        m_UiUitil.Get("btn_next").RemoveListenerForBtn();
    }
}
