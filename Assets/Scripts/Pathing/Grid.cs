using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public enum PathType
{
    FLOODFILL,
    DIJKSTRA
}

public class Grid : MonoBehaviour
{
    // Drag the slider to watch flood-fill expand its search!
    [SerializeField]
    int count;

    [SerializeField]
    GameObject tilePrefab;

    [SerializeField]
    PathType pathType;

    int rows = 10;
    int cols = 20;
    List<List<GameObject>> tileObjects = new List<List<GameObject>>();

    int[,] tiles =
    {
        //0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // 0
        { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // 1
        { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // 2
        { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // 3
        { 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // 4
        { 1, 0, 0, 1, 0, 0, 0, 0, 1, 2, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1 }, // 5
        { 1, 0, 0, 1, 0, 0, 0, 0, 0, 2, 3, 3, 0, 0, 0, 0, 0, 0, 0, 1 }, // 6
        { 1, 0, 0, 1, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 1 }, // 7
        { 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // 8
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }  // 9
    };

    Cell start = new Cell { col = 6, row = 5 };
    Cell end = new Cell { col = 10, row = 4 };

    void Start()
    {
        // List is "last in first out (LIFO)".
        // Queue is "first in first out (FIFO)".
        List<int> nl = new List<int>();
        Queue<int> nq = new Queue<int>();

        // Same is queue, but each value has a priority. Lower = higher priority.
        PriorityQueue<int, float> pq = new PriorityQueue<int, float>();

        nl.Add(1);
        nl.Add(2);
        nl.Add(3);

        nq.Enqueue(1);
        nq.Enqueue(2);
        nq.Enqueue(3);

        pq.Enqueue(1, 1.1f);
        pq.Enqueue(2, 0.3f);
        pq.Enqueue(3, 0.6f);

        nl.RemoveAt(nl.Count - 1);
        nq.Dequeue();

        // Prints "1, 2" since 3 was last in (thus first out)
        for (int i = 0; i <  nl.Count; i++)
            Debug.Log(nl[i]);

        // Prints "2, 3" since 1 was first in (thus first out)
        while (nq.Count > 0)
            Debug.Log(nq.Dequeue());

        // Prints "2, 3, 1" since queue prioritizes lowest-to-highest
        while (pq.Count > 0)
            Debug.Log(pq.Dequeue());

        float xStart = 0.5f;
        float yStart = rows - 0.5f;
        float x = xStart;
        float y = yStart;

        for (int row = 0; row < rows; row++)
        {
            // Create a new list of tiles for each row
            tileObjects.Add(new List<GameObject>());

            // Add a tile to the list for each column of the row
            for (int col = 0; col < cols; col++)
            {
                GameObject tile = Instantiate(tilePrefab);
                tile.transform.position = new Vector3(x, y);
                tileObjects[row].Add(tile);

                x += 1.0f;
            }

            // Move to start of next row
            x = xStart;
            y -= 1.0f;
        }
    }

    void Update()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                ColorTile(new Cell { col = col, row = row });
            }
        }

        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;

        Cell mouseCell = WorldToGrid(mouse);
        ColorTile(mouseCell, Color.cyan);

        foreach (Cell adj in Pathing.Adjacents(mouseCell, rows, cols))
            ColorTile(adj, Color.magenta);

        // Choose between flood-fill (bad) vs Dijkstra's (good)
        // 300 vs 75 iterations -- Dijkstra's priority-queue is a significant improvement!
        List<Cell> path = new List<Cell>();
        if (pathType == PathType.FLOODFILL)
            path = Pathing.FloodFill(start, end, tiles, count, this);
        else if (pathType == PathType.DIJKSTRA)
            path = Pathing.Dijkstra(start, end, tiles, count, this);

        // TODO -- make an object follow the path
        // Do so by storing a current position and a next position, then moving between the two.
        // Be sure to update current & next based on proximity, and prevent out of range errors!

        foreach (Cell cell in path)
            ColorTile(cell, Color.cyan);

        // TODO -- right-click to set the start tile, left click to set the end tile.
        ColorTile(start, Color.green);
        ColorTile(end, Color.red);
    }

    public void ColorTile(Cell cell, Color color)
    {
        GameObject tile = tileObjects[cell.row][cell.col];
        tile.GetComponent<SpriteRenderer>().color = color;
    }

    public void ColorTile(Cell cell)
    {
        int value = tiles[cell.row, cell.col];
        Color[] colors =
        {
            Color.gray,     // 0 = Air
            Color.black,    // 1 = Wall
            Color.blue,     // 2 = Water
            Color.green     // 3 = Grass
        };
        ColorTile(cell, colors[value]);
    }

    // Hint: Clamp will be helpful for Task 1!
    Cell WorldToGrid(Vector3 world)
    {
        int col = (int)world.x;
        int row = (rows - 1) - (int)world.y;
        col = Mathf.Clamp(col, 0, cols - 1);
        row = Mathf.Clamp(row, 0, rows - 1);
        return new Cell { col = col, row = row };
    }

    // TODO -- given a cell (array coordinates), what's its world-space position (Unity coordinates) 
    Vector3 WorldToGrid(Cell cell)
    {
        return Vector3.zero;
    }

    void GridTest()
    {
        Cell topLeft = new Cell { col = 0, row = 0 };
        Cell topRight = new Cell { col = cols - 1, row = 0 };
        Cell botLeft = new Cell { col = 0, row = rows - 1 };
        Cell botRight = new Cell { col = cols - 1, row = rows - 1 };

        ColorTile(topLeft, Color.red);      // top-left
        ColorTile(topRight, Color.green);   // top-right
        ColorTile(botLeft, Color.blue);     // bot-right
        ColorTile(botRight, Color.magenta); // bot-left
    }
}
