using System;
using UnityEngine;

public class FragmentAssemblyWipeStageController : MonoBehaviour
{
    private FragmentAssemblyManager assemblyManager;
    private FragmentWipeController wipeController;

    private FragmentAssemblyWipeUITool brushTool;
    private FragmentAssemblyWipeUITool towelTool;

    private GameObject wireframeObject;
    private GameObject backgroundObject;
    private GameObject cleanBackgroundObject;

    private float brushSize;
    private float towelSize;
    private float brushCompletePercent;
    private float towelCompletePercent;

    private bool hasCompleted;

    public FragmentAssemblyStage CurrentStage { get; private set; }

    public event Action OnLevelCompleted;

    public void Init(
        FragmentAssemblyManager manager,
        FragmentWipeController wipe,
        FragmentAssemblyWipeUITool brush,
        FragmentAssemblyWipeUITool towel,
        GameObject wireframe,
        GameObject background,
        GameObject cleanBackground,
        FragmentAssemblyLevelData data)
    {
        Unsubscribe();

        assemblyManager = manager;
        wipeController = wipe;
        brushTool = brush;
        towelTool = towel;
        wireframeObject = wireframe;
        backgroundObject = background;
        cleanBackgroundObject = cleanBackground;

        brushSize = data.brushSize;
        towelSize = data.towelSize;
        brushCompletePercent = data.dustCompletePercent;
        towelCompletePercent = data.polishCompletePercent;

        hasCompleted = false;
        CurrentStage = FragmentAssemblyStage.Assembly;

        if (wireframeObject != null)
        {
            wireframeObject.SetActive(true);
        }

        if (backgroundObject != null)
        {
            backgroundObject.SetActive(false);
        }

        if (cleanBackgroundObject != null)
        {
            cleanBackgroundObject.SetActive(false);
        }

        if (wipeController != null)
        {
            wipeController.DisableWiping();
            wipeController.OnWipeCompleted += HandleWipeCompleted;
        }

        if (brushTool != null)
        {
            brushTool.DisableTool();
        }

        if (towelTool != null)
        {
            towelTool.DisableTool();
        }

        if (assemblyManager != null)
        {
            assemblyManager.OnAssemblyCompleted +=
                HandleAssemblyCompleted;
        }
    }

    private void HandleAssemblyCompleted()
    {
        if (CurrentStage != FragmentAssemblyStage.Assembly) return;

        CurrentStage = FragmentAssemblyStage.DustBrushing;

        if (wireframeObject != null)
        {
            wireframeObject.SetActive(false);
        }

        if (assemblyManager != null)
        {
            assemblyManager.SetPiecesVisible(false);
        }

        if (cleanBackgroundObject != null)
        {
            cleanBackgroundObject.SetActive(true);
        }

        if (backgroundObject != null)
        {
            backgroundObject.SetActive(true);
        }

        // 刷子：从 100% Alpha 擦到 50%。
        wipeController.BeginStage(
            brushSize,
            brushCompletePercent,
            1f,
            0.5f
        );

        brushTool.EnableTool();
    }

    private void HandleWipeCompleted()
    {
        if (CurrentStage == FragmentAssemblyStage.DustBrushing)
        {
            HandleBrushCompleted();
        }
        else if (CurrentStage == FragmentAssemblyStage.TowelPolishing)
        {
            HandleTowelCompleted();
        }
    }

    private void HandleBrushCompleted()
    {
        brushTool.DisableTool();

        // 包括未刷到的剩余区域，整图统一为 50%。
        wipeController.SetUniformAlpha(0.5f);

        CurrentStage = FragmentAssemblyStage.TowelPolishing;

        // 毛巾：清空遮罩和进度，从 50% Alpha 擦到 0%。
        wipeController.BeginStage(
            towelSize,
            towelCompletePercent,
            0.5f,
            1f
        );

        towelTool.EnableTool();
    }

    private void HandleTowelCompleted()
    {
        if (hasCompleted) return;

        hasCompleted = true;
        CurrentStage = FragmentAssemblyStage.Completed;

        towelTool.DisableTool();

        // 达标后清除全部残留，只保留干净底图。
        wipeController.SetUniformAlpha(0f);

        if (backgroundObject != null)
        {
            backgroundObject.SetActive(false);
        }

        OnLevelCompleted?.Invoke();
    }

    private void Unsubscribe()
    {
        if (assemblyManager != null)
        {
            assemblyManager.OnAssemblyCompleted -=
                HandleAssemblyCompleted;
        }

        if (wipeController != null)
        {
            wipeController.OnWipeCompleted -= HandleWipeCompleted;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}