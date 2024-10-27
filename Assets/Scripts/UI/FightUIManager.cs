using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.SceneManagement; // Import SceneManagement for restarting the game
using System.Collections.Generic; // Needed for List<>

public class FightUIManager : MonoBehaviour
{
    public GameObject fightPanel;       // The fight UI panel
    public Button attackButton;         // Attack button
    public Button healButton;           // Heal button
    public TMP_Text combatLogText;      // TMP_Text for combat log
    public GameObject moveButtons;      // Parent GameObject for movement buttons
    public Button closeFightUIButton;   // Button to close the fight UI

    // UI elements for displaying player
    public Image playerImage;           // Image component for displaying the player
    public TMP_Text playerNameText;     // TMP_Text to display player's name
    public TMP_Text playerHealthText;   // TMP_Text to display player's health

    // UI elements for displaying enemies
    public Image[] enemyImages = new Image[8]; // Array of Image components for displaying up to 8 enemies
    public TMP_Text[] enemyNameTexts = new TMP_Text[8]; // Array of TMP_Text for enemy names
    public TMP_Text[] enemyHealthTexts = new TMP_Text[8]; // Array of TMP_Text for enemy health

    // UI elements for death message and restart functionality
    public TMP_Text deathMessageText;   // TMP_Text to display "You Died" message
    public Button restartButton;        // Button to restart the game

    private PlayerStats playerStats;    // Reference to the player's stats
    private List<EnemyStats> enemyStatsList; // Reference to the list of enemies
    private int currentEnemyIndex = 0;  // Track which enemy is currently being fought
    private CharacterMovement characterMovement; // Reference to the CharacterMovement script

    public bool InCombat;

    public GameObject enemyPrefab; // Prefab for the enemy

    void Start()
    {
        // Assign button listeners
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        healButton.onClick.AddListener(OnHealButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        closeFightUIButton.onClick.AddListener(CloseFightUI);

        // Initially hide the fight panel, death message, and restart button
        fightPanel.SetActive(false);
        deathMessageText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        closeFightUIButton.gameObject.SetActive(false);

        // Find the CharacterMovement component on the player
        characterMovement = FindObjectOfType<CharacterMovement>();
    }

    // Method to start the fight and spawn enemies
    public void StartFight(PlayerStats player)
    {
        int enemyCount = Random.Range(1, 9); // Spawn between 1 and 8 enemies
        enemyStatsList = new List<EnemyStats>(); // Initialize enemy stats list

        // Clear previous enemy images and texts
        ResetEnemyUI();

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyGO = Instantiate(enemyPrefab); // Instantiate enemy prefab
            EnemyStats enemyStats = enemyGO.GetComponent<EnemyStats>(); // Get the EnemyStats component
            enemyStatsList.Add(enemyStats); // Add to the list of enemies

            // Update the UI for this enemy
            if (i < enemyImages.Length) // Ensure we don't exceed the array length
            {
                enemyImages[i].gameObject.SetActive(true); // Show the enemy image
                enemyNameTexts[i].gameObject.SetActive(true); // Show the enemy name text
                enemyHealthTexts[i].gameObject.SetActive(true); // Show the enemy health text
            }
        }

        // Initialize fight with the player
        StartFight(player, enemyStatsList);
    }

    // Method to initialize and show the fight UI for multiple enemies
    public void StartFight(PlayerStats player, List<EnemyStats> enemies)
    {
        playerStats = player;
        enemyStatsList = enemies;
        currentEnemyIndex = 0; // Start with the first enemy
        InCombat = true;

        // Update the UI with player and enemy details
        UpdatePlayerUI();
        UpdateEnemyUI();

        // Show the fight UI and hide movement buttons
        fightPanel.SetActive(true);
        moveButtons.SetActive(false);

        // Log initial combat message
        UpdateCombatLog("A wild " + enemyStatsList[currentEnemyIndex].name + " appears!");
    }

    private void UpdatePlayerUI()
    {
        if (playerStats != null)
        {
            playerImage.sprite = playerStats.GetComponent<SpriteRenderer>().sprite; // Display player sprite
            playerNameText.text = playerStats.name; // Display player name
            playerHealthText.text = "Health: " + playerStats.currentHealth; // Display player health
        }
    }

    private void UpdateEnemyUI()
    {
        for (int i = 0; i < enemyStatsList.Count; i++)
        {
            if (i < enemyStatsList.Count)
            {
                var currentEnemy = enemyStatsList[i];
                if (currentEnemy != null && currentEnemy.gameObject != null) // Check if the enemy still exists
                {
                    enemyImages[i].sprite = currentEnemy.GetComponent<SpriteRenderer>().sprite;
                    enemyNameTexts[i].text = currentEnemy.name;
                    enemyHealthTexts[i].text = "Health: " + currentEnemy.currentHealth;
                }
            }
        }
    }

    private void ResetEnemyUI()
    {
        foreach (var image in enemyImages)
        {
            image.gameObject.SetActive(false); // Hide all enemy images
        }

        foreach (var text in enemyNameTexts)
        {
            text.text = ""; // Clear enemy names
        }

        foreach (var healthText in enemyHealthTexts)
        {
            healthText.text = ""; // Clear health texts
        }
    }

    private void OnAttackButtonClicked()
    {
        if (playerStats != null && enemyStatsList.Count > currentEnemyIndex)
        {
            EnemyStats currentEnemy = enemyStatsList[currentEnemyIndex];

            // Player attacks the current enemy
            playerStats.Attack(currentEnemy);
            UpdateCombatLog("Player attacks " + currentEnemy.name + " for " + playerStats.attackPower + " damage.");
            UpdateEnemyHealthUI(); // Update enemy health display

            // Check if the current enemy is still alive
            if (currentEnemy.currentHealth > 0)
            {
                // Enemy attacks back
                currentEnemy.Attack(playerStats);
                UpdateCombatLog(currentEnemy.name + " attacks player for " + currentEnemy.attackPower + " damage.");
                UpdatePlayerHealthUI(); // Update player health display
            }

            // Check the combat result
            CheckCombatResult();
        }
    }

    private void OnHealButtonClicked()
    {
        if (playerStats != null)
        {
            // Heal the player (for example, heal 20 health points)
            playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + 20, playerStats.maxHealth);
            UpdateCombatLog("Player heals for 20 health. Current health: " + playerStats.currentHealth);
            UpdatePlayerHealthUI(); // Update player health display

            // Enemy's turn after healing
            if (enemyStatsList[currentEnemyIndex].currentHealth > 0)
            {
                enemyStatsList[currentEnemyIndex].Attack(playerStats);
                UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " attacks player for " + enemyStatsList[currentEnemyIndex].attackPower + " damage.");
                UpdatePlayerHealthUI(); // Update player health display
            }

            // Check the combat result
            CheckCombatResult();
        }
    }

    private void UpdateCombatLog(string message)
    {
        combatLogText.text += message + "\n"; // Update TMP text
    }

    private void ClearCombatLog()
    {
        combatLogText.text = ""; // Clear the TMP text
    }

    private void UpdatePlayerHealthUI()
    {
        if (playerHealthText != null && playerStats != null)
        {
            playerHealthText.text = "Health: " + playerStats.currentHealth; // Update health display
        }
    }

    private void UpdateEnemyHealthUI()
    {
        for (int i = 0; i < enemyStatsList.Count; i++)
        {
            if (enemyHealthTexts[i] != null)
            {
                enemyHealthTexts[i].text = "Health: " + enemyStatsList[i].currentHealth; // Update health display
            }
        }
    }

    private void CheckCombatResult()
    {
        if (playerStats.currentHealth <= 0)
        {
            UpdateCombatLog("Player has been defeated!");
            ShowDeathMessage(); // Show the "You Died" message and restart button
        }
        else if (enemyStatsList[currentEnemyIndex].currentHealth <= 0)
        {
            UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " has been defeated!");

            currentEnemyIndex++; // Move to the next enemy

            // Check if there are more enemies to fight
            if (currentEnemyIndex < enemyStatsList.Count)
            {
                UpdateEnemyUI(); // Update the UI with the new enemy
                UpdateCombatLog("A wild " + enemyStatsList[currentEnemyIndex].name + " appears!");
            }
            else
            {
                // All enemies are defeated
                EndFight(); // End the fight after all enemies are defeated
            }
        }
    }

    private void ShowDeathMessage()
    {
        deathMessageText.gameObject.SetActive(true);
        deathMessageText.text = "You Died"; // Display death message
        restartButton.gameObject.SetActive(true); // Show restart button

        // Hide other UI elements related to combat
        attackButton.gameObject.SetActive(false);
        healButton.gameObject.SetActive(false);
    }

    private void OnRestartButtonClicked()
    {
        // Clear combat log before restarting
        ClearCombatLog();

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnEnemyDeath(EnemyStats enemy)
    {
        EndFight();
    }

    private void EndFight()
    {
        ClearCombatLog(); // Clear combat log when the fight ends
        fightPanel.SetActive(false);  // Hide the fight UI
        moveButtons.SetActive(true);  // Show movement buttons again
        InCombat = false;

        foreach (var enemy in enemyStatsList)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject); // Destroy any remaining enemy game objects
            }
        }

        if (characterMovement != null)
        {
            characterMovement.MoveToTileCenterIfNeeded();
        }

        closeFightUIButton.gameObject.SetActive(true); // Show the close button
    }

    private void CloseFightUI()
    {
        fightPanel.SetActive(false);
        moveButtons.SetActive(true);
        closeFightUIButton.gameObject.SetActive(false); // Hide the close button
        ClearCombatLog(); // Clear combat log 
    }
}
