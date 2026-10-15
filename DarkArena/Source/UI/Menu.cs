using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DarkArena.UI;

/// <summary>Вертикальное меню из кнопок 280×48 со скруглением 12 и обводкой при наведении.</summary>
public sealed class Menu
{
    public const int ButtonW = 280, ButtonH = 48, Radius = 12, Gap = 16;
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public List<string> Items { get; }
    private int _hover = -1;
    private int _selected;

    public Menu(string title, params string[] items) { Title = title; Items = new(items); }

    private Rectangle ButtonRect(int i, Rectangle vp)
    {
        int total = Items.Count * ButtonH + (Items.Count - 1) * Gap;
        int y0 = vp.Height / 2 - total / 2 + 40;
        return new Rectangle(vp.Width / 2 - ButtonW / 2, y0 + i * (ButtonH + Gap), ButtonW, ButtonH);
    }

    /// <returns>Индекс выбранного пункта или -1.</returns>
    public int Update(MouseState m, MouseState pm, KeyboardState kb, KeyboardState pkb, Rectangle vp)
    {
        _hover = -1;
        for (int i = 0; i < Items.Count; i++)
            if (ButtonRect(i, vp).Contains(m.Position)) { _hover = i; _selected = i; }
        if (_hover >= 0 && m.LeftButton == ButtonState.Released && pm.LeftButton == ButtonState.Pressed) return _hover;
        if (Pressed(kb, pkb, Keys.Down) || Pressed(kb, pkb, Keys.S)) _selected = (_selected + 1) % Items.Count;
        if (Pressed(kb, pkb, Keys.Up) || Pressed(kb, pkb, Keys.W)) _selected = (_selected + Items.Count - 1) % Items.Count;
        if (Pressed(kb, pkb, Keys.Enter)) return _selected;
        return -1;
    }

    private static bool Pressed(KeyboardState kb, KeyboardState pkb, Keys k) => kb.IsKeyDown(k) && pkb.IsKeyUp(k);

    public void Draw(Ui ui, Rectangle vp)
    {
        ui.Fill(vp, Color.Black * 0.6f); // тёмная подложка
        float titleY = ButtonRect(0, vp).Top - 90;
        ui.TextCentered(Title, new Vector2(vp.Width / 2f, titleY), Color.White, ui.Title);
        if (!string.IsNullOrEmpty(Subtitle))
            ui.TextCentered(Subtitle, new Vector2(vp.Width / 2f, titleY + 44), new Color(200, 205, 230));
        for (int i = 0; i < Items.Count; i++)
        {
            var r = ButtonRect(i, vp);
            bool active = i == _hover || (_hover < 0 && i == _selected);
            ui.FillRounded(r, Radius, active ? Palette.ButtonHover : Palette.Button);
            if (active) ui.OutlineRounded(r, Radius, Palette.Accent);
            ui.TextCentered(Items[i], r.Center.ToVector2(), Color.White);
        }
    }
}
