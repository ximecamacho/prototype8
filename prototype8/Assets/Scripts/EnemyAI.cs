using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Return,
    }

    [Header("Detection")]
    public float sightRange = 5f;
    public float hearingRange = 2.5f;
    public LayerMask obstacleLayer;

    [Header("Movement")]
    public float patrolSpeed = 1.8f;
    public float chaseSpeed = 3.8f;
    public float maxChaseDistance = 8f;

    [Header("Patrol")]
    public Vector2[] patrolPoints;
    public float waitTimeAtPoint = 0.8f;

    public EnemyState currentState = EnemyState.Patrol;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private SpriteRenderer alertIndicator;
    private int patrolIndex;
    private float waitTimer;
    private Vector2 startPosition;
    private float stuckTimer;
    private Vector2 lastPos;

    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathRecalcTimer;
    private const float PATH_RECALC_INTERVAL = 0.3f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        lastPos = startPosition;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (patrolPoints != null && patrolPoints.Length > 0)
            patrolIndex = Random.Range(0, patrolPoints.Length);

        var ind = new GameObject("AlertIndicator");
        ind.transform.SetParent(transform);
        ind.transform.localPosition = new Vector3(0, 0.8f, 0);
        alertIndicator = ind.AddComponent<SpriteRenderer>();
        alertIndicator.sprite = CreateDiamondSprite();
        alertIndicator.color = new Color(1f, 0.3f, 0.3f, 0f);
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
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            if (Vector2.Distance(transform.position, lastPos) < 0.15f)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;
            lastPos = transform.position;

            if (stuckTimer > 1.5f)
            {
                stuckTimer = 0f;
                currentState = EnemyState.Return;
                currentPath = null;
            }
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                DoPatrol();
                TryDetectPlayer();
                break;
            case EnemyState.Chase:
                DoChase();
                break;
            case EnemyState.Return:
                DoReturn();
                break;
        }

        if (alertIndicator != null)
        {
            var c = alertIndicator.color;
            c.a = currentState == EnemyState.Chase ? 1f : 0f;
            alertIndicator.color = c;
        }

        if (sr != null)
        {
            if (rb.linearVelocity.x > 0.1f)
                sr.flipX = false;
            else if (rb.linearVelocity.x < -0.1f)
                sr.flipX = true;
        }
    }

    void DoPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = patrolPoints[patrolIndex];
        float dist = Vector2.Distance(transform.position, target);

        if (dist < 0.4f)
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                waitTimer = waitTimeAtPoint;
            }
        }
        else
        {
            MoveToward(target, patrolSpeed);
        }
    }

    void TryDetectPlayer()
    {
        if (player == null)
            return;

        float r = LevelGenerator.safeZoneRadius;
        Vector2 pp = player.position;
        if (
            Vector2.Distance(pp, LevelGenerator.entrancePos) < r
            || Vector2.Distance(pp, LevelGenerator.exitPos) < r
        )
            return;

        float dist = Vector2.Distance(transform.position, player.position);

        var pc = player.GetComponent<PlayerController>();
        if (dist < hearingRange && pc != null && pc.IsMoving)
        {
            currentState = EnemyState.Chase;
            currentPath = null;
            pathRecalcTimer = 0f;
            return;
        }

        if (dist < sightRange)
        {
            currentState = EnemyState.Chase;
            currentPath = null;
            pathRecalcTimer = 0f;
        }
    }

    void DoChase()
    {
        if (player == null)
        {
            currentState = EnemyState.Return;
            currentPath = null;
            return;
        }

        float r = LevelGenerator.safeZoneRadius;
        Vector2 pp = player.position;
        if (
            Vector2.Distance(pp, LevelGenerator.entrancePos) < r
            || Vector2.Distance(pp, LevelGenerator.exitPos) < r
        )
        {
            currentState = EnemyState.Return;
            currentPath = null;
            return;
        }

        if (Vector2.Distance(transform.position, startPosition) > maxChaseDistance)
        {
            currentState = EnemyState.Return;
            currentPath = null;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > sightRange * 1.5f)
        {
            currentState = EnemyState.Return;
            currentPath = null;
            return;
        }

        pathRecalcTimer -= Time.deltaTime;
        if (pathRecalcTimer <= 0f || currentPath == null)
        {
            pathRecalcTimer = PATH_RECALC_INTERVAL;
            var lg = LevelGenerator.Instance;
            if (lg != null)
            {
                currentPath = lg.FindPath(transform.position, player.position, maxChaseDistance);
                pathIndex = 1;
            }
        }

        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector2 waypoint = currentPath[pathIndex];
            float wpDist = Vector2.Distance(transform.position, waypoint);

            if (wpDist < 0.3f)
            {
                pathIndex++;
                if (pathIndex >= currentPath.Count)
                {
                    pathRecalcTimer = 0f;
                }
            }
            else
            {
                MoveToward(waypoint, chaseSpeed);
            }
        }
        else
        {
            MoveToward(player.position, chaseSpeed);
        }

        if (dist < 0.6f)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.OnCaught(this);
        }
    }

    void DoReturn()
    {
        float dist = Vector2.Distance(transform.position, startPosition);
        if (dist < 0.5f)
        {
            currentState = EnemyState.Patrol;
            waitTimer = waitTimeAtPoint;
            rb.linearVelocity = Vector2.zero;
            currentPath = null;
        }
        else
        {
            pathRecalcTimer -= Time.deltaTime;
            if (pathRecalcTimer <= 0f || currentPath == null)
            {
                pathRecalcTimer = PATH_RECALC_INTERVAL * 2f;
                var lg = LevelGenerator.Instance;
                if (lg != null)
                {
                    currentPath = lg.FindPath(
                        transform.position,
                        startPosition,
                        maxChaseDistance * 2f
                    );
                    pathIndex = 1;
                }
            }

            if (currentPath != null && pathIndex < currentPath.Count)
            {
                Vector2 waypoint = currentPath[pathIndex];
                if (Vector2.Distance(transform.position, waypoint) < 0.3f)
                    pathIndex++;

                if (pathIndex < currentPath.Count)
                    MoveToward(currentPath[pathIndex], patrolSpeed * 0.8f);
                else
                    MoveToward(startPosition, patrolSpeed * 0.8f);
            }
            else
            {
                MoveToward(startPosition, patrolSpeed * 0.8f);
            }
        }

        TryDetectPlayer();
    }

    void MoveToward(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    Sprite CreateDiamondSprite()
    {
        var tex = new Texture2D(8, 8);
        tex.filterMode = FilterMode.Point;
        for (int x = 0; x < 8; x++)
        for (int y = 0; y < 8; y++)
            tex.SetPixel(x, y, Color.clear);
        Color w = Color.white;
        tex.SetPixel(3, 0, w);
        tex.SetPixel(4, 0, w);
        tex.SetPixel(2, 1, w);
        tex.SetPixel(3, 1, w);
        tex.SetPixel(4, 1, w);
        tex.SetPixel(5, 1, w);
        tex.SetPixel(1, 2, w);
        tex.SetPixel(2, 2, w);
        tex.SetPixel(3, 2, w);
        tex.SetPixel(4, 2, w);
        tex.SetPixel(5, 2, w);
        tex.SetPixel(6, 2, w);
        tex.SetPixel(2, 3, w);
        tex.SetPixel(3, 3, w);
        tex.SetPixel(4, 3, w);
        tex.SetPixel(5, 3, w);
        tex.SetPixel(3, 4, w);
        tex.SetPixel(4, 4, w);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }
}
