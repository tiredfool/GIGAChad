using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public Text titleText;
    public Text scoreValueText;

    private int score = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        titleText.gameObject.SetActive(false);      // 초기 비활성화
        scoreValueText.gameObject.SetActive(false); // 초기 비활성화
        UpdateScoreText();
    }

    public void StartGameUI()
    {
        titleText.gameObject.SetActive(true);
        scoreValueText.gameObject.SetActive(true);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();

        if (score >= 5000)
        {
            Stage2Manager.instance.EndGameByScore();
        }
    }

    private void UpdateScoreText()
    {
        scoreValueText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

}
