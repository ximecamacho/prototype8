using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>();
    private Dictionary<string, int> orbs = new Dictionary<string, int>();

    public event Action<string> OnKeyCollected;

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

    public bool HasOrb(string colorId)
    {
        return orbs.ContainsKey(colorId) && orbs[colorId] > 0;
    }

    public void AddOrb(string colorId)
    {
        if (!orbs.ContainsKey(colorId))
            orbs[colorId] = 0;
        orbs[colorId]++;
    }

    public void UseOrb(string colorId)
    {
        if (orbs.ContainsKey(colorId) && orbs[colorId] > 0)
            orbs[colorId]--;
    }

    public int OrbCount(string colorId)
    {
        return orbs.ContainsKey(colorId) ? orbs[colorId] : 0;
    }

    public int TotalOrbCount
    {
        get
        {
            int total = 0;
            foreach (var kv in orbs) total += kv.Value;
            return total;
        }
    }

    public int KeyCount => keys.Count;

    public void Clear()
    {
        keys.Clear();
        orbs.Clear();
    }
}
