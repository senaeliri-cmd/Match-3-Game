using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public LevelInfo level1;
    Cell[, ] board = new Cell[6, 6];

    public  Items pembePrefab;
    public  Items babybluePrefab;
    public  Items morPrefab;
    public  Items butteryellowPrefab;

    public Items DiscoBallPrefab;
    public Items RocketPrefab;
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
        if(Physics2D.OverlapPoint(mousePos) != null){
           Items clickedItem =Physics2D.OverlapPoint(mousePos).GetComponent<Items>();
           if(clickedItem != null){
            SwapItems(a, clickedItem);
            CheckMatches(a);
            if(clickedItem != null){ CheckMatches(clickedItem);}
            FallItems();
            SpawnItems();
           }
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
                    
                Destroy(board[start, y].item.gameObject);
                board[start, y].item = null;
                start++;
                }
            }

            else{
                while(ystart <= y + d){
                    if(ystart == y){
                        ystart++;
                        continue;
                }
            
                Destroy(board[x, ystart].item.gameObject);
                board[x, ystart].item = null;
                ystart++;
            }
            }
            Destroy(board[x, y].item.gameObject);
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
                Destroy(board[start, y].item.gameObject);
                board[start, y].item = null;
                start++;
                
            }

            while( ystart <= y+ d){
                if(ystart == y){
                    ystart++;
                    continue;
                }
                Destroy(board[x, ystart].item.gameObject);
                board[x, ystart].item = null;
                ystart++;
            }
            Destroy(board[x, y].item.gameObject);
            Items bomb = Instantiate(BombPrefab, new Vector2(x,y), Quaternion.identity, null);
            board[x,y].item = bomb;
            bomb.type = CandyType.bomb;
            bomb.x = x;
            bomb.y = y;
            bomb.boardManager = this;
            bomb.itemName = "bomb";
        }

        else if(horizantalAdjoint == 4 || verticalAdjoint == 4){
            if(horizantalAdjoint == 4){
                while(start <= x + b){
                    if(start == x){start++; continue;}
                    Destroy(board[start, y].item.gameObject);
                    board[start, y].item = null;
                    start++;
                }
            }
            else{
                while(ystart <= y + d){
                    if(ystart == y){
                        ystart++;
                        continue;
                    }
                    Destroy(board[x, ystart].item.gameObject);
                    board[x, ystart].item= null;
                    ystart++;
                }
            }
            Destroy(board[x, y].item.gameObject);
            Items rocket = Instantiate(RocketPrefab, new Vector2(x,y), Quaternion.identity, null);
            board[x,y].item = rocket;
            rocket.type = CandyType.rocket;
            rocket.x = x;
            rocket.y = y;
            rocket.boardManager = this;
            rocket.itemName = "rocket";
        }
        else if(horizantalAdjoint == 3 || verticalAdjoint == 3){
            if(horizantalAdjoint == 3){
                while(start <= x + b){
                    Destroy(board[start, y].item.gameObject);
                    board[start, y].item = null;
                    start++;
            }
            }
            else{
                while(ystart <= y + d){
                    Destroy(board[x, ystart].item.gameObject);
                    board[x, ystart].item= null;
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
}