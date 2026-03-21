using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>();
    private HashSet<string> notes = new HashSet<string>();

    public event Action<string> OnKeyCollected;
    public event Action<string, string> OnNoteCollected;

    public bool HasKey(string keyId) => keys.Contains(keyId);

    public void AddKey(string keyId)
    {
        keys.Add(keyId);
        OnKeyCollected?.Invoke(keyId);
    }

    public void UseKey(string keyId)
    {
        keys.Remove(keyId);
    }

    public void AddNote(string noteId, string text)
    {
        if (!notes.Contains(noteId))
        {
            notes.Add(noteId);
            OnNoteCollected?.Invoke(noteId, text);
        }
    }

    public int KeyCount => keys.Count;

    public void Clear()
    {
        keys.Clear();
        notes.Clear();
    }
}
