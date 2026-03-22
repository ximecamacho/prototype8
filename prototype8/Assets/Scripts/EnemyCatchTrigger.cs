using UnityEngine;

public class EnemyCatchTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive)
            return;

        Vector2 playerPos = other.transform.position;
        float r = LevelGenerator.safeZoneRadius;
        if (
            Vector2.Distance(playerPos, LevelGenerator.entrancePos) < r
            || Vector2.Distance(playerPos, LevelGenerator.exitPos) < r
        )
            return;

        var pc = other.GetComponent<PlayerController>();
        var enemy = GetComponent<EnemyAI>();
        if (pc != null && enemy != null)
            pc.OnCaught(enemy);
    }
}
