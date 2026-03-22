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
    static readonly Color DOOR_EXIT = new Color(0.7f, 0.2f, 0.2f);
    static readonly Color KEY_COLOR = new Color(1f, 0.85f, 0.2f);
    static readonly Color SAFE_ZONE_COLOR = new Color(0.2f, 0.6f, 0.3f, 0.08f);

    public static readonly string[] ORB_COLOR_IDS = { "cyan", "magenta", "yellow" };
    public static readonly Color[] ORB_COLORS =
    {
        new Color(0.0f, 0.9f, 0.9f),
        new Color(0.9f, 0.0f, 0.9f),
        new Color(1.0f, 0.9f, 0.2f),
    };

    public static Vector2 entrancePos = new Vector2(1.5f, 1.5f);
    public static Vector2 exitPos;
    public static float safeZoneRadius = 3.5f;

    public const float CELL = 1f;
    public bool[,] grid;
    public bool[,] reachable;
    public int gridW,
        gridH;

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
            enemySpeed = 2.5f,
            enemySightRange = 5f,
        };
        levels[2] = new LevelConfig
        {
            name = "Captain's Quarters",
            width = 22,
            height = 16,
            theme = LevelTheme.PirateShip,
            enemyCount = 2,
            hasHiddenPassage = true,
            hasPuzzle = true,
            enemySpeed = 2.8f,
            enemySightRange = 5.5f,
        };
        levels[3] = new LevelConfig
        {
            name = "Castle Dungeon",
            width = 24,
            height = 18,
            theme = LevelTheme.Castle,
            enemyCount = 2,
            hasHiddenPassage = true,
            hasPuzzle = true,
            enemySpeed = 3.2f,
            enemySightRange = 6f,
        };
        levels[4] = new LevelConfig
        {
            name = "Throne Room",
            width = 26,
            height = 20,
            theme = LevelTheme.Castle,
            enemyCount = 3,
            hasHiddenPassage = true,
            hasPuzzle = true,
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
        Random.InitState(GameManager.Instance.runSeed + levelIndex * 1000);
        BuildRoom(config);
    }

    void BuildRoom(LevelConfig config)
    {
        float w = config.width,
            h = config.height;
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

        CreateWallObj("WallN", new Vector2(w / 2f, h + 0.25f), new Vector2(w + 1, 0.5f), wallCol);
        CreateWallObj("WallS", new Vector2(w / 2f, -0.25f), new Vector2(w + 1, 0.5f), wallCol);
        CreateWallObj("WallE", new Vector2(w + 0.25f, h / 2f), new Vector2(0.5f, h + 1), wallCol);
        CreateWallObj("WallW", new Vector2(-0.25f, h / 2f), new Vector2(0.5f, h + 1), wallCol);

        exitPos = new Vector2(w - 2, h - 0.5f);

        BuildGrid(config, wallCol);

        SpawnPlayer(new Vector2(1.5f, 1.5f));
        PlaceSafeZone("EntranceSafe", entrancePos);
        PlaceSafeZone("ExitSafe", exitPos);
        PlaceExitDoor(config, w, h);

        if (!config.hasPuzzle)
            PlaceKey(config, w, h);
        PlaceEnemies(config, w, h);
        if (config.hasPuzzle)
            PlacePuzzle(config, w, h);
        if (config.hasHiddenPassage)
            PlaceHiddenPassage(config, w, h, wallCol);
    }

    void BuildGrid(LevelConfig config, Color wallCol)
    {
        gridW = config.width;
        gridH = config.height;
        grid = new bool[gridW, gridH];

        int shapeCount = Mathf.Max(4, (gridW * gridH) / 40);
        for (int i = 0; i < shapeCount; i++)
        {
            int shape = Random.Range(0, 3);
            switch (shape)
            {
                case 0:
                    StampRect();
                    break;
                case 1:
                    StampL();
                    break;
                case 2:
                    StampU();
                    break;
            }
        }

        int protCount = Mathf.Max(3, (gridW + gridH) / 8);
        for (int i = 0; i < protCount; i++)
            StampProtrusion();

        WidenNarrowPassages();
        EnsureConnectivity();
        WidenNarrowPassages();
        FloodFillReachable();
        RenderGrid(wallCol);
    }

    bool IsSafeCell(int gx, int gy)
    {
        float wx = gx + 0.5f,
            wy = gy + 0.5f;
        return Vector2.Distance(new Vector2(wx, wy), entrancePos) < safeZoneRadius + 1f
            || Vector2.Distance(new Vector2(wx, wy), exitPos) < safeZoneRadius + 1f;
    }

    void SetWall(int x, int y)
    {
        if (x < 1 || x >= gridW - 1 || y < 1 || y >= gridH - 1)
            return;
        if (IsSafeCell(x, y))
            return;
        grid[x, y] = true;
    }

    void StampRect()
    {
        int bw = Random.Range(2, 5);
        int bh = Random.Range(1, 3);
        if (Random.value > 0.5f)
        {
            int t = bw;
            bw = bh;
            bh = t;
        }
        int sx = Random.Range(2, gridW - bw - 2);
        int sy = Random.Range(2, gridH - bh - 2);
        for (int x = sx; x < sx + bw; x++)
        for (int y = sy; y < sy + bh; y++)
            SetWall(x, y);
    }

    void StampL()
    {
        int armA = Random.Range(3, 6);
        int armB = Random.Range(2, 4);
        int sx = Random.Range(2, gridW - armA - 2);
        int sy = Random.Range(2, gridH - armB - 2);
        bool horiz = Random.value > 0.5f;

        if (horiz)
        {
            for (int x = sx; x < sx + armA; x++)
                SetWall(x, sy);
            int endX = Random.value > 0.5f ? sx : sx + armA - 1;
            int dir = Random.value > 0.5f ? 1 : -1;
            for (int i = 1; i <= armB; i++)
                SetWall(endX, sy + i * dir);
        }
        else
        {
            for (int y = sy; y < sy + armA; y++)
                SetWall(sx, y);
            int endY = Random.value > 0.5f ? sy : sy + armA - 1;
            int dir = Random.value > 0.5f ? 1 : -1;
            for (int i = 1; i <= armB; i++)
                SetWall(sx + i * dir, endY);
        }
    }

    void StampU()
    {
        int baseLen = Random.Range(3, 6);
        int armLen = Random.Range(2, 4);
        int sx = Random.Range(2, gridW - baseLen - 2);
        int sy = Random.Range(2, gridH - armLen - 2);
        bool horiz = Random.value > 0.5f;

        if (horiz)
        {
            for (int x = sx; x < sx + baseLen; x++)
                SetWall(x, sy);
            for (int i = 1; i <= armLen; i++)
            {
                SetWall(sx, sy + i);
                SetWall(sx + baseLen - 1, sy + i);
            }
        }
        else
        {
            for (int y = sy; y < sy + baseLen; y++)
                SetWall(sx, y);
            for (int i = 1; i <= armLen; i++)
            {
                SetWall(sx + i, sy);
                SetWall(sx + i, sy + baseLen - 1);
            }
        }
    }

    void StampProtrusion()
    {
        int wall = Random.Range(0, 4);
        int len = Random.Range(2, 5);
        switch (wall)
        {
            case 0:
            {
                int x = Random.Range(3, gridW - 3);
                for (int i = 0; i < len; i++)
                    SetWall(x, gridH - 2 - i);
                break;
            }
            case 1:
            {
                int x = Random.Range(3, gridW - 3);
                for (int i = 0; i < len; i++)
                    SetWall(x, 1 + i);
                break;
            }
            case 2:
            {
                int y = Random.Range(3, gridH - 3);
                for (int i = 0; i < len; i++)
                    SetWall(gridW - 2 - i, y);
                break;
            }
            case 3:
            {
                int y = Random.Range(3, gridH - 3);
                for (int i = 0; i < len; i++)
                    SetWall(1 + i, y);
                break;
            }
        }
    }

    void WidenNarrowPassages()
    {
        bool changed = true;
        int iterations = 0;
        while (changed && iterations < 15)
        {
            changed = false;
            iterations++;
            bool[,] toRemove = new bool[gridW, gridH];

            for (int x = 1; x < gridW - 1; x++)
            {
                for (int y = 1; y < gridH - 1; y++)
                {
                    if (grid[x, y])
                        continue;
                    if (IsSafeCell(x, y))
                        continue;

                    int hSpan = HSpan(x, y);
                    int vSpan = VSpan(x, y);

                    if (hSpan < 3 && vSpan < 3)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx,
                                ny = y + dy;
                            if (nx >= 1 && nx < gridW - 1 && ny >= 1 && ny < gridH - 1)
                                if (grid[nx, ny])
                                    toRemove[nx, ny] = true;
                        }
                    }
                }
            }

            for (int x = 0; x < gridW; x++)
            for (int y = 0; y < gridH; y++)
                if (toRemove[x, y])
                {
                    grid[x, y] = false;
                    changed = true;
                }
        }
    }

    int HSpan(int x, int y)
    {
        int count = 1;
        for (int dx = 1; x + dx < gridW && !grid[x + dx, y]; dx++)
            count++;
        for (int dx = -1; x + dx >= 0 && !grid[x + dx, y]; dx--)
            count++;
        return count;
    }

    int VSpan(int x, int y)
    {
        int count = 1;
        for (int dy = 1; y + dy < gridH && !grid[x, y + dy]; dy++)
            count++;
        for (int dy = -1; y + dy >= 0 && !grid[x, y + dy]; dy--)
            count++;
        return count;
    }

    void EnsureConnectivity()
    {
        FloodFillReachable();

        bool[,] visited = new bool[gridW, gridH];
        for (int x = 0; x < gridW; x++)
        for (int y = 0; y < gridH; y++)
            visited[x, y] = grid[x, y] || reachable[x, y];

        for (int x = 1; x < gridW - 1; x++)
        {
            for (int y = 1; y < gridH - 1; y++)
            {
                if (visited[x, y])
                    continue;
                CarveCorridorToReachable(x, y);
                FloodFillReachable();
                for (int ax = 0; ax < gridW; ax++)
                for (int ay = 0; ay < gridH; ay++)
                    visited[ax, ay] = grid[ax, ay] || reachable[ax, ay];
            }
        }
    }

    void CarveCorridorToReachable(int fromX, int fromY)
    {
        var queue = new Queue<(int x, int y)>();
        var cameFrom = new Dictionary<(int, int), (int, int)>();
        queue.Enqueue((fromX, fromY));
        cameFrom[(fromX, fromY)] = (-1, -1);

        (int tx, int ty) = (-1, -1);

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();

            if (reachable[cx, cy])
            {
                tx = cx;
                ty = cy;
                break;
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };
            for (int d = 0; d < 4; d++)
            {
                int nx = cx + dx[d],
                    ny = cy + dy[d];
                if (nx < 1 || nx >= gridW - 1 || ny < 1 || ny >= gridH - 1)
                    continue;
                if (cameFrom.ContainsKey((nx, ny)))
                    continue;
                cameFrom[(nx, ny)] = (cx, cy);
                queue.Enqueue((nx, ny));
            }
        }

        if (tx < 0)
            return;

        var cur = (tx, ty);
        while (cur != (-1, -1))
        {
            int cx = cur.Item1,
                cy = cur.Item2;
            for (int ddx = -1; ddx <= 1; ddx++)
            for (int ddy = -1; ddy <= 1; ddy++)
            {
                int nx = cx + ddx,
                    ny = cy + ddy;
                if (nx >= 1 && nx < gridW - 1 && ny >= 1 && ny < gridH - 1)
                    grid[nx, ny] = false;
            }
            cur = cameFrom.ContainsKey(cur) ? cameFrom[cur] : (-1, -1);
        }
    }

    void FloodFillReachable()
    {
        reachable = new bool[gridW, gridH];
        int sx = Mathf.Clamp(Mathf.FloorToInt(entrancePos.x), 0, gridW - 1);
        int sy = Mathf.Clamp(Mathf.FloorToInt(entrancePos.y), 0, gridH - 1);
        grid[sx, sy] = false;

        var queue = new Queue<(int, int)>();
        queue.Enqueue((sx, sy));
        reachable[sx, sy] = true;

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };
            for (int d = 0; d < 4; d++)
            {
                int nx = cx + dx[d],
                    ny = cy + dy[d];
                if (nx < 0 || nx >= gridW || ny < 0 || ny >= gridH)
                    continue;
                if (reachable[nx, ny] || grid[nx, ny])
                    continue;
                reachable[nx, ny] = true;
                queue.Enqueue((nx, ny));
            }
        }
    }

    void RenderGrid(Color wallCol)
    {
        int idx = 0;
        for (int y = 0; y < gridH; y++)
        {
            int runStart = -1;
            for (int x = 0; x <= gridW; x++)
            {
                bool isWall = x < gridW && grid[x, y];
                if (isWall && runStart < 0)
                    runStart = x;
                else if (!isWall && runStart >= 0)
                {
                    int runLen = x - runStart;
                    float cx = runStart + runLen / 2f;
                    float cy = y + 0.5f;
                    CreateWallObj(
                        $"W{idx++}",
                        new Vector2(cx, cy),
                        new Vector2(runLen, 1f),
                        wallCol
                    );
                    runStart = -1;
                }
            }
        }
    }

    public List<Vector2> FindPath(Vector2 from, Vector2 to, float maxDist)
    {
        if (grid == null)
            return null;

        int sx = Mathf.Clamp(Mathf.FloorToInt(from.x), 0, gridW - 1);
        int sy = Mathf.Clamp(Mathf.FloorToInt(from.y), 0, gridH - 1);
        int tx = Mathf.Clamp(Mathf.FloorToInt(to.x), 0, gridW - 1);
        int ty = Mathf.Clamp(Mathf.FloorToInt(to.y), 0, gridH - 1);

        if (grid[tx, ty])
        {
            var near = NearestFloor(tx, ty);
            if (near.HasValue)
            {
                tx = near.Value.x;
                ty = near.Value.y;
            }
            else
                return null;
        }

        var cameFrom = new Dictionary<(int, int), (int, int)>();
        var queue = new Queue<(int, int)>();
        queue.Enqueue((sx, sy));
        cameFrom[(sx, sy)] = (-1, -1);
        int maxCells = 2000;
        int visited = 0;

        while (queue.Count > 0 && visited < maxCells)
        {
            var (cx, cy) = queue.Dequeue();
            visited++;

            if (cx == tx && cy == ty)
            {
                var path = new List<Vector2>();
                var cur = (tx, ty);
                while (cur != (-1, -1))
                {
                    path.Add(new Vector2(cur.Item1 + 0.5f, cur.Item2 + 0.5f));
                    cur = cameFrom[cur];
                }
                path.Reverse();
                return path;
            }

            int[] dx = { 1, -1, 1, -1, 1, -1, 0, 0 };
            int[] dy = { 1, 1, -1, -1, 0, 0, 1, -1 };
            for (int d = 0; d < 8; d++)
            {
                int nx = cx + dx[d],
                    ny = cy + dy[d];
                if (nx < 0 || nx >= gridW || ny < 0 || ny >= gridH)
                    continue;
                if (grid[nx, ny] || cameFrom.ContainsKey((nx, ny)))
                    continue;
                if (d < 4 && (grid[cx + dx[d], cy] || grid[cx, cy + dy[d]]))
                    continue;
                float distFromStart = Vector2.Distance(new Vector2(nx + 0.5f, ny + 0.5f), from);
                if (distFromStart > maxDist)
                    continue;
                cameFrom[(nx, ny)] = (cx, cy);
                queue.Enqueue((nx, ny));
            }
        }
        return null;
    }

    (int x, int y)? NearestFloor(int gx, int gy)
    {
        for (int r = 1; r < 8; r++)
        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            int nx = gx + dx,
                ny = gy + dy;
            if (nx >= 0 && nx < gridW && ny >= 0 && ny < gridH && !grid[nx, ny])
                return (nx, ny);
        }
        return null;
    }

    Vector2 FindCellCenter(float minX, float maxX, float minY, float maxY, int maxAttempts = 60)
    {
        int gxMin = Mathf.Max(1, Mathf.FloorToInt(minX));
        int gxMax = Mathf.Min(gridW - 2, Mathf.FloorToInt(maxX));
        int gyMin = Mathf.Max(1, Mathf.FloorToInt(minY));
        int gyMax = Mathf.Min(gridH - 2, Mathf.FloorToInt(maxY));

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int gx = Random.Range(gxMin, gxMax + 1);
            int gy = Random.Range(gyMin, gyMax + 1);
            if (!grid[gx, gy] && reachable[gx, gy])
                return new Vector2(gx + 0.5f, gy + 0.5f);
        }
        for (int gx = gxMin; gx <= gxMax; gx++)
        for (int gy = gyMin; gy <= gyMax; gy++)
            if (!grid[gx, gy] && reachable[gx, gy])
                return new Vector2(gx + 0.5f, gy + 0.5f);

        return new Vector2(gridW / 2f + 0.5f, gridH / 2f + 0.5f);
    }

    Vector2 FindCellCenterAwayFrom(
        float minX,
        float maxX,
        float minY,
        float maxY,
        List<Vector2> avoid,
        float minDist,
        int maxAttempts = 80
    )
    {
        int gxMin = Mathf.Max(1, Mathf.FloorToInt(minX));
        int gxMax = Mathf.Min(gridW - 2, Mathf.FloorToInt(maxX));
        int gyMin = Mathf.Max(1, Mathf.FloorToInt(minY));
        int gyMax = Mathf.Min(gridH - 2, Mathf.FloorToInt(maxY));

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int gx = Random.Range(gxMin, gxMax + 1);
            int gy = Random.Range(gyMin, gyMax + 1);
            if (grid[gx, gy] || !reachable[gx, gy])
                continue;

            Vector2 pos = new Vector2(gx + 0.5f, gy + 0.5f);
            bool tooClose = false;
            foreach (var a in avoid)
                if (Vector2.Distance(pos, a) < minDist)
                {
                    tooClose = true;
                    break;
                }
            if (!tooClose)
                return pos;
        }
        return FindCellCenter(minX, maxX, minY, maxY);
    }

    public bool IsFloorAndReachable(float wx, float wy)
    {
        int gx = Mathf.FloorToInt(wx),
            gy = Mathf.FloorToInt(wy);
        if (gx < 0 || gx >= gridW || gy < 0 || gy >= gridH)
            return false;
        return !grid[gx, gy] && reachable[gx, gy];
    }

    GameObject CreateWallObj(string name, Vector2 pos, Vector2 scale, Color color)
    {
        var wall = CreateSprite(name, pos, SpriteFactory.CreateRect(16, 16), color, 3);
        wall.transform.localScale = new Vector3(scale.x, scale.y, 1);
        wall.transform.parent = currentLevelRoot.transform;
        wall.AddComponent<BoxCollider2D>();
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
            obj.transform.localScale = Vector3.one * 0.735f;
            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.5f);
            obj.AddComponent<PlayerController>();
            obj.AddComponent<PlayerInventory>();
            obj.tag = "Player";
            currentPlayer = obj.GetComponent<PlayerController>();
            DontDestroyOnLoad(obj);
        }
        currentPlayer.transform.position = new Vector3(pos.x, pos.y, 0);
        currentPlayer.SetCanMove(true);
        currentPlayer.GetComponent<PlayerInventory>()?.Clear();

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

    void PlaceSafeZone(string name, Vector2 center)
    {
        var zone = CreateSprite(name, center, SpriteFactory.CreateCircle(32), SAFE_ZONE_COLOR, 2);
        zone.transform.localScale = Vector3.one * safeZoneRadius * 2f;
        zone.transform.parent = currentLevelRoot.transform;
    }

    void PlaceExitDoor(LevelConfig config, float w, float h)
    {
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
        if (config.hasPuzzle)
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
        Vector2 pos = FindCellCenter(2, w - 2, 2, h - 2);
        var obj = CreateSprite("Key", pos, SpriteFactory.CreateKeySprite(), KEY_COLOR, 8);
        obj.transform.localScale = Vector3.one * 0.8f;
        obj.transform.parent = currentLevelRoot.transform;
        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        var pickup = obj.AddComponent<PickupItem>();
        pickup.itemType = PickupItem.ItemType.Key;
        pickup.keyId = $"key_level_{GameManager.Instance.currentLevel}";
    }

    void PlaceEnemies(LevelConfig config, float w, float h)
    {
        Color[] eColors =
        {
            new Color(0.15f, 0.05f, 0.2f),
            new Color(0.1f, 0.1f, 0.15f),
            new Color(0.2f, 0.05f, 0.05f),
            new Color(0.05f, 0.1f, 0.15f),
            new Color(0.12f, 0.02f, 0.12f),
        };

        for (int i = 0; i < config.enemyCount; i++)
        {
            Vector2 ePos = FindCellCenter(3, w - 3, 3, h - 3);
            if (
                Vector2.Distance(ePos, entrancePos) < safeZoneRadius + 2f
                || Vector2.Distance(ePos, exitPos) < safeZoneRadius + 2f
            )
                ePos = FindCellCenter(w * 0.3f, w * 0.7f, h * 0.3f, h * 0.7f);

            var enemyObj = CreateSprite(
                $"Enemy_{i}",
                ePos,
                SpriteFactory.CreateEnemySprite(eColors[i % eColors.Length]),
                Color.white,
                10
            );
            enemyObj.transform.parent = currentLevelRoot.transform;

            var rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var solidCol = enemyObj.AddComponent<CircleCollider2D>();
            solidCol.radius = 0.25f;
            var trigCol = enemyObj.AddComponent<CircleCollider2D>();
            trigCol.radius = 0.3f;
            trigCol.isTrigger = true;

            var enemy = enemyObj.AddComponent<EnemyAI>();
            enemy.patrolSpeed = config.enemySpeed;
            enemy.chaseSpeed = config.enemySpeed * 1.6f;
            enemy.sightRange = config.enemySightRange;
            enemy.obstacleLayer = LayerMask.GetMask("Default");

            int ptCount = Random.Range(3, 6);
            Vector2[] pts = new Vector2[ptCount];
            for (int p = 0; p < ptCount; p++)
            {
                pts[p] = FindCellCenter(
                    Mathf.Max(2, ePos.x - 5),
                    Mathf.Min(w - 2, ePos.x + 5),
                    Mathf.Max(2, ePos.y - 5),
                    Mathf.Min(h - 2, ePos.y + 5)
                );
            }
            enemy.patrolPoints = pts;
            enemy.waitTimeAtPoint = Random.Range(0.5f, 1.5f);
            enemyObj.tag = "Enemy";
            enemyObj.AddComponent<EnemyCatchTrigger>();
        }
    }

    void PlacePuzzle(LevelConfig config, float w, float h)
    {
        Door exitDoor = null;
        foreach (var d in currentLevelRoot.GetComponentsInChildren<Door>())
            if (d.isExitDoor)
            {
                exitDoor = d;
                break;
            }

        var pmObj = new GameObject("PuzzleManager");
        pmObj.transform.parent = currentLevelRoot.transform;
        var pm = pmObj.AddComponent<PuzzleManager>();
        pm.mode = PuzzleManager.PuzzleMode.AllActive;
        pm.linkedDoor = exitDoor;

        int pieceCount = 3;
        var pieces = new PuzzleObject[pieceCount];
        var placed = new List<Vector2>();

        for (int i = 0; i < pieceCount; i++)
        {
            Vector2 pos = FindCellCenterAwayFrom(2, w - 2, 2, h - 2, placed, 3f);
            placed.Add(pos);

            string colorId = ORB_COLOR_IDS[i];
            Color tipColor = ORB_COLORS[i];

            var obj = CreateSprite(
                $"Lever_{i}",
                pos,
                SpriteFactory.CreateLeverSprite(),
                Color.white,
                8
            );
            obj.transform.localScale = Vector3.one * 0.7f;
            obj.transform.parent = currentLevelRoot.transform;
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.8f);

            var puz = obj.AddComponent<PuzzleObject>();
            puz.puzzleType = PuzzleObject.PuzzleType.Lever;
            puz.sequenceIndex = i;
            puz.requiredColorId = colorId;
            puz.leverTipColor = tipColor;

            var tip = CreateSprite(
                $"LeverTip_{i}",
                pos + new Vector2(0, 0.5f),
                SpriteFactory.CreateCircle(8),
                tipColor,
                9
            );
            tip.transform.localScale = Vector3.one * 0.3f;
            tip.transform.parent = obj.transform;

            pieces[i] = puz;
        }
        pm.puzzlePieces = pieces;

        for (int i = 0; i < pieceCount; i++)
        {
            Vector2 pos = FindCellCenterAwayFrom(2, w - 2, 2, h - 2, placed, 3f);
            placed.Add(pos);

            var obj = CreateSprite(
                $"Orb_{i}",
                pos,
                SpriteFactory.CreateCircle(12),
                ORB_COLORS[i],
                8
            );
            obj.transform.localScale = Vector3.one * 0.5f;
            obj.transform.parent = currentLevelRoot.transform;
            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            var pickup = obj.AddComponent<PickupItem>();
            pickup.itemType = PickupItem.ItemType.Orb;
            pickup.orbColorId = ORB_COLOR_IDS[i];
        }
    }

    void PlaceHiddenPassage(LevelConfig config, float w, float h, Color wallCol)
    {
        Vector2 pos = FindCellCenter(w * 0.3f, w * 0.7f, h * 0.3f, h * 0.7f);
        var obj = CreateSprite("HiddenPassage", pos, SpriteFactory.CreateRect(16, 16), wallCol, 3);
        obj.transform.localScale = new Vector3(1.2f, 0.5f, 1);
        obj.transform.parent = currentLevelRoot.transform;
        obj.AddComponent<BoxCollider2D>();
        obj.AddComponent<HiddenPassage>().revealDistance = 1.5f;
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
    public float enemySpeed;
    public float enemySightRange;
}
