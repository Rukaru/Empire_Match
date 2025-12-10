using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class puzzle : MonoBehaviour
{
    public Texture[] elements;
    public int totalColumns = 9;
    public int totalRows = 9;

    [System.Serializable]
    public class PuzzleElement
    {
        public Texture texture;
        public Vector2 position;
    }

    List<List<PuzzleElement>> columns = new List<List<PuzzleElement>>();

    int selectedColumn = -1;
    int selectedRow = -1;
    int score;

    // Start is called before the first frame update
    void Start()
    {
        //óÒÇèâä˙âª
        for(int x=0; x < totalColumns; x++)
        {
            List<PuzzleElement> column = new List<PuzzleElement>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
