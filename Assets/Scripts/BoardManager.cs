using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic; //List<(int x, int y)> gibi bir yapıyı kullanmak için

public class BoardManager : MonoBehaviour
{
    public  Items pembePrefab;
    public  Items babybluePrefab;
    public  Items morPrefab;
    public  Items butteryellowPrefab;

    public Items DiscoBallPrefab;
    public Items VerticalRocketPrefab;
    public Items HorizontalRocketPrefab;
    public Items BombPrefab;

    public int selectedX;
    public int selectedY;

    public Items[] prefabs;
    public Items[] allPrefabs;
    public LevelInfo level1;
    public LevelInfo SpecialItemTestCase;
    Cell[, ] board;
    private Dictionary<CandyType, Items> candyDict = new Dictionary<CandyType, Items>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        LoadLevelInfo(SpecialItemTestCase);
    }
    void LoadLevelInfo(LevelInfo level){
        board = new Cell[level.rows[0].Split(',').Length, level.rows.Length];
        for(int x = 0 ; x < board.GetLength(0) ; x++){
            for(int y = 0 ; y < board.GetLength(1) ; y ++){
                Cell cell = new Cell(x, y, null);
                board[x, y] = cell;
            }
        }
        for(int x = 0 ; x < board.GetLength(0) ; x++){
            for(int y = 0 ; y < board.GetLength(1) ; y ++){
                string itemName = level.rows[level.rows.Length-1-y].Split(',')[x];

                if(Enum.TryParse(itemName, out CandyType candy)){
                    if(candyDict.TryGetValue(candy, out Items item)){
                        CreateItem(item, x, y);
                    }
                    else{
                        Debug.LogWarning($"'{itemName}' için prefab bağlanmamış @ ({x},{y})");
                    }
                }            
                else{
                    Debug.LogWarning($"Bilinmeyen item adı: '{itemName}' @ ({x},{y})");
                }
            }
        }

    }
    void Awake(){
        foreach(Items prefab in allPrefabs){
            if(prefab== null){continue;}
            if(candyDict.ContainsKey(prefab.type)){
                Debug.LogWarning("Same key " + prefab.type);
                continue;
                }
            candyDict.Add(prefab.type, prefab);

        }
    }
    // Update is called once per frame
   void Update(){
    // 1. Mouse basıldı → seç
    int releaseX;
    int releaseY;
    if(Mouse.current.leftButton.wasPressedThisFrame){
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        selectedX = Mathf.RoundToInt(mousePos.x);
        selectedY = Mathf.RoundToInt(mousePos.y);
    }

    // 3. Mouse bırakıldı → swap et
    if(Mouse.current.leftButton.wasReleasedThisFrame && IsValidCoordinate(selectedX, selectedY) && board[selectedX, selectedY] != null){
        Vector2 mousePos2 = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        releaseX = Mathf.RoundToInt(mousePos2.x);
        releaseY = Mathf.RoundToInt(mousePos2.y);
        HandleRelease(selectedX, selectedY, releaseX, releaseY);
        selectedX = -1;
        selectedY = -1;
        }
        
    }

    Items CreateItem(Items prefab, int x, int y){
        Items obj = Instantiate(prefab, GridToWorld(x, y), Quaternion.identity, null);
        obj.boardManager = this;
        board[x, y].item = obj;
        return obj;
    }

    void HandleRelease(int pressX, int pressY, int releaseX, int releaseY){
        if(!IsValidCoordinate(releaseX, releaseY) || !IsValidCoordinate(pressX, pressY)){return;}

            if(board[releaseX, releaseY].item == null || board[pressX, pressY].item == null){return;}
            SwapItems(pressX, pressY, releaseX, releaseY);
            if(!HasMatch(pressX, pressY) && !HasMatch(releaseX, releaseY) &&  !Items.IsSpecial(board[pressX, pressY].item.type) &&  !Items.IsSpecial(board[releaseX, releaseY].item.type)){
                SwapItems(pressX, pressY, releaseX, releaseY);
                return;
            }
            
            if(pressX == releaseX && pressY == releaseY){
                if(Items.IsSpecial(board[pressX, pressY].item.type)){ClearCell(pressX, pressY);}
                else{CheckMatches(pressX, pressY);}
                FallItems();
                SpawnItems();
                return;
            }

            if(Items.IsSpecial(board[releaseX, releaseY].item.type)){ClearCell(releaseX, releaseY);}
            else{CheckMatches(releaseX, releaseY);}
            
            Items other = board[pressX, pressY].item; 

            if(other != null){
                if(Items.IsSpecial(other.type)){ClearCell(pressX, pressY);}
                else{CheckMatches(pressX, pressY);}
            }            
            FallItems();
            SpawnItems();
        }
    
    bool HasMatch(int x, int y){
        (int a, int b) = CountHorizontal(x, y);
        int horizontal = a + b + 1;
        (int c, int d) = CountVertical(x, y); 
        int vertical = c + d + 1;
        
        return horizontal >= 3 || vertical >= 3;
    }

    
        
   

    void CheckMatches(int itemCellX, int itemCellY){

        (int a, int b) = CountHorizontal(itemCellX, itemCellY);
        int horizantalAdjoint = a + b + 1;
        (int c, int d) = CountVertical(itemCellX, itemCellY); 
        int verticalAdjoint = c + d + 1;
        int start = itemCellX - a;
        int ystart = itemCellY - c;
        

        int x = itemCellX;
        int y = itemCellY;
        int j = y;

        if( horizantalAdjoint >= 5 || verticalAdjoint >= 5){
            if(horizantalAdjoint >= 5){
                while(start <= itemCellX + b){
                    if(start == x){
                        start++;
                        continue;
                    }
                    
                ClearCell(start, y);
                start++;
                }
            }

            if(verticalAdjoint >= 5){
                while(ystart <= y + d){
                    if(ystart == y){
                        ystart++;
                        continue;
                    }
                
                    ClearCell(x,ystart);
                    ystart++;
                }
            }
            ClearCell(x,y);
            Items discoBall = CreateItem(DiscoBallPrefab, x, y);
            discoBall.type = CandyType.discoBall;
            discoBall.itemName = "discoBall";
            
            
        }
        else if(horizantalAdjoint >= 3 && verticalAdjoint >=3){
            while(start <= x + b){
                if(start == x){
                    start++;
                    continue;
                }
                ClearCell(start, y);
                start++;
                
            }

            while( ystart <= y+ d){
                if(ystart == y){
                    ystart++;
                    continue;
                }
                ClearCell(x, ystart);
                ystart++;
            }
            ClearCell(x,y);
            Items bomb = CreateItem(BombPrefab, x, y);
            bomb.type = CandyType.bomb;
            bomb.itemName = "bomb";
        }

        else if(horizantalAdjoint == 4 ){
            while(start <= x + b){
                if(start == x){start++; continue;}
                ClearCell(start, y);
                start++;
            }
            
            ClearCell(x,y);
            Items horizontalRocket = CreateItem(HorizontalRocketPrefab, x, y);
            horizontalRocket.type = CandyType.horizontalRocket;
            horizontalRocket.itemName = "horizantalRocket";
        }
        else if(verticalAdjoint == 4){
            while(ystart <= y + d){
                if(ystart == y){
                    ystart++;
                    continue;
                }
                ClearCell(x,ystart);
                ystart++;
                }
            
            ClearCell(x,y);
            Items verticalRocket = CreateItem(VerticalRocketPrefab, x, y);
            verticalRocket.type = CandyType.verticalRocket;
            verticalRocket.itemName = "verticalRocket";
        }

        else if(horizantalAdjoint == 3 || verticalAdjoint == 3){
            if(horizantalAdjoint == 3){
                while(start <= x + b){
                    ClearCell(start,y);
                    start++;
            }
            }
            else{
                while(ystart <= y + d){
                   ClearCell(x, ystart);
                    ystart++;
                }
            }
        }
        //TOPAC TO BE ADDED LATER//
        else{

        }
    }

    (int, int) CountHorizontal(int cellX, int cellY){
        int x = cellX + 1;
        int y = cellY;
        int leftNum = 0;
        int rightNum = 0;
        CandyType correctType = board[cellX,cellY].item.type;

        while(IsValidCoordinate(x, y) && board[x, y] != null && board[x, y].item != null &&  board[x, y].item.type ==  correctType){
            rightNum++;
            x++;
        }
        //reset x again
        x = cellX - 1;

        while(IsValidCoordinate(x, y) && board[x, y] != null && board[x, y].item != null && board[x, y].item.type == correctType){
            leftNum++;
            x--;
        }

        return (leftNum, rightNum);

    }
    (int, int) CountVertical(int cellX, int cellY){
        int x = cellX;
        int y = cellY + 1;
        int upNum = 0;
        int downNum = 0;
        CandyType correctType = board[cellX,cellY].item.type;

        while(IsValidCoordinate(x, y) && board[x, y] != null && board[x, y].item != null && board[x, y].item.type == correctType){
            upNum++;
            y++;
        }
        //reset x again
        y = cellY - 1;

        while(IsValidCoordinate(x, y) && board[x, y] != null && board[x, y].item != null && board[x, y].item.type == correctType){
            downNum++;
            y--;
        }
        //4. Magic number 6: Roadmap'inde Refactor #1 maddesi olarak zaten var — bu 6'lar boardWidth/boardHeight olacak ve LevelInfo'dan gelecek

        return (downNum, upNum); //item ın kendisini de saymamak için bir çıkarttım

    }
    void FallItems(){

        for(int x = 0 ; x < board.GetLength(0) ; x++){
           for(int y = 0 ; y < board.GetLength(1) ; y++){
                int a = 1;
                if(board[x, y] != null && board[x, y].item == null){

                    while(y + a < board.GetLength(1) && board[x, y + a].item == null){a++;}

                    if(y + a < board.GetLength(1) && board[x, y + a] != null){ 

                        MoveItem(x, y + a, x, y);
                
                    }   
                }
            }   
        }
    }

    void SwapItems(int aX, int aY, int bX, int bY){

        if((Math.Abs(aX - bX) == 1 && Math.Abs(aY - bY) == 0) ||
            (Math.Abs(aX - bX) == 0 && Math.Abs(aY - bY) == 1)){

            //Görsel değişim swap ı
            board[aX, aY].item.transform.position = GridToWorld(bX, bY);
            board[bX, bY].item.transform.position = GridToWorld(aX, aY);
            
            //Gridde konum Items konum swap ı
            int savedX = aX;
            int savedY = aY;
            aX = bX;
            aY = bY;
            bX = savedX;
            bY = savedY;

            Items tempItem = board[aX, aY].item;
            board[aX, aY].item = board[bX, bY].item;
            board[bX, bY].item = tempItem;
        }
    }

    void SpawnItems(){
        for(int x = 0 ; x < board.GetLength(0) ; x++){
            for(int y = board.GetLength(1)-1 ; y >= 0 ; y--){
                if(board[x, y] != null && board[x, y].item == null){
                    CreateItem(prefabs[Random.Range(0, prefabs.Length)], x, y);
                    
                }
                
            }
        }
    }

    void ClearCell(int x, int y){
        if (!IsValidCoordinate(x, y)){
            return;
        }
        if(board[x,y]== null || board[x,y].item == null){return;}

        CandyType type = board[x,y].item.type; 
        Destroy(board[x, y].item.gameObject);
        board[x, y].item = null;

        if(Items.IsSpecial(type)){
            Debug.Log("SpecialItem found");
            ActivateSpecial(x, y, type);
        }
    }

    void ActivateSpecial(int x, int y, CandyType type){
        Debug.Log($"ActivateSpecial: {type} @ ({x},{y})");
        if(type == CandyType.verticalRocket){
            for(int i = 0 ; i < board.GetLength(1); i++){
                ClearCell(x, i);
            }
        }
        else if(type == CandyType.horizontalRocket){
            for(int i = 0 ; i < board.GetLength(0) ; i++){
                ClearCell(i, y);
            }
        }
        else if(type == CandyType.bomb){
            for(int i = x - 1 ; i < x + 2 ; i++){
                for(int j = y - 1 ; j < y + 2 ; j++){
                    ClearCell(i, j);
                }
            }
        }
        /*
        *
        *
        
        else if(type == CandyType.discoBall){
            List<(int s, int f)> toBombed = TypesOnBoard(type); // Burada discoball un hangi itemla yer değiştirdiğinin bilgisi lazım.
            foreach(var (row, col) in toBombed){
                ClearCell(row,col);
            }
        }
        */
    }

    void MoveItem(int fromX, int fromY, int toX, int toY){
        if(!IsValidCoordinate(fromX, fromY) || !IsValidCoordinate(toX, toY)){return;}
        
        if(board[toX, toY].item != null){
            Debug.LogError("Goal cell is full can not move should have been swapedItem");
            return;
            } 

        if(board[fromX, fromY].item == null){
            Debug.LogError("MoveItem: Item to be moved is null");
            return;
        }
        Items item = board[fromX, fromY].item;
        board[toX, toY].item = board[fromX, fromY].item;
        board[fromX, fromY].item = null;

        item.transform.position = GridToWorld(toX, toY);
    }

    bool IsValidCoordinate(int x, int y){
        return(0<= x && x < board.GetLength(0) && 0<= y && y < board.GetLength(1)); 
    }

   Vector2 GridToWorld(int x, int y) => new Vector2(x, y);
/*
    List<(int x, int y)> TypesOnBoard(CandyType type){
        List<(int x, int y)> list = new List<(int x, int y)>();
        for(int i = 0; i < board.GetLength(0) ; i++){
            for(int j = 0 ; j < board.GetLength(1); j++){
                if(board[i,j].item.type == type){
                    list.Add((i,j));
                }
            }
        }
        return list;
    }
    */
}