using System;
using Microsoft.Xna.Framework;

namespace DarkArena;

/// <summary>Вращающийся кристалл-икосаэдр, который можно подобрать.</summary>
public sealed class Collectible
{
    public Vector3 Position { get; }
    public bool Collected { get; private set; }
    private float _t;

    public Collectible(Vector3 pos, float phase) { Position = pos; _t = phase; }

    public bool Update(float dt, Player player)
    {
        if (Collected) return false;
        _t += dt;
        var center = player.Position + new Vector3(0, 0.9f, 0);
        if (Vector3.Distance(center, Position) < 1.3f) { Collected = true; return true; }
        return false;
    }

    public void Draw(Primitives prim)
    {
        if (Collected) return;
        var bob = MathF.Sin(_t * 2f) * 0.2f;
        prim.DrawCrystal(Matrix.CreateScale(0.9f) * Matrix.CreateRotationX(_t) * Matrix.CreateRotationY(_t * 2f) * Matrix.CreateTranslation(Position + new Vector3(0, bob, 0)),
            Palette.Collectible, 0.45f);
    }
}
