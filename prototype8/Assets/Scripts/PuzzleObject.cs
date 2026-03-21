using System;
using UnityEngine;

public class PuzzleObject : MonoBehaviour, IInteractable
{
    public enum PuzzleType
    {
        Lever,
        PressurePlate,
        SequenceButton,
        Lantern,
    }

    [Header("Puzzle")]
    public PuzzleType puzzleType = PuzzleType.Lever;
    public string puzzleGroupId = "puzzle_01";
    public int sequenceIndex = 0;

    [Header("State")]
    public bool isActivated = false;

    [Header("Linked")]
    public Door linkedDoor;
    public PuzzleManager puzzleManager;

    [Header("Visual")]
    private SpriteRenderer sr;
    private Color activeColor = new Color(0.3f, 0.9f, 0.3f);
    private Color inactiveColor = new Color(0.4f, 0.4f, 0.4f);

    public event Action<PuzzleObject> OnActivated;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    public string GetPromptText() => "";

    public bool CanInteract()
    {
        if (puzzleType == PuzzleType.PressurePlate)
            return false;
        if (puzzleType == PuzzleType.Lantern && isActivated)
            return false;
        return true;
    }

    public void Interact(PlayerController player)
    {
        Activate();
    }

    public void Activate()
    {
        isActivated = !isActivated;

        if (puzzleType == PuzzleType.Lantern)
        {
            isActivated = true;
            gameObject.tag = "Light";
        }

        UpdateVisual();
        OnActivated?.Invoke(this);
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

        if (puzzleType == PuzzleType.Lantern && isActivated)
            sr.color = new Color(1f, 0.9f, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (puzzleType == PuzzleType.PressurePlate && other.CompareTag("Player"))
            if (!isActivated)
                Activate();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (puzzleType == PuzzleType.PressurePlate && other.CompareTag("Player"))
            Deactivate();
    }
}
