

using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY, bool first = false)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (isWhiteTeam ? 1 : -1);

        //ONE IN FRONT
        if (currentY + direction > 0 && currentY + direction < tileCountY)
            if (board[currentX, currentY + direction] == null)
                r.Add(new Vector2Int(currentX, currentY + direction));

        //DIAGONAL
        if (currentY + direction > -1 && currentY + direction < tileCountY && currentX + direction > -1 && currentX + direction < tileCountX)
        {
            if (board[currentX + direction, currentY + direction] != null)
                r.Add(new Vector2Int(currentX + direction, currentY + direction));
        }
        if (currentY + direction > -1 && currentY + direction < tileCountY && currentX - direction > -1 && currentX - direction < tileCountX)
        {
            if (board[currentX - direction, currentY + direction] != null)
                r.Add(new Vector2Int(currentX - direction, currentY + direction));
        }

        //TWO IN FRONT

        if (isWhiteTeam && first)
        {
            r.Add(new Vector2Int(currentX, currentY + direction * 2));
        }

        return r;
    }
}
