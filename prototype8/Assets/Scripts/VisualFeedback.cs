using System.Collections;
using UnityEngine;

public class VisualFeedback : MonoBehaviour
{
    public static VisualFeedback Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void FlashScreen(Color color, float duration = 0.3f)
    {
        StartCoroutine(DoFlash(color, duration));
    }

    IEnumerator DoFlash(Color color, float duration)
    {
        var obj = new GameObject("Flash");
        var canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(color.r, color.g, color.b, 0.15f);
        img.raycastTarget = false;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0.15f, 0f, elapsed / duration);
            img.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }
        Destroy(obj);
    }

    public void BounceObject(Transform obj, float height = 0.3f, float duration = 0.4f)
    {
        if (obj != null)
            StartCoroutine(DoBounce(obj, height, duration));
    }

    IEnumerator DoBounce(Transform obj, float height, float duration)
    {
        if (obj == null)
            yield break;
        Vector3 start = obj.localPosition;
        float elapsed = 0;
        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float bounce = Mathf.Sin(t * Mathf.PI) * height;
            obj.localPosition = start + Vector3.up * bounce;
            yield return null;
        }
        if (obj != null)
            obj.localPosition = start;
    }

    public void PopAndVanish(GameObject obj, float duration = 0.3f)
    {
        if (obj != null)
            StartCoroutine(DoPopVanish(obj, duration));
    }

    IEnumerator DoPopVanish(GameObject obj, float duration)
    {
        if (obj == null)
            yield break;
        Vector3 origScale = obj.transform.localScale;
        float elapsed = 0;

        while (elapsed < duration * 0.3f && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            obj.transform.localScale = origScale * (1f + t * 0.5f);
            yield return null;
        }

        elapsed = 0;
        Vector3 bigScale = obj != null ? obj.transform.localScale : origScale;
        while (elapsed < duration * 0.7f && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.7f);
            obj.transform.localScale = Vector3.Lerp(bigScale, Vector3.zero, t);

            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }
            yield return null;
        }
        if (obj != null)
            Destroy(obj);
    }

    public void RingBurst(
        Vector2 position,
        Color color,
        float maxRadius = 2f,
        float duration = 0.5f
    )
    {
        StartCoroutine(DoRingBurst(position, color, maxRadius, duration));
    }

    IEnumerator DoRingBurst(Vector2 position, Color color, float maxRadius, float duration)
    {
        var obj = new GameObject("RingBurst");
        obj.transform.position = new Vector3(position.x, position.y, 0);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(32, Color.white);
        sr.color = color;
        sr.sortingOrder = 20;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0.1f, maxRadius, t);
            obj.transform.localScale = Vector3.one * scale;
            sr.color = new Color(color.r, color.g, color.b, 1f - t);
            yield return null;
        }
        Destroy(obj);
    }

    public void ShakeObject(Transform obj, float intensity = 0.1f, float duration = 0.3f)
    {
        if (obj != null)
            StartCoroutine(DoShakeObj(obj, intensity, duration));
    }

    IEnumerator DoShakeObj(Transform obj, float intensity, float duration)
    {
        if (obj == null)
            yield break;
        Vector3 orig = obj.localPosition;
        float elapsed = 0;
        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            float remaining = 1f - (elapsed / duration);
            obj.localPosition = orig + (Vector3)Random.insideUnitCircle * intensity * remaining;
            yield return null;
        }
        if (obj != null)
            obj.localPosition = orig;
    }

    public void SpawnParticles(Vector2 position, Color color, int count = 5)
    {
        for (int i = 0; i < count; i++)
            StartCoroutine(DoParticle(position, color));
    }

    IEnumerator DoParticle(Vector2 position, Color color)
    {
        var obj = new GameObject("Particle");
        obj.transform.position = new Vector3(
            position.x + Random.Range(-0.3f, 0.3f),
            position.y + Random.Range(-0.2f, 0.2f),
            0
        );
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateRect(4, 4, Color.white);
        sr.color = color;
        sr.sortingOrder = 25;
        obj.transform.localScale = Vector3.one * Random.Range(0.15f, 0.35f);

        float life = Random.Range(0.4f, 0.8f);
        float elapsed = 0;
        Vector2 vel = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1.5f));

        while (elapsed < life)
        {
            elapsed += Time.unscaledDeltaTime;
            obj.transform.position += (Vector3)vel * Time.unscaledDeltaTime;
            float t = elapsed / life;
            sr.color = new Color(color.r, color.g, color.b, 1f - t);
            obj.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 0.05f, t);
            yield return null;
        }
        Destroy(obj);
    }

    private GameObject pauseOverlay;

    public void ShowPauseIndicator()
    {
        if (pauseOverlay != null)
            return;
        pauseOverlay = new GameObject("PauseOverlay");
        var canvas = pauseOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CreateBorderBar(pauseOverlay.transform, "Top", new Vector2(0, 0.96f), new Vector2(1, 1));
        CreateBorderBar(pauseOverlay.transform, "Bot", new Vector2(0, 0), new Vector2(1, 0.04f));
        CreateBorderBar(pauseOverlay.transform, "Left", new Vector2(0, 0), new Vector2(0.02f, 1));
        CreateBorderBar(pauseOverlay.transform, "Right", new Vector2(0.98f, 0), new Vector2(1, 1));
    }

    void CreateBorderBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.3f, 0.8f, 0.9f, 0.6f);
        img.raycastTarget = false;
    }

    public void HidePauseIndicator()
    {
        if (pauseOverlay != null)
        {
            Destroy(pauseOverlay);
            pauseOverlay = null;
        }
    }

    void Update()
    {
        if (pauseOverlay != null)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * 3f) * 0.3f + 0.5f;
            foreach (var img in pauseOverlay.GetComponentsInChildren<UnityEngine.UI.Image>())
                img.color = new Color(0.3f, 0.8f, 0.9f, pulse);
        }
    }
}
