using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    public Image enemyImage;
    public TMP_Text enemyNameText;
    public TMP_Text enemyHealthText;

    private EnemyStats enemyStats;

    // Initializes the UI with enemy data
    public void Initialize(EnemyStats stats)
    {
        enemyStats = stats;
        enemyImage.sprite = stats.GetComponent<SpriteRenderer>().sprite;
        enemyNameText.text = stats.name;
        UpdateHealth(stats.currentHealth);
    }

    // Updates the health text for this enemy
    public void UpdateHealth(int health)
    {
        enemyHealthText.text = "Health: " + health;
    }
}
