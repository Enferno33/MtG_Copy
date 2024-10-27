using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 50;   // Maximum health of the enemy
    public int currentHealth;    // Current health of the enemy
    public int attackPower = 15; // Attack power of the enemy
    public int defense = 5;      // Defense of the enemy

    private PlayerStats playerStats;       // Reference to the player's stats

    void Start()
    {
        // Initialize the enemy's current health
        currentHealth = maxHealth;

        // Find the PlayerStats component in the scene (assuming one player)
        playerStats = FindObjectOfType<PlayerStats>();

        // Start the automatic attack coroutine
        StartCoroutine(AutoAttack());
    }

    // Coroutine to handle automatic attacks at fixed intervals
    private System.Collections.IEnumerator AutoAttack()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(3f); // Fixed 3 seconds interval

            // Perform the attack if the player is still alive
            if (playerStats != null && playerStats.currentHealth > 0)
            {
                Attack(playerStats);
                Debug.Log(name + " attacks player for " + attackPower + " damage.");
            }
        }
    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Max(damage - defense, 0); // Calculate damage after defense
        currentHealth -= damageTaken;
        Debug.Log("Enemy took " + damageTaken + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Set health to 0 to avoid negative values
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
        Debug.Log("Enemy has died.");
        // Handle enemy death (e.g., stop attacks, remove from game, drop loot)
        StopAllCoroutines(); // Stop auto-attacks when the enemy dies

        // Instead of destroying the enemy immediately, signal to the FightUIManager that this enemy has died
        FightUIManager fightUI = FindObjectOfType<FightUIManager>();
        if (fightUI != null)
        {
            fightUI.OnEnemyDeath(this); // Notify the FightUIManager that this enemy has died
        }

        // Optionally add some delay before destroying the enemy GameObject
        Destroy(gameObject, 1f); // Destroy the enemy GameObject after a short delay
    }
}
