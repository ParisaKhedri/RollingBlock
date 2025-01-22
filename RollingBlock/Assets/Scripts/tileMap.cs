using System.Collections.Generic;
using UnityEngine;

public class tileMap : MonoBehaviour
{
    public GameObject tilePrefab;    // Assign your tile prefab in the Inspector
    public int gridWidth = 10;      // Number of tiles in width
    public int gridHeight = 10;     // Number of tiles in height
    private Grid grid;

    public List<List<int>> tileMapRep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GetComponent<Grid>(); // Get the Grid component on this GameObject
        this.tileMapRep = new List<List<int>>();

        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned!");
            return;
        }
        makeTileMapRep(30);
        // time sleep kanse måste vänta tills listan är klar??? :(

        GenerateTiles();

    }

    // Update is called once per frame
    void Update()
    {
        

    }

    void GenerateTiles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (tileMapRep[x][y] == 1)
                {
                    // Calculate the local position for this tile
                    Vector3Int cellPosition = new Vector3Int(x, y, 0);
                    Vector3 worldPosition = grid.CellToWorld(cellPosition);

                    // Instantiate the tile prefab at the calculated position
                    Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
                }
                
            }
        }
    }
    void makeTileMapRep(int level)
    {
        tileMapRep = new List<List<int>>();
        float emptyTileProbability = Mathf.Clamp01((40f - level) / 40f); // Higher level = more 0's
        bool[,] visited = new bool[gridWidth, gridHeight];

        // Initialize grid with all zeros
        for (int y = 0; y < gridHeight; y++)
        {
            List<int> row = new List<int>();
            for (int x = 0; x < gridWidth; x++)
            {
                row.Add(0); // Start with no walkable tiles
            }
            tileMapRep.Add(row);
        }

        // Ensure (0,0) always has a tile
        tileMapRep[0][0] = 1;

        // Generate a single continuous path starting from (0,0)
        GenerateContinuousPathFor1x1x3();

        // Add extra tiles based on difficulty level
        AddRandomTiles(emptyTileProbability);
    }

    void GenerateContinuousPathFor1x1x3()
    {
        // Start at position (0,0)
        int currentX = 0;
        int currentY = 0;

        // Continue generating the path
        while (true)
        {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();

            // Check valid moves (up, down, left, right)
            if (currentY + 3 < gridHeight && tileMapRep[currentY + 3][currentX] == 0) // Move down
                possibleMoves.Add(new Vector2Int(currentX, currentY + 3));
            if (currentY - 1 >= 0 && tileMapRep[currentY - 1][currentX] == 0) // Move up
                possibleMoves.Add(new Vector2Int(currentX, currentY - 1));
            if (currentX + 1 < gridWidth && tileMapRep[currentY][currentX + 1] == 0) // Move right
                possibleMoves.Add(new Vector2Int(currentX + 1, currentY));
            if (currentX - 1 >= 0 && tileMapRep[currentY][currentX - 1] == 0) // Move left
                possibleMoves.Add(new Vector2Int(currentX - 1, currentY));

            if (possibleMoves.Count == 0)
                break; // No more valid moves, path generation is complete

            // Choose a random move
            Vector2Int nextMove = possibleMoves[Random.Range(0, possibleMoves.Count)];

            // Ensure the new tile supports the 1x1x3 character
            if (Mathf.Abs(nextMove.x - currentX) == 1) // Horizontal move
            {
                tileMapRep[currentY][nextMove.x] = 1;
            }
            else if (Mathf.Abs(nextMove.y - currentY) == 3) // Vertical move
            {
                tileMapRep[nextMove.y - 2][currentX] = 1;
                tileMapRep[nextMove.y - 1][currentX] = 1;
                tileMapRep[nextMove.y][currentX] = 1;
            }

            // Update current position
            currentX = nextMove.x;
            currentY = nextMove.y;
        }
    }

    void AddRandomTiles(float emptyTileProbability)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (tileMapRep[y][x] == 0 && Random.value > emptyTileProbability)
                {
                    // Ensure the new tile won't break continuity
                    if (CanAddTile(x, y))
                    {
                        tileMapRep[y][x] = 1;

                        // Ensure vertical continuity for 1x1x3 standing
                        if (y + 1 < gridHeight) tileMapRep[y + 1][x] = 1;
                        if (y + 2 < gridHeight) tileMapRep[y + 2][x] = 1;
                    }
                }
            }
        }
    }

    bool CanAddTile(int x, int y)
    {
        // Ensure the new tile is connected to the existing path
        if (x > 0 && tileMapRep[y][x - 1] == 1) return true; // Left
        if (x < gridWidth - 1 && tileMapRep[y][x + 1] == 1) return true; // Right
        if (y > 0 && tileMapRep[y - 1][x] == 1) return true; // Above
        if (y < gridHeight - 1 && tileMapRep[y + 1][x] == 1) return true; // Below
        return false;
    }




    /* // this version does not think about having at least a two neghboring ones for the charecter to be able to move
    void makeTileMapRep(int level)
    {
        tileMapRep = new List<List<int>>();
        float emptyTileProbability = Mathf.Clamp01((40f - level) / 40f); // Higher level = more 0's
        bool[,] visited = new bool[gridWidth, gridHeight];

        // Initialize grid with all zeros
        for (int y = 0; y < gridHeight; y++)
        {
            List<int> row = new List<int>();
            for (int x = 0; x < gridWidth; x++)
            {
                row.Add(0); // Start with no walkable tiles
            }
            tileMapRep.Add(row);
        }

        // Generate a continuous path
        GenerateContinuousPath(visited);

        // Add random tiles to make the map more complex, considering the difficulty level
        AddRandomTiles(emptyTileProbability);
    }

    void GenerateContinuousPath(bool[,] visited)
    {
        int currentX = Random.Range(0, gridWidth); // Random start column
        int currentY = 0;                         // Start at the top row

        while (currentY < gridHeight)
        {
            // Mark current tile as walkable
            tileMapRep[currentY][currentX] = 1;
            visited[currentX, currentY] = true;

            // Decide next move (up/down/left/right)
            List<Vector2Int> possibleMoves = new List<Vector2Int>();

            if (currentY < gridHeight - 1 && !visited[currentX, currentY + 1]) // Down
                possibleMoves.Add(new Vector2Int(currentX, currentY + 1));
            if (currentX > 0 && !visited[currentX - 1, currentY]) // Left
                possibleMoves.Add(new Vector2Int(currentX - 1, currentY));
            if (currentX < gridWidth - 1 && !visited[currentX + 1, currentY]) // Right
                possibleMoves.Add(new Vector2Int(currentX + 1, currentY));

            if (possibleMoves.Count > 0)
            {
                Vector2Int nextMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
                currentX = nextMove.x;
                currentY = nextMove.y;
            }
            else
            {
                break; // No more valid moves
            }
        }
    }

    void AddRandomTiles(float emptyTileProbability)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (tileMapRep[y][x] == 0 && Random.value > emptyTileProbability)
                {
                    // Ensure the new tile won't introduce diagonal connections
                    if (HasValidNeighbors(x, y))
                    {
                        tileMapRep[y][x] = 1;
                    }
                }
            }
        }
    }

    bool HasValidNeighbors(int x, int y)
    {
        // Check only horizontal and vertical neighbors
        bool hasHorizontalNeighbor = (x > 0 && tileMapRep[y][x - 1] == 1) || (x < gridWidth - 1 && tileMapRep[y][x + 1] == 1);
        bool hasVerticalNeighbor = (y > 0 && tileMapRep[y - 1][x] == 1) || (y < gridHeight - 1 && tileMapRep[y + 1][x] == 1);

        return hasHorizontalNeighbor || hasVerticalNeighbor;
    }*/




    /*  // old code makes tilemap based on the level still makes diagonal tile neighbours
    void makeTileMapRep(int level)
    {
        tileMapRep = new List<List<int>>();

        float emptyTileProbability = Mathf.Clamp(level / 40f, 0.1f, 1f); // Higher level = more 0's
        int characterHeight = 2; // Character's height in tiles

        // Initialize the grid with all 1s
        for (int y = 0; y < gridHeight; y++)
        {
            List<int> row = new List<int>();
            for (int x = 0; x < gridWidth; x++)
            {
                row.Add(1); // Start with all walkable tiles
            }
            tileMapRep.Add(row);
        }

        // Introduce 0s based on difficulty level
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (Random.value < emptyTileProbability)
                {
                    // Check if removing this tile would break continuity
                    if (IsTileRemovable(x, y, characterHeight))
                    {
                        tileMapRep[y][x] = 0;
                    }
                }
            }
        }

        // Ensure there's a continuous path
        MakeContinuous();
    }

    bool IsTileRemovable(int x, int y, int characterHeight)
    {
        // Prevent removing tiles that are part of the top rows (character height dependency)
        if (y < characterHeight - 1) return false;

        // Ensure at least one horizontal or vertical neighbor is a 1
        bool hasNeighbor = false;

        if (x > 0 && tileMapRep[y][x - 1] == 1) hasNeighbor = true; // Left
        if (x < gridWidth - 1 && tileMapRep[y][x + 1] == 1) hasNeighbor = true; // Right
        if (y > 0 && tileMapRep[y - 1][x] == 1) hasNeighbor = true; // Above
        if (y < gridHeight - 1 && tileMapRep[y + 1][x] == 1) hasNeighbor = true; // Below

        return hasNeighbor;
    }

    void MakeContinuous()
    {
        // Ensure at least one continuous walkable path from top to bottom
        int currentX = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            tileMapRep[y][currentX] = 1; // Ensure a path in the current column

            // Randomly decide to move left, right, or stay in the same column
            if (Random.value > 0.5f && currentX > 0) currentX--; // Move left
            else if (Random.value > 0.5f && currentX < gridWidth - 1) currentX++; // Move right
        }
    }*/


    /* // old code no consideration on level and charecters size 
    void makeTileMAprep()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            // Create a new row
            List<int> row = new List<int>();
            for (int x = 0; x < gridWidth; x++)
            {
                // Generate a random value for each cell (either 0 or 1)
                // For simplicity, start with a random distribution, but ensure it remains continuous
                int tile = Random.Range(0, 2); // 0 or 1
                row.Add(tile);
            }

            // Add the row to the tileMapRep
            tileMapRep.Add(row);
        }

        // Ensure the tile map is walkable and continuous
        MakeContinuous();

    }

    void MakeContinuous()
    {
        // Ensure the first and last rows are fully walkable for continuity
        for (int x = 0; x < gridWidth; x++)
        {
            tileMapRep[0][x] = 1; // Top row
            tileMapRep[gridHeight - 1][x] = 1; // Bottom row
        }

        // Ensure there's a walkable path in the middle rows
        for (int y = 1; y < gridHeight - 1; y++)
        {
            bool rowHasPath = false;
            for (int x = 0; x < gridWidth; x++)
            {
                if (tileMapRep[y][x] == 1)
                {
                    rowHasPath = true;
                    break;
                }
            }

            // If no path exists, force a path in this row
            if (!rowHasPath)
            {
                int randomTile = Random.Range(0, gridWidth);
                tileMapRep[y][randomTile] = 1;
            }
        }
    }*/

}
