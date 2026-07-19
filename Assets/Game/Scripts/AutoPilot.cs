using UnityEngine;

/// Dev-only playtest bot: flaps toward the next gap. Never present in the saved scene;
/// attach at runtime (or from a test) to smoke-test the whole loop.
public class AutoPilot : MonoBehaviour
{
    public int updateCount;
    public int flapCount;

    Bird bird;
    Rigidbody2D rb;

    void Awake()
    {
        bird = GetComponent<Bird>();
        rb = GetComponent<Rigidbody2D>();
    }

    // FixedUpdate so the bot stays playable even when the editor renders at low fps.
    void FixedUpdate()
    {
        updateCount++;
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            return;

        float targetY = 0f;
        float bestX = float.MaxValue;
        foreach (var zone in Object.FindObjectsByType<ScoreZone>(FindObjectsSortMode.None))
        {
            float x = zone.transform.position.x;
            if (x > transform.position.x - 0.3f && x < bestX)
            {
                bestX = x;
                targetY = zone.transform.position.y;
            }
        }

        if (rb.linearVelocity.y <= 0f && transform.position.y < targetY - 0.1f)
        {
            flapCount++;
            bird.Flap();
        }
    }
}
