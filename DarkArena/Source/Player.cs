using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DarkArena;

/// <summary>Игрок: движение WASD относительно камеры, прыжок, гравитация, коллизии AABB.</summary>
public sealed class Player
{
    public Vector3 Position;          // точка у ног
    public Vector3 Velocity;
    public float Facing;              // угол поворота модели (рад)
    public bool OnGround { get; private set; }

    public static readonly Vector3 HalfExtents = new(0.4f, 0.9f, 0.4f);
    private const float Speed = 7f, JumpSpeed = 8f, Gravity = 22f;
    private float _walkAnim;

    public Player(Vector3 start) { Position = start; }

    public BoundingBox Bounds => new(Position - new Vector3(HalfExtents.X, 0, HalfExtents.Z),
                                     Position + new Vector3(HalfExtents.X, HalfExtents.Y * 2, HalfExtents.Z));

    public void Update(float dt, KeyboardState kb, KeyboardState prevKb, float cameraYaw, Level level)
    {
        var input = Vector2.Zero;
        if (kb.IsKeyDown(Keys.W)) input.Y += 1;
        if (kb.IsKeyDown(Keys.S)) input.Y -= 1;
        if (kb.IsKeyDown(Keys.D)) input.X += 1;
        if (kb.IsKeyDown(Keys.A)) input.X -= 1;

        var forward = new Vector3(-MathF.Sin(cameraYaw), 0, -MathF.Cos(cameraYaw));
        var right = new Vector3(-forward.Z, 0, forward.X);
        var move = Vector3.Zero;
        if (input != Vector2.Zero)
        {
            input.Normalize();
            move = forward * input.Y + right * input.X;
            Facing = MathF.Atan2(move.X, move.Z);
            _walkAnim += dt * 10f;
        }
        Velocity.X = move.X * Speed;
        Velocity.Z = move.Z * Speed;

        if (OnGround && kb.IsKeyDown(Keys.Space) && prevKb.IsKeyUp(Keys.Space))
            Velocity.Y = JumpSpeed;
        Velocity.Y -= Gravity * dt;

        // Раздельное разрешение по осям
        MoveAxis(new Vector3(Velocity.X * dt, 0, 0), level);
        MoveAxis(new Vector3(0, 0, Velocity.Z * dt), level);
        OnGround = false;
        if (MoveAxis(new Vector3(0, Velocity.Y * dt, 0), level))
        {
            if (Velocity.Y < 0) OnGround = true;
            Velocity.Y = 0;
        }

        if (Position.Y < -20) Position = new Vector3(0, 2, 0); // страховка
    }

    /// <returns>true, если было столкновение.</returns>
    private bool MoveAxis(Vector3 delta, Level level)
    {
        Position += delta;
        bool hit = false;
        foreach (var s in level.Solids)
        {
            var b = Bounds;
            if (!b.Intersects(s)) continue;
            hit = true;
            if (delta.X > 0) Position.X = s.Min.X - HalfExtents.X - 0.001f;
            else if (delta.X < 0) Position.X = s.Max.X + HalfExtents.X + 0.001f;
            if (delta.Z > 0) Position.Z = s.Min.Z - HalfExtents.Z - 0.001f;
            else if (delta.Z < 0) Position.Z = s.Max.Z + HalfExtents.Z + 0.001f;
            if (delta.Y > 0) Position.Y = s.Min.Y - HalfExtents.Y * 2 - 0.001f;
            else if (delta.Y < 0) Position.Y = s.Max.Y;
        }
        return hit;
    }

    /// <summary>Low-poly капсула с визором.</summary>
    public void Draw(Primitives prim)
    {
        var root = Matrix.CreateRotationY(Facing) * Matrix.CreateTranslation(Position);
        float swing = OnGround ? MathF.Sin(_walkAnim) * 0.15f : 0f;
        // «Капсула»: цилиндрическое тело + полусферы-икосаэдры сверху и снизу
        prim.DrawCylinder(Matrix.CreateScale(0.8f, 0.9f, 0.8f) * Matrix.CreateTranslation(0, 0.85f, 0) * root, Palette.Player);
        prim.DrawCrystal(Matrix.CreateScale(0.8f) * Matrix.CreateTranslation(0, 1.3f, 0) * root, Palette.Player);
        prim.DrawCrystal(Matrix.CreateScale(0.8f) * Matrix.CreateTranslation(0, 0.4f, 0) * root, Palette.Player);
        // Визор — показывает направление
        prim.DrawCube(Matrix.CreateScale(0.5f, 0.14f, 0.12f) * Matrix.CreateTranslation(0, 1.35f, 0.36f) * root, Palette.Visor, 0.8f);
        prim.DrawCube(Matrix.CreateScale(0.18f, 0.12f, 0.22f) * Matrix.CreateTranslation(-0.2f, 0.06f, swing) * root, Palette.Visor);
        prim.DrawCube(Matrix.CreateScale(0.18f, 0.12f, 0.22f) * Matrix.CreateTranslation(0.2f, 0.06f, -swing) * root, Palette.Visor);
    }
}
