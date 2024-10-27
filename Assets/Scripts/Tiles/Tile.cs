using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;   // The enemy prefab
    [Range(0, 100)] public float spawnChance; // The chance that this enemy will spawn
}

public class Tile : MonoBehaviour
{
    public bool canMoveUp;
    public bool canMoveDown;
    public bool canMoveLeft;
    public bool canMoveRight;

    private Vector2 position; // The grid position of the tile

    // Path prefabs for different directions
    public GameObject pathUpPrefab;
    public GameObject pathDownPrefab;
    public GameObject pathLeftPrefab;
    public GameObject pathRightPrefab;

    // List of possible enemies to spawn on this tile
    public List<EnemySpawnInfo> enemySpawnInfos = new List<EnemySpawnInfo>();
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List of spawned enemies

    // Initialize the tile's paths; ensure backtracking path is always open
    public void InitializeTile(bool startingTile, Vector2 entryDirection)
    {
        // Clear previously spawned enemies
        ClearEnemies();

        if (startingTile)
        {
            // For the starting tile, only allow upward movement
            canMoveUp = true;
            canMoveDown = false;
            canMoveLeft = false;
            canMoveRight = false;
        }
        else
        {
            // Randomize paths only once when the tile is first generated
            RandomizePaths();

            // Ensure the path back to the previous tile is open based on entry direction
            if (entryDirection == Vector2.up) canMoveDown = true;
            else if (entryDirection == Vector2.down) canMoveUp = true;
            else if (entryDirection == Vector2.left) canMoveRight = true;
            else if (entryDirection == Vector2.right) canMoveLeft = true;

            // Spawn a random number of enemies on this tile
            SpawnEnemies();
        }

        // Spawn paths based on the available directions
        SpawnPaths();
    }

    private void RandomizePaths()
    {
        // Randomize the tile's paths (only called once upon initial tile generation)
        canMoveUp = Random.value > 0.5f;
        canMoveDown = Random.value > 0.5f;
        canMoveLeft = Random.value > 0.5f;
        canMoveRight = Random.value > 0.5f;
    }

    // Clear previously spawned enemies
    private void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            Destroy(enemy); // Remove enemy GameObjects from the scene
        }
        spawnedEnemies.Clear(); // Clear the list for new spawns
    }

    // Spawn the paths based on the allowed directions
    private void SpawnPaths()
    {
        float halfTileSize = 5f; // Half the size of the tile (10 units), used to position paths correctly

        if (canMoveUp && pathUpPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(0, halfTileSize, 0); // Position path halfway up the tile
            GameObject path = Instantiate(pathUpPrefab, pathPosition, Quaternion.identity, transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
        if (canMoveDown && pathDownPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(0, -halfTileSize, 0); // Position path halfway down the tile
            GameObject path = Instantiate(pathDownPrefab, pathPosition, Quaternion.identity, transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
        if (canMoveLeft && pathLeftPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(-halfTileSize, 0, 0); // Position path halfway left on the tile
            GameObject path = Instantiate(pathLeftPrefab, pathPosition, Quaternion.Euler(0, 0, 90), transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
        if (canMoveRight && pathRightPrefab != null)
        {
            Vector3 pathPosition = transform.position + new Vector3(halfTileSize, 0, 0); // Position path halfway right on the tile
            GameObject path = Instantiate(pathRightPrefab, pathPosition, Quaternion.Euler(0, 0, -90), transform);
            path.transform.localScale = new Vector3(0.1f, 1, 1); // Set scale to (0.1, 1, 1)
        }
    }

    // Spawn a random number of enemies based on their spawn chances
    private void SpawnEnemies()
    {
        // Random number of enemies to spawn (1 to 8) for this specific tile
        int enemyCount = Random.Range(1, 9);

        // Reset the spawnedEnemies list to ensure new enemies can be spawned
        spawnedEnemies.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            // Select a random enemy prefab based on their spawn chance
            List<GameObject> eligibleEnemies = new List<GameObject>();
            foreach (var enemyInfo in enemySpawnInfos)
            {
                if (Random.Range(0f, 100f) <= enemyInfo.spawnChance)
                {
                    eligibleEnemies.Add(enemyInfo.enemyPrefab);
                }
            }

            if (eligibleEnemies.Count > 0)
            {
                // Pick a random enemy prefab from the eligible list
                GameObject selectedEnemyPrefab = eligibleEnemies[Random.Range(0, eligibleEnemies.Count)];
                Vector3 enemyPosition = transform.position; // Spawn enemies at the center of the tile
                GameObject spawnedEnemy = Instantiate(selectedEnemyPrefab, enemyPosition, Quaternion.identity, transform);
                spawnedEnemies.Add(spawnedEnemy);
            }
        }
    }

    // Check if the tile has any enemies
    public bool HasEnemy()
    {
        return spawnedEnemies.Count > 0;
    }

    // Get the list of spawned enemies
    public List<GameObject> GetEnemies()
    {
        return spawnedEnemies;
    }

    // Save the current state of the tile's paths for future restoration
    public TileState SaveState()
    {
        return new TileState
        {
            Position = position,
            CanMoveUp = canMoveUp,
            CanMoveDown = canMoveDown,
            CanMoveLeft = canMoveLeft,
            CanMoveRight = canMoveRight
        };
    }

    // Restore the tile's paths from a saved state
    public void RestoreState(TileState state)
    {
        canMoveUp = state.CanMoveUp;
        canMoveDown = state.CanMoveDown;
        canMoveLeft = state.CanMoveLeft;
        canMoveRight = state.CanMoveRight;
        position = state.Position;

        // Ensure paths are spawned based on restored states
        SpawnPaths();
    }

    // Set the grid position of the tile
    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }

    public Vector2 GetPosition()
    {
        return position;
    }
}
