using Microsoft.Xna.Framework;

namespace DarkArena;

/// <summary>Палитра от Design Bot: low-poly, только цвета вершин, без текстур.</summary>
public static class Palette
{
    public static readonly Color SkyTop = new(0x0E, 0x10, 0x20);
    public static readonly Color SkyBottom = new(0x1E, 0x23, 0x47);
    public static readonly Color Fog = SkyBottom;
    public static readonly Color Floor = new(0x2A, 0x2F, 0x4F);
    public static readonly Color Grid = new(0x3B, 0x42, 0x70);
    public static readonly Color Wall = new(0x45, 0x4C, 0x7A);
    public static readonly Color Obstacle = new(0x5A, 0x62, 0x9A);
    public static readonly Color ObstacleEdge = Grid;
    public static readonly Color Player = new(0x4F, 0xC3, 0xF7);
    public static readonly Color Visor = new(0x0E, 0x10, 0x20);
    public static readonly Color Collectible = new(0xFF, 0xD5, 0x4F);
    public static readonly Color Hazard = new(0xFF, 0x53, 0x70);
    public static readonly Color Accent = Player;
    public static readonly Color Button = new(0x25, 0x2A, 0x4A);
    public static readonly Color ButtonHover = new(0x30, 0x36, 0x5E);
}
