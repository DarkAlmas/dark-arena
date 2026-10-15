using System;
using Microsoft.Xna.Framework;

namespace DarkArena;

/// <summary>Камера от третьего лица, вращается мышью вокруг игрока.</summary>
public sealed class ThirdPersonCamera
{
    public float Yaw;
    public float Pitch = 0.35f;
    public float Distance = 8f;
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public float Sensitivity = 0.004f;

    public void Rotate(Point mouseDelta)
    {
        Yaw -= mouseDelta.X * Sensitivity;
        Pitch = MathHelper.Clamp(Pitch + mouseDelta.Y * Sensitivity, -0.2f, 1.3f);
    }

    public void Update(Vector3 target, float aspect, Level level = null)
    {
        var focus = target + new Vector3(0, 1.4f, 0);
        var offset = new Vector3(MathF.Sin(Yaw) * MathF.Cos(Pitch), MathF.Sin(Pitch), MathF.Cos(Yaw) * MathF.Cos(Pitch)) * Distance;
        float dist = Distance;
        if (level != null)
        {
            // Коллизия камеры: луч от игрока к камере, подтягиваем камеру ближе препятствия
            var ray = new Ray(focus, Vector3.Normalize(offset));
            foreach (var s in level.Solids)
            {
                var grown = new BoundingBox(s.Min - new Vector3(0.25f), s.Max + new Vector3(0.25f));
                if (grown.Contains(focus) != ContainmentType.Disjoint) continue;
                var hit = ray.Intersects(grown);
                if (hit.HasValue && hit.Value < dist) dist = hit.Value;
            }
            dist = MathF.Max(dist, 0.8f);
        }
        View = Matrix.CreateLookAt(focus + Vector3.Normalize(offset) * dist, focus, Vector3.Up);
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), aspect, 0.1f, 200f);
    }
}
