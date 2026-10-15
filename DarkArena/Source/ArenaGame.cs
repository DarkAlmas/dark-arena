using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DarkArena.UI;

namespace DarkArena;

public enum GameState { MainMenu, Settings, Playing, Paused, GameOver }

/// <summary>Главный класс игры: состояния, цикл обновления и отрисовки.</summary>
public sealed class ArenaGame : Game
{
    private const int TotalCrystals = 30;
    private const float RoundTime = 180f;

    private readonly GraphicsDeviceManager _graphics;
    private readonly string _screenshotPath; // режим smoke-теста: --screenshot <путь>
    private int _frames;

    private Primitives _prim;
    private Ui _ui;
    private Level _level;
    private Player _player;
    private ThirdPersonCamera _camera;
    private readonly List<Collectible> _crystals = new();
    private readonly Particles _particles = new();

    private Menu _mainMenu, _pauseMenu, _settingsMenu, _gameOverMenu;
    private GameState _state = GameState.MainMenu;
    private GameState _settingsReturn = GameState.MainMenu;
    private int _collected;
    private float _timeLeft, _health, _hazardCooldown;
    private bool _showFps;
    private float _fps, _fpsAccum; private int _fpsFrames;
    private bool _invertY;

    private KeyboardState _kb, _pkb;
    private MouseState _ms, _pms;

    public ArenaGame(string screenshotPath = null)
    {
        _screenshotPath = screenshotPath;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            PreferMultiSampling = screenshotPath == null, // в Xvfb MSAA недоступен
            SynchronizeWithVerticalRetrace = true,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Dark Arena";
        Window.AllowUserResizing = true;
    }

    protected override void LoadContent()
    {
        _prim = new Primitives(GraphicsDevice);
        _ui = new Ui(GraphicsDevice, Content.Load<SpriteFont>("Fonts/Hud"), Content.Load<SpriteFont>("Fonts/Title"));
        _level = new Level();
        _camera = new ThirdPersonCamera();
        _mainMenu = new Menu("DARK ARENA", "Играть", "Настройки", "Выход") { Subtitle = "Собери все кристаллы, пока не кончилось время" };
        _pauseMenu = new Menu("ПАУЗА", "Продолжить", "Настройки", "В главное меню");
        _settingsMenu = new Menu("НАСТРОЙКИ", "", "", "Назад");
        _gameOverMenu = new Menu("", "Играть снова", "В главное меню");
        NewRound();
        if (_screenshotPath != null) { StartPlaying(); _camera.Yaw = 0.6f; _camera.Pitch = 0.3f; }
    }

    private void NewRound()
    {
        _player = new Player(new Vector3(0, 0, 15));
        _crystals.Clear();
        var rng = new Random(42);
        while (_crystals.Count < TotalCrystals)
        {
            var p = new Vector3(rng.Next(-18, 19), 0, rng.Next(-18, 19));
            // 1 м над полом; пересэмплируем, если рядом препятствие или опасная зона
            var c = new Vector3(p.X, 1f, p.Z);
            var probe = new BoundingBox(c - new Vector3(0.8f, 1f, 0.8f), c + new Vector3(0.8f, 1.5f, 0.8f));
            bool blocked = false;
            foreach (var s in _level.Solids) if (s.Max.Y > 0.01f && s.Intersects(probe)) blocked = true;
            foreach (var h in _level.Hazards) if (h.Intersects(probe)) blocked = true;
            if (blocked) continue;
            _crystals.Add(new Collectible(c, (float)rng.NextDouble() * 6f));
        }
        _collected = 0; _timeLeft = RoundTime; _health = 1f;
    }

    private void StartPlaying() { _state = GameState.Playing; IsMouseVisible = false; CenterMouse(); }

    private void ToMenu(GameState s) { _state = s; IsMouseVisible = true; }

    private void CenterMouse()
    {
        var vp = GraphicsDevice.Viewport;
        Mouse.SetPosition(vp.Width / 2, vp.Height / 2);
        _ms = _pms = Mouse.GetState();
    }

    private bool Pressed(Keys k) => _kb.IsKeyDown(k) && _pkb.IsKeyUp(k);

    protected override void Update(GameTime gameTime)
    {
        float dt = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.05f);
        _pkb = _kb; _pms = _ms;
        _kb = Keyboard.GetState(); _ms = Mouse.GetState();
        var vp = GraphicsDevice.Viewport.Bounds;

        if (Pressed(Keys.F3)) _showFps = !_showFps;
        _fpsAccum += (float)gameTime.ElapsedGameTime.TotalSeconds; // реальный FPS считается в Draw

        switch (_state)
        {
            case GameState.MainMenu:
                switch (_mainMenu.Update(_ms, _pms, _kb, _pkb, vp))
                {
                    case 0: NewRound(); StartPlaying(); break;
                    case 1: _settingsReturn = GameState.MainMenu; _state = GameState.Settings; break;
                    case 2: Exit(); break;
                }
                break;
            case GameState.Settings:
                _settingsMenu.Items[0] = $"Чувствительность: {_camera.Sensitivity * 1000:0.0}";
                _settingsMenu.Items[1] = $"Инверсия Y: {(_invertY ? "вкл" : "выкл")}";
                switch (_settingsMenu.Update(_ms, _pms, _kb, _pkb, vp))
                {
                    case 0: _camera.Sensitivity = _camera.Sensitivity >= 0.008f ? 0.002f : _camera.Sensitivity + 0.001f; break;
                    case 1: _invertY = !_invertY; break;
                    case 2: _state = _settingsReturn; break;
                }
                if (Pressed(Keys.Escape)) _state = _settingsReturn;
                break;
            case GameState.Paused:
                if (Pressed(Keys.Escape)) { StartPlaying(); break; }
                switch (_pauseMenu.Update(_ms, _pms, _kb, _pkb, vp))
                {
                    case 0: StartPlaying(); break;
                    case 1: _settingsReturn = GameState.Paused; _state = GameState.Settings; break;
                    case 2: ToMenu(GameState.MainMenu); break;
                }
                break;
            case GameState.GameOver:
                switch (_gameOverMenu.Update(_ms, _pms, _kb, _pkb, vp))
                {
                    case 0: NewRound(); StartPlaying(); break;
                    case 1: ToMenu(GameState.MainMenu); break;
                }
                break;
            case GameState.Playing:
                UpdatePlaying(dt, vp);
                break;
        }

        _particles.Update(dt);
        _camera.Update(_player.Position, GraphicsDevice.Viewport.AspectRatio, _level);
        base.Update(gameTime);
    }

    private void UpdatePlaying(float dt, Rectangle vp)
    {
        if (Pressed(Keys.Escape)) { ToMenu(GameState.Paused); return; }

        if (IsActive && _screenshotPath == null)
        {
            var center = new Point(vp.Width / 2, vp.Height / 2);
            var delta = _ms.Position - center;
            if (_invertY) delta.Y = -delta.Y;
            _camera.Rotate(delta);
            Mouse.SetPosition(center.X, center.Y);
            _ms = Mouse.GetState();
        }

        _player.Update(dt, _kb, _pkb, _camera.Yaw, _level);

        foreach (var c in _crystals)
            if (c.Update(dt, _player)) { _collected++; _particles.Burst(c.Position); }

        // Опасные зоны: урон 20% раз в 0.5 с
        _hazardCooldown -= dt;
        var feet = _player.Bounds;
        foreach (var h in _level.Hazards)
            if (feet.Intersects(h) && _hazardCooldown <= 0) { _health -= 0.2f; _hazardCooldown = 0.5f; }

        _timeLeft -= dt;
        if (_collected >= TotalCrystals) EndRound("ПОБЕДА!", $"Все кристаллы собраны, осталось {_timeLeft:0} с");
        else if (_timeLeft <= 0) EndRound("ВРЕМЯ ВЫШЛО", $"Собрано {_collected} из {TotalCrystals}");
        else if (_health <= 0) EndRound("ПОРАЖЕНИЕ", $"Собрано {_collected} из {TotalCrystals}");
    }

    private void EndRound(string title, string subtitle)
    {
        _gameOverMenu.Title = title; _gameOverMenu.Subtitle = subtitle;
        ToMenu(GameState.GameOver);
    }

    protected override void Draw(GameTime gameTime)
    {
        _fpsFrames++;
        if (_fpsAccum >= 0.5f) { _fps = _fpsFrames / _fpsAccum; _fpsFrames = 0; _fpsAccum = 0; }

        var vp = GraphicsDevice.Viewport.Bounds;
        GraphicsDevice.Clear(Palette.SkyTop);

        // Небо: полноэкранный градиент
        _prim.DrawSky(Palette.SkyTop, Palette.SkyBottom, _camera.View, _camera.Projection);

        // 3D: сначала непрозрачное (запись глубины включена)
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullClockwise; // меши заданы против часовой стрелки (CCW = лицевая грань)
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        _prim.SetCamera(_camera.View, _camera.Projection);
        _level.Draw(_prim);
        foreach (var c in _crystals) c.Draw(_prim);
        _player.Draw(_prim);
        _particles.Draw(_prim);

        // Затем полупрозрачное: сетка (без записи глубины)
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        _level.DrawGrid(_prim);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        // 2D-интерфейс
        _ui.Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        if (_state != GameState.MainMenu)
            Hud.Draw(_ui, vp, _collected, TotalCrystals, _timeLeft, _health, _showFps, _fps);
        switch (_state)
        {
            case GameState.MainMenu: _mainMenu.Draw(_ui, vp); break;
            case GameState.Settings: _settingsMenu.Draw(_ui, vp); break;
            case GameState.Paused: _pauseMenu.Draw(_ui, vp); break;
            case GameState.GameOver: _gameOverMenu.Draw(_ui, vp); break;
        }
        if (_state == GameState.MainMenu && _showFps) _ui.Text($"FPS: {_fps:0}", new Vector2(Ui.Margin), Color.White);
        _ui.Batch.End();

        base.Draw(gameTime);

        if (_screenshotPath != null && ++_frames == 90) SaveScreenshot();
    }

    private void SaveScreenshot()
    {
        int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
        var data = new Color[w * h];
        GraphicsDevice.GetBackBufferData(data);
        using var tex = new Texture2D(GraphicsDevice, w, h);
        tex.SetData(data);
        using var fs = File.Create(_screenshotPath);
        tex.SaveAsPng(fs, w, h);
        Console.WriteLine($"Screenshot saved: {_screenshotPath}");
        Exit();
    }
}
