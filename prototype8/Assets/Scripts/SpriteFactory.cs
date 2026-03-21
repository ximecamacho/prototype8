using UnityEngine;

public static class SpriteFactory
{
    public static Sprite CreateRect(int w, int h, Color? color = null)
    {
        Color c = color ?? Color.white;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            tex.SetPixel(x, y, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), Mathf.Max(w, h));
    }

    public static Sprite CreateCircle(int size, Color? color = null)
    {
        Color c = color ?? Color.white;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        float r = size / 2f;
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
            tex.SetPixel(x, y, dist < r - 0.5f ? c : Color.clear);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreatePlayerSprite()
    {
        int w = 16,
            h = 24;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color clear = Color.clear;
        Color skin = new Color(0.85f, 0.75f, 0.65f);
        Color hair = new Color(0.4f, 0.25f, 0.15f);
        Color shirt = new Color(0.3f, 0.5f, 0.8f);
        Color pants = new Color(0.25f, 0.25f, 0.35f);
        Color eye = new Color(0.2f, 0.6f, 1f);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            tex.SetPixel(x, y, clear);

        FillRect(tex, 4, 0, 4, 3, pants);
        FillRect(tex, 9, 0, 4, 3, pants);
        FillRect(tex, 5, 3, 3, 5, pants);
        FillRect(tex, 9, 3, 3, 5, pants);
        FillRect(tex, 4, 8, 9, 8, shirt);
        FillRect(tex, 2, 9, 2, 5, shirt);
        FillRect(tex, 13, 9, 2, 5, shirt);
        FillRect(tex, 2, 9, 2, 1, skin);
        FillRect(tex, 13, 9, 2, 1, skin);
        FillRect(tex, 6, 16, 5, 2, skin);
        FillRect(tex, 4, 18, 9, 5, skin);
        FillRect(tex, 3, 22, 11, 2, hair);
        FillRect(tex, 4, 21, 9, 1, hair);
        tex.SetPixel(6, 20, eye);
        tex.SetPixel(10, 20, eye);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.2f), 16);
    }

    public static Sprite CreateEnemySprite(Color bodyColor)
    {
        int w = 16,
            h = 20;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color clear = Color.clear;
        Color mask = new Color(0.9f, 0.9f, 0.9f);
        Color eyes = new Color(1f, 0.3f, 0.3f);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            tex.SetPixel(x, y, clear);

        FillRect(tex, 4, 0, 8, 4, bodyColor);
        FillRect(tex, 3, 4, 10, 8, bodyColor);
        FillRect(tex, 3, 12, 10, 7, bodyColor);
        FillRect(tex, 4, 13, 8, 5, mask);
        tex.SetPixel(6, 16, eyes);
        tex.SetPixel(10, 16, eyes);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.2f), 16);
    }

    public static Sprite CreateKeySprite()
    {
        int s = 12;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color gold = new Color(1f, 0.85f, 0.2f);
        for (int x = 0; x < s; x++)
        for (int y = 0; y < s; y++)
            tex.SetPixel(x, y, Color.clear);

        FillRect(tex, 4, 7, 4, 4, gold);
        tex.SetPixel(5, 8, Color.clear);
        tex.SetPixel(6, 8, Color.clear);
        FillRect(tex, 5, 1, 2, 6, gold);
        FillRect(tex, 7, 2, 2, 2, gold);
        FillRect(tex, 7, 5, 1, 1, gold);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateDoorSprite()
    {
        int w = 16,
            h = 20;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color door = new Color(0.5f, 0.3f, 0.15f);
        Color frame = new Color(0.35f, 0.2f, 0.1f);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            tex.SetPixel(x, y, Color.clear);

        FillRect(tex, 0, 0, w, h, frame);
        FillRect(tex, 2, 0, w - 4, h - 2, door);
        FillRect(tex, 11, 8, 2, 2, new Color(0.8f, 0.7f, 0.3f));

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    public static Sprite CreateSymbolSprite(int symbolIndex)
    {
        int s = 16;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color c = Color.white;

        for (int x = 0; x < s; x++)
        for (int y = 0; y < s; y++)
            tex.SetPixel(x, y, Color.clear);

        switch (symbolIndex % 6)
        {
            case 0:
                for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    if (d >= 4 && d < 6)
                        tex.SetPixel(x, y, c);
                }
                break;
            case 1:
                for (int row = 0; row < 12; row++)
                {
                    int left = 8 - row;
                    int right = 8 + row;
                    if (left >= 0 && left < s)
                        tex.SetPixel(left, row + 2, c);
                    if (right >= 0 && right < s)
                        tex.SetPixel(right, row + 2, c);
                    if (row == 0 || row == 11)
                        for (int x = left; x <= right; x++)
                            if (x >= 0 && x < s)
                                tex.SetPixel(x, row + 2, c);
                }
                break;
            case 2:
                FillRect(tex, 3, 3, 10, 1, c);
                FillRect(tex, 3, 12, 10, 1, c);
                FillRect(tex, 3, 3, 1, 10, c);
                FillRect(tex, 12, 3, 1, 10, c);
                break;
            case 3:
                FillRect(tex, 6, 2, 4, 12, c);
                FillRect(tex, 2, 6, 12, 4, c);
                break;
            case 4:
                for (int i = 0; i < 7; i++)
                {
                    tex.SetPixel(8 - i, 8 + i, c);
                    tex.SetPixel(8 + i, 8 + i, c);
                    tex.SetPixel(8 - i, 8 - i, c);
                    tex.SetPixel(8 + i, 8 - i, c);
                }
                break;
            case 5:
                for (int i = 0; i < 14; i++)
                {
                    tex.SetPixel(1 + i, 1 + i, c);
                    tex.SetPixel(14 - i, 1 + i, c);
                }
                FillRect(tex, 7, 2, 2, 12, c);
                FillRect(tex, 2, 7, 12, 2, c);
                break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateFragmentSprite()
    {
        int s = 10;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color purple = new Color(0.6f, 0.15f, 0.8f);
        for (int x = 0; x < s; x++)
        for (int y = 0; y < s; y++)
            tex.SetPixel(x, y, Color.clear);

        tex.SetPixel(4, 0, purple);
        tex.SetPixel(5, 0, purple);
        FillRect(tex, 3, 1, 4, 2, purple);
        FillRect(tex, 2, 3, 6, 3, purple);
        FillRect(tex, 3, 6, 4, 2, purple);
        FillRect(tex, 4, 8, 2, 2, purple);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateLanternSprite()
    {
        int s = 12;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color frame = new Color(0.5f, 0.4f, 0.2f);
        Color glass = new Color(0.8f, 0.7f, 0.3f, 0.6f);

        for (int x = 0; x < s; x++)
        for (int y = 0; y < s; y++)
            tex.SetPixel(x, y, Color.clear);

        FillRect(tex, 5, 10, 2, 2, frame);
        FillRect(tex, 3, 9, 6, 1, frame);
        FillRect(tex, 2, 2, 1, 7, frame);
        FillRect(tex, 9, 2, 1, 7, frame);
        FillRect(tex, 3, 2, 6, 7, glass);
        FillRect(tex, 2, 1, 8, 1, frame);
        FillRect(tex, 3, 0, 6, 1, frame);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateLeverSprite()
    {
        int w = 8,
            h = 16;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color metal = new Color(0.6f, 0.6f, 0.6f);
        Color handle = new Color(0.8f, 0.3f, 0.3f);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            tex.SetPixel(x, y, Color.clear);

        FillRect(tex, 1, 0, 6, 3, metal);
        FillRect(tex, 3, 3, 2, 10, metal);
        FillRect(tex, 2, 13, 4, 3, handle);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    public static Sprite CreateFoodSprite()
    {
        int s = 10;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color food = new Color(0.4f, 0.75f, 0.25f);
        for (int x = 0; x < s; x++)
        for (int y = 0; y < s; y++)
            tex.SetPixel(x, y, Color.clear);

        FillRect(tex, 3, 1, 4, 2, food);
        FillRect(tex, 2, 3, 6, 4, food);
        FillRect(tex, 3, 7, 4, 2, food);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    static void FillRect(Texture2D tex, int x0, int y0, int w, int h, Color c)
    {
        for (int x = x0; x < x0 + w && x < tex.width; x++)
        for (int y = y0; y < y0 + h && y < tex.height; y++)
            tex.SetPixel(x, y, c);
    }
}
