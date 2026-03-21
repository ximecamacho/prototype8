using UnityEngine;

public class HiddenPassage : MonoBehaviour
{
    [Header("Passage")]
    public float revealDistance = 1.5f;

    [Header("Visual Hint")]
    public bool showSubtleHint = true;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool isRevealed = false;
    private Color wallColor;
    private float originalAlpha;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (sr != null)
        {
            wallColor = sr.color;
            originalAlpha = wallColor.a;

            if (showSubtleHint)
            {
                Color c = wallColor;
                c.b += 0.03f;
                sr.color = c;
                wallColor = c;
            }
        }
    }

    void Update()
    {
        if (isRevealed)
            return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        float dist = Vector2.Distance(transform.position, player.transform.position);

        if (dist < revealDistance * 0.6f)
        {
            Reveal();
        }
        else if (dist < revealDistance * 1.5f)
        {
            float t = 1f - Mathf.Clamp01((dist - revealDistance * 0.6f) / (revealDistance * 0.9f));
            Color c = wallColor;
            c.a = Mathf.Lerp(originalAlpha, 0.3f, t);
            sr.color = c;
        }
    }

    void Reveal()
    {
        isRevealed = true;

        if (col != null)
            col.enabled = false;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.15f;
            sr.color = c;
        }
    }
}
