using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public struct Cell
{
    public int col;
    public int row;

    public static bool Equals(Cell a, Cell b)
    {
        return a.col == b.col && a.row == b.row;
    }

    public static Cell Invalid()
    {
        return new Cell { col = -1, row = -1 };
    }
}

public struct Node
{
    public Cell curr;   // Current
    public Cell prev;   // Parent ("what came before?")
    public float cost;
}

public static class Pathing
{
    public static List<Cell> FloodFill(Cell start, Cell end, int[,] tiles, int count, Grid grid = null)
    {
        int rows = tiles.GetLength(0);
        int cols = tiles.GetLength(1);
        bool[,] visited = new bool[rows, cols];
        Node[,] nodes = new Node[rows, cols];
        for (int row = 0;  row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Label walls as "visited" to prevent them from being explored!
                visited[row, col] = tiles[row, col] == 1;
                nodes[row, col].curr = new Cell { row = row, col = col };
                nodes[row, col].prev = Cell.Invalid();
            }
        }

        Queue<Cell> frontier = new Queue<Cell>();
        frontier.Enqueue(start);

        bool found = false;
        for (int i = 0; i < count; i++)
        {
            Cell cell = frontier.Dequeue();
            visited[cell.col, cell.row] = true;

            if (Cell.Equals(cell, end))
            {
                found = true;
                break;
            }

            if (grid != null)
                grid.ColorTile(cell, Color.magenta);

            foreach (Cell adj in Adjacents(cell, rows, cols))
            {
                // Enqueue only if unvisited (otherwise infinite loop)!
                if (!visited[adj.col, adj.row])
                {
                    frontier.Enqueue(adj);
                    nodes[adj.row, adj.col].prev = cell;
                    // Set parent ("prev") of adj to be cell (since cell was before adj)
                }
            }
        }

        // Using grid as a debug renderer via ColorTile
        if (grid != null)
        {
            // Remember to make start & end the correct colours (1%) ;)
            grid.ColorTile(start, Color.green);
            grid.ColorTile(end, Color.red);
        }

        // Retrace our steps if we found a path!
        return found ? Retrace(nodes, start, end) : new List<Cell>();
    }

    public static List<Cell> Dijkstra(Cell start, Cell end, int[,] tiles, int count, Grid grid = null)
    {
        int rows = tiles.GetLength(0);
        int cols = tiles.GetLength(1);

        // No need to keep track of "visited" nodes since Dijkstra's associates cost with nodes (preventing infinite re-exploration)
        Node[,] nodes = new Node[rows, cols];
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                nodes[row, col].curr = new Cell { row = row, col = col };
                nodes[row, col].prev = Cell.Invalid();
                nodes[row, col].cost = float.MaxValue;
            }
        }

        PriorityQueue<Cell, float> frontier = new PriorityQueue<Cell, float>();
        frontier.Enqueue(start, 0.0f);
        nodes[start.row, start.col].cost = 0.0f;
        // Must set start cost to 0 otherwise it's infinity and Dijkstra's thinks its on the optimal path

        bool found = false;
        for (int i = 0; i < count; i++)
        {
            Cell cell = frontier.Dequeue();

            if (Cell.Equals(cell, end))
            {
                found = true;
                break;
            }

            if (grid != null)
                grid.ColorTile(cell, Color.magenta);

            foreach (Cell adj in Adjacents(cell, rows, cols))
            {
                // (Optional) -- prevent walls from being explored
                // (They won't be on the path regardless if we give them a high cost)
                bool isWall = tiles[adj.row, adj.col] == 1;
                if (isWall)
                    continue;

                float prevCost = nodes[adj.row, adj.col].cost;
                float currCost = nodes[cell.row, cell.col].cost + Cost(adj, tiles);
                if (currCost < prevCost)
                {
                    frontier.Enqueue(adj, currCost);
                    nodes[adj.row, adj.col].cost = currCost;
                    nodes[adj.row, adj.col].prev = cell;
                }
            }
        }

        // Using grid as a debug renderer via ColorTile
        if (grid != null)
        {
            // Remember to make start & end the correct colours (1%) ;)
            grid.ColorTile(start, Color.green);
            grid.ColorTile(end, Color.red);
        }

        // Retrace our steps if we found a path!
        return found ? Retrace(nodes, start, end) : new List<Cell>();
    }

    // Associate a movement cost with different tile types!
    public static float Cost(Cell cell, int[,] tiles)
    {
        switch (tiles[cell.row, cell.col])
        {
            // Air
            case 0:
                return 1.0f;

            // Wall
            case 1:
                return 100.0f;

            // Water
            case 2:
                return 25.0f;

            // Grass
            case 3:
                return 10.0f;
        }
        return 0.0f;
    }

    // Task 2: Follow the pseudocode to create an algorithm that makes a list of cells
    // by looking up the parent of the current cell, then reversing to go from start to end.
    public static List<Cell> Retrace(Node[,] nodes, Cell start, Cell end)
    {
        List<Cell> path = new List<Cell>();

        // let curr = end
        Cell curr = end;

        // let prev = parent of curr's node
        Cell prev = nodes[curr.row, curr.col].prev;

        // loop until prev is invalid
        while(!Cell.Equals(prev, Cell.Invalid()))
        {
            // Add curr to path
            path.Add(curr);

            // Set curr equal to prev
            curr = prev;

            // Set prev equal to parent of curr's node
            prev = nodes[curr.row, curr.col].prev;
        }

        // Add start manually since it's parent is invalid so the loop won't add it!
        path.Add(start);

        // Reverse path (since it currently goes from end to start)
        return path;
    }

    // Task 1: Follow the pseudocode to create an algorithm that makes a list of cells
    // which are adjacent (left, right, up, down) of the passed in cell.
    // *Ensure cells do not cause out-of-bounds errors (> 0, < rows & cols)*
    public static List<Cell> Adjacents(Cell cell, int rows, int cols)
    {
        List<Cell> cells = new List<Cell>();

        // left-case
        if (cell.col - 1 >= 0)
            cells.Add(new Cell { col = cell.col - 1, row = cell.row });

        // right-case
        if (cell.col + 1 < cols)
            cells.Add(new Cell { col = cell.col + 1, row = cell.row });

        // up-case
        if (cell.row - 1 >= 0)
            cells.Add(new Cell { col = cell.col, row = cell.row - 1 });

        // down-case
        if (cell.row + 1 < rows)
            cells.Add(new Cell { col = cell.col, row = cell.row + 1 });
        
        return cells;
    }
}
