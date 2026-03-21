using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Alert,
        Chase,
        Return,
    }

    [Header("Identity")]
    public string enemyName = "Cursed Crew";
    public EnemyType enemyType = EnemyType.Wanderer;

    [Header("Detection")]
    public float sightRange = 5f;
    public float hearingRange = 2.5f;
    public float chaseRange = 8f;
    public LayerMask obstacleLayer;

    [Header("Movement")]
    public float patrolSpeed = 1.8f;
    public float chaseSpeed = 3.8f;

    [Header("Patrol")]
    public Vector2[] patrolPoints;
    public bool randomizePatrol = true;
    public float waitTimeAtPoint = 1.5f;

    [Header("State")]
    public EnemyState currentState = EnemyState.Patrol;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private int currentPatrolIndex;
    private float waitTimer;
    private Vector2 lastKnownPlayerPos;
    private float lostPlayerTimer;
    private float alertDuration = 3f;
    private Vector2 startPosition;
    private float speedModifier = 1f;

    private SpriteRenderer alertIndicator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (randomizePatrol && patrolPoints != null && patrolPoints.Length > 1)
            currentPatrolIndex =
                Mathf.Abs(GameManager.Instance.runSeed + GetInstanceID()) % patrolPoints.Length;

        var indicatorObj = new GameObject("AlertIndicator");
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        alertIndicator = indicatorObj.AddComponent<SpriteRenderer>();
        alertIndicator.sprite = CreateDiamondSprite();
        alertIndicator.color = new Color(1f, 0.3f, 0.3f, 0);
        alertIndicator.sortingOrder = 15;
    }

    void Update()
    {
        if (
            player == null
            || GameManager.Instance == null
            || !GameManager.Instance.isGameActive
            || GameManager.Instance.isPaused
        )
        {
            if (rb != null && GameManager.Instance != null && GameManager.Instance.isPaused)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateSpeedModifier();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                CheckForPlayer();
                break;
            case EnemyState.Alert:
                AlertBehavior();
                CheckForPlayer();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Return:
                ReturnToPatrol();
                break;
        }

        float targetAlpha =
            currentState == EnemyState.Chase ? 1f : (currentState == EnemyState.Alert ? 0.6f : 0f);
        if (alertIndicator != null)
        {
            Color c = alertIndicator.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
            alertIndicator.color = c;
        }

        if (rb.linearVelocity.x > 0.1f && sr != null)
            sr.flipX = false;
        else if (rb.linearVelocity.x < -0.1f && sr != null)
            sr.flipX = true;
    }

    void UpdateSpeedModifier()
    {
        speedModifier = 1f;
        var hits = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (var col in hits)
        {
            if (col.CompareTag("Light"))
            {
                speedModifier = 0.5f;
                break;
            }
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                return;
            }
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            MoveTowards((Vector2)transform.position + randomDir * 3f, patrolSpeed);
            if (Random.value < 0.01f)
                waitTimer = waitTimeAtPoint;
            return;
        }

        Vector2 target = patrolPoints[currentPatrolIndex];
        float dist = Vector2.Distance(transform.position, target);

        if (dist < 0.3f)
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                currentPatrolIndex = randomizePatrol
                    ? Random.Range(0, patrolPoints.Length)
                    : (currentPatrolIndex + 1) % patrolPoints.Length;
                waitTimer = waitTimeAtPoint + Random.Range(-0.5f, 0.5f);
            }
        }
        else
        {
            MoveTowards(target, patrolSpeed);
        }
    }

    void CheckForPlayer()
    {
        if (player == null)
            return;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        var pc = player.GetComponent<PlayerController>();
        float detectionMod = (pc != null && pc.isHoldingBreath) ? 0.3f : 1f;

        if (distToPlayer < hearingRange * detectionMod && pc != null && pc.IsMoving)
        {
            float hearChance = pc.IsSprinting ? 1f : 0.5f;
            if (Random.value < hearChance)
            {
                lastKnownPlayerPos = player.position;
                currentState = EnemyState.Chase;
                return;
            }
        }

        if (distToPlayer < sightRange * detectionMod)
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            var hit = Physics2D.Raycast(transform.position, dir, distToPlayer, obstacleLayer);
            if (hit.collider == null)
            {
                lastKnownPlayerPos = player.position;
                currentState = EnemyState.Chase;
            }
        }
    }

    void AlertBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        lostPlayerTimer -= Time.deltaTime;
        if (lostPlayerTimer <= 0)
            currentState = EnemyState.Return;
    }

    void ChasePlayer()
    {
        if (player == null)
            return;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        var pc = player.GetComponent<PlayerController>();
        float detectionMod = (pc != null && pc.isHoldingBreath) ? 0.3f : 1f;

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        var hit = Physics2D.Raycast(transform.position, dir, distToPlayer, obstacleLayer);
        bool canSee = hit.collider == null && distToPlayer < chaseRange * detectionMod;

        if (canSee)
        {
            lastKnownPlayerPos = player.position;
            MoveTowards(player.position, chaseSpeed);

            if (distToPlayer < 0.6f)
                CatchPlayer();
        }
        else
        {
            MoveTowards(lastKnownPlayerPos, chaseSpeed * 0.8f);
            if (Vector2.Distance(transform.position, lastKnownPlayerPos) < 0.5f)
            {
                currentState = EnemyState.Alert;
                lostPlayerTimer = alertDuration;
            }
        }
    }

    void ReturnToPatrol()
    {
        Vector2 returnTarget =
            (patrolPoints != null && patrolPoints.Length > 0)
                ? patrolPoints[currentPatrolIndex]
                : startPosition;
        MoveTowards(returnTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, returnTarget) < 0.5f)
        {
            currentState = EnemyState.Patrol;
            waitTimer = waitTimeAtPoint;
        }
        CheckForPlayer();
    }

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed * speedModifier;
    }

    void CatchPlayer()
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.OnCaught(this);
    }

    Sprite CreateDiamondSprite()
    {
        var tex = new Texture2D(8, 8);
        tex.filterMode = FilterMode.Point;
        Color clear = Color.clear;
        Color white = Color.white;
        for (int x = 0; x < 8; x++)
        for (int y = 0; y < 8; y++)
            tex.SetPixel(x, y, clear);
        tex.SetPixel(3, 0, white);
        tex.SetPixel(4, 0, white);
        tex.SetPixel(2, 1, white);
        tex.SetPixel(3, 1, white);
        tex.SetPixel(4, 1, white);
        tex.SetPixel(5, 1, white);
        tex.SetPixel(1, 2, white);
        tex.SetPixel(2, 2, white);
        tex.SetPixel(3, 2, white);
        tex.SetPixel(4, 2, white);
        tex.SetPixel(5, 2, white);
        tex.SetPixel(6, 2, white);
        tex.SetPixel(2, 3, white);
        tex.SetPixel(3, 3, white);
        tex.SetPixel(4, 3, white);
        tex.SetPixel(5, 3, white);
        tex.SetPixel(3, 4, white);
        tex.SetPixel(4, 4, white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }
}

public enum EnemyType
{
    Wanderer,
    Patroller,
    Stalker,
    Guardian,
}
