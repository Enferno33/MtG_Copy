using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FightUIManager : MonoBehaviour
{
    public GameObject fightPanel;
    public Button attackButton;
    public Button healButton;
    public TMP_Text combatLogText;
    public GameObject moveButtons;
    public Button closeFightUIButton;

    public Image playerImage;
    public TMP_Text playerNameText;
    public TMP_Text playerHealthText;

    public GameObject enemyUIPrefab;
    public Transform enemyUIContainer;
    public Sprite bonesSprite;

    public TMP_Text deathMessageText;
    public Button restartButton;

    private PlayerStats playerStats;
    private List<EnemyStats> enemyStatsList;
    private List<GameObject> enemyUIElements = new List<GameObject>();
    private int currentEnemyIndex = 0;
    private CharacterMovement characterMovement;

    public bool InCombat;

    public GameObject enemyPrefab;

    void Start()
    {
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        healButton.onClick.AddListener(OnHealButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        closeFightUIButton.onClick.AddListener(CloseFightUI);

        fightPanel.SetActive(false);
        deathMessageText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        closeFightUIButton.gameObject.SetActive(false);

        characterMovement = FindObjectOfType<CharacterMovement>();

         // Clear enemy UI container at the start of the game
        ClearEnemyUIContainer();
    }

    // Method to clear the enemyUIContainer
    private void ClearEnemyUIContainer()
    {
        foreach (Transform child in enemyUIContainer)
        {
            Destroy(child.gameObject); // Destroy each child GameObject
        }
        enemyUIElements.Clear(); // Clear the list to ensure consistency
        Debug.Log("Cleared enemy UI container.");
    }

    void Update()
    {
        if (InCombat && Input.GetKeyDown(KeyCode.Tab))
        {
            CycleEnemyTarget();
        }
    }

    public void PreStartFight(PlayerStats player)
    {
        ClearEnemyUIContainer();

        int enemyCount = Random.Range(1, 9);
        enemyStatsList = new List<EnemyStats>();
        enemyUIElements = new List<GameObject>(); // Ensure it's cleared before use

        ResetEnemyUI();

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyGO = Instantiate(enemyPrefab);
            EnemyStats enemyStats = enemyGO.GetComponent<EnemyStats>();

            // Ensure health initialization here
            enemyStats.InitializeHealth();
            enemyStatsList.Add(enemyStats);

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

        // Optionally, delay the full update to ensure all data is loaded
        Invoke(nameof(UpdateAllEnemyUI), 0.1f);  // Slight delay to allow full initialization
    }
    private void UpdateAllEnemyUI()
    {
        for (int i = 0; i < enemyStatsList.Count; i++)
        {
            UpdateEnemyUI(i);  // Update each enemy UI after full initialization
        }
        Debug.Log("All enemy UIs updated with finalized health.");
    }

    private void UpdateEnemyUI(int index)
    {
        if (index < enemyStatsList.Count && index < enemyUIElements.Count)
        {
            EnemyStats enemyStats = enemyStatsList[index];
            GameObject enemyUI = enemyUIElements[index];

            Image enemyImage = enemyUI.transform.Find("EnemyImage").GetComponent<Image>();
            TMP_Text enemyNameText = enemyUI.transform.Find("EnemyName").GetComponent<TMP_Text>();
            TMP_Text enemyHealthText = enemyUI.transform.Find("EnemyHealth").GetComponent<TMP_Text>();

            // Set image to bones sprite if enemy is dead
            if (enemyStats.currentHealth <= 0)
            {
                enemyImage.sprite = bonesSprite;
            }
            else
            {
                enemyImage.sprite = enemyStats.GetComponent<SpriteRenderer>().sprite;
            }

            enemyNameText.text = enemyStats.name;
            enemyHealthText.text = "Health: " + enemyStats.currentHealth;
        }
    }


    public void StartFight(PlayerStats player, List<EnemyStats> enemies)
    {
        playerStats = player;
        enemyStatsList = enemies;
        currentEnemyIndex = 0;
        InCombat = true;
        

        UpdatePlayerUI();
        for (int i = 0; i < enemyStatsList.Count; i++)
        {
            UpdateEnemyUI(i); // Ensure 'i' is passed as an argument to UpdateEnemyUI
        }
        fightPanel.SetActive(true);
        moveButtons.SetActive(false);

        UpdateCombatLog("A wild " + enemyStatsList[currentEnemyIndex].name + " appears!");
        HighlightEnemy(currentEnemyIndex);
    }

    private void UpdatePlayerUI()
    {
        if (playerStats != null)
        {
            playerImage.sprite = playerStats.GetComponent<SpriteRenderer>().sprite;
            playerNameText.text = playerStats.name;
            playerHealthText.text = "Health: " + playerStats.currentHealth;
        }
    }

    private void ResetEnemyUI()
    {
        foreach (GameObject enemyUI in enemyUIElements)
        {
            Destroy(enemyUI);
        }
        enemyUIElements.Clear();
    }

    private void CycleEnemyTarget()
    {
        UnhighlightEnemy(currentEnemyIndex);
        currentEnemyIndex = (currentEnemyIndex + 1) % enemyStatsList.Count;
        HighlightEnemy(currentEnemyIndex);
    }

    private void HighlightEnemy(int index)
    {
        if (index < enemyUIElements.Count)
        {
            enemyUIElements[index].transform.Find("EnemyImage").GetComponent<Image>().color = Color.red;
        }
    }

    private void UnhighlightEnemy(int index)
    {
        if (index < enemyUIElements.Count)
        {
            enemyUIElements[index].transform.Find("EnemyImage").GetComponent<Image>().color = Color.white;
        }
    }

    private void OnAttackButtonClicked()
    {
        if (playerStats != null && currentEnemyIndex < enemyStatsList.Count)
        {
            EnemyStats currentEnemy = enemyStatsList[currentEnemyIndex];
            playerStats.Attack(currentEnemy);
            UpdateCombatLog("Player attacks " + currentEnemy.name + " for " + playerStats.attackPower + " damage.");
            UpdateEnemyUI(currentEnemyIndex);

            if (currentEnemy.currentHealth > 0)
            {
                currentEnemy.Attack(playerStats);
                UpdateCombatLog(currentEnemy.name + " attacks player for " + currentEnemy.attackPower + " damage.");
                UpdatePlayerHealthUI();
            }

            CheckCombatResult();
        }
    }

    private void OnHealButtonClicked()
    {
        if (playerStats != null)
        {
            playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + 20, playerStats.maxHealth);
            UpdateCombatLog("Player heals for 20 health. Current health: " + playerStats.currentHealth);
            UpdatePlayerHealthUI();

            if (enemyStatsList[currentEnemyIndex].currentHealth > 0)
            {
                enemyStatsList[currentEnemyIndex].Attack(playerStats);
                UpdateCombatLog(enemyStatsList[currentEnemyIndex].name + " attacks player for " + enemyStatsList[currentEnemyIndex].attackPower + " damage.");
                UpdatePlayerHealthUI();
            }

            CheckCombatResult();
        }
    }

    private void UpdateCombatLog(string message)
    {
        combatLogText.text += message + "\n";
    }

    private void ClearCombatLog()
    {
        combatLogText.text = "";
    }

    private void UpdatePlayerHealthUI()
    {
        if (playerHealthText != null && playerStats != null)
        {
            playerHealthText.text = "Health: " + playerStats.currentHealth;
        }
    }

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

    private void ShowDeathMessage()
    {
        deathMessageText.gameObject.SetActive(true);
        deathMessageText.text = "You Died";
        restartButton.gameObject.SetActive(true);

        attackButton.gameObject.SetActive(false);
        healButton.gameObject.SetActive(false);
    }

    private void OnRestartButtonClicked()
    {
        ClearCombatLog();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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

    private void CloseFightUI()
    {
        fightPanel.SetActive(false);
        moveButtons.SetActive(true);
        closeFightUIButton.gameObject.SetActive(false);
        ClearCombatLog();
    }
}
