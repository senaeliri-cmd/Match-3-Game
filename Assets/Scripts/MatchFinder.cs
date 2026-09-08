using System.Collections.Generic;
public class MatchFinder

{
    
    /* GOAL: Cascade class goal -----> tahtanın en alt solundan başlayarak eşleşmeleri bulsun sol sağ alt üst olarak
    *PARAMETER: parametre -----> olarak board alıyor
    *RETURN: eğer özel item gelirse diye ------> bulunduğu konumu va etrafındakilerin uzunluğunu döndürüyor.
    */

    Cell[,] board;
    
 

    public Dictionary<(int, int), MatchGroup> FindMatches(Cell[,] board){
        int width  = board.GetLength(0);
        int height = board.GetLength(1);

        Dictionary<(int, int), MatchGroup> cellToGroup = new Dictionary<(int, int), MatchGroup>();
        List<MatchGroup> groupList = new List<MatchGroup>();
        
 
        for(int y = 0 ; y < height; y++){
            for(int x = 0 ; x < width; x++){
                if(!IsValidCoordinate(width, height, x, y) || board[x,y] == null ||board[x,y].item == null){continue;}
                if(Items.IsSpecial(board[x, y].item.type)){
                    continue;
                }

                (int left, int right) = FindHorizontal(board, x, y);
                (int top, int bottom) = FindVertical(board, x, y);

                if(HasMatchHorizontal(left, right) && left == 0){  
                    MatchGroup group =  new MatchGroup((x, y), (left, right), (top, bottom));
                    cellToGroup[(x, y)] = group;
                    
                    
                    for(int xl = x + 1; xl <= x + right ; xl++){
                        cellToGroup[(xl,y)] = group;
                    }
                }
            }
        }
        
        for(int x = 0 ; x < width; x++){
            for(int y = 0 ; y < height; y++){
                if(!IsValidCoordinate(width, height, x, y) || board[x,y] == null ||board[x,y].item == null||Items.IsSpecial(board[x, y].item.type)){
                    continue;}
 

                (int left, int right) = FindHorizontal(board, x, y);
                (int top, int bottom) = FindVertical(board, x, y);

                if(HasMatchVertical(top, bottom) && bottom == 0){
                    bool found = false;
                    MatchGroup groupFoundTrue = null;
                    for(int yl = y; yl <= y + top ; yl++){
                        if(cellToGroup.ContainsKey((x,yl))){
                            found = true; 
                            cellToGroup[(x,yl)].Vertical = (top, bottom);
                            groupFoundTrue = cellToGroup[(x,yl)];
                        }
                    }
                   
                   MatchGroup group = found ? groupFoundTrue : new MatchGroup((x, y), (left, right), (top, bottom));
                   for(int yl = y ; yl <= y + top ; yl++){
                        cellToGroup[(x, yl)] = group;
                    }
                }
            }
        }
        return cellToGroup;
    }
    
    (int left, int right) FindHorizontal(Cell[,] board, int x, int y){
        int leftNum = 0;
        int rightNum = 0;
        int currX = x;
        int currY = y;
        int nextX = currX + 1;
        int nextY = currY + 1;

        

        while(IsValidCoordinate(board.GetLength(0), board.GetLength(1),nextX, y) &&
         board[nextX, y] != null && board[nextX, y].item != null){
            if(board[currX, y].item.type == board[nextX, y].item.type ){rightNum++;}

            else{break;}
            nextX++;
            currX++;

        }

        currX  = x;
        nextX = x -1;
        while(IsValidCoordinate(board.GetLength(0), board.GetLength(1),nextX, y) && 
        board[nextX, y] != null && board[nextX, y].item != null){
            if(board[currX, y].item.type == board[nextX, y].item.type ){leftNum++;}

            else{break;}
            nextX--;
            currX--;

        }

        return (leftNum, rightNum);

    }

    (int top, int bottom) FindVertical(Cell[,] board, int x, int y){
        int topNum = 0;
        int bottomNum = 0;
        int currX = x;
        int currY = y;
        int nextX = currX + 1;
        int nextY = currY + 1;



        while(IsValidCoordinate(board.GetLength(0), board.GetLength(1),x, nextY)
         &&board[x, nextY] != null && board[x, nextY].item != null){
            if(board[x, currY].item.type == board[x, nextY].item.type ){topNum++;}

            else{break;}
            nextY++;
            currY++;

        }

        currY  = y;
        nextY = y -1;
        while(IsValidCoordinate(board.GetLength(0), board.GetLength(1),x, nextY) && 
        board[x, nextY] != null && board[x, nextY].item != null){
            if(board[x, currY].item.type == board[x, nextY].item.type ){bottomNum++;}

            else{break;}
            nextY--;
            currY--;

        }

        return (topNum, bottomNum);

    }

    bool HasMatchHorizontal(int left, int right){
        if(left + right >= 2 ){
            return true;
        }
        return false;
    }
     bool HasMatchVertical(int top, int bottom){
        if(top + bottom >= 2 ){
            return true;
        }
        return false;
    }

    bool IsValidCoordinate( int width, int height, int x, int y){
        return(0<= x && x < width && 0<= y && y < height); 
    }
}
