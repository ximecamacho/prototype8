using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public enum PuzzleMode { AllActive }

    public PuzzleMode mode = PuzzleMode.AllActive;
    public PuzzleObject[] puzzlePieces;
    public Door linkedDoor;

    private bool isSolved = false;

    void Start()
    {
        foreach (var piece in puzzlePieces)
            piece.puzzleManager = this;
    }

    public void OnPuzzlePieceActivated(PuzzleObject piece)
    {
        if (isSolved) return;
        CheckAllActive();
    }

    void CheckAllActive()
    {
        foreach (var piece in puzzlePieces)
        {
            if (!piece.isActivated)
                return;
        }
        SolvePuzzle();
    }

    void SolvePuzzle()
    {
        isSolved = true;
        if (linkedDoor != null)
            linkedDoor.Unlock();
    }
}
