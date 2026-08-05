using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic; //List<(int x, int y)> gibi bir yapıyı kullanmak için

public class BoardManager : MonoBehaviour
{
    public LevelInfo level1;
    Cell[, ] board = new Cell[6, 6];

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        for(int x = 0 ; x < 6 ; x++){
            for(int y = 0 ; y < 6 ; y ++){
                string itemname = level1.rows[x].Split(',')[y];
                
                Items obj;
                if(itemname == "pembe"){
                    obj = Instantiate(pembePrefab, new Vector2(x, y), Quaternion.identity, null);
                }
                else if(itemname == "babyblue"){
                    obj = Instantiate(babybluePrefab, new Vector2(x, y), Quaternion.identity, null);
                }
                else if(itemname == "butteryellow"){
                    obj = Instantiate(butteryellowPrefab, new Vector2(x, y), Quaternion.identity, null);
                }
                else if(itemname == "mor"){
                    obj = Instantiate(morPrefab, new Vector2(x, y), Quaternion.identity, null);
                }
                else{
                    Debug.LogWarning("isim yok");
                    continue;
                }

                Cell cell = new Cell(x, y, obj);
                board[x, y] = cell;
                obj.boardManager = this;
                obj.x = x;
                obj.y = y;
            
            }
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

    void HandleRelease(Items a){
        Vector2 mousePos= Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 overlapPoint = Physics2D.OverlapPoint(mousePos);
        if(overlapPoint != null){
            Items clickedItem = overlapPoint.GetComponent<Items>();
            if(clickedItem == null){return;}
            SwapItems(a, clickedItem);
            if(clickedItem == a){
                if(Items.IsSpecial(a)){ClearCell(a.x, a.y);}
                else{CheckMathces(a);}
                FallItems();
                SpawnItems();
                return;
            }
            if(Items.IsSpecial(a)){ClearCell(a.x, a.y);}
            else{CheckMathces(a);}
            if(Items.IsSpecial(clickedItem)){ClearCell(clickedItem.x, clickedItem.y);}
            else{CheckMatches(clickedItem);}
            FallItems();
            SpawnItems();
        }
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

            else{
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
            Items discoBall = Instantiate(DiscoBallPrefab, new Vector2(x,y), Quaternion.identity, null);
            board[x,y].item = discoBall;
            discoBall.type = CandyType.discoBall;
            discoBall.x = x;
            discoBall.y = y;
            discoBall.boardManager = this;
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
            Items bomb = Instantiate(BombPrefab, new Vector2(x,y), Quaternion.identity, null);
            board[x,y].item = bomb;
            bomb.type = CandyType.bomb;
            bomb.x = x;
            bomb.y = y;
            bomb.boardManager = this;
            bomb.itemName = "bomb";
        }

        else if(horizantalAdjoint == 4 ){
            while(start <= x + b){
                if(start == x){start++; continue;}
                ClearCell(start, y);
                start++;
            }
            
            ClearCell(x,y);
            Items horizontalRocket = Instantiate(HorizontalRocketPrefab, new Vector2(x,y), Quaternion.identity, null);
            board[x,y].item = horizontalRocket;
            horizontalRocket.type = CandyType.horizontalRocket;
            horizontalRocket.x = x;
            horizontalRocket.y = y;
            horizontalRocket.boardManager = this;
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
            Items verticalRocket = Instantiate(VerticalRocketPrefab, new Vector2(x,y), Quaternion.identity, null);
            board[x,y].item = verticalRocket;
            verticalRocket.type = CandyType.verticalRocket;
            verticalRocket.x = x;
            verticalRocket.y = y;
            verticalRocket.boardManager = this;
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
        for(int column = 0 ; column < 6 ; column++){
           for(int row = 0 ; row < 6 ; row++){
                a = 1;
                if(board[column, row] != null && board[column, row].item == null){
                    while(row + a < 6 && board[column, row + a].item == null){a++;}
                    if(row+a < 6){
                        board[column, row + a].item.x = column;
                        
                        board[column, row + a].item.y = row;
                        board[column, row].item = board[column, row + a].item;
                        board[column, row].item.transform.position = new Vector2(column, row);
                        board[column, row + a].item = null;
                
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
        for(int column = 0 ; column < 6 ; column++){
            for(int row = 5 ; row >= 0 ; row--){
                if(board[column, row] != null && board[column, row].item == null){
                    Items item = Instantiate(prefabs[Random.Range(0, prefabs.Length)], new Vector2(column, row), Quaternion.identity, null);
                    item.boardManager = this;
                    item.x = column;
                    item.y = row;
                    board[column, row].item = item;
                    
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