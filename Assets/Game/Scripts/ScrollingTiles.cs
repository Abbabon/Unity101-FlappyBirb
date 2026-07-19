using UnityEngine;

/// Scrolls a row of repeating tiles left, recycling them off-screen.
/// speedFactor 1 = foreground (ground/pipes speed); < 1 = parallax background.
public class ScrollingTiles : MonoBehaviour
{
    public GameConfig config;
    public Transform[] tiles;
    public float tileWidth = 3.36f;
    [Range(0f, 1f)] public float speedFactor = 1f;

    bool running = true;
    float recycleX;

    public void Stop() => running = false;

    void Start()
    {
        var cam = Camera.main;
        float halfW = cam.orthographicSize * cam.aspect;
        recycleX = -(halfW + tileWidth / 2f);
        for (int i = 0; i < tiles.Length; i++)
            tiles[i].position = new Vector3(recycleX + tileWidth * (i + 0.5f), tiles[i].position.y, 0f);
    }

    void Update()
    {
        if (!running) return;
        float dx = config.scrollSpeed * speedFactor * Time.deltaTime;
        foreach (var t in tiles)
        {
            t.position += Vector3.left * dx;
            if (t.position.x < recycleX)
                t.position += Vector3.right * (tileWidth * tiles.Length);
        }
    }
}
