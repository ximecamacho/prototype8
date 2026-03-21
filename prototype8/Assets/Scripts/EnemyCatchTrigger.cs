using UnityEngine;

public class EnemyCatchTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive)
            return;

        var pc = other.GetComponent<PlayerController>();
        var enemy = GetComponent<EnemyAI>();
        if (pc != null && enemy != null)
            pc.OnCaught(enemy);
    }
}
