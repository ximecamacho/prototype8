using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public enum DoorType
    {
        Key,
        Puzzle,
        Hidden,
    }

    [Header("Door Settings")]
    public DoorType doorType = DoorType.Key;
    public string requiredKeyId = "";
    public bool isLocked = true;
    public bool isExitDoor = false;

    [Header("Hidden Door")]
    public bool isHidden = false;
    public float revealDistance = 1.2f;

    [Header("Visual")]
    private SpriteRenderer sr;
    private Color lockedColor = new Color(0.6f, 0.15f, 0.15f);
    private Color unlockedColor = new Color(0.15f, 0.6f, 0.15f);
    private Color hiddenColor;

    private bool isOpen = false;
    private bool isRevealed = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateRect(16, 16);
        }

        if (isHidden)
            hiddenColor = sr.color;
        else
            sr.color = isLocked ? lockedColor : unlockedColor;

        sr.sortingOrder = 5;
    }

    void Update()
    {
        if (isHidden && !isRevealed)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < revealDistance)
                {
                    float pulse = Mathf.Sin(Time.time * 4f) * 0.05f;
                    sr.color = new Color(
                        hiddenColor.r + pulse,
                        hiddenColor.g + pulse,
                        hiddenColor.b + pulse + 0.02f
                    );
                }
                if (dist < revealDistance * 0.5f)
                    RevealDoor();
            }
        }

        if (!isOpen && !isHidden && isLocked && sr != null)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.08f;
            sr.color = new Color(lockedColor.r + pulse, lockedColor.g, lockedColor.b);
        }
    }

    void RevealDoor()
    {
        isRevealed = true;
        isHidden = false;
        sr.color = lockedColor;
    }

    public string GetPromptText() => "";

    public bool CanInteract()
    {
        if (isHidden && !isRevealed)
            return false;
        return !isOpen;
    }

    public void Interact(PlayerController player)
    {
        if (isOpen)
            return;

        if (!isLocked)
        {
            Open(player);
            return;
        }

        switch (doorType)
        {
            case DoorType.Key:
                var inv = player.GetComponent<PlayerInventory>();
                if (inv != null && inv.HasKey(requiredKeyId))
                {
                    inv.UseKey(requiredKeyId);
                    Unlock();
                    Open(player);
                }
                else
                {
                    VisualFeedback.Instance?.FlashScreen(new Color(1f, 0.15f, 0.15f), 0.2f);
                }
                break;

            case DoorType.Puzzle:
                VisualFeedback.Instance?.FlashScreen(new Color(1f, 0.15f, 0.15f), 0.2f);
                break;
        }
    }

    public void Unlock()
    {
        isLocked = false;
        if (sr != null)
            sr.color = unlockedColor;
    }

    void Open(PlayerController player)
    {
        isOpen = true;
        if (isExitDoor)
        {
            GameManager.Instance?.CompleteLevel();
        }
        else
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.2f;
                sr.color = c;
            }
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }
}
