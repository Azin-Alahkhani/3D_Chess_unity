

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

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveHistoryList, ref List<Vector2Int> availableMoves)
    {
        int direction = (isWhiteTeam) ? 1 : -1;
       
        if (moveHistoryList.Count > 0)
        {
            Vector2Int[] lastMove = moveHistoryList[moveHistoryList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn )
            {

                
                if (lastMove[1].y == currentY)
                    {
                    
                    if (lastMove[1].x == currentX - 1) //on the left
                        {
                        Debug.Log("left" );
                        availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                            return SpecialMove.Enpassant;
                        }
                        if (lastMove[1].x == currentX + 1) //on the right
                        {
                        Debug.Log("right ");
                        availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                            return SpecialMove.Enpassant;
                        }
                    }
                

            }

            if ((isWhiteTeam && currentY == 6 ) ||(!isWhiteTeam && currentY == 1))
                return SpecialMove.Promotion;
        }


        return SpecialMove.None;
    }
}
