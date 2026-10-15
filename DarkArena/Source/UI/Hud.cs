using System;
using Microsoft.Xna.Framework;

namespace DarkArena.UI;

/// <summary>HUD: кристаллы слева сверху, таймер справа сверху, полоса здоровья снизу по центру, FPS (F3).</summary>
public static class Hud
{
    public static void Draw(Ui ui, Rectangle vp, int collected, int total, float timeLeft, float health, bool showFps, float fps)
    {
        int m = Ui.Margin;
        // Иконка кристалла — жёлтый ромб из повёрнутого квадрата
        ui.Batch.Draw(ui.Pixel, new Vector2(m + 10, m + 13), null, Palette.Collectible, MathF.PI / 4, new Vector2(.5f), 14f, 0, 0);
        ui.Text($"{collected} / {total}", new Vector2(m + 28, m), Color.White);

        var t = TimeSpan.FromSeconds(Math.Max(0, Math.Ceiling(timeLeft)));
        string timer = $"{(int)t.TotalMinutes:00}:{t.Seconds:00}";
        var size = ui.Font.MeasureString(timer);
        ui.Text(timer, new Vector2(vp.Width - m - size.X, m), timeLeft < 30 ? Palette.Hazard : Color.White);

        var bar = new Rectangle(vp.Width / 2 - 120, vp.Height - m - 8, 240, 8);
        ui.FillRounded(bar, 4, Color.Black * 0.5f);
        int w = (int)(240 * MathHelper.Clamp(health, 0, 1));
        if (w >= 8) ui.FillRounded(new Rectangle(bar.X, bar.Y, w, 8), 4, health > 0.3f ? Palette.Player : Palette.Hazard);

        if (showFps) ui.Text($"FPS: {fps:0}", new Vector2(m, m + 32), new Color(180, 255, 180));
    }
}
