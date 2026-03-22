using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public enum ItemType
    {
        Key,
        Orb,
    }

    [Header("Item")]
    public ItemType itemType = ItemType.Key;

    [Header("Key")]
    public string keyId = "key_01";

    [Header("Orb")]
    public string orbColorId = "cyan";

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

            case ItemType.Orb:
                inv?.AddOrb(orbColorId);
                if (vf != null)
                {
                    vf.FlashScreen(sr != null ? sr.color : Color.white, 0.15f);
                    vf.SpawnParticles(pos, sr != null ? sr.color : Color.white, 4);
                }
                break;
        }

        Destroy(gameObject);
    }
}
