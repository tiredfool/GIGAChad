using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // ΩÃ±€≈Ê

    public Text scoreValueText;
    private int score = 0;

    private void Awake()
    {
        // ΩÃ±€≈Ê ¿ŒΩ∫≈œΩ∫ º≥¡§
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreValueText.text = score.ToString();
    }
}
