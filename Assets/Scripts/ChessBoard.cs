using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SpecialMove
{
    None = 0,
    Enpassant = 1,
    Castling = 2,
    Promotion = 3,
    firstWhitePawn = 4
}
public class ChessBoard : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private Material material;
    [SerializeField] private Material hoverMaterial;

    //CONSTS
    private const int X_COUNT = 8;
    private const int Y_COUNT = 8;
    private float tileSize = 1;
    private float yOffset = 2.15f;
    private const float deathSize = 0.4f;
    private const float deathSpacing = 0.4f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    //ART
    [SerializeField] private GameObject[] piecesPrefabs;
    [SerializeField] private Material[] teamMaterials; //0 white, 1 black
    [SerializeField] private GameObject[] piecesPrefabs2;

    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject pauseScreen;

    //LOGIC
    private ChessPiece[,] chessPieces;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHoveringPosition = -Vector2Int.one;
    private Vector3 bounds;
    public ChessPiece currentlyDragging;
    public List<ChessPiece> deadWhites = new List<ChessPiece>();
    public List<ChessPiece> deadBlack = new List<ChessPiece>();
    public float draggingOffset = 0.75f;
    public List<Vector2Int> availableMoves = new List<Vector2Int>();
    public bool isFirstMove;
    public bool isWhiteTurn;

    public List<Vector2Int[]> moveHistoryList = new List<Vector2Int[]>();
    public SpecialMove specialMove;

    public bool isPaused = false;

    //UI
    public TMP_Text timerTXT;
    private float timer = 0;
    public GameObject[] turnsTxt;
    public TMP_Text[] deadCounts;
    public GameObject checkTextGo;

    private void Awake()
    {
        isFirstMove = true;
        isWhiteTurn = true;
        GenerateTiles(tileSize, X_COUNT, Y_COUNT);
        SpawnPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                pauseScreen.SetActive(true);
            }
            else pauseScreen.SetActive(false);
        }
        if (!isPaused)
        {
            UpdateTimer();
            RaycastHit info;
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
            {
                Vector2Int hitPosition = LookUpTileIndex(info.transform.gameObject);

                if (Input.GetMouseButtonDown(0))
                {


                    if (chessPieces[hitPosition.x, hitPosition.y] != null)
                    {

                        //check turn here
                        if (chessPieces[hitPosition.x, hitPosition.y].isWhiteTeam == isWhiteTurn)
                        {
                            currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                            if (isFirstMove && currentlyDragging.type == ChessPieceType.Pawn)
                            {
                                availableMoves = currentlyDragging.GetPossibleMoves(ref chessPieces, X_COUNT, Y_COUNT, true);
                            }
                            else
                                availableMoves = currentlyDragging.GetPossibleMoves(ref chessPieces, X_COUNT, Y_COUNT);
                            HighlightTiles();
                            specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveHistoryList, ref availableMoves);


                            PreventCheckMate();

                        }
                    }
                }
                if (Input.GetMouseButtonUp(0) && currentlyDragging != null)
                {
                    Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                    bool isValidMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);

                    if (!isValidMove)
                    {
                        currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));

                    }

                    //put it back

                    currentlyDragging = null;
                    RemoveHighlightTiles();

                }


                if (currentHoveringPosition == -Vector2Int.one)
                {
                    currentHoveringPosition = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                    ChangeTileColorOnHoverChange(hoverMaterial, tiles[currentHoveringPosition.x, currentHoveringPosition.y]);

                }
                if (currentHoveringPosition != hitPosition)
                {
                    tiles[currentHoveringPosition.x, currentHoveringPosition.y].layer = ContainsValidMove(ref availableMoves, currentHoveringPosition) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                    currentHoveringPosition = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }


            }

            else
            {

                if (currentHoveringPosition != -Vector2Int.one)
                {

                    tiles[currentHoveringPosition.x, currentHoveringPosition.y].layer = ContainsValidMove(ref availableMoves, currentHoveringPosition) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                    currentHoveringPosition = -Vector2Int.one;
                }
                if (currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                    currentlyDragging = null;
                    RemoveHighlightTiles();

                }
            }

            if (currentlyDragging)
            {
                Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
                float distance = 0;
                if (horizontalPlane.Raycast(ray, out distance))
                {
                    currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * draggingOffset);
                }
            }

        }
    }

    public void UpdateTimer()
    {
        timer += Time.deltaTime;

        int seconds = (int)(timer % 60);
        int minutes = (int)(timer / 60);

        timerTXT.text = (minutes > 9 ? minutes : "0" + minutes) + " : " + (seconds > 9 ? seconds : "0" + seconds);
    }

    public void UpdateDeadCounts()
    {
        deadCounts[0].text = "White : " + deadWhites.Count;
        deadCounts[1].text = "Black : " + deadBlack.Count;
    }
    public void PreventCheckMate()
    {

        ChessPiece targetKing = null;

        for (int x = 0; x < X_COUNT; x++)
        {
            for (int y = 0; y < Y_COUNT; y++)
            {
                if (chessPieces[x, y] != null)
                    if (chessPieces[x, y].type == ChessPieceType.king)
                    {
                        if (chessPieces[x, y].isWhiteTeam == currentlyDragging.isWhiteTeam)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
            }
        }

        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);

    }

    public void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        //
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        //

        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);

            if (cp.type == ChessPieceType.king)
                kingPositionThisSim = new Vector2Int(simX, simY);

            ChessPiece[,] simBoard = new ChessPiece[X_COUNT, Y_COUNT];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            //simulating the board + a track of the enemy pieces that can potentially kill the king(all)

            for (int x = 0; x < X_COUNT; x++)
            {
                for (int y = 0; y < Y_COUNT; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simBoard[x, y] = chessPieces[x, y];
                        if (chessPieces[x, y].isWhiteTeam != currentlyDragging.isWhiteTeam)
                            simAttackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
            //simulate the move

            simBoard[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simBoard[simX, simY] = cp;


            var deadEnemy = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);

            if (deadEnemy != null)
                simAttackingPieces.Remove(deadEnemy);

            //get all the enemy pieces' available moves
            List<Vector2Int> attackMoves = new List<Vector2Int>();
            for (int j = 0; j < simAttackingPieces.Count; j++)
            {
                var enemySimMoves = simAttackingPieces[j].GetPossibleMoves(ref simBoard, X_COUNT, Y_COUNT);
                for (int k = 0; k < enemySimMoves.Count; k++)
                {
                    attackMoves.Add(enemySimMoves[k]);
                }
            }
            // Does the simulated move endanger our king?
            if (ContainsValidMove(ref attackMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            cp.currentX = actualX;
            cp.currentY = actualY;

        }

        //remove from current available move list
        foreach (var move in movesToRemove)
        {
            moves.Remove(move);
        }


    }

    
    public bool IsItCheckMate()
    {
        var lastMove = moveHistoryList[moveHistoryList.Count-1];
        bool targetTeamIsWhite = (chessPieces[lastMove[1].x, lastMove[1].y].isWhiteTeam);

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();


        ChessPiece targetKing = null;
        for (int x = 0; x < X_COUNT; x++)
        {
            for (int y = 0; y < Y_COUNT; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x,y].isWhiteTeam == targetTeamIsWhite)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.king)
                            targetKing = chessPieces[x, y];
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                    
                }
            }
        }


        List<Vector2Int> currentPossibleMoves = new List<Vector2Int>();
        for(int i = 0; i< attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetPossibleMoves(ref chessPieces, X_COUNT, Y_COUNT);
            for (int j = 0; j < pieceMoves.Count; j++)
            {
                currentPossibleMoves.Add(pieceMoves[j]);
            }
        }

        if(ContainsValidMove(ref currentPossibleMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
           // checkTextGo.SetActive(true);
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                var defendingMoves = defendingPieces[i].GetPossibleMoves(ref chessPieces, X_COUNT, Y_COUNT);
                Debug.Log(defendingMoves[0]);
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                Debug.Log(defendingMoves.Count);
                if (defendingMoves.Count == 0)
                    return true;
            }
        }
        
        return false;
    }

    private void GenerateTiles(float tileSize, int xCount, int yCount)
    {

        yOffset += transform.position.y;
        bounds = new Vector3((xCount / 2) * tileSize, 0, (yCount / 2) * tileSize) + boardCenter;

        tiles = new GameObject[xCount, yCount];

        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileGameObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileGameObject.transform.parent = transform;

        Mesh mesh = new Mesh();

        tileGameObject.AddComponent<MeshFilter>().mesh = mesh;
        tileGameObject.AddComponent<MeshRenderer>().material = material;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;


        mesh.RecalculateNormals();

        tileGameObject.layer = LayerMask.NameToLayer("Tile");
        tileGameObject.AddComponent<BoxCollider>();



        return tileGameObject;
    }

    public void SpawnPieces()
    {

        chessPieces = new ChessPiece[X_COUNT, Y_COUNT];
        //white team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, true);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, true);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, true);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, true);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.king, true);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, true);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, true);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, true);

        for (int i = 0; i < X_COUNT; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, true);
        }

        //black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, false);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, false);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, false);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.king, false);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, false);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, false);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, false);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, false);

        for (int i = 0; i < X_COUNT; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, false);
        }


    }

    public ChessPiece SpawnSinglePiece(ChessPieceType type, bool isWhiteTeam)
    {
        ChessPiece cp;

        int index = (isWhiteTeam) ? (int)type - 1 : (int)type + 5;

        cp = Instantiate(piecesPrefabs2[index], transform).GetComponent<ChessPiece>();



        cp.type = type;
        cp.isWhiteTeam = isWhiteTeam;


        return cp;

    }

    public void PositionAllPieces()
    {
        for (int x = 0; x < X_COUNT; x++)
            for (int y = 0; y < Y_COUNT; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }

    public void PositionSinglePiece(int xC, int yC, bool force)
    { //force is to decide whether the positioning is to be smooth(mid game) or rapid(start)    
        chessPieces[xC, yC].currentX = xC;
        chessPieces[xC, yC].currentY = yC;
        chessPieces[xC, yC].SetPosition(GetTileCenter(xC, yC), force);
    }

    public Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    public Vector2Int LookUpTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < X_COUNT; x++)
            for (int y = 0; y < Y_COUNT; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);


        return -Vector2Int.one;
    }

    public void ChangeTileColorOnHoverChange(Material mat, GameObject tile)
    {
        // tile.GetComponent<MeshRenderer>().material = mat;
    }

    public bool MoveTo(ChessPiece currentPiece, int x, int y)
    {
        bool canMove = true;

        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        //is the tile occupid?
        if (chessPieces[x, y] != null)
        {

            ChessPiece otherPiece = chessPieces[x, y];
            if (otherPiece.isWhiteTeam == currentPiece.isWhiteTeam)
            {
                return false;
            }
            else
            {
                if (otherPiece.type == ChessPieceType.king)
                    CheckMate(otherPiece.isWhiteTeam);

                if (otherPiece.isWhiteTeam)
                {


                    deadWhites.Add(otherPiece);
                    otherPiece.SetScale(deathSize * Vector3.one);
                    otherPiece.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                        - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + Vector3.forward * deathSpacing * deadWhites.Count);

                }
                else
                {
                    deadBlack.Add(otherPiece);
                    otherPiece.SetScale(deathSize * Vector3.one);
                    otherPiece.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                        - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + Vector3.back * deathSpacing * deadBlack.Count);
                }
                UpdateDeadCounts();


            }
        }

        Vector2Int previousPosition = new Vector2Int(currentPiece.currentX, currentPiece.currentY);

        chessPieces[x, y] = currentPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y, false);

        isWhiteTurn = !isWhiteTurn;

        ToggleTurnText();
        isFirstMove = false;

        moveHistoryList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();

        if (IsItCheckMate())
            CheckMate(!currentPiece.isWhiteTeam);

        return true;
    }
    public GameObject blackCamera;
    void ToggleTurnText()
    {
        turnsTxt[0].SetActive(isWhiteTurn);
        turnsTxt[1].SetActive(!isWhiteTurn);

        blackCamera.SetActive(!isWhiteTurn);
        
    }

    public void HighlightTiles()
    {
        foreach (var tile in availableMoves)
        {
            tiles[tile.x, tile.y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    public void RemoveHighlightTiles()
    {
        foreach (var tile in availableMoves)
        {
            tiles[tile.x, tile.y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }

    public bool ContainsValidMove(ref List<Vector2Int> validMoves, Vector2Int tile)
    {
        foreach (Vector2Int move in validMoves)
        {
            if (move.x == tile.x && move.y == tile.y)
                return true;
        }
        return false;
    }

    public void CheckMate(bool isWhiteTeam)
    {
        isPaused = true;
        DisplayEndScreen(!isWhiteTeam);

    }


    public void ProcessSpecialMove()
    {

        if (specialMove == SpecialMove.Enpassant)
        {
            ChessPiece enemyPawn = chessPieces[moveHistoryList[moveHistoryList.Count - 2][1].x, moveHistoryList[moveHistoryList.Count - 2][1].y];
            if (enemyPawn.isWhiteTeam)
            {


                deadWhites.Add(enemyPawn);
                enemyPawn.SetScale(deathSize * Vector3.one);
                enemyPawn.SetPosition(
                    new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + Vector3.forward * deathSpacing * deadWhites.Count);

            }
            else
            {
                deadBlack.Add(enemyPawn);
                enemyPawn.SetScale(deathSize * Vector3.one);
                enemyPawn.SetPosition(
                    new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + Vector3.back * deathSpacing * deadBlack.Count);
            }




        }


        if(specialMove == SpecialMove.Promotion)
        {
            Vector2Int lastMove = moveHistoryList[moveHistoryList.Count - 1][1];
            ChessPiece pawnToPromote = chessPieces[lastMove.x, lastMove.y];

            if((pawnToPromote.isWhiteTeam && pawnToPromote.currentY == 7)|| (!pawnToPromote.isWhiteTeam && pawnToPromote.currentY == 0))
            {
                ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, pawnToPromote.isWhiteTeam);
                newQueen.transform.position = chessPieces[lastMove.x, lastMove.y].transform.position;
                Destroy(chessPieces[lastMove.x, lastMove.y].gameObject);
                chessPieces[lastMove.x, lastMove.y] = newQueen;
                PositionSinglePiece(lastMove.x, lastMove.y, true);
            }
            
        }

        specialMove = SpecialMove.None;
    }

    public void DisplayEndScreen(bool isWhite)
    {
        endScreen.SetActive(true);
        endScreen.transform.GetChild(isWhite ? 0 : 1).gameObject.SetActive(true);
    }

    public void OnResetClicked()
    {

        Debug.Log("clicked");
        endScreen.SetActive(false);
        endScreen.transform.GetChild(0).gameObject.SetActive(false);
        endScreen.transform.GetChild(1).gameObject.SetActive(false);
        pauseScreen.SetActive(false);

        //reset fields
        currentlyDragging = null;
        availableMoves.Clear();
        moveHistoryList.Clear();
        timer = 0;
        timerTXT.text = "00 : 00";
        deadCounts[0].text = "White : 0";
        deadCounts[1].text = "Black : 0";
        //clear the board
        for (int i = 0; i < X_COUNT; i++)
        {
            for (int j = 0; j < Y_COUNT; j++)
            {
                if (chessPieces[i, j] != null)
                {
                    Destroy(chessPieces[i, j].gameObject);

                }
                chessPieces[i, j] = null;
            }
        }
        for (int i = 0; i < deadWhites.Count; i++)
        {
            Destroy(deadWhites[i].gameObject);
        }
        deadWhites.Clear();
        for (int i = 0; i < deadBlack.Count; i++)
        {
            Destroy(deadBlack[i].gameObject);
        }
        deadBlack.Clear();

        SpawnPieces();
        PositionAllPieces();
        isWhiteTurn = true;
        isFirstMove = true;
        isPaused = false;
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene(0);
    }



}
