

using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY, bool first = false)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (isWhiteTeam ? 1 : -1);

        //MOVE TO FRONT
        for (int i = 1; i < tileCountY - currentY; i++)
        {
           
            if (board[currentX, currentY + i] == null)
                r.Add(new Vector2Int(currentX, currentY + i));
            else
            {
                if (board[currentX , currentY+i].isWhiteTeam != isWhiteTeam)
                    r.Add(new Vector2Int(currentX , currentY+i));
                break;
            }
        }
        //MOVE TO BACK
        for (int i = 1; i <= currentY; i++)
        {
            if (board[currentX, currentY - i] == null)
                r.Add(new Vector2Int(currentX, currentY - i));
            else
            {
                if (board[currentX, currentY - i].isWhiteTeam != isWhiteTeam)
                    r.Add(new Vector2Int(currentX , currentY - i));
                break;
            }
        }
        //MOVE TO RIGHT

        for (int i = 1; i < tileCountX - currentX; i++)
        {
            if (board[currentX + i, currentY] == null)
                r.Add(new Vector2Int(currentX + i, currentY));
            else
            {
                if (board[currentX + i, currentY].isWhiteTeam != isWhiteTeam)
                    r.Add(new Vector2Int(currentX + i, currentY));
                break;
            }
        }
        //MOVE TO LEFT
        for (int i = 1; i <= currentX; i++)
        {
            if (board[currentX - i, currentY] == null)
                r.Add(new Vector2Int(currentX - i, currentY));
            else
            {
                if (board[currentX - i, currentY].isWhiteTeam != isWhiteTeam)
                r.Add(new Vector2Int(currentX - i, currentY));
                break;
            }
                
        }

        return r;
    }
}
