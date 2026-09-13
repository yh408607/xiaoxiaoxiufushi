using System;
using UnityEngine;

/// <summary>
/// 拼接关卡的擦拭阶段控制器。
/// 管理刷子除尘和毛巾擦亮两个连续阶段。
/// </summary>
public class FragmentAssemblyWipeStageController : MonoBehaviour
{
    private FragmentAssemblyManager assemblyManager;
    private FragmentWipeController dustController;
    private FragmentWipeController polishController;
    private FragmentAssemblyWipeUITool brushTool;
    private FragmentAssemblyWipeUITool towelTool;
    private GameObject wireframeObject;
    private GameObject backgroundObject;
    private bool hasCompleted;

    public FragmentAssemblyStage CurrentStage { get; private set; }
    public event Action OnLevelCompleted;

    public void Init(
        FragmentAssemblyManager manager,
        FragmentWipeController dust,
        FragmentWipeController polish,
        FragmentAssemblyWipeUITool brush,
        FragmentAssemblyWipeUITool towel,
        GameObject wireframe,
        GameObject background)
    {
        Unsubscribe();
        assemblyManager = manager;
        dustController = dust;
        polishController = polish;
        brushTool = brush;
        towelTool = towel;
        wireframeObject = wireframe;
        backgroundObject = background;
        hasCompleted = false;
        CurrentStage = FragmentAssemblyStage.Assembly;

        Debug.Log(
            "[FragmentAssemblyWipeStageController] 初始化。"
            + " assemblyManager=" + (assemblyManager != null)
            + " dustController=" + (dustController != null)
            + " polishController=" + (polishController != null)
            + " brushTool=" + (brushTool != null)
            + " towelTool=" + (towelTool != null),
            this
        );

        if (wireframeObject != null)
        {
            wireframeObject.SetActive(true);
        }

        if (backgroundObject != null)
        {
            backgroundObject.SetActive(false);
        }

        DisableAllWipeTools();
        if (assemblyManager != null)
        {
            assemblyManager.OnAssemblyCompleted +=
                HandleAssemblyCompleted;
        }

        if (dustController != null)
        {
            dustController.OnWipeCompleted += HandleDustCompleted;
        }

        if (polishController != null)
        {
            polishController.OnWipeCompleted += HandlePolishCompleted;
        }
    }

    private void HandleAssemblyCompleted()
    {
        if (CurrentStage != FragmentAssemblyStage.Assembly) return;

        CurrentStage = FragmentAssemblyStage.DustBrushing;

        Debug.Log(
            "[FragmentAssemblyWipeStageController] 拼接完成，进入刷子除尘阶段。",
            this
        );

        if (wireframeObject != null)
        {
            wireframeObject.SetActive(false);
        }

        if (backgroundObject != null)
        {
            backgroundObject.SetActive(true);
        }

        // 完整底图已经显示后隐藏所有碎片，避免碎片之间的细小边缘或缝隙影响最终效果。
        if (assemblyManager != null)
        {
            assemblyManager.SetPiecesVisible(false);
        }

        if (dustController != null)
        {
            dustController.EnableWiping();
        }

        if (brushTool != null)
        {
            brushTool.EnableTool();

            Debug.Log(
                "[FragmentAssemblyWipeStageController] 已启用刷子。"
                + " activeSelf=" + brushTool.gameObject.activeSelf
                + " isEnabled=" + brushTool.IsEnabled,
                brushTool
            );
        }
        else
        {
            Debug.LogError(
                "[FragmentAssemblyWipeStageController] 刷子引用为空，无法显示刷子 UI。",
                this
            );
        }
    }

    private void HandleDustCompleted()
    {
        if (CurrentStage != FragmentAssemblyStage.DustBrushing) return;

        CurrentStage = FragmentAssemblyStage.TowelPolishing;
        if (brushTool != null)
        {
            brushTool.DisableTool();
        }

        if (polishController != null)
        {
            polishController.EnableWiping();
        }

        if (towelTool != null)
        {
            towelTool.EnableTool();
        }
    }

    private void HandlePolishCompleted()
    {
        if (CurrentStage != FragmentAssemblyStage.TowelPolishing ||
            hasCompleted)
        {
            return;
        }

        hasCompleted = true;
        CurrentStage = FragmentAssemblyStage.Completed;
        if (towelTool != null)
        {
            towelTool.DisableTool();
        }

        OnLevelCompleted?.Invoke();
    }

    private void DisableAllWipeTools()
    {
        if (dustController != null) dustController.DisableWiping();
        if (polishController != null) polishController.DisableWiping();
        if (brushTool != null) brushTool.DisableTool();
        if (towelTool != null) towelTool.DisableTool();
    }

    private void Unsubscribe()
    {
        if (assemblyManager != null)
        {
            assemblyManager.OnAssemblyCompleted -=
                HandleAssemblyCompleted;
        }

        if (dustController != null)
        {
            dustController.OnWipeCompleted -= HandleDustCompleted;
        }

        if (polishController != null)
        {
            polishController.OnWipeCompleted -= HandlePolishCompleted;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
