using System;
using UnityEngine;

/// <summary>
/// 碎片拼接管理器。
/// 负责监听所有碎片的固定事件，并判断整件文物是否拼接完成。
/// </summary>
public class FragmentAssemblyManager : MonoBehaviour
{
    // 当前关卡中的全部碎片。
    private FragmentAssemblyPiece[] pieces;

    // 已经正确固定的碎片数量。
    private int fixedCount;
    private bool assemblyCompleted;

    /// <summary>
    /// 全部碎片完成拼接时触发。
    /// </summary>
    public event Action OnAssemblyCompleted;

    /// <summary>
    /// 已完成拼接的碎片数量。
    /// </summary>
    public int FixedCount => fixedCount;

    /// <summary>
    /// 当前关卡碎片总数量。
    /// </summary>
    public int PieceCount => pieces == null ? 0 : pieces.Length;

    /// <summary>
    /// 设置所有碎片对象的显示状态。
    /// 拼接完成后可以隐藏碎片，只保留完整底图作为最终视觉效果。
    /// </summary>
    public void SetPiecesVisible(bool visible)
    {
        if (pieces == null)
        {
            return;
        }

        foreach (FragmentAssemblyPiece piece in pieces)
        {
            if (piece != null)
            {
                piece.gameObject.SetActive(visible);
            }
        }
    }

    /// <summary>
    /// 初始化并监听所有碎片。
    /// </summary>
    public void Init(FragmentAssemblyPiece[] assemblyPieces)
    {
        UnsubscribeFromPieces();

        pieces = assemblyPieces ?? new FragmentAssemblyPiece[0];
        fixedCount = 0;
        assemblyCompleted = false;

        Debug.Log(
            "[FragmentAssemblyManager] 初始化。碎片数量："
            + pieces.Length,
            this
        );

        foreach (FragmentAssemblyPiece piece in pieces)
        {
            if (piece == null)
            {
                continue;
            }

            piece.OnFixed -= HandlePieceFixed;
            piece.OnFixed += HandlePieceFixed;

            // 如果初始化时碎片已经固定，也要计入完成数量。
            if (piece.IsFixed)
            {
                fixedCount++;
            }
        }

        Debug.Log(
            "[FragmentAssemblyManager] 初始化完成。固定数量："
            + fixedCount + "/" + PieceCount,
            this
        );

        CheckAssemblyCompleted();
    }

    /// <summary>
    /// 响应单个碎片固定事件。
    /// </summary>
    private void HandlePieceFixed(FragmentAssemblyPiece piece)
    {
        if (piece == null)
        {
            Debug.LogWarning(
                "[FragmentAssemblyManager] 收到空碎片的固定事件。",
                this
            );
            return;
        }

        fixedCount++;

        Debug.Log(
            "[FragmentAssemblyManager] 碎片固定："
            + piece.PieceId
            + "，进度：" + fixedCount + "/" + PieceCount,
            this
        );

        CheckAssemblyCompleted();
    }

    /// <summary>
    /// 判断是否所有碎片都已经固定。
    /// </summary>
    private void CheckAssemblyCompleted()
    {
        if (PieceCount == 0 || assemblyCompleted)
        {
            return;
        }

        if (fixedCount >= PieceCount)
        {
            assemblyCompleted = true;

            Debug.Log(
                "[FragmentAssemblyManager] 全部碎片已固定，触发拼接完成事件。",
                this
            );

            OnAssemblyCompleted?.Invoke();
        }
    }

    /// <summary>
    /// 取消对碎片事件的监听，避免对象销毁后残留引用。
    /// </summary>
    private void UnsubscribeFromPieces()
    {
        if (pieces == null)
        {
            return;
        }

        foreach (FragmentAssemblyPiece piece in pieces)
        {
            if (piece != null)
            {
                piece.OnFixed -= HandlePieceFixed;
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromPieces();
    }
}
