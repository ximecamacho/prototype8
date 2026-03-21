using UnityEngine;

public class NoteInteractionBridge : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating(nameof(TrySubscribe), 0.5f, 1f);
    }

    void TrySubscribe()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null)
            return;

        var inv = player.GetComponent<PlayerInventory>();
        if (inv == null)
            return;

        inv.OnNoteCollected -= OnHintCollected;
        inv.OnNoteCollected += OnHintCollected;
        inv.OnKeyCollected -= OnKeyCollected;
        inv.OnKeyCollected += OnKeyCollected;

        CancelInvoke(nameof(TrySubscribe));
    }

    void OnHintCollected(string id, string text)
    {
        // Visual flash feedback handled by PickupItem -> UIManager.ShowHintIllumination
    }

    void OnKeyCollected(string keyId)
    {
        var cam = FindFirstObjectByType<CameraController>();
        cam?.Shake(0.08f, 0.2f);
    }
}
