using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public static int w = 10;
	public static int h = 24;
	public static Transform[,] grid = new Transform[w, h];

    public static Vector2 ToGrid(Vector2 v){
        Vector2 x = new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
        return x;
    }
	public static bool InsideBorder(Vector2 pos)
	{
		return ((int)pos.x >= 0 &&
				(int)pos.x < w &&
				(int)pos.y >= 0);
	}

    public static void DestroyRow(int y)
    {
        MainController.score++;
        LoggerCSV.GetInstance().AddEvent(LoggerCSV.EVENT_SCORE, MainController.score.ToString());
        for (int x = 0; x < w; x++){
            Destroy(grid[x,y].gameObject);
            grid[x, y] = null;
        }
    }

	public static void DecreaseRow(int y)
	{
		for (int x = 0; x < w; ++x)
		{
			if (grid[x, y] != null)
			{
				// Move one towards bottom
				grid[x, y - 1] = grid[x, y];
				grid[x, y] = null;

				// Update Block position
				grid[x, y - 1].position += new Vector3(0, -1, 0);
			}
		}
	}
	public static void DecreaseRowsAbove(int y)
	{
		for (int i = y; i < h; ++i)
			DecreaseRow(i);
	}

    public static bool FullRow(int y){
        for (int x = 0; x < w; x++){
            if (grid[x, y] == null) return false;
        }
        return true;
    }

    public static void DeleteFullRows(){
        for (int y = 0; y < h; y++){
            if (FullRow(y)){
                DestroyRow(y);
                DecreaseRowsAbove(y+1);
                y--;
            }
        }
    }
}
