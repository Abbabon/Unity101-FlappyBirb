using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public ScoreDisplay hudScore;
    public GameObject getReadyPanel;
    public GameObject gameOverPanel;
    public ScoreDisplay finalScore;
    public ScoreDisplay bestScore;
    public GameObject newBestTag;
    public Image flash;

    public void ShowGetReady()
    {
        getReadyPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        if (flash) flash.gameObject.SetActive(false);
        hudScore.gameObject.SetActive(true);
        hudScore.SetValue(0);
    }

    public void ShowHud() => getReadyPanel.SetActive(false);

    public void SetScore(int score) => hudScore.SetValue(score);

    public void ShowGameOver(int score, int best, bool newBest)
    {
        StartCoroutine(GameOverRoutine(score, best, newBest));
    }

    IEnumerator GameOverRoutine(int score, int best, bool newBest)
    {
        if (flash)
        {
            flash.gameObject.SetActive(true);
            var c = flash.color;
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                c.a = 1f - t / 0.15f;
                flash.color = c;
                yield return null;
            }
            flash.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        hudScore.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        finalScore.SetValue(score);
        bestScore.SetValue(best);
        if (newBestTag) newBestTag.SetActive(newBest);
        GameManager.Instance.audioManager.PlaySwoosh();
    }
}
