using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    private GameObject currentLevelRoot;
    private PlayerController currentPlayer;
    private LevelConfig[] levels;

    static readonly Color FLOOR_PIRATE = new Color(0.22f, 0.16f, 0.10f);
    static readonly Color FLOOR_CASTLE = new Color(0.25f, 0.24f, 0.22f);
    static readonly Color WALL_PIRATE = new Color(0.12f, 0.08f, 0.04f);
    static readonly Color WALL_CASTLE = new Color(0.15f, 0.14f, 0.13f);
    static readonly Color DOOR_LOCKED = new Color(0.6f, 0.15f, 0.15f);
    static readonly Color DOOR_EXIT = new Color(0.7f, 0.2f, 0.2f);
    static readonly Color PLAYER_COLOR = new Color(0.3f, 0.55f, 0.85f);
    static readonly Color ENEMY_COLOR = new Color(0.15f, 0.05f, 0.2f);
    static readonly Color KEY_COLOR = new Color(1f, 0.85f, 0.2f);
    static readonly Color HINT_COLOR = new Color(0.3f, 0.8f, 0.9f);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeLevels();
    }

    void Start()
    {
        GameManager.Instance.OnLevelChanged += GenerateLevel;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelChanged -= GenerateLevel;
    }

    void InitializeLevels()
    {
        levels = new LevelConfig[5];

        levels[0] = new LevelConfig
        {
            name = "The Brig",
            width = 17,
            height = 13,
            theme = LevelTheme.PirateShip,
            enemyCount = 2,
            hasHiddenPassage = false,
            hasPuzzle = false,
            hasSymbolDoor = false,
            hintColors = new Color[] { HINT_COLOR },
            enemySpeed = 2f,
            enemySightRange = 4.5f,
        };

        levels[1] = new LevelConfig
        {
            name = "Cargo Hold",
            width = 20,
            height = 15,
            theme = LevelTheme.PirateShip,
            enemyCount = 3,
            hasHiddenPassage = true,
            hasPuzzle = false,
            hasSymbolDoor = false,
            hintColors = new Color[] { HINT_COLOR, new Color(0.9f, 0.7f, 0.2f) },
            enemySpeed = 2.5f,
            enemySightRange = 5f,
        };

        levels[2] = new LevelConfig
        {
            name = "Captain's Quarters",
            width = 22,
            height = 16,
            theme = LevelTheme.PirateShip,
            enemyCount = 4,
            hasHiddenPassage = true,
            hasPuzzle = true,
            hasSymbolDoor = true,
            symbolSequence = new int[] { 0, 3, 1, 4 },
            puzzleType = PuzzleManager.PuzzleMode.Sequence,
            hintColors = new Color[] { HINT_COLOR, new Color(0.8f, 0.4f, 0.9f) },
            enemySpeed = 2.8f,
            enemySightRange = 5.5f,
        };

        levels[3] = new LevelConfig
        {
            name = "Castle Dungeon",
            width = 24,
            height = 18,
            theme = LevelTheme.Castle,
            enemyCount = 5,
            hasHiddenPassage = true,
            hasPuzzle = true,
            hasSymbolDoor = true,
            symbolSequence = new int[] { 2, 5, 0, 3 },
            puzzleType = PuzzleManager.PuzzleMode.TimedSequence,
            hintColors = new Color[] { HINT_COLOR, new Color(0.9f, 0.3f, 0.3f) },
            enemySpeed = 3.2f,
            enemySightRange = 6f,
        };

        levels[4] = new LevelConfig
        {
            name = "Throne Room",
            width = 26,
            height = 20,
            theme = LevelTheme.Castle,
            enemyCount = 6,
            hasHiddenPassage = true,
            hasPuzzle = true,
            hasSymbolDoor = true,
            symbolSequence = new int[] { 4, 4, 4, 1 },
            puzzleType = PuzzleManager.PuzzleMode.TimedSequence,
            hintColors = new Color[] { new Color(1f, 0.85f, 0.3f), new Color(0.6f, 0.2f, 0.8f) },
            enemySpeed = 3.8f,
            enemySightRange = 6.5f,
        };
    }

    public void GenerateLevel(int levelIndex)
    {
        if (currentLevelRoot != null)
            Destroy(currentLevelRoot);
        currentLevelRoot = new GameObject($"Level_{levelIndex}");

        var config = levels[Mathf.Clamp(levelIndex, 0, levels.Length - 1)];
        int seed = GameManager.Instance.runSeed + levelIndex * 1000;
        Random.InitState(seed);

        BuildRoom(config);
    }

    void BuildRoom(LevelConfig config)
    {
        float w = config.width;
        float h = config.height;
        Color floorCol = config.theme == LevelTheme.PirateShip ? FLOOR_PIRATE : FLOOR_CASTLE;
        Color wallCol = config.theme == LevelTheme.PirateShip ? WALL_PIRATE : WALL_CASTLE;

        var floor = CreateSprite(
            "Floor",
            new Vector2(w / 2f, h / 2f),
            SpriteFactory.CreateRect(32, 32),
            floorCol,
            0
        );
        floor.transform.localScale = new Vector3(w, h, 1);
        floor.transform.parent = currentLevelRoot.transform;

        for (int x = 0; x <= (int)w; x++)
        {
            var line = CreateSprite(
                $"GridV_{x}",
                new Vector2(x, h / 2f),
                SpriteFactory.CreateRect(1, 32),
                new Color(floorCol.r + 0.03f, floorCol.g + 0.03f, floorCol.b + 0.03f),
                1
            );
            line.transform.localScale = new Vector3(0.05f, h, 1);
            line.transform.parent = currentLevelRoot.transform;
        }
        for (int y = 0; y <= (int)h; y++)
        {
            var line = CreateSprite(
                $"GridH_{y}",
                new Vector2(w / 2f, y),
                SpriteFactory.CreateRect(32, 1),
                new Color(floorCol.r + 0.03f, floorCol.g + 0.03f, floorCol.b + 0.03f),
                1
            );
            line.transform.localScale = new Vector3(w, 0.05f, 1);
            line.transform.parent = currentLevelRoot.transform;
        }

        CreateWall("WallN", new Vector2(w / 2f, h + 0.25f), new Vector2(w + 1, 0.5f), wallCol);
        CreateWall("WallS", new Vector2(w / 2f, -0.25f), new Vector2(w + 1, 0.5f), wallCol);
        CreateWall("WallE", new Vector2(w + 0.25f, h / 2f), new Vector2(0.5f, h + 1), wallCol);
        CreateWall("WallW", new Vector2(-0.25f, h / 2f), new Vector2(0.5f, h + 1), wallCol);

        BuildInternalWalls(config, w, h, wallCol);

        SpawnPlayer(new Vector2(1.5f, 1.5f));

        PlaceExitDoor(config, w, h);

        if (!config.hasSymbolDoor && !config.hasPuzzle)
            PlaceKey(config, w, h);

        PlaceEnemies(config, w, h);

        PlaceHints(config, w, h);

        if (config.hasPuzzle)
            PlacePuzzle(config, w, h);

        if (config.hasHiddenPassage)
            PlaceHiddenPassage(config, w, h, wallCol);
    }

    void BuildInternalWalls(LevelConfig config, float w, float h, Color wallCol)
    {
        int cellsX = Mathf.Max(3, config.width / 4);
        int cellsY = Mathf.Max(3, config.height / 4);
        float cellW = w / cellsX;
        float cellH = h / cellsY;

        int wallIdx = 0;

        for (int cx = 1; cx < cellsX; cx++)
        {
            float x = cx * cellW;
            int gapCount = Random.Range(1, 3);
            float segStart = 1f;

            float[] gaps = new float[gapCount];
            for (int g = 0; g < gapCount; g++)
                gaps[g] = Random.Range(2f, h - 2f);
            System.Array.Sort(gaps);

            for (int g = 0; g <= gapCount; g++)
            {
                float segEnd = g < gapCount ? gaps[g] - 0.8f : h - 1f;
                if (segEnd > segStart + 1f)
                {
                    float segLen = segEnd - segStart;
                    float segMid = (segStart + segEnd) / 2f;
                    Vector2 pos = new Vector2(x, segMid);

                    if (
                        Vector2.Distance(pos, new Vector2(1.5f, 1.5f)) > 2.5f
                        && Vector2.Distance(pos, new Vector2(w - 2, h - 1)) > 2.5f
                    )
                    {
                        CreateWall($"MWallV_{wallIdx++}", pos, new Vector2(0.5f, segLen), wallCol);
                    }
                }
                segStart = g < gapCount ? gaps[g] + 0.8f : h;
            }
        }

        for (int cy = 1; cy < cellsY; cy++)
        {
            float y = cy * cellH;
            int gapCount = Random.Range(1, 3);
            float segStart = 1f;

            float[] gaps = new float[gapCount];
            for (int g = 0; g < gapCount; g++)
                gaps[g] = Random.Range(2f, w - 2f);
            System.Array.Sort(gaps);

            for (int g = 0; g <= gapCount; g++)
            {
                float segEnd = g < gapCount ? gaps[g] - 0.8f : w - 1f;
                if (segEnd > segStart + 1f)
                {
                    float segLen = segEnd - segStart;
                    float segMid = (segStart + segEnd) / 2f;
                    Vector2 pos = new Vector2(segMid, y);

                    if (
                        Vector2.Distance(pos, new Vector2(1.5f, 1.5f)) > 2.5f
                        && Vector2.Distance(pos, new Vector2(w - 2, h - 1)) > 2.5f
                    )
                    {
                        CreateWall($"MWallH_{wallIdx++}", pos, new Vector2(segLen, 0.5f), wallCol);
                    }
                }
                segStart = g < gapCount ? gaps[g] + 0.8f : w;
            }
        }

        int pillarCount = config.width / 5;
        for (int i = 0; i < pillarCount; i++)
        {
            float px = Random.Range(3f, w - 3f);
            float py = Random.Range(3f, h - 3f);
            Vector2 ppos = new Vector2(px, py);

            if (Vector2.Distance(ppos, new Vector2(1.5f, 1.5f)) < 2.5f)
                continue;
            if (Vector2.Distance(ppos, new Vector2(w - 2, h - 1)) < 2.5f)
                continue;

            float armLen = Random.Range(1f, 2.5f);
            bool horiz = Random.value > 0.5f;
            CreateWall(
                $"Pillar_{wallIdx++}",
                ppos,
                horiz ? new Vector2(armLen, 0.5f) : new Vector2(0.5f, armLen),
                wallCol
            );

            if (Random.value < 0.5f)
            {
                Vector2 armOffset = horiz
                    ? new Vector2(armLen * 0.4f, armLen * 0.3f)
                    : new Vector2(armLen * 0.3f, armLen * 0.4f);
                CreateWall(
                    $"PillarArm_{wallIdx++}",
                    ppos + armOffset,
                    horiz ? new Vector2(0.5f, armLen * 0.6f) : new Vector2(armLen * 0.6f, 0.5f),
                    wallCol
                );
            }
        }
    }

    GameObject CreateWall(string name, Vector2 pos, Vector2 scale, Color color)
    {
        var wall = CreateSprite(name, pos, SpriteFactory.CreateRect(16, 16), color, 3);
        wall.transform.localScale = new Vector3(scale.x, scale.y, 1);
        wall.transform.parent = currentLevelRoot.transform;

        var col = wall.AddComponent<BoxCollider2D>();
        wall.layer = LayerMask.NameToLayer("Default");
        wall.isStatic = true;
        return wall;
    }

    void SpawnPlayer(Vector2 pos)
    {
        if (currentPlayer == null)
        {
            var obj = new GameObject("Player");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreatePlayerSprite();
            sr.sortingOrder = 10;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.6f);

            obj.AddComponent<PlayerController>();
            obj.AddComponent<PlayerInventory>();
            obj.tag = "Player";

            currentPlayer = obj.GetComponent<PlayerController>();
            DontDestroyOnLoad(obj);
        }

        currentPlayer.transform.position = new Vector3(pos.x, pos.y, 0);
        currentPlayer.SetCanMove(true);
        var inv = currentPlayer.GetComponent<PlayerInventory>();
        inv?.Clear();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(pos.x, pos.y, -10f);
            var cc = cam.GetComponent<CameraController>();
            if (cc != null)
            {
                cc.target = currentPlayer.transform;
                cc.SnapToTarget();
            }
        }
    }

    void PlaceExitDoor(LevelConfig config, float w, float h)
    {
        Vector2 exitPos = new Vector2(w - 2, h - 0.5f);

        var doorObj = CreateSprite(
            "ExitDoor",
            exitPos,
            SpriteFactory.CreateDoorSprite(),
            DOOR_EXIT,
            5
        );
        doorObj.transform.parent = currentLevelRoot.transform;

        var col = doorObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        var door = doorObj.AddComponent<Door>();
        door.isExitDoor = true;

        if (config.hasSymbolDoor)
        {
            door.doorType = Door.DoorType.Symbol;
            door.requiredSymbolSequence = config.symbolSequence;
            door.isLocked = true;
        }
        else if (config.hasPuzzle)
        {
            door.doorType = Door.DoorType.Puzzle;
            door.isLocked = true;
        }
        else
        {
            door.doorType = Door.DoorType.Key;
            door.requiredKeyId = $"key_level_{GameManager.Instance.currentLevel}";
            door.isLocked = true;
        }

        var entrance = CreateSprite(
            "Entrance",
            new Vector2(1.5f, 0.5f),
            SpriteFactory.CreateRect(16, 16),
            new Color(0.2f, 0.5f, 0.2f, 0.5f),
            2
        );
        entrance.transform.localScale = new Vector3(1.2f, 0.3f, 1);
        entrance.transform.parent = currentLevelRoot.transform;
    }

    void PlaceKey(LevelConfig config, float w, float h)
    {
        float kx,
            ky;
        int placement = Random.Range(0, 3);
        if (placement == 0)
        {
            kx = Random.Range(w * 0.6f, w - 2f);
            ky = Random.Range(1.5f, h * 0.35f);
        }
        else if (placement == 1)
        {
            kx = Random.Range(1.5f, w * 0.3f);
            ky = Random.Range(h * 0.6f, h - 2f);
        }
        else
        {
            kx = w * 0.5f + Random.Range(-2f, 2f);
            ky = h * 0.5f + Random.Range(-2f, 2f);
        }

        var keyObj = CreateSprite(
            "Key",
            new Vector2(kx, ky),
            SpriteFactory.CreateKeySprite(),
            KEY_COLOR,
            8
        );
        keyObj.transform.localScale = Vector3.one * 0.8f;
        keyObj.transform.parent = currentLevelRoot.transform;

        var col = keyObj.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        var pickup = keyObj.AddComponent<PickupItem>();
        pickup.itemType = PickupItem.ItemType.Key;
        pickup.keyId = $"key_level_{GameManager.Instance.currentLevel}";
    }

    void PlaceEnemies(LevelConfig config, float w, float h)
    {
        Color[] enemyColors =
        {
            new Color(0.15f, 0.05f, 0.2f),
            new Color(0.1f, 0.1f, 0.15f),
            new Color(0.2f, 0.05f, 0.05f),
            new Color(0.05f, 0.1f, 0.15f),
            new Color(0.12f, 0.02f, 0.12f),
        };

        for (int i = 0; i < config.enemyCount; i++)
        {
            float ex,
                ey;

            if (i == 0)
            {
                ex = w * 0.5f + Random.Range(-2f, 2f);
                ey = h * 0.5f + Random.Range(-2f, 2f);
            }
            else if (i == 1)
            {
                ex = w - Random.Range(3f, 5f);
                ey = h - Random.Range(3f, 5f);
            }
            else
            {
                float t = (float)i / config.enemyCount;
                ex = Mathf.Lerp(3f, w - 3f, t) + Random.Range(-2f, 2f);
                ey = Mathf.Lerp(3f, h - 3f, Random.value) + Random.Range(-1f, 1f);
            }

            ex = Mathf.Clamp(ex, 2f, w - 2f);
            ey = Mathf.Clamp(ey, 2f, h - 2f);

            if (Vector2.Distance(new Vector2(ex, ey), new Vector2(1.5f, 1.5f)) < 5f)
                ey = Mathf.Max(h * 0.5f, ey);

            Color eColor = enemyColors[i % enemyColors.Length];
            var enemyObj = CreateSprite(
                $"Enemy_{i}",
                new Vector2(ex, ey),
                SpriteFactory.CreateEnemySprite(eColor),
                Color.white,
                10
            );
            enemyObj.transform.parent = currentLevelRoot.transform;

            var rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = enemyObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.6f);
            col.isTrigger = true;

            var enemy = enemyObj.AddComponent<EnemyAI>();
            enemy.patrolSpeed = config.enemySpeed;
            enemy.chaseSpeed = config.enemySpeed * 1.6f;
            enemy.sightRange = config.enemySightRange;
            enemy.obstacleLayer = LayerMask.GetMask("Default");

            int pointCount = Random.Range(3, 6);
            Vector2[] points = new Vector2[pointCount];
            for (int p = 0; p < pointCount; p++)
            {
                float px = Mathf.Clamp(ex + Random.Range(-4f, 4f), 2f, w - 2f);
                float py = Mathf.Clamp(ey + Random.Range(-4f, 4f), 2f, h - 2f);
                points[p] = new Vector2(px, py);
            }
            enemy.patrolPoints = points;
            enemy.waitTimeAtPoint = Random.Range(0.5f, 1.5f);

            enemyObj.tag = "Enemy";

            var trigger = enemyObj.AddComponent<EnemyCatchTrigger>();
        }
    }

    void PlaceHints(LevelConfig config, float w, float h)
    {
        if (config.hintColors == null)
            return;

        for (int i = 0; i < config.hintColors.Length; i++)
        {
            float hx = Random.Range(2f, w - 2f);
            float hy = Random.Range(2f, h - 2f);

            var hintObj = CreateSprite(
                $"Hint_{i}",
                new Vector2(hx, hy),
                SpriteFactory.CreateCircle(12),
                config.hintColors[i],
                7
            );
            hintObj.transform.localScale = Vector3.one * 0.5f;
            hintObj.transform.parent = currentLevelRoot.transform;

            var col = hintObj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var pickup = hintObj.AddComponent<PickupItem>();
            pickup.itemType = PickupItem.ItemType.Hint;
            pickup.hintGlowColor = config.hintColors[i];

            if (
                config.hasSymbolDoor
                && config.symbolSequence != null
                && i < config.symbolSequence.Length
            )
            {
                pickup.hintSprite = SpriteFactory.CreateSymbolSprite(config.symbolSequence[i]);
            }

            var glowObj = CreateSprite(
                $"HintGlow_{i}",
                new Vector2(hx, hy),
                SpriteFactory.CreateCircle(32),
                new Color(
                    config.hintColors[i].r,
                    config.hintColors[i].g,
                    config.hintColors[i].b,
                    0.1f
                ),
                2
            );
            glowObj.transform.localScale = Vector3.one * 2f;
            glowObj.transform.parent = currentLevelRoot.transform;
        }
    }

    void PlacePuzzle(LevelConfig config, float w, float h)
    {
        Door exitDoor = null;
        var doors = currentLevelRoot.GetComponentsInChildren<Door>();
        foreach (var d in doors)
            if (d.isExitDoor)
            {
                exitDoor = d;
                break;
            }

        var pmObj = new GameObject("PuzzleManager");
        pmObj.transform.parent = currentLevelRoot.transform;
        var pm = pmObj.AddComponent<PuzzleManager>();
        pm.mode = config.puzzleType;
        pm.linkedDoor = exitDoor;

        int pieceCount = 3;
        var pieces = new PuzzleObject[pieceCount];
        int[] sequence = new int[pieceCount];

        for (int i = 0; i < pieceCount; i++)
        {
            float px = Random.Range(2f, w - 2f);
            float py = Random.Range(2f, h - 2f);

            var leverObj = CreateSprite(
                $"Lever_{i}",
                new Vector2(px, py),
                SpriteFactory.CreateLeverSprite(),
                Color.white,
                8
            );
            leverObj.transform.localScale = Vector3.one * 0.7f;
            leverObj.transform.parent = currentLevelRoot.transform;

            var col = leverObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.8f);

            var puzzle = leverObj.AddComponent<PuzzleObject>();
            puzzle.puzzleType = PuzzleObject.PuzzleType.Lever;
            puzzle.sequenceIndex = i;

            pieces[i] = puzzle;
            sequence[i] = i;
        }

        for (int i = sequence.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (sequence[i], sequence[j]) = (sequence[j], sequence[i]);
        }

        pm.puzzlePieces = pieces;
        pm.correctSequence = sequence;
        pm.timeWindow = 8f - GameManager.Instance.currentLevel;
    }

    void PlaceHiddenPassage(LevelConfig config, float w, float h, Color wallCol)
    {
        float hx = Random.Range(w * 0.3f, w * 0.7f);
        float hy = Random.Range(h * 0.3f, h * 0.7f);

        var passageObj = CreateSprite(
            "HiddenPassage",
            new Vector2(hx, hy),
            SpriteFactory.CreateRect(16, 16),
            wallCol,
            3
        );
        passageObj.transform.localScale = new Vector3(1.2f, 0.5f, 1);
        passageObj.transform.parent = currentLevelRoot.transform;

        var col = passageObj.AddComponent<BoxCollider2D>();
        var passage = passageObj.AddComponent<HiddenPassage>();
        passage.revealDistance = 1.5f;
    }

    GameObject CreateSprite(string name, Vector2 pos, Sprite sprite, Color color, int sortOrder)
    {
        var obj = new GameObject(name);
        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortOrder;
        return obj;
    }
}

public enum LevelTheme
{
    PirateShip,
    Castle,
}

[System.Serializable]
public class LevelConfig
{
    public string name;
    public int width,
        height;
    public LevelTheme theme;
    public int enemyCount;
    public bool hasHiddenPassage;
    public bool hasPuzzle;
    public bool hasSymbolDoor;
    public int[] symbolSequence;
    public PuzzleManager.PuzzleMode puzzleType;
    public Color[] hintColors;
    public float enemySpeed;
    public float enemySightRange;
}
