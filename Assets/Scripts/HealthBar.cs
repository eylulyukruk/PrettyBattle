
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;

    public Color blueTeamColor =
        new Color(0.25f, 0.65f, 1f);

    public Color redTeamColor =
        new Color(1f, 0.3f, 0.4f);

    private Health health;
    private TeamMember teamMember;
    private Camera mainCamera;

    void Start()
    {
        health = GetComponentInParent<Health>();
        teamMember = GetComponentInParent<TeamMember>();
        mainCamera = Camera.main;

        if (fillImage != null && teamMember != null)
        {
            fillImage.color = teamMember.teamID == 1
                ? blueTeamColor
                : redTeamColor;
        }

        UpdateHealthBar();
    }

    void LateUpdate()
    {
        if (health == null)
            return;

        UpdateHealthBar();

        if (mainCamera != null)
        {
            transform.rotation =
                mainCamera.transform.rotation;
        }
    }

    private void UpdateHealthBar()
    {
        if (health == null || fillImage == null)
            return;

        fillImage.fillAmount =
            health.HealthPercentage;
    }
}
