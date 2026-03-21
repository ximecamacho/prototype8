using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public enum ItemType
    {
        Key,
        Hint,
    }

    [Header("Item")]
    public ItemType itemType = ItemType.Key;
    public string itemId = "key_01";

    [Header("Key")]
    public string keyId = "key_01";

    [Header("Hint")]
    public Color hintGlowColor = Color.cyan;
    public Sprite hintSprite;

    private SpriteRenderer sr;
    private bool collected = false;

    static readonly Color KEY_FLASH = new Color(1f, 0.85f, 0.2f);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (collected)
            return;

        float bob = Mathf.Sin(Time.time * 2f + transform.position.x) * 0.05f;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y + bob * Time.deltaTime,
            0
        );
    }

    public string GetPromptText() => "";

    public bool CanInteract() => !collected;

    public void Interact(PlayerController player)
    {
        if (collected)
            return;
        collected = true;

        var inv = player.GetComponent<PlayerInventory>();
        var vf = VisualFeedback.Instance;
        Vector2 pos = transform.position;

        switch (itemType)
        {
            case ItemType.Key:
                inv?.AddKey(keyId);
                if (vf != null)
                {
                    vf.FlashScreen(KEY_FLASH, 0.15f);
                    vf.RingBurst(pos, KEY_FLASH, 1.2f, 0.3f);
                    vf.SpawnParticles(pos, KEY_FLASH, 4);
                }
                break;

            case ItemType.Hint:
                inv?.AddNote(itemId, "");
                UIManager.Instance?.ShowHintIllumination(hintGlowColor, hintSprite);
                if (vf != null)
                    vf.SpawnParticles(pos, hintGlowColor, 3);
                break;
        }

        Destroy(gameObject);
    }
}
