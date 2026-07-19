using UnityEngine;

/// All gameplay tuning in one place (see Docs/GDD.md §3.3).
[CreateAssetMenu(menuName = "FlappyBird/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Bird")]
    public float gravityScale = 1.8f;
    public float flapVelocity = 5.0f;
    public float terminalVelocity = -10f;
    public float flapPitch = 25f;
    public float divePitch = -90f;
    public float diveVelocityThreshold = -4f;
    public float rotationLerpSpeed = 8f;
    public float ceilingY = 2.4f;

    [Header("World")]
    public float scrollSpeed = 1.6f;
    public float pipeSpacing = 1.8f;
    public float gapHeight = 1.2f;
    public float gapCenterMin = -0.44f;
    public float gapCenterMax = 1.56f;

    [Header("Flow")]
    public float restartLockout = 0.5f;
}
