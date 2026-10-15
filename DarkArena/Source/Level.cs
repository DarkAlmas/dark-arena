using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DarkArena;

/// <summary>Арена: пол, стены по периметру и препятствия-коробки (AABB).</summary>
public sealed class Level
{
    public const float HalfSize = 20f;
    public List<BoundingBox> Solids { get; } = new();
    private readonly List<(BoundingBox box, Color color)> _draw = new();

    public Level()
    {
        // Пол (верх на y = 0)
        Add(new Vector3(-HalfSize, -1, -HalfSize), new Vector3(HalfSize, 0, HalfSize), Palette.Floor);
        // Стены
        var wall = Palette.Wall;
        Add(new(-HalfSize - 1, 0, -HalfSize - 1), new(HalfSize + 1, 3, -HalfSize), wall);
        Add(new(-HalfSize - 1, 0, HalfSize), new(HalfSize + 1, 3, HalfSize + 1), wall);
        Add(new(-HalfSize - 1, 0, -HalfSize), new(-HalfSize, 3, HalfSize), wall);
        Add(new(HalfSize, 0, -HalfSize), new(HalfSize + 1, 3, HalfSize), wall);
        // Препятствия
        var obs = Palette.Obstacle;
        Add(new(-8, 0, -8), new(-4, 2, -4), obs);
        Add(new(5, 0, -10), new(9, 1, -6), obs);
        Add(new(6, 0, 5), new(8, 3, 12), obs);
        Add(new(-12, 0, 6), new(-6, 1.2f, 9), obs);
        Add(new(-2, 0, 10), new(2, 2.4f, 13), obs);
        Add(new(-15, 0, -15), new(-12, 4, -12), obs);
    }

    private void Add(Vector3 min, Vector3 max, Color c)
    {
        var b = new BoundingBox(min, max);
        Solids.Add(b);
        _draw.Add((b, c));
    }

    public List<BoundingBox> Hazards { get; } = new()
    {
        new(new Vector3(0, 0, -4), new Vector3(3, 0.15f, -1)),
        new(new Vector3(10, 0, -2), new Vector3(13, 0.15f, 1)),
        new(new Vector3(-8, 0, 0), new Vector3(-5, 0.15f, 3)),
    };

    /// <summary>Тонкие рёбра по контуру препятствия, чтобы оно читалось на фоне стен.</summary>
    private static void DrawEdges(Primitives prim, BoundingBox b)
    {
        const float t = 0.06f;
        var size = b.Max - b.Min; var c = (b.Max + b.Min) / 2f;
        foreach (var y in new[] { b.Min.Y + t / 2, b.Max.Y })
        {
            foreach (var z in new[] { b.Min.Z, b.Max.Z })
                prim.DrawCube(Matrix.CreateScale(size.X + t, t, t) * Matrix.CreateTranslation(c.X, y, z), Palette.ObstacleEdge, 0.6f);
            foreach (var x in new[] { b.Min.X, b.Max.X })
                prim.DrawCube(Matrix.CreateScale(t, t, size.Z + t) * Matrix.CreateTranslation(x, y, c.Z), Palette.ObstacleEdge, 0.6f);
        }
        foreach (var x in new[] { b.Min.X, b.Max.X })
        foreach (var z in new[] { b.Min.Z, b.Max.Z })
            prim.DrawCube(Matrix.CreateScale(t, size.Y, t) * Matrix.CreateTranslation(x, c.Y, z), Palette.ObstacleEdge, 0.6f);
    }

    /// <summary>Сетка на полу: полупрозрачная, рисуется после непрозрачной геометрии.</summary>
    public void DrawGrid(Primitives prim)
    {
        int i = 0;
        for (float x = -HalfSize; x <= HalfSize; x += 2f, i++)
        {
            bool major = i % 5 == 0; // каждые 10 м (шаг 2 м)
            var c = Palette.Grid;
            float a = major ? 0.6f : 0.35f, w = major ? 0.075f : 0.05f;
            prim.DrawCube(Matrix.CreateScale(w, 0.01f, HalfSize * 2) * Matrix.CreateTranslation(x, 0.006f, 0), c, 0f, false, a);
            prim.DrawCube(Matrix.CreateScale(HalfSize * 2, 0.01f, w) * Matrix.CreateTranslation(0, 0.006f, x), c, 0f, false, a);
        }
    }

    public void Draw(Primitives prim)
    {
        foreach (var h in Hazards)
        {
            var hs = h.Max - h.Min;
            prim.DrawCube(Matrix.CreateScale(hs) * Matrix.CreateTranslation((h.Max + h.Min) / 2f), Palette.Hazard, 0.6f);
        }
        foreach (var (b, c) in _draw)
        {
            var size = b.Max - b.Min;
            var center = (b.Max + b.Min) / 2f;
            prim.DrawCube(Matrix.CreateScale(size) * Matrix.CreateTranslation(center), c);
            if (c == Palette.Obstacle) DrawEdges(prim, b);
        }
    }
}
