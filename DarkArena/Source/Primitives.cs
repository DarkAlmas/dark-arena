using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkArena;

/// <summary>Генерация простых мешей (куб, октаэдр) и их отрисовка через BasicEffect.</summary>
public sealed class Primitives
{
    private readonly GraphicsDevice _gd;
    public BasicEffect Effect { get; }
    private readonly VertexBuffer _cube;
    private readonly VertexBuffer _crystal;
    private readonly VertexBuffer _cylinder;

    public Primitives(GraphicsDevice gd)
    {
        _gd = gd;
        Effect = new BasicEffect(gd) { LightingEnabled = true, PreferPerPixelLighting = true };
        Effect.DirectionalLight0.Enabled = true;
        Effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.4f, -1f, -0.6f));
        Effect.DirectionalLight0.DiffuseColor = new Vector3(0.75f, 0.75f, 0.8f);
        Effect.DirectionalLight0.SpecularColor = new Vector3(0.15f);
        Effect.DirectionalLight1.Enabled = false;
        Effect.DirectionalLight2.Enabled = false;
        Effect.AmbientLightColor = new Vector3(0.3f, 0.34f, 0.48f); // голубоватый ambient
        Effect.FogEnabled = true;
        Effect.FogColor = Palette.Fog.ToVector3();
        Effect.FogStart = 25f; Effect.FogEnd = 60f;
        _cube = Build(gd, CubeVerts());
        _crystal = Build(gd, IcosaVerts());
        _cylinder = Build(gd, CylinderVerts(8));
    }

    private static VertexBuffer Build(GraphicsDevice gd, VertexPositionNormalTexture[] v)
    {
        var vb = new VertexBuffer(gd, typeof(VertexPositionNormalTexture), v.Length, BufferUsage.WriteOnly);
        vb.SetData(v);
        return vb;
    }

    private static void Tri(List<VertexPositionNormalTexture> l, Vector3 a, Vector3 b, Vector3 c)
    {
        var n = Vector3.Normalize(Vector3.Cross(b - a, c - a));
        l.Add(new(a, n, Vector2.Zero)); l.Add(new(b, n, Vector2.Zero)); l.Add(new(c, n, Vector2.Zero));
    }

    /// <summary>Единичный куб от -0.5 до 0.5.</summary>
    private static VertexPositionNormalTexture[] CubeVerts()
    {
        var l = new List<VertexPositionNormalTexture>();
        Vector3[] p =
        {
            new(-.5f,-.5f,-.5f), new(.5f,-.5f,-.5f), new(.5f,.5f,-.5f), new(-.5f,.5f,-.5f),
            new(-.5f,-.5f,.5f), new(.5f,-.5f,.5f), new(.5f,.5f,.5f), new(-.5f,.5f,.5f),
        };
        int[][] faces = { new[]{4,5,6,7}, new[]{1,0,3,2}, new[]{5,1,2,6}, new[]{0,4,7,3}, new[]{7,6,2,3}, new[]{0,1,5,4} };
        foreach (var f in faces)
        {
            Tri(l, p[f[0]], p[f[1]], p[f[2]]);
            Tri(l, p[f[0]], p[f[2]], p[f[3]]);
        }
        return l.ToArray();
    }

    /// <summary>Икосаэдр радиуса 0.5 с плоскими нормалями (low-poly).</summary>
    private static VertexPositionNormalTexture[] IcosaVerts()
    {
        float t = (1f + MathF.Sqrt(5f)) / 2f;
        Vector3[] v =
        {
            new(-1,t,0), new(1,t,0), new(-1,-t,0), new(1,-t,0),
            new(0,-1,t), new(0,1,t), new(0,-1,-t), new(0,1,-t),
            new(t,0,-1), new(t,0,1), new(-t,0,-1), new(-t,0,1),
        };
        for (int i = 0; i < v.Length; i++) v[i] = Vector3.Normalize(v[i]) * 0.5f;
        int[] f =
        {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1,
        };
        var l = new List<VertexPositionNormalTexture>();
        for (int i = 0; i < f.Length; i += 3) Tri(l, v[f[i]], v[f[i + 1]], v[f[i + 2]]);
        return l.ToArray();
    }

    /// <summary>Восьмигранная призма радиуса 0.5, высоты 1 (без крышек).</summary>
    private static VertexPositionNormalTexture[] CylinderVerts(int n)
    {
        var l = new List<VertexPositionNormalTexture>();
        for (int i = 0; i < n; i++)
        {
            float a0 = MathHelper.TwoPi * i / n, a1 = MathHelper.TwoPi * (i + 1) / n;
            var p0 = new Vector3(MathF.Sin(a0) * .5f, 0, MathF.Cos(a0) * .5f);
            var p1 = new Vector3(MathF.Sin(a1) * .5f, 0, MathF.Cos(a1) * .5f);
            var up = new Vector3(0, .5f, 0);
            Tri(l, p0 - up, p1 - up, p1 + up);
            Tri(l, p0 - up, p1 + up, p0 + up);
        }
        return l.ToArray();
    }

    public void DrawCylinder(Matrix world, Color color) => Draw(_cylinder, world, color, 0f, true);

    private VertexPositionColor[] _sky;
    private BasicEffect _skyEffect;

    /// <summary>Полноэкранный квад с вертикальным градиентом (верх → горизонт).</summary>
    public void DrawSky(Color top, Color horizon)
    {
        _skyEffect ??= new BasicEffect(_gd) { VertexColorEnabled = true, World = Matrix.Identity, View = Matrix.Identity, Projection = Matrix.Identity };
        _sky ??= new VertexPositionColor[4];
        _sky[0] = new(new Vector3(-1, 1, 0), top); _sky[1] = new(new Vector3(1, 1, 0), top);
        _sky[2] = new(new Vector3(-1, -1, 0), horizon); _sky[3] = new(new Vector3(1, -1, 0), horizon);
        _gd.DepthStencilState = DepthStencilState.None;
        _gd.BlendState = BlendState.Opaque;
        _gd.RasterizerState = RasterizerState.CullNone;
        _skyEffect.CurrentTechnique.Passes[0].Apply();
        _gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, _sky, 0, 2);
    }

    public void SetCamera(Matrix view, Matrix proj)
    {
        Effect.View = view; Effect.Projection = proj;
        // Свет «из-за плеча камеры»: направление взгляда, наклонённое вниз
        var fwd = Matrix.Invert(view).Forward;
        fwd.Y = 0;
        if (fwd.LengthSquared() < 1e-4f) fwd = Vector3.Forward;
        fwd.Normalize();
        Effect.DirectionalLight0.Direction = Vector3.Normalize(fwd + new Vector3(0.35f, -1.1f, 0) );
    }

    public void DrawCube(Matrix world, Color color, float emissive = 0f, bool lit = true, float alpha = 1f) => Draw(_cube, world, color, emissive, lit, alpha);
    public void DrawCrystal(Matrix world, Color color, float emissive = 0f) => Draw(_crystal, world, color, emissive, true);

    private void Draw(VertexBuffer vb, Matrix world, Color color, float emissive, bool lit, float alpha = 1f)
    {
        Effect.LightingEnabled = lit;
        Effect.Alpha = alpha;
        Effect.World = world;
        Effect.DiffuseColor = color.ToVector3();
        Effect.EmissiveColor = color.ToVector3() * emissive;
        _gd.SetVertexBuffer(vb);
        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _gd.DrawPrimitives(PrimitiveType.TriangleList, 0, vb.VertexCount / 3);
        }
    }
}
