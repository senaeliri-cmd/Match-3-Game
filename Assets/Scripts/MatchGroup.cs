using System.Collections.Generic;
public class MatchGroup
{
    public (int posX, int posY) Position {get; set;}
    public (int left, int right) Horizon {get; set;}
    public (int top, int bottom) Vertical{get; set;}
    public HashSet<(int x, int y)> Cells{get; set;}

    public MatchGroup((int posX, int posY) position, (int left, int right) horizon, 
                        (int top, int bottom) vertical){
        Position = position;
        Horizon = horizon;
        Vertical = vertical;
        Cells = new HashSet<(int x, int y)>();

    }
}
