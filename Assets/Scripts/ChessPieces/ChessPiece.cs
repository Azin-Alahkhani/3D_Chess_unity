using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    king = 6
}
public class ChessPiece : MonoBehaviour
{
  
    public ChessPieceType type;

    public int currentX;
    public int currentY;
    public bool isWhiteTeam;

    public Vector3 desiredPosition;
    public Vector3 desiredScale = Vector3.one;

    public float baseScale;

    public int lerpTime = 200;

    private void Start()
    {
       
        if (type == ChessPieceType.Knight) {
            baseScale = 0.14f;

            desiredScale = new Vector3(0.14f, 0.14f, 0.14f);

        }
        else
        {
            baseScale = 1;
            desiredScale = Vector3.one;
        }
        transform.rotation = Quaternion.Euler(isWhiteTeam ? Vector3.zero : new Vector3(0, 180, 0));
    }
    public void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 20);
    }
    public virtual void SetPosition(Vector3 position, bool force=false) { 
        desiredPosition = position;

        if (force)
        {
            transform.position = desiredPosition;
        }
       
    }

    public virtual void SetScale(Vector3 scale, bool force=false)
    {
        desiredScale = baseScale* scale;

        if (force)
        {
            transform.localScale = desiredScale;
        }

    }

    public virtual List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board , int tileCountX, int tileCountY, bool isFirstMove=false)
    {
            List<Vector2Int> r = new List<Vector2Int>();


        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }
    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board,  ref List<Vector2Int[]> moveHistoryList, ref List<Vector2Int> availableMoves)
    {

        return SpecialMove.None;

    }


    }
