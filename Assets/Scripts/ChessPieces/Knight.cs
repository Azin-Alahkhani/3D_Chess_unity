

using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    [SerializeField] private Animator animator;
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY, bool first = false)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int x, y;
        x = currentX + 2;
        if (x < tileCountX)
        {
            y = currentY - 1;
            if (y >= 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x, y));
            }
            y = currentY + 1;
            if (y < tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x,y));
            }
        }
        x = currentX - 2;
        if (x>= 0)
        {
            y = currentY - 1;
            if (y >= 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x, y));
            }
            y = currentY + 1;
            if (y < tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x,y));
            }
        }
        y = currentY + 2;
        if (y< tileCountY)
        {
            x = currentX - 1;
            if (x >= 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x,y));
            }
            x = currentX + 1;
            if (x < tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        y = currentY - 2;

        if (y >= 0)
        {
            x = currentX - 1;
            if (x >= 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x, y));
            }
            x = currentX + 1;
            if (x < tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            {
                r.Add(new Vector2Int(x, y));
            }
        }


        return r;
    }

   
}
