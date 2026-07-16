using UnityEngine;

public class Cell
{
    public int x;
    public int y;
    public Items item;

    public Cell(int x, int y, Items item){
        this.x = x;
        this.y = y;
        this.item = item;
    }
    
}
