using UnityEngine;
using System.Collections.Generic;

public class TileGenerator : MonoBehaviour
{
    public GameObject tilePrefab; // Assign the Tile prefab in the inspector
    public GameObject playerPrefab; // Assign the Player prefab in the inspector
    public int tileSize = 10;      // Size of each tile
    private Vector2 currentTilePosition = Vector2.zero; // Starting position on the grid
    private Dictionary<Vector2, GameObject> spawnedTiles = new Dictionary<Vector2, GameObject>(); // Store active tiles
    private Dictionary<Vector2, TileState> visitedTiles = new Dictionary<Vector2, TileState>();   // Store visited tile states
    private Stack<Vector2> playerPath = new Stack<Vector2>(); // Track the path history of the player

    private GameObject playerInstance; // Reference to the spawned player instance

    void Start()
    {
        // Generate the first tile at the starting position
        GenerateTile(currentTilePosition, true, Vector2.zero); // Starting tile has no entry direction

        // Spawn the player prefab at the starting position
        SpawnPlayerAtStart();

        // Add the starting position to the path stack
        playerPath.Push(currentTilePosition);
    }

    // Spawn the player prefab at the initial tile's position without snapping to the center unnecessarily
    private void SpawnPlayerAtStart()
    {
        if (playerPrefab != null)
        {
            // Spawn the player at the current tile position
            playerInstance = Instantiate(playerPrefab, currentTilePosition, Quaternion.identity);

            // Ensure the player GameObject is active
            playerInstance.SetActive(true);
        }
        else
        {
            Debug.LogError("Player prefab is not assigned in the TileGenerator.");
        }
    }

    public void MoveToNextTile(Vector2 direction)
    {
        // Calculate the next tile's position based on the movement direction
        Vector2 nextTilePosition = currentTilePosition + (direction * tileSize);

        // Allow backtracking: check if moving directly back to the previous tile in the stack
        if (playerPath.Count > 1 && nextTilePosition == playerPath.Peek())
        {
            playerPath.Pop(); // Backtrack to the previous tile
            currentTilePosition = nextTilePosition;
            Debug.Log("Backtracking to tile at position: " + nextTilePosition);
            return;
        }

        // Handle new tile spawning or revisiting of existing tiles
        if (!spawnedTiles.ContainsKey(nextTilePosition))
        {
            // Restore state if tile was visited before, else generate a new tile
            if (visitedTiles.ContainsKey(nextTilePosition))
            {
                RestoreTile(nextTilePosition);
                Debug.Log("Restoring visited tile at position: " + nextTilePosition);
            }
            else
            {
                GenerateTile(nextTilePosition, false, direction); // Pass the entry direction
                Debug.Log("Generating new tile at position: " + nextTilePosition);
            }
        }

        // Update the current position and push the new position onto the path stack
        currentTilePosition = nextTilePosition;
        playerPath.Push(currentTilePosition);
    }

    private void GenerateTile(Vector2 position, bool initialTile, Vector2 entryDirection)
    {
        // Instantiate a new tile at the specified position
        GameObject newTile = Instantiate(tilePrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, transform);

        // Set up the tile's properties and state
        Tile tileScript = newTile.GetComponent<Tile>();
        if (tileScript != null)
        {
            tileScript.SetPosition(position);

            // Initialize the tile with paths and save its state
            tileScript.InitializeTile(startingTile: initialTile, entryDirection: entryDirection);
            if (!initialTile) 
            {
                visitedTiles[position] = tileScript.SaveState(); // Save the state for future restoration
            }
        }
        else
        {
            Debug.LogError("Tile prefab is missing the Tile script.");
        }

        // Add the newly created tile to the spawned tiles dictionary
        spawnedTiles[position] = newTile;
    }

    private void RestoreTile(Vector2 position)
    {
        // Restore a previously visited tile to its saved state
        GameObject restoredTile = Instantiate(tilePrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, transform);
        Tile tileScript = restoredTile.GetComponent<Tile>();
        if (tileScript != null)
        {
            tileScript.RestoreState(visitedTiles[position]); // Use the saved state to restore paths
        }
        else
        {
            Debug.LogError("Tile prefab is missing the Tile script.");
        }

        // Add the restored tile to the dictionary of spawned tiles
        spawnedTiles[position] = restoredTile;
    }

    public Vector2 GetCurrentTilePosition()
    {
        return currentTilePosition;
    }

    // Retrieve a tile at a specific position on the grid
    public GameObject GetTileAtPosition(Vector2 position)
    {
        spawnedTiles.TryGetValue(position, out GameObject tile);
        return tile;
    }
}
