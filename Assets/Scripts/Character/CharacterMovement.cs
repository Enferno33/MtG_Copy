using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 2f; // Movement speed of the player
    private Vector2 targetPosition; // Target position to move towards
    private bool isMoving = false; // Flag to indicate if the player is currently moving
    private Vector2 moveDirection; // Current movement direction
    private bool awaitingBattle = false; // Flag to check if a battle should start after reaching the center
    public bool inCombat;
    
    // UI Buttons for controlling player movement
    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;

    private TileGenerator tileGenerator; // Reference to the TileGenerator
    private Tile currentTile;            // The tile the player is currently on
    private Animator animator;           // Reference to the Animator component
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private FightUIManager fightUIManager; // Reference to the FightUIManager

    void Start()
    {
        // Initialize target position to the character's starting position
        targetPosition = transform.position;

        // Assign button click events for movement
        upButton.onClick.AddListener(() => SetMoveDirection(Vector2.up));
        downButton.onClick.AddListener(() => SetMoveDirection(Vector2.down));
        leftButton.onClick.AddListener(() => SetMoveDirection(Vector2.left));
        rightButton.onClick.AddListener(() => SetMoveDirection(Vector2.right));

        // Find the TileGenerator and FightUIManager in the scene
        tileGenerator = FindObjectOfType<TileGenerator>();
        fightUIManager = FindObjectOfType<FightUIManager>();

        // Initialize the current tile to the first one
        UpdateCurrentTile();

        // Get references to Animator and SpriteRenderer components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Handle continuous movement if a direction is set
        if (isMoving)
        {
            MoveToTarget();
        }

        // Handle keyboard input for movement
        HandleKeyboardInput();
    }

    // Method to set the movement direction and start moving
    public void SetMoveDirection(Vector2 direction)
    {
        if (!isMoving && CanMoveInDirection(direction)) // Prevent setting a new direction if already moving
        {
            moveDirection = direction; // Set the movement direction
            targetPosition = GetNextTileCenter(direction); // Calculate the next tile's center
            isMoving = true; // Start moving

            // Spawn the next tile immediately when movement starts
            tileGenerator.MoveToNextTile(direction); // This spawns the next tile immediately

            // Set the Animator "move" parameter to true
            if (animator != null)
            {
                animator.SetBool("move", true);
            }

            // Flip the sprite if moving left
            if (direction == Vector2.left)
            {
                spriteRenderer.flipX = true; // Flip the sprite when moving left
            }
            else if (direction == Vector2.right)
            {
                spriteRenderer.flipX = false; // Reset the sprite flip when moving right
            }
        }
    }

    // Check if the player can move in the given direction based on the current tile's paths
    private bool CanMoveInDirection(Vector2 direction)
    {
        if (currentTile == null) return false;

        if (direction == Vector2.up && currentTile.canMoveUp) return true;
        if (direction == Vector2.down && currentTile.canMoveDown) return true;
        if (direction == Vector2.left && currentTile.canMoveLeft) return true;
        if (direction == Vector2.right && currentTile.canMoveRight) return true;

        return false;
    }

    // Method to smoothly move the character to the target position
    private void MoveToTarget()
    {
        // Move towards the target position at a set speed
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if the character has reached the target position
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false; // Stop movement

            // Update current tile when reaching the center
            UpdateCurrentTile();

            // Set the Animator "move" parameter to false when movement stops
            if (animator != null)
            {
                animator.SetBool("move", false);
            }

            // Trigger the fight if awaiting battle after reaching the center
            if (awaitingBattle)
            {
                awaitingBattle = false; // Reset the flag
                TriggerFightUIIfEnemyPresent();
            }
        }
    }

    // Calculate the center position of the next tile in the given direction
    private Vector2 GetNextTileCenter(Vector2 direction)
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        return currentPos + direction * tileGenerator.tileSize; // Move to the center of the next tile
    }

    private void UpdateCurrentTile()
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        currentTile = tileGenerator.GetTileAtPosition(currentPos)?.GetComponent<Tile>();

        if (currentTile != null)
        {
            // Update UI button interactivity based on the tile's paths
            UpdateButtonInteractivity();

            // Check if there's an enemy on this tile and set to await battle if needed
            if (currentTile.HasEnemy()) // Assuming Tile has a method to check for enemies
            {
                awaitingBattle = true; // Set to trigger battle after movement completes
            }
        }
    }

    private void UpdateButtonInteractivity()
    {
        // Set button interactable states based on the tile's paths
        upButton.interactable = currentTile.canMoveUp;
        downButton.interactable = currentTile.canMoveDown;
        leftButton.interactable = currentTile.canMoveLeft;
        rightButton.interactable = currentTile.canMoveRight;
    }

    // Handle arrow key input for movement
    private void HandleKeyboardInput()
    {
        if (!isMoving && !inCombat) // Only check for input if not currently moving
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && CanMoveInDirection(Vector2.up))
            {
                SetMoveDirection(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && CanMoveInDirection(Vector2.down))
            {
                SetMoveDirection(Vector2.down);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && CanMoveInDirection(Vector2.left))
            {
                SetMoveDirection(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && CanMoveInDirection(Vector2.right))
            {
                SetMoveDirection(Vector2.right);
            }
        }
    }

    // Method to move the player to the center of the tile if needed after a battle
    public void MoveToTileCenterIfNeeded()
    {
        if (!isMoving && Vector2.Distance(transform.position, GetCurrentTileCenter()) > 0.1f)
        {
            // Set target position to the current tile's center and start moving
            targetPosition = GetCurrentTileCenter();
            isMoving = true;

            // Set the Animator "move" parameter to true
            if (animator != null)
            {
                animator.SetBool("move", true);
            }
        }
        else
        {
            // Immediately trigger the fight if already at the center
            TriggerFightUIIfEnemyPresent();
        }
    }

    // Calculate the current tile's center position
    private Vector2 GetCurrentTileCenter()
    {
        return currentTile != null ? currentTile.GetPosition() : transform.position;
    }

    // Trigger the fight UI if any enemies are present
    private void TriggerFightUIIfEnemyPresent()
    {
        if (currentTile != null && currentTile.HasEnemy())
        {
            List<GameObject> enemies = currentTile.GetEnemies();
            List<EnemyStats> enemyStatsList = new List<EnemyStats>();

            foreach (GameObject enemy in enemies)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                if (enemyStats != null)
                {
                    enemyStatsList.Add(enemyStats);
                }
            }

            PlayerStats playerStats = GetComponent<PlayerStats>();

            if (enemyStatsList.Count > 0 && playerStats != null)
            {
                fightUIManager.PreStartFight(playerStats);
                //fightUIManager.StartFight(playerStats, enemyStatsList);
            }
        }
    }
}
