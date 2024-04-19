

using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY, bool first = false)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        int x, y;


        //RIGHT
        x = currentX + 1;

        if (x < tileCountX)
        {
            y = currentY;
            if (board[x,y]== null || board[x,y].isWhiteTeam != isWhiteTeam)  {
                r.Add(new Vector2Int(x, y));
            }
            y = currentY + 1;
            if (y < tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
                r.Add(new Vector2Int(x, y));
            y = currentY - 1;

            if (y >= 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
                r.Add(new Vector2Int(x, y));
        }

        //LEFT
        x = currentX - 1;

        if (x >= 0)
        {
            y = currentY;
            if (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam)
            {
                r.Add(new Vector2Int(x, y));
            }
            y = currentY + 1;
            if (y < tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
                r.Add(new Vector2Int(x, y));
            y = currentY - 1;

            if (y >= 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
                r.Add(new Vector2Int(x, y));
        }

        //UP
        x = currentX;
        y = currentY + 1;

        if(y< tileCountY && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            r.Add(new Vector2Int(x, y));

        //DOWN

        x = currentX;
        y = currentY - 1;

        if (y > 0 && (board[x, y] == null || board[x, y].isWhiteTeam != isWhiteTeam))
            r.Add(new Vector2Int(x, y));
        return r;
    }
}
