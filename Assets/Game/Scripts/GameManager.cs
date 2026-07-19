using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { GetReady, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameConfig config;
    public Bird bird;
    public PipeSpawner pipeSpawner;
    public ScrollingTiles ground;
    public ScrollingTiles backgroundScroller;
    public UIManager ui;
    public AudioManager audioManager;
    public SpriteRenderer[] backgroundRenderers;
    public Sprite[] backgroundVariants; // day, night

    public GameState State { get; private set; } = GameState.GetReady;
    public int Score { get; private set; }
    public int BestScore { get; private set; }

    const string BestScoreKey = "HighScore";
    float gameOverTime;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (backgroundRenderers != null && backgroundVariants != null && backgroundVariants.Length > 0)
        {
            var sprite = backgroundVariants[Random.Range(0, backgroundVariants.Length)];
            foreach (var r in backgroundRenderers)
                if (r != null) r.sprite = sprite;
        }
    }

    void Start() => ui.ShowGetReady();

    void Update()
    {
        if (!FlapInput.Pressed()) return;

        // Frozen after losing focus mid-run: first tap resumes (and flaps).
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            if (State == GameState.Playing)
            {
                bird.Flap();
                audioManager.PlayWing();
            }
            return;
        }

        switch (State)
        {
            case GameState.GetReady:
                StartRun();
                break;
            case GameState.Playing:
                bird.Flap();
                audioManager.PlayWing();
                break;
            case GameState.GameOver:
                if (Time.unscaledTime - gameOverTime > config.restartLockout)
                    Restart();
                break;
        }
    }

    void StartRun()
    {
        State = GameState.Playing;
        bird.Begin();
        bird.Flap();
        audioManager.PlayWing();
        pipeSpawner.Begin();
        ui.ShowHud();
    }

    public void AddScore()
    {
        if (State != GameState.Playing) return;
        Score++;
        ui.SetScore(Score);
        audioManager.PlayPoint();
    }

    public void OnBirdHit()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        gameOverTime = Time.unscaledTime;
        pipeSpawner.Stop();
        ground.Stop();
        backgroundScroller.Stop();
        audioManager.PlayHit();
        audioManager.PlayDie(0.3f);

        bool newBest = Score > BestScore;
        if (newBest)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }
        ui.ShowGameOver(Score, BestScore, newBest);
    }

    void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    void OnApplicationPause(bool paused)
    {
        // Never let the player die off-screen on mobile; tap resumes.
        if (paused && State == GameState.Playing)
            Time.timeScale = 0f;
    }
}
