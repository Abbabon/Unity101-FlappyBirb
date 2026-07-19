using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Bird : MonoBehaviour
{
    public GameConfig config;
    public Sprite[] yellowFrames; // down, mid, up
    public Sprite[] redFrames;
    public Sprite[] blueFrames;
    public float frameRate = 10f;

    static readonly int[] AnimSequence = { 0, 1, 2, 1 };

    Rigidbody2D rb;
    SpriteRenderer sr;
    Sprite[] flapFrames;
    float startY;
    float bobTimer;
    float animTimer;
    int animStep;
    bool physicsActive;
    bool dead;
    bool flapQueued;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var variants = new[] { yellowFrames, redFrames, blueFrames };
        flapFrames = variants[Random.Range(0, variants.Length)];
        if (flapFrames == null || flapFrames.Length < 3)
            flapFrames = yellowFrames;
        if (flapFrames != null && flapFrames.Length >= 2)
            sr.sprite = flapFrames[1];
    }

    void Start()
    {
        // Bird sits 28% of the view width from the left edge, regardless of aspect.
        var cam = Camera.main;
        float halfW = cam.orthographicSize * cam.aspect;
        transform.position = new Vector3(-0.44f * halfW, transform.position.y, 0f);
        startY = transform.position.y;
    }

    public void Begin()
    {
        physicsActive = true;
        rb.gravityScale = config.gravityScale;
    }

    public void Flap()
    {
        if (dead || !physicsActive) return;
        flapQueued = true;
    }

    void Update()
    {
        if (!dead)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / frameRate)
            {
                animTimer -= 1f / frameRate;
                animStep = (animStep + 1) % AnimSequence.Length;
                if (flapFrames != null && flapFrames.Length >= 3)
                    sr.sprite = flapFrames[AnimSequence[animStep]];
            }
        }

        if (!physicsActive)
        {
            bobTimer += Time.deltaTime;
            transform.position = new Vector3(transform.position.x, startY + Mathf.Sin(bobTimer * 4f) * 0.08f, 0f);
            return;
        }

        // Cosmetic pitch: nose up while rising, tip toward -90° as the dive builds.
        float vy = rb.linearVelocity.y;
        float t = Mathf.InverseLerp(1f, config.diveVelocityThreshold, vy);
        float target = Mathf.Lerp(config.flapPitch, config.divePitch, t);
        float z = Mathf.LerpAngle(transform.eulerAngles.z, target, config.rotationLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }

    void FixedUpdate()
    {
        if (!physicsActive) return;

        if (flapQueued)
        {
            flapQueued = false;
            rb.linearVelocity = new Vector2(0f, config.flapVelocity); // velocity is REPLACED, not added
            transform.rotation = Quaternion.Euler(0f, 0f, config.flapPitch);
        }

        var v = rb.linearVelocity;
        if (v.y < config.terminalVelocity)
            rb.linearVelocity = new Vector2(v.x, config.terminalVelocity);

        // Ceiling is clamped, not lethal (classic behavior).
        if (transform.position.y > config.ceilingY && rb.linearVelocity.y > 0f)
        {
            transform.position = new Vector3(transform.position.x, config.ceilingY, 0f);
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ScoreZone>() != null)
        {
            GameManager.Instance.AddScore();
            return;
        }
        Die(other.name); // any other trigger is a pipe
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Die(collision.collider.name); // ground
    }

    void Die(string cause)
    {
        if (dead || !physicsActive) return;
        dead = true;
        flapQueued = false;
        Debug.Log($"[Bird] died: hit '{cause}' at y={transform.position.y:F2}, t={Time.time:F2}");
        GameManager.Instance.OnBirdHit();
    }
}
