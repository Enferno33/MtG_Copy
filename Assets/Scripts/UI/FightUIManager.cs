using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FightUIManager : MonoBehaviour
{
    // UI elements for fight interaction
    public GameObject fightPanel;
    public Button attackButton;
    public Button healButton;
    public TMP_Text combatLogText;
    public GameObject moveButtons;
    public Button closeFightUIButton;

    // UI elements for displaying player stats
    public Image playerImage;
    public TMP_Text playerNameText;
    public TMP_Text playerHealthText;

    // Enemy UI prefab and container for dynamically instantiated enemy UI elements
    public GameObject enemyUIPrefab;
    public Transform enemyUIContainer;
    public Sprite bonesSprite; // Sprite to display when an enemy is defeated

    // UI elements for death message and restart functionality
    public TMP_Text deathMessageText;
    public Button restartButton;

    // Internal references to manage combat entities and state
    private PlayerStats playerStats;
    private List<EnemyStats> enemyStatsList;
    private List<GameObject> enemyUIElements = new List<GameObject>();
    private int currentEnemyIndex = 0;
    private CharacterMovement characterMovement;

    public bool InCombat; // Tracks if the player is currently in combat

    public GameObject enemyPrefab; // Prefab for spawning enemies during the fight

    void Start()
    {
        // Add listeners for button actions
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        healButton.onClick.AddListener(OnHealButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        closeFightUIButton.onClick.AddListener(CloseFightUI);

        // Hide combat-related UI elements at the start
        fightPanel.SetActive(false);
        deathMessageText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        closeFightUIButton.gameObject.SetActive(false);

        // Find and store a reference to the CharacterMovement component
        characterMovement = FindObjectOfType<CharacterMovement>();

        // Clear enemy UI container to ensure it starts empty
        ClearEnemyUIContainer();
    }

    // Clears all enemy UI elements from the container at the start of the game
    private void ClearEnemyUIContainer()
    {
        foreach (Transform child in enemyUIContainer)
        {
            Destroy(child.gameObject); // Destroy each child GameObject in the container
        }
        enemyUIElements.Clear(); // Clear list to keep track of UI elements
        Debug.Log("Cleared enemy UI container.");
    }

    void Update()
    {
        // Switches target enemy when TAB is pressed if in combat
        if (InCombat && Input.GetKeyDown(KeyCode.Tab))
        {
            CycleEnemyTarget();
        }
    }

    // Initializes the fight by creating enemy UI elements and setting up player stats
    public void PreStartFight(PlayerStats player)
    {
        // Clear any existing UI elements from previous fights
        ClearEnemyUIContainer();

        // Spawn a random number of enemies
        int enemyCount = Random.Range(1, 9);
        enemyStatsList = new List<EnemyStats>();
        enemyUIElements = new List<GameObject>();

        ResetEnemyUI();

        for (int i = 0; i < enemyCount; i++)
        {
            // Instantiate an enemy prefab and initialize its health
            GameObject enemyGO = Instantiate(enemyPrefab);
            EnemyStats enemyStats = enemyGO.GetComponent<EnemyStats>();
            enemyStats.InitializeHealth();
            enemyStatsList.Add(enemyStats);

            // Instantiate the corresponding UI element for the enemy
            GameObject enemyUI = Instantiate(enemyUIPrefab, enemyUIContainer);
            if (enemyUI != null)
            {
                enemyUIElements.Add(enemyUI);
                Debug.Log($"Instantiated enemy UI prefab for enemy {i}");
                UpdateEnemyUI(i);
            }
            else
            {
                Debug.LogError("Failed to instantiate enemyUIPrefab. Please check if the prefab is assigned.");
            }
        }

        StartFight(player, enemyStatsList);

        // Slight delay to allow all data to load before fully updating UI elements
        Invoke(nameof(UpdateAllEnemyUI), 0.1f);
    }

    // Updates all enemy UI elements to reflect accurate health information
    private void UpdateAllEnemyUI()
    {
        for (int i = 0; i < enemyStatsList.Count; i++)
        {
            UpdateEnemyUI(i);
        }
        Debug.Log("All enemy UIs updated with finalized health.");
    }

    // Updates the specific UI for each enemy, showing health and name, and changing to bonesSprite if dead
    private void UpdateEnemyUI(int index)
    {
        if (index < enemyStatsList.Count && index < enemyUIElements.Count)
        {
            EnemyStats enemyStats = enemyStatsList[index];
            GameObject enemyUI = enemyUIElements[index];

            Image enemyImage = enemyUI.transform.Find("EnemyImage").GetComponent<Image>();
            TMP_Text enemyNameText = enemyUI.transform.Find("EnemyName").GetComponent<TMP_Text>();
            TMP_Text enemyHealthText = enemyUI.transform.Find("EnemyHealth").GetComponent<TMP_Text>();

            // Use bones sprite if the enemy is dead, otherwise use their default sprite
            enemyImage.sprite = enemyStats.currentHealth <= 0 ? bonesSprite : enemyStats.GetComponent<SpriteRenderer>().sprite;
            enemyNameText.text = enemyStats.name;
            enemyHealthText.text = "Health: " + enemyStats.currentHealth;
        }
    }

    // Starts the combat UI display and initializes player and enemies
    public void StartFight(PlayerStats player, List<EnemyStats> enemies)
    {
        playerStats = player;
        enemyStatsList = enemies;
        currentEnemyIndex = 0;
        InCombat = true;

        // Update player and enemy UI for fight preparation
        UpdatePlayerUI();
        for (int i = 0; i < enemyStatsList.Count; i++)
        {
            UpdateEnemyUI(i);
        }
        fightPanel.SetActive(true);
        moveButtons.SetActive(false);

        UpdateCombatLog("A wild " + enemyStatsList[currentEnemyIndex].name + " appears!");
        HighlightEnemy(currentEnemyIndex);
    }

    // Updates the player's UI display to reflect current stats
    private void UpdatePlayerUI()
    {
        if (playerStats != null)
        {
            playerImage.sprite = playerStats.GetComponent<SpriteRenderer>().sprite;
            playerNameText.text = playerStats.name;
            playerHealthText.text = "Health: " + playerStats.currentHealth;
        }
    }

    // Clears enemy UI elements from the previous fight
    private void ResetEnemyUI()
    {
        foreach (GameObject enemyUI in enemyUIElements)
        {
            Destroy(enemyUI);
        }
        enemyUIElements.Clear();
    }

    // Cycles to the next enemy target
    private void CycleEnemyTarget()
    {
        UnhighlightEnemy(currentEnemyIndex);
        currentEnemyIndex = (currentEnemyIndex + 1) % enemyStatsList.Count;
        HighlightEnemy(currentEnemyIndex);
    }

    // Highlights the currently targeted enemy
    private void HighlightEnemy(int index)
    {
        if (index < enemyUIElements.Count)
        {
            enemyUIElements[index].transform.Find("EnemyImage").GetComponent<Image>().color = Color.red;
        }
    }

    // Removes the highlight from an enemy
    private void UnhighlightEnemy(int index)
    {
        if (index < enemyUIElements.Count)
        {
            enemyUIElements[index].transform.Find("EnemyImage").GetComponent<Image>().color = Color.white;
        }
    }

    // Handles player attack on the current enemy target
    private void OnAttackButtonClicked()
    {
        if (playerStats != null && currentEnemyIndex < enemyStatsList.Count)
        {
            EnemyStats currentEnemy = enemyStatsList[currentEnemyIndex];
            playerStats.Attack(currentEnemy);
            UpdateCombatLog("Player attacks " + currentEnemy.name + " for " + playerStats.attackPower + " damage.");
            UpdateEnemyUI(currentEnemyIndex);

            // Enemy counter-attacks if still alive
            if (currentEnemy.currentHealth > 0)
            {
                currentEnemy.Attack(playerStats);
                UpdateCombatLog(currentEnemy.name + " attacks player for " + currentEnemy.attackPower + " damage.");
                UpdatePlayerHealthUI();
            }

            CheckCombatResult();
        }
    }

    // Handles player healing and potential enemy attack response
    private void OnHealButtonClicked()
    {
        if (playerStats != null)
        {
            playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + 20, playerStats.maxHealth);
            UpdateCombatLog("Player heals for 20 health. Current health: " + playerStats.currentHealth);
            UpdatePlayerHealthUI();

            // Enemy attack after healing if still in combat
            if (enemyStatsList[currentEnemyIndex].currentHealth > 0)
            {
                enemyStatsList[currentEnemyIndex].Attack(playerStats);
                UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " attacks player for " + enemyStatsList[currentEnemyIndex].attackPower + " damage.");
                UpdatePlayerHealthUI();
            }

            CheckCombatResult();
        }
    }

    // Adds a new entry to the combat log display
    private void UpdateCombatLog(string message)
    {
        combatLogText.text += message + "\n";
    }

    // Clears all messages from the combat log
    private void ClearCombatLog()
    {
        combatLogText.text = "";
    }

    // Updates player health display UI
    private void UpdatePlayerHealthUI()
    {
        if (playerHealthText != null && playerStats != null)
        {
            playerHealthText.text = "Health: " + playerStats.currentHealth;
        }
    }

    // Checks combat state to determine if the player or all enemies have been defeated
    private void CheckCombatResult()
    {
        if (playerStats.currentHealth <= 0)
        {
            UpdateCombatLog("Player has been defeated!");
            ShowDeathMessage();
        }
        else if (enemyStatsList.TrueForAll(enemy => enemy.currentHealth <= 0))
        {
            UpdateCombatLog("All enemies have been defeated!");
            EndFight();
        }
        else if (enemyStatsList[currentEnemyIndex].currentHealth <= 0)
        {
            UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " has been defeated!");
            CycleEnemyTarget();
        }
    }

    // Displays a death message and disables combat UI upon player defeat
    private void ShowDeathMessage()
    {
        deathMessageText.gameObject.SetActive(true);
        deathMessageText.text = "You Died";
        restartButton.gameObject.SetActive(true);

        attackButton.gameObject.SetActive(false);
        healButton.gameObject.SetActive(false);
    }

    // Restarts the current scene to reset the game after defeat
    private void OnRestartButtonClicked()
    {
        ClearCombatLog();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Ends the fight, clearing UI and resetting necessary elements
    private void EndFight()
    {
        ClearCombatLog();
        fightPanel.SetActive(false);
        moveButtons.SetActive(true);
        InCombat = false;

        foreach (var enemy in enemyStatsList)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        if (characterMovement != null)
        {
            characterMovement.MoveToTileCenterIfNeeded();
        }

        closeFightUIButton.gameObject.SetActive(true);
    }

    // Closes the fight UI and clears the combat log
    private void CloseFightUI()
    {
        fightPanel.SetActive(false);
        moveButtons.SetActive(true);
        closeFightUIButton.gameObject.SetActive(false);
        ClearCombatLog();
    }
}
