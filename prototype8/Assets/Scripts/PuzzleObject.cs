using System;
using UnityEngine;

public class PuzzleObject : MonoBehaviour, IInteractable
{
    public enum PuzzleType
    {
        Lever,
    }

    [Header("Puzzle")]
    public PuzzleType puzzleType = PuzzleType.Lever;
    public int sequenceIndex = 0;

    [Header("Color Requirement")]
    public string requiredColorId = "";
    public Color leverTipColor = Color.grey;

    [Header("State")]
    public bool isActivated = false;

    [Header("Linked")]
    public PuzzleManager puzzleManager;

    private SpriteRenderer sr;
    private Color activeColor = new Color(0.3f, 0.9f, 0.3f);
    private Color inactiveColor = new Color(0.4f, 0.4f, 0.4f);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    public string GetPromptText() => "";

    public bool CanInteract() => true;

    public void Interact(PlayerController player)
    {
        if (isActivated)
        {
            Deactivate();
            var inv = player.GetComponent<PlayerInventory>();
            if (inv != null && !string.IsNullOrEmpty(requiredColorId))
                inv.AddOrb(requiredColorId);
            return;
        }

        if (!string.IsNullOrEmpty(requiredColorId))
        {
            var inv = player.GetComponent<PlayerInventory>();
            if (inv == null || !inv.HasOrb(requiredColorId))
            {
                var vf = VisualFeedback.Instance;
                if (vf != null)
                    vf.FlashScreen(new Color(1f, 0.15f, 0.15f), 0.2f);
                return;
            }
            inv.UseOrb(requiredColorId);
        }

        isActivated = true;
        UpdateVisual();
        puzzleManager?.OnPuzzlePieceActivated(this);
    }

    public void Deactivate()
    {
        isActivated = false;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (sr == null)
            return;
        sr.color = isActivated ? activeColor : inactiveColor;
    }
}
