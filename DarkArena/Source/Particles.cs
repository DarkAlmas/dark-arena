using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DarkArena;

/// <summary>Простой взрыв частиц при подборе кристалла.</summary>
public sealed class Particles
{
    private struct P { public Vector3 Pos, Vel; public float Life; }
    private readonly List<P> _list = new();
    private readonly Random _rng = new();

    public void Burst(Vector3 at, int count = 24)
    {
        for (int i = 0; i < count; i++)
        {
            var dir = new Vector3((float)_rng.NextDouble() - .5f, (float)_rng.NextDouble(), (float)_rng.NextDouble() - .5f);
            _list.Add(new P { Pos = at, Vel = Vector3.Normalize(dir) * (3f + (float)_rng.NextDouble() * 3f), Life = 0.8f });
        }
    }

    public void Update(float dt)
    {
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            var p = _list[i];
            p.Life -= dt; p.Vel.Y -= 9f * dt; p.Pos += p.Vel * dt;
            if (p.Life <= 0) _list.RemoveAt(i); else _list[i] = p;
        }
    }

    public void Draw(Primitives prim)
    {
        foreach (var p in _list)
            prim.DrawCube(Matrix.CreateScale(0.15f * p.Life / 0.8f + 0.02f) * Matrix.CreateTranslation(p.Pos), Palette.Collectible, 1f, false);
    }
}
