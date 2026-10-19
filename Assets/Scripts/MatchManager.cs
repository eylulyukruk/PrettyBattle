
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    private static int blueRoundWins = 0;
    private static int redRoundWins = 0;

    public int BlueRoundWins => blueRoundWins;
    public int RedRoundWins => redRoundWins;

    public bool IsRoundOver => roundEnded;
    public bool IsFinalResult => matchEnded;

    // Health.cs uses this property.
    public bool IsMatchOver => roundEnded;

    public string ResultMessage => resultMessage;

    public float roundBreakDuration = 3f;

    private int blueAlive;
    private int redAlive;

    private bool roundEnded = false;
    private bool matchEnded = false;

    private string resultMessage = "";

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSavedRounds()
    {
        blueRoundWins = 0;
        redRoundWins = 0;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        TeamMember[] characters =
            FindObjectsByType<TeamMember>(
                FindObjectsSortMode.None
            );

        foreach (TeamMember character in characters)
        {
            if (character.GetComponent<Health>() == null)
                continue;

            if (character.teamID == 1)
                blueAlive++;
            else if (character.teamID == 2)
                redAlive++;
        }

        Debug.Log(
            "Round started! Blue: " + blueAlive +
            " | Red: " + redAlive
        );
    }

    public void CharacterDied(int teamID)
    {
        if (roundEnded)
            return;

        if (teamID == 1)
        {
            blueAlive--;
        }
        else if (teamID == 2)
        {
            redAlive--;
        }
        else
        {
            return;
        }

        if (redAlive <= 0)
            EndRound(1);
        else if (blueAlive <= 0)
            EndRound(2);
    }

    private void EndRound(int winningTeam)
    {
        roundEnded = true;
        StopCombat();

        if (winningTeam == 1)
        {
            blueRoundWins++;
            resultMessage = "BLUE WINS THE ROUND!";
        }
        else
        {
            redRoundWins++;
            resultMessage = "RED WINS THE ROUND!";
        }

        if (blueRoundWins >= 2 || redRoundWins >= 2)
        {
            matchEnded = true;

            resultMessage = blueRoundWins >= 2
                ? "VICTORY!"
                : "DEFEAT!";

            Debug.Log("MATCH ENDED: " + resultMessage);
        }
        else
        {
            StartCoroutine(StartNextRound());
        }
    }

    private void StopCombat()
    {
        EnemyAI[] bots =
            FindObjectsByType<EnemyAI>(
                FindObjectsSortMode.None
            );

        foreach (EnemyAI bot in bots)
        {
            bot.enabled = false;

            NavMeshAgent agent =
                bot.GetComponent<NavMeshAgent>();

            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = true;
        }

        PlayerMovement movement =
            FindFirstObjectByType<PlayerMovement>();

        if (movement != null)
            movement.enabled = false;

        PlayerShooting shooting =
            FindFirstObjectByType<PlayerShooting>();

        if (shooting != null)
            shooting.enabled = false;

        Projectile[] bullets =
            FindObjectsByType<Projectile>(
                FindObjectsSortMode.None
            );

        foreach (Projectile bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }
    }

    private IEnumerator StartNextRound()
    {
        yield return new WaitForSeconds(roundBreakDuration);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void PlayAgain()
    {
        if (!matchEnded)
            return;

        blueRoundWins = 0;
        redRoundWins = 0;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
