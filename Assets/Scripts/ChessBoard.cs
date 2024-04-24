using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SpecialMove
{
    None=0,
    Enpassant =1,
    Castling=2,
    prmotion=3,
    firstWhitePawn =4
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
    private float yOffset = 0.15f;
    private const float deathSize = 0.4f;
    private const float deathSpacing = 0.4f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    //ART
    [SerializeField] private GameObject[] piecesPrefabs;
    [SerializeField] private Material[] teamMaterials; //0 white, 1 black


    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject pauseScreen;

    //LOGIC
    private ChessPiece[,] chessPieces;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHoveringPosition = - Vector2Int.one;
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

    public bool isPaused= false;

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

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if(isPaused)
            {
                pauseScreen.SetActive(true);
            }
            else pauseScreen.SetActive(false);
        }
        if (!isPaused)
        {
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
                    tiles[currentHoveringPosition.x, currentHoveringPosition.y].layer = IsHighlighted(ref availableMoves, currentHoveringPosition) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                    currentHoveringPosition = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }


            }

            else
            {

                if (currentHoveringPosition != -Vector2Int.one)
                {

                    tiles[currentHoveringPosition.x, currentHoveringPosition.y].layer = IsHighlighted(ref availableMoves, currentHoveringPosition) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                    currentHoveringPosition = -Vector2Int.one;
                }
                if (currentlyDragging && Input.GetMouseButtonUp(0))
                {
                    currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                    currentlyDragging = null;
                    RemoveHighlightTiles();

                }
            }

            //if (currentlyDragging)
            //{
            //    Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            //    float distance = 0;
            //    if (horizontalPlane.Raycast(ray, out distance))
            //    {
            //        currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * draggingOffset);
            //    }
            //}

        }
    }
    public void PreventCheckMate()
    {

        ChessPiece targetKing = null;

        for(int x= 0; x< tiles.Length; x++)
        {
            for(int y=0;y< tiles.Length; y++)
            {
                if (chessPieces[x,y].type == ChessPieceType.king)
                {
                    if (chessPieces[x,y].isWhiteTeam == currentlyDragging.isWhiteTeam)
                    {
                        targetKing = chessPieces[x, y];
                    }
                }
            }
        }



    }

    public void SimulateMoveForSinglePiece( ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {

    }
    public void DisplayPauseMenu() { }
    private void GenerateTiles(float tileSize, int xCount, int yCount)
    {

        yOffset += transform.position.y;
        bounds = new Vector3((xCount/2)*tileSize,0,(yCount/2)*tileSize) + boardCenter;

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

    public void SpawnPieces() {

        chessPieces = new ChessPiece[X_COUNT ,Y_COUNT];
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
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, false);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.king, false);
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
        
        cp = Instantiate(piecesPrefabs[index],transform).GetComponent<ChessPiece>();

       

        cp.type = type;
        cp.isWhiteTeam = isWhiteTeam;

        //cp.gameObject.GetComponent<MeshRenderer>().material = teamMaterials[isWhiteTeam ? 0 : 1];

        return cp;

    }
   
    public void PositionAllPieces() {
        for (int x = 0; x < X_COUNT; x++)
            for (int y = 0; y < Y_COUNT; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);      
    }

    public void PositionSinglePiece(int xC, int yC, bool force)
    { //force is to decide whether the positioning is to be smooth(mid game) or rapid(start)    
        chessPieces[xC, yC].currentX = xC;
        chessPieces[xC,yC].currentY = yC;
        chessPieces[xC, yC].SetPosition(GetTileCenter(xC, yC),force);
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

    public bool MoveTo(ChessPiece currentPiece,int x, int y)
    {
        bool canMove = true;

        if (!IsHighlighted(ref availableMoves, new Vector2Int(x, y)))
            return false;

        //is the tile occupid?
        if (chessPieces[x,y] != null)
        {

            ChessPiece otherPiece = chessPieces[x,y];
            if (otherPiece.isWhiteTeam == currentPiece.isWhiteTeam)
            {
                return false;
            }
            else  
            {
                if (otherPiece.isWhiteTeam)
                {
                    if (otherPiece.type == ChessPieceType.king)
                        CheckMate(otherPiece.isWhiteTeam);

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

                
            }
        }

        Vector2Int previousPosition = new Vector2Int(currentPiece.currentX,currentPiece.currentY);

        chessPieces[x, y] = currentPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y,false);

        isWhiteTurn = !isWhiteTurn;
        isFirstMove = false;

        moveHistoryList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();

        return canMove;
    }

    public void HighlightTiles() {
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

    public bool IsHighlighted(ref List<Vector2Int> validMoves,Vector2Int tile)
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
        DisplayEndScreen(!isWhiteTeam);
    }
    
    
    public void ProcessSpecialMove()
    {

        if(specialMove == SpecialMove.Enpassant)
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



        specialMove = SpecialMove.None;
    }
    
    public void DisplayEndScreen(bool isWhite)
    {
        endScreen.SetActive(true);
        endScreen.transform.GetChild(isWhite ? 0 : 1).gameObject.SetActive(true);
    }

    public void OnResetClicked() {

        endScreen.SetActive(false);
        endScreen.transform.GetChild(0).gameObject.SetActive(false);
        endScreen.transform.GetChild(1).gameObject.SetActive(false);

        //reset fields
        currentlyDragging = null;
        availableMoves.Clear();
        moveHistoryList.Clear();

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

    public void OnExitClicked() {
        Application.Quit();
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene(0);
    }



}
