
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Scoreboard")]
    public TMP_Text blueScoreText;
    public TMP_Text redScoreText;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text resultTitleText;
    public TMP_Text resultSubtitleText;
    public TMP_Text finalScoreText;
    public Button playAgainButton;

    [Header("Result Colors")]
    public Color victoryColor =
        new Color(0.51f, 0.89f, 0.69f);

    public Color defeatColor =
        new Color(1f, 0.49f, 0.58f);

    void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(
                OnPlayAgainClicked
            );
        }
    }

    void Update()
    {
        MatchManager match = MatchManager.Instance;

        if (match == null)
            return;

        // Update round scores.
        if (blueScoreText != null)
        {
            blueScoreText.text =
                "BLUE " + match.BlueRoundWins;
        }

        if (redScoreText != null)
        {
            redScoreText.text =
                "RED " + match.RedRoundWins;
        }

        if (resultPanel == null)
            return;

        // Show the result panel when a round ends.
        bool showResult = match.IsRoundOver;

        if (resultPanel.activeSelf != showResult)
        {
            resultPanel.SetActive(showResult);
        }

        if (!showResult)
            return;

        // Update the result title.
        if (resultTitleText != null)
        {
            resultTitleText.text =
                match.ResultMessage;

            if (match.IsFinalResult)
            {
                resultTitleText.color =
                    match.BlueRoundWins >= 2
                        ? victoryColor
                        : defeatColor;
            }
            else
            {
                resultTitleText.color = Color.white;
            }
        }

        // Update the final score.
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "BLUE " + match.BlueRoundWins +
                " - " + match.RedRoundWins + " RED";
        }

        // Update the subtitle.
        if (resultSubtitleText != null)
        {
            if (match.IsFinalResult)
            {
                resultSubtitleText.text =
                    match.BlueRoundWins >= 2
                        ? "Your team won the match!"
                        : "Your team lost the match!";
            }
            else
            {
                resultSubtitleText.text =
                    "Next round starting...";
            }
        }

        // Show Play Again only after the match ends.
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(
                match.IsFinalResult
            );
        }
    }

    private void OnPlayAgainClicked()
    {
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.PlayAgain();
        }
    }

    void OnDestroy()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(
                OnPlayAgainClicked
            );
        }
    }
}
