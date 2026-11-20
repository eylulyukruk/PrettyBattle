
using UnityEngine;

public class BlizzShootEvent : MonoBehaviour
{
    private PlayerShooting playerShooting;

    void Awake()
    {
        playerShooting = GetComponentInParent<PlayerShooting>();
    }

    public void FireSnowball()
    {
        if (playerShooting != null)
        {
            playerShooting.Shoot();
        }
    }
}
