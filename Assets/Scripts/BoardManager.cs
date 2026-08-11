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

    public Items selectedItem;
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
    if(Mouse.current.leftButton.wasPressedThisFrame){
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if(hit.collider != null){
            selectedItem = hit.collider.GetComponent<Items>();
        }
    }

    // 3. Mouse bırakıldı → swap et
    if(Mouse.current.leftButton.wasReleasedThisFrame && selectedItem != null){
        HandleRelease(selectedItem);
        selectedItem = null;
        }
    }

    Items CreateItem(Items prefab, int x, int y){
        Items obj = Instantiate(prefab, new Vector2(x, y), Quaternion.identity, null);
        obj.boardManager = this;
        obj.x = x;
        obj.y = y;
        board[x, y].item = obj;
        return obj;
    }

    void HandleRelease(Items a){
        Vector2 mousePos= Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D overlapPoint = Physics2D.OverlapPoint(mousePos);
        if(overlapPoint != null){
            Items clickedItem = overlapPoint.GetComponent<Items>();
            if(clickedItem == null){return;}
            SwapItems(a, clickedItem);
            if(!HasMatch(a) && ! HasMatch(clickedItem) &&  !Items.IsSpecial(a.type) &&  !Items.IsSpecial(clickedItem.type)){
                SwapItems(a, clickedItem);
                return;
            }
            if(clickedItem == a){
                if(Items.IsSpecial(a.type)){ClearCell(a.x, a.y);}
                else{CheckMatches(a);}
                FallItems();
                SpawnItems();
                return;
            }
            int x = clickedItem.x;
            int y = clickedItem.y;
            Items other = board[x, y].item;
            if(Items.IsSpecial(a.type)){ClearCell(a.x, a.y);}
            else{CheckMatches(a);}
            if(other != null){
            if(Items.IsSpecial(other.type)){ClearCell(other.x, other.y);}
            else{CheckMatches(other);}
}            FallItems();
            SpawnItems();
        }
    }
    bool HasMatch(Items swapedItem){
        (int a, int b) = CountHorizontal(swapedItem);
        int horizontal = a + b + 1;
        (int c, int d) = CountVertical(swapedItem); 
        int vertical = c + d + 1;
        
        return horizontal >= 3 || vertical >= 3;
    }

    
        
   

    void CheckMatches(Items clickItem){
        (int a, int b) = CountHorizontal(clickItem);
        int horizantalAdjoint = a + b + 1;
        (int c, int d) = CountVertical(clickItem); 
        int verticalAdjoint = c + d + 1;
        int start = clickItem.x - a;
        int ystart = clickItem.y - c;
        

        int x = clickItem.x;
        int y = clickItem.y;
        int j = y;

        if( horizantalAdjoint >= 5 || verticalAdjoint >= 5){
            if(horizantalAdjoint >= 5){
                while(start <= clickItem.x + b){
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

    (int, int) CountHorizontal(Items item){
        int x = item.x + 1;
        int y = item.y;
        int leftNum = 0;
        int rightNum = 0;
        CandyType correctType = item.type;

        while(x >= 0 && x < 6 && y >= 0 && y < 6 && board[x, y] != null && board[x, y].item != null &&  board[x, y].item.type ==  correctType){
            rightNum++;
            x++;
        }
        //reset x again
        x = item.x - 1;

        while(x >= 0 && x < 6 && y >= 0 && y < 6 && board[x, y] != null && board[x, y].item != null && board[x, y].item.type == correctType){
            leftNum++;
            x--;
        }

        return (leftNum, rightNum);

    }
    (int, int) CountVertical(Items item){
        int x = item.x;
        int y = item.y + 1;
        int upNum = 0;
        int downNum = 0;
        CandyType correctType = item.type;

        while(x >= 0 && x < 6 && y >= 0 && y < 6 && board[x, y] != null && board[x, y].item != null && board[x, y].item.type == correctType){
            upNum++;
            y++;
        }
        //reset x again
        y = item.y - 1;

        while(x >= 0 && x < 6 && y >= 0 && y < 6 && board[x, y] != null && board[x, y].item != null && board[x, y].item.type == correctType){
            downNum++;
            y--;
        }
        //4. Magic number 6: Roadmap'inde Refactor #1 maddesi olarak zaten var — bu 6'lar boardWidth/boardHeight olacak ve LevelInfo'dan gelecek

        return (downNum, upNum); //item ın kendisini de saymamak için bir çıkarttım

    }
    void FallItems(){
        int a = 1;
        for(int x = 0 ; x < board.GetLength(0) ; x++){
           for(int y = 0 ; y < board.GetLength(1) ; y++){
                a = 1;
                if(board[x, y] != null && board[x, y].item == null){
                    while(y + a < board.GetLength(1) && board[x, y + a].item == null){a++;}
                    if(y+a < board.GetLength(1)){
                        board[x, y + a].item.x = x;
                        
                        board[x, y + a].item.y = y;
                        board[x, y].item = board[x, y + a].item;
                        board[x, y].item.transform.position = new Vector2(x, y);
                        board[x, y + a].item = null;
                
                    }   
                }
            }   
        }
    }

    void SwapItems(Items a, Items b){
        if((Math.Abs(a.x - b.x) == 1 && Math.Abs(a.y - b.y) == 0) ||
            (Math.Abs(a.x - b.x) == 0 && Math.Abs(a.y - b.y) == 1)){
            //Gridde konum Items konum swap ı
            int savedX = a.x;
            int savedY = a.y;
            a.x =b.x;
            a.y = b.y;
            b.x = savedX;
            b.y = savedY;

            //Görsel değişim swap ı
            Vector3 temPos = a.transform.position;
            a.transform.position = b.transform.position;
            b.transform.position = temPos;

            board[a.x, a.y].item = a;
            board[b.x, b.y].item = b;
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
        if (x < 0 || x >= board.GetLength(0) || y < 0 || y >= board.GetLength(1)){
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