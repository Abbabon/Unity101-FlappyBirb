#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// One-shot builder: import settings, config asset, pipe prefab, full game scene.
public static class GameSetup
{
    const string Root = "Assets/Game";
    const string SpriteDir = Root + "/Sprites";
    const string AudioDir = Root + "/Audio";
    const string ScenePath = "Assets/Scenes/Game.unity";
    const float TileWidth = 3.36f; // base.png: 336 px @ 100 PPU

    [MenuItem("Tools/Flappy Bird/Build Game Scene")]
    public static void Build()
    {
        ConfigureSpriteImports();
        var config = CreateOrLoadConfig();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var pipePrefab = BuildPipePrefab(config);

        // Camera
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 2.56f; // 512 px reference height @ 100 PPU
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(0x4E, 0xC0, 0xCA, 0xFF);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // Background: tiled row scrolling at quarter speed for parallax depth
        const float BgTileWidth = 2.88f; // background-day.png: 288 px @ 100 PPU
        var bgGo = new GameObject("Background");
        var bgScroller = bgGo.AddComponent<ScrollingTiles>();
        bgScroller.config = config;
        bgScroller.tileWidth = BgTileWidth;
        bgScroller.speedFactor = 0.25f;
        var bgSprite = LoadSprite("background-day");
        var bgTiles = new Transform[5];
        var bgRenderers = new SpriteRenderer[5];
        for (int i = 0; i < bgTiles.Length; i++)
        {
            var tile = new GameObject("BackgroundTile" + i);
            tile.transform.SetParent(bgGo.transform);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = bgSprite;
            sr.sortingOrder = 0;
            tile.transform.position = new Vector3(i * BgTileWidth, 0f, 0f);
            bgTiles[i] = tile.transform;
            bgRenderers[i] = sr;
        }
        bgScroller.tiles = bgTiles;

        // Ground (visual tiles + one solid collider along the top)
        var groundGo = new GameObject("Ground");
        var groundScroller = groundGo.AddComponent<ScrollingTiles>();
        groundScroller.config = config;
        groundScroller.tileWidth = TileWidth;
        groundScroller.speedFactor = 1f;
        var baseSprite = LoadSprite("base");
        var tiles = new Transform[4];
        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = new GameObject("GroundTile" + i);
            tile.transform.SetParent(groundGo.transform);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = baseSprite;
            sr.sortingOrder = 20;
            tile.transform.position = new Vector3(i * TileWidth, -2.0f, 0f);
            tiles[i] = tile.transform;
        }
        groundScroller.tiles = tiles;
        var groundCol = groundGo.AddComponent<BoxCollider2D>();
        groundCol.offset = new Vector2(0f, -2.0f);
        groundCol.size = new Vector2(30f, 1.12f);

        // Bird
        var birdGo = new GameObject("Bird");
        var birdSr = birdGo.AddComponent<SpriteRenderer>();
        birdSr.sprite = LoadSprite("yellowbird-midflap");
        birdSr.sortingOrder = 30;
        var rb = birdGo.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var circle = birdGo.AddComponent<CircleCollider2D>();
        circle.radius = 0.12f;
        var bird = birdGo.AddComponent<Bird>();
        bird.config = config;
        bird.yellowFrames = Frames("yellowbird");
        bird.redFrames = Frames("redbird");
        bird.blueFrames = Frames("bluebird");
        birdGo.transform.position = new Vector3(-0.63f, 0f, 0f);

        // Pipe spawner
        var spawnerGo = new GameObject("PipeSpawner");
        var spawner = spawnerGo.AddComponent<PipeSpawner>();
        spawner.config = config;
        spawner.pipePairPrefab = pipePrefab;

        // UI
        var ui = BuildUI();

        // GameManager + Audio
        var gmGo = new GameObject("GameManager");
        var audioSource = gmGo.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        var musicSource = gmGo.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.4f;
        var audio = gmGo.AddComponent<AudioManager>();
        audio.source = audioSource;
        audio.musicSource = musicSource;
        audio.music = LoadClip("music");
        audio.wing = LoadClip("wing");
        audio.point = LoadClip("point");
        audio.hit = LoadClip("hit");
        audio.die = LoadClip("die");
        audio.swoosh = LoadClip("swoosh");
        var gm = gmGo.AddComponent<GameManager>();
        gm.config = config;
        gm.bird = bird;
        gm.pipeSpawner = spawner;
        gm.ground = groundScroller;
        gm.backgroundScroller = bgScroller;
        gm.ui = ui;
        gm.audioManager = audio;
        gm.backgroundRenderers = bgRenderers;
        gm.backgroundVariants = new[] { LoadSprite("background-day"), LoadSprite("background-night") };

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        Debug.Log("[GameSetup] Flappy Bird scene built and saved to " + ScenePath);
    }

    static void ConfigureSpriteImports()
    {
        foreach (var file in Directory.GetFiles(SpriteDir, "*.png"))
        {
            var path = file.Replace('\\', '/');
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    static GameConfig CreateOrLoadConfig()
    {
        string path = Root + "/GameConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, path);
        }
        return config;
    }

    static GameObject BuildPipePrefab(GameConfig config)
    {
        string dir = Root + "/Prefabs";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder(Root, "Prefabs");
        string path = dir + "/PipePair.prefab";

        var pipeSprite = LoadSprite("pipe-green");
        float halfGap = config.gapHeight / 2f;
        float halfPipe = 1.6f; // pipe-green.png: 320 px tall @ 100 PPU

        var root = new GameObject("PipePair");

        var top = new GameObject("TopPipe");
        top.transform.SetParent(root.transform);
        top.transform.localPosition = new Vector3(0f, halfGap + halfPipe, 0f);
        var topSr = top.AddComponent<SpriteRenderer>();
        topSr.sprite = pipeSprite;
        topSr.flipY = true;
        topSr.sortingOrder = 10;
        var topCol = top.AddComponent<BoxCollider2D>();
        topCol.isTrigger = true;
        topCol.size = new Vector2(0.52f, 3.2f);

        var bottom = new GameObject("BottomPipe");
        bottom.transform.SetParent(root.transform);
        bottom.transform.localPosition = new Vector3(0f, -(halfGap + halfPipe), 0f);
        var bottomSr = bottom.AddComponent<SpriteRenderer>();
        bottomSr.sprite = pipeSprite;
        bottomSr.sortingOrder = 10;
        var bottomCol = bottom.AddComponent<BoxCollider2D>();
        bottomCol.isTrigger = true;
        bottomCol.size = new Vector2(0.52f, 3.2f);

        var zone = new GameObject("ScoreZone");
        zone.transform.SetParent(root.transform);
        zone.transform.localPosition = new Vector3(0.3f, 0f, 0f); // just past the trailing edge
        var zoneCol = zone.AddComponent<BoxCollider2D>();
        zoneCol.isTrigger = true;
        zoneCol.size = new Vector2(0.1f, 6f);
        zone.AddComponent<ScoreZone>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static UIManager BuildUI()
    {
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(288f, 512f);
        // The camera always shows a fixed world height, so UI must scale with
        // screen height only — a width blend balloons the UI on wide aspects.
        scaler.matchWidthOrHeight = 1f;

        var ui = canvasGo.AddComponent<UIManager>();
        var digits = new Sprite[10];
        for (int i = 0; i < 10; i++)
            digits[i] = LoadSprite(i.ToString());

        // HUD score, top center
        var hudGo = new GameObject("HudScore", typeof(RectTransform));
        hudGo.transform.SetParent(canvasGo.transform, false);
        var hudRt = (RectTransform)hudGo.transform;
        hudRt.anchorMin = hudRt.anchorMax = new Vector2(0.5f, 1f);
        hudRt.pivot = new Vector2(0.5f, 1f);
        hudRt.anchoredPosition = new Vector2(0f, -40f);
        var hudScore = hudGo.AddComponent<ScoreDisplay>();
        hudScore.digitSprites = digits;

        // Get Ready panel
        var getReady = new GameObject("GetReadyPanel", typeof(RectTransform));
        getReady.transform.SetParent(canvasGo.transform, false);
        MakeImage(getReady.transform, "Message", LoadSprite("message"), new Vector2(0f, 0f));

        // Game Over panel
        var gameOver = new GameObject("GameOverPanel", typeof(RectTransform));
        gameOver.transform.SetParent(canvasGo.transform, false);
        MakeImage(gameOver.transform, "GameOverTitle", LoadSprite("gameover"), new Vector2(0f, 140f));
        MakeLabel(gameOver.transform, "ScoreLabel", "SCORE", new Vector2(0f, 80f));
        var finalScore = MakeScore(gameOver.transform, "FinalScore", digits, new Vector2(0f, 40f));
        MakeLabel(gameOver.transform, "BestLabel", "BEST", new Vector2(0f, -10f));
        var bestScore = MakeScore(gameOver.transform, "BestScore", digits, new Vector2(0f, -50f));
        var newTag = MakeLabel(gameOver.transform, "NewTag", "NEW!", new Vector2(60f, -10f));
        newTag.color = new Color32(0xE8, 0x5D, 0x42, 0xFF);
        MakeLabel(gameOver.transform, "RestartHint", "TAP TO RESTART", new Vector2(0f, -120f));
        gameOver.SetActive(false);

        // Death flash
        var flashGo = new GameObject("Flash", typeof(RectTransform), typeof(Image));
        flashGo.transform.SetParent(canvasGo.transform, false);
        var flashRt = (RectTransform)flashGo.transform;
        flashRt.anchorMin = Vector2.zero;
        flashRt.anchorMax = Vector2.one;
        flashRt.offsetMin = flashRt.offsetMax = Vector2.zero;
        var flash = flashGo.GetComponent<Image>();
        flash.color = Color.white;
        flash.raycastTarget = false;
        flashGo.SetActive(false);

        ui.hudScore = hudScore;
        ui.getReadyPanel = getReady;
        ui.gameOverPanel = gameOver;
        ui.finalScore = finalScore;
        ui.bestScore = bestScore;
        ui.newBestTag = newTag.gameObject;
        ui.flash = flash;
        return ui;
    }

    static Image MakeImage(Transform parent, string name, Sprite sprite, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        if (sprite != null) img.SetNativeSize();
        ((RectTransform)go.transform).anchoredPosition = anchoredPos;
        return img;
    }

    static Text MakeLabel(Transform parent, string name, string text, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(200f, 20f);
        rt.anchoredPosition = anchoredPos;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        return label;
    }

    static ScoreDisplay MakeScore(Transform parent, string name, Sprite[] digits, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).anchoredPosition = anchoredPos;
        var display = go.AddComponent<ScoreDisplay>();
        display.digitSprites = digits;
        return display;
    }

    static Sprite LoadSprite(string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + "/" + name + ".png");

    static AudioClip LoadClip(string name) =>
        AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + "/" + name + ".wav");

    static Sprite[] Frames(string prefix) => new[]
    {
        LoadSprite(prefix + "-downflap"),
        LoadSprite(prefix + "-midflap"),
        LoadSprite(prefix + "-upflap"),
    };
}
#endif
