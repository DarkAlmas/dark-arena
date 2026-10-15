using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkArena.UI;

/// <summary>Примитивы 2D-интерфейса: скруглённые прямоугольники, текст с тенью, градиент.</summary>
public sealed class Ui
{
    private readonly GraphicsDevice _gd;
    public SpriteBatch Batch { get; }
    public SpriteFont Font { get; }
    public SpriteFont Title { get; }
    public Texture2D Pixel { get; }
    private readonly Dictionary<(int, int, int, bool), Texture2D> _rounded = new();
    public const int Margin = 24;

    public Ui(GraphicsDevice gd, SpriteFont font, SpriteFont title)
    {
        _gd = gd; Font = font; Title = title;
        Batch = new SpriteBatch(gd);
        Pixel = new Texture2D(gd, 1, 1);
        Pixel.SetData(new[] { Color.White });
    }

    /// <summary>Белая маска скруглённого прямоугольника (заливка или контур 2px), с кешем.</summary>
    private Texture2D Rounded(int w, int h, int r, bool outline)
    {
        if (_rounded.TryGetValue((w, h, r, outline), out var t)) return t;
        var data = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float d = Sdf(x + .5f, y + .5f, w, h, r);
            float a = outline ? Math.Clamp(1f - Math.Abs(d + 1f) + 0.5f, 0, 1) : Math.Clamp(0.5f - d, 0, 1);
            data[y * w + x] = Color.White * a;
        }
        t = new Texture2D(_gd, w, h);
        t.SetData(data);
        _rounded[(w, h, r, outline)] = t;
        return t;
    }

    private static float Sdf(float x, float y, int w, int h, int r)
    {
        float qx = Math.Abs(x - w / 2f) - (w / 2f - r);
        float qy = Math.Abs(y - h / 2f) - (h / 2f - r);
        float ox = Math.Max(qx, 0), oy = Math.Max(qy, 0);
        return MathF.Sqrt(ox * ox + oy * oy) + Math.Min(Math.Max(qx, qy), 0) - r;
    }

    public void FillRounded(Rectangle r, int radius, Color c) => Batch.Draw(Rounded(r.Width, r.Height, radius, false), r, c);
    public void OutlineRounded(Rectangle r, int radius, Color c) => Batch.Draw(Rounded(r.Width, r.Height, radius, true), r, c);
    public void Fill(Rectangle r, Color c) => Batch.Draw(Pixel, r, c);

    public void Text(string s, Vector2 pos, Color c, SpriteFont font = null)
    {
        font ??= Font;
        Batch.DrawString(font, s, pos + new Vector2(1, 2), Color.Black * 0.6f);
        Batch.DrawString(font, s, pos, c);
    }

    public void TextCentered(string s, Vector2 center, Color c, SpriteFont font = null)
    {
        font ??= Font;
        Text(s, center - font.MeasureString(s) / 2f, c, font);
    }

    /// <summary>Вертикальный градиент неба (полосами — дёшево и без шейдеров).</summary>
    public void SkyGradient(Rectangle vp, Color top, Color bottom)
    {
        const int bands = 64;
        for (int i = 0; i < bands; i++)
        {
            int y0 = vp.Height * i / bands, y1 = vp.Height * (i + 1) / bands;
            Fill(new Rectangle(0, y0, vp.Width, y1 - y0), Color.Lerp(top, bottom, i / (float)(bands - 1)));
        }
    }
}
