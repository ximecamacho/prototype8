using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public enum PuzzleMode
    {
        AllActive,
        Sequence,
        TimedSequence,
    }

    [Header("Puzzle Configuration")]
    public string puzzleId = "puzzle_01";
    public PuzzleMode mode = PuzzleMode.AllActive;
    public PuzzleObject[] puzzlePieces;
    public Door linkedDoor;

    [Header("Sequence Mode")]
    public int[] correctSequence;
    private List<int> playerSequence = new List<int>();

    [Header("Timed Mode")]
    public float timeWindow = 5f;
    private float sequenceTimer = 0;
    private bool sequenceStarted = false;

    [Header("Hidden Hint")]
    public string hintDescription = "Look at the paintings...";

    private bool isSolved = false;

    void Start()
    {
        foreach (var piece in puzzlePieces)
        {
            piece.puzzleManager = this;
        }
    }

    void Update()
    {
        if (isSolved)
            return;

        if (mode == PuzzleMode.TimedSequence && sequenceStarted)
        {
            sequenceTimer -= Time.deltaTime;
            if (sequenceTimer <= 0)
            {
                ResetPuzzle();
            }
        }
    }

    public void OnPuzzlePieceActivated(PuzzleObject piece)
    {
        if (isSolved)
            return;

        switch (mode)
        {
            case PuzzleMode.AllActive:
                CheckAllActive();
                break;
            case PuzzleMode.Sequence:
            case PuzzleMode.TimedSequence:
                HandleSequenceInput(piece);
                break;
        }
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

    void HandleSequenceInput(PuzzleObject piece)
    {
        int pieceIndex = System.Array.IndexOf(puzzlePieces, piece);
        if (pieceIndex < 0)
            return;

        if (!sequenceStarted && mode == PuzzleMode.TimedSequence)
        {
            sequenceStarted = true;
            sequenceTimer = timeWindow;
        }

        playerSequence.Add(pieceIndex);

        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (i >= correctSequence.Length || playerSequence[i] != correctSequence[i])
            {
                ResetPuzzle();
                return;
            }
        }

        if (playerSequence.Count == correctSequence.Length)
        {
            SolvePuzzle();
        }
    }

    void SolvePuzzle()
    {
        isSolved = true;
        if (linkedDoor != null)
        {
            linkedDoor.Unlock();
        }
    }

    void ResetPuzzle()
    {
        playerSequence.Clear();
        sequenceStarted = false;
        sequenceTimer = 0;

        foreach (var piece in puzzlePieces)
        {
            piece.Deactivate();
        }
    }
}
