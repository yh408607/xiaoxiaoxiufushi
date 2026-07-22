using System.Collections;
using UnityEngine;

public class PieceIdentity : MonoBehaviour
{
    [SerializeField] private string pieceId;

    public string PieceId => pieceId;

    public void Init(string id)
    {
        pieceId = id;
    }
}