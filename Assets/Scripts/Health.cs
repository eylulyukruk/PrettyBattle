
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;

    private float currentHealth;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;

    public float HealthPercentage =>
        maxHealth > 0f
            ? Mathf.Clamp01(currentHealth / maxHealth)
            : 0f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        if (MatchManager.Instance != null &&
            MatchManager.Instance.IsMatchOver)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        Debug.Log(
            gameObject.name +
            " Health: " + currentHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        TeamMember team =
            GetComponent<TeamMember>();

        if (team != null &&
            MatchManager.Instance != null)
        {
            MatchManager.Instance.CharacterDied(
                team.teamID
            );
        }

        Debug.Log(gameObject.name + " eliminated!");

        Destroy(gameObject);
    }
}
