using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 50;   // Maximum health of the enemy
    public int currentHealth;    // Current health of the enemy
    public int attackPower = 15; // Attack power of the enemy
    public int defense = 5;      // Defense of the enemy

    private PlayerStats playerStats;       // Reference to the player's stats
    private FightUIManager fightUIManager; // Reference to the FightUIManager
    private EnemyUI enemyUI;               // Reference to the EnemyUI component for UI updates

    void Start()
    {
        // Initialize health at the start
        InitializeHealth();

        // Find necessary components in the scene
        playerStats = FindObjectOfType<PlayerStats>();
        fightUIManager = FindObjectOfType<FightUIManager>();
        enemyUI = GetComponentInChildren<EnemyUI>();

        // Initialize EnemyUI with current stats, if available
        if (enemyUI != null)
        {
            enemyUI.Initialize(this);
        }

        // Start the automatic attack coroutine
        StartCoroutine(AutoAttack());
    }

    // Method to initialize the health of the enemy
    public void InitializeHealth()
    {
        currentHealth = maxHealth;
    }

    // Coroutine to handle automatic attacks at fixed intervals
    private System.Collections.IEnumerator AutoAttack()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(3f); // Fixed 3 seconds interval

            // Perform the attack if the player is still alive and the enemy is in combat
            if (playerStats != null && playerStats.currentHealth > 0 && fightUIManager.InCombat)
            {
                Attack(playerStats);
                Debug.Log(name + " attacks player for " + attackPower + " damage.");
            }
        }
    }

    // Method to take damage and update UI
    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Max(damage - defense, 0); // Calculate damage after defense
        currentHealth -= damageTaken;
        Debug.Log(name + " took " + damageTaken + " damage. Current health: " + currentHealth);

        // Update health in the EnemyUI if it exists
        if (enemyUI != null)
        {
            enemyUI.UpdateHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Ensure health doesn’t go negative
            Die();
        }
    }

    // Method to attack the player
    public void Attack(PlayerStats player)
    {
        player.TakeDamage(attackPower);
    }

    // Method called when the enemy's health reaches zero
    private void Die()
    {
        Debug.Log(name + " has died.");

        // Stop auto-attacks and notify FightUIManager of this enemy's death
        StopAllCoroutines(); // Stop auto-attacks when the enemy dies

        if (fightUIManager != null)
        {
            //fightUIManager.OnEnemyDeath(this); // Notify FightUIManager of enemy death
        }

        // Optionally disable the enemy visually without destroying it immediately
        gameObject.SetActive(false);
    }
}
