using UnityEngine;

public class Items : MonoBehaviour
{
    public Sprite sprite;
    public CandyType type;
    public BoardManager boardManager;
    public string itemName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static bool IsSpecial(CandyType type){
        Debug.Log("In IsSpecial");
        return type == CandyType.verticalRocket ||type == CandyType.horizontalRocket || type == CandyType.bomb || type == CandyType.discoBall;
        
    }

   
}
