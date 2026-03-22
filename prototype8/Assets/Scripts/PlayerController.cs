using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Interaction")]
    public float interactRange = 1.2f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;
    private IInteractable nearestInteractable;
    private SpriteRenderer sr;

    private int facingDir = 0;

    public event Action<IInteractable> OnNearInteractable;

    public bool IsMoving => moveInput.magnitude > 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!canMove)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
                facingDir = h > 0 ? 2 : 1;
            else
                facingDir = v > 0 ? 3 : 0;
        }

        if (sr != null)
            sr.flipX = facingDir == 1;

        if (Input.GetKeyDown(KeyCode.Space))
            TryInteract();

        CheckNearbyInteractables();
    }

    void FixedUpdate()
    {
        if (!canMove)
            return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void CheckNearbyInteractables()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
        IInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }
        }

        nearestInteractable = closest;
        OnNearInteractable?.Invoke(closest);
    }

    void TryInteract()
    {
        if (nearestInteractable != null && nearestInteractable.CanInteract())
            nearestInteractable.Interact(this);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!value)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;
        }
    }

    public void OnCaught(EnemyAI enemy)
    {
        SetCanMove(false);
        GameManager.Instance.TriggerTranfur();
    }
}
