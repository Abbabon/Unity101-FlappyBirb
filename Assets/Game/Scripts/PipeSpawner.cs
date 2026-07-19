using System.Collections.Generic;
using UnityEngine;

/// Spawns pooled pipe pairs at fixed horizontal spacing and scrolls them left.
public class PipeSpawner : MonoBehaviour
{
    public GameConfig config;
    public GameObject pipePairPrefab;

    readonly List<Transform> active = new List<Transform>();
    readonly Queue<Transform> pool = new Queue<Transform>();
    bool running;
    float distanceSinceSpawn;
    float spawnX;
    float despawnX;

    public void Begin()
    {
        var cam = Camera.main;
        float halfW = cam.orthographicSize * cam.aspect;
        spawnX = halfW + 0.4f;
        despawnX = -spawnX;
        distanceSinceSpawn = 0f;
        running = true;
    }

    public void Stop() => running = false;

    void Update()
    {
        if (!running) return;

        float dx = config.scrollSpeed * Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var t = active[i];
            t.position += Vector3.left * dx;
            if (t.position.x < despawnX)
            {
                active.RemoveAt(i);
                t.gameObject.SetActive(false);
                pool.Enqueue(t);
            }
        }

        distanceSinceSpawn += dx;
        if (distanceSinceSpawn >= config.pipeSpacing)
        {
            distanceSinceSpawn -= config.pipeSpacing;
            Spawn();
        }
    }

    void Spawn()
    {
        Transform t = pool.Count > 0 ? pool.Dequeue() : Instantiate(pipePairPrefab, transform).transform;
        float gapY = Random.Range(config.gapCenterMin, config.gapCenterMax);
        t.position = new Vector3(spawnX, gapY, 0f);
        t.gameObject.SetActive(true);
        active.Add(t);
    }
}
