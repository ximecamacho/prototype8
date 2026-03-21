using UnityEngine;

public class InteractPromptUpdater : MonoBehaviour
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

        player.OnNearInteractable -= UpdatePrompt;
        player.OnNearInteractable += UpdatePrompt;
        CancelInvoke(nameof(TrySubscribe));
    }

    void UpdatePrompt(IInteractable interactable)
    {
        UIManager.Instance?.UpdateInteractPrompt(interactable != null);
    }
}
