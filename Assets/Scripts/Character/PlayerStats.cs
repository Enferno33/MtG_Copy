using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;  // Maximum health of the player
    public int currentHealth;    // Current health of the player
    public int attackPower = 20; // Attack power of the player
    public int defense = 10;     // Defense of the player

    void Start()
    {
        // Initialize the player's current health
        currentHealth = maxHealth;
    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Max(damage - defense, 0); // Calculate damage after defense
        currentHealth -= damageTaken;
        Debug.Log("Player took " + damageTaken + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to attack an enemy
    public void Attack(EnemyStats enemy)
    {
        enemy.TakeDamage(attackPower);
        Debug.Log("Player attacks enemy for " + attackPower + " damage.");
    }

    // Method called when the player's health reaches zero
    private void Die()
    {
        Debug.Log("Player has died.");
        // Handle player death (e.g., game over, respawn)
    }
}
