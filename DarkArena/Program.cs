using System;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;

// Запуск: DarkArena [--screenshot путь.png] [--bench [N]]
//   --screenshot — автотест: 90 кадров и снимок; --bench — замер FPS N секунд (по умолчанию 10) в bench.txt
// Графика: сначала пробуем MSAA; если устройство не создаётся (например, программный OpenGL), повторяем без MSAA.
// DARKARENA_NO_MSAA=1 — сразу без MSAA.
string shot = null;
float bench = 0f;
var a = Environment.GetCommandLineArgs();
for (int i = 0; i < a.Length; i++)
{
    if (a[i] == "--screenshot" && i + 1 < a.Length) shot = a[i + 1];
    if (a[i] == "--bench")
    {
        bench = 10f;
        if (i + 1 < a.Length && float.TryParse(a[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var n) && n > 0) bench = n;
    }
}

bool forceOff = Environment.GetEnvironmentVariable("DARKARENA_NO_MSAA") != null;
var modes = forceOff
    ? new[] { (msaa: false, note: " (DARKARENA_NO_MSAA)") }
    : new[] { (msaa: true, note: ""), (msaa: false, note: " (fallback)") };

for (int m = 0; m < modes.Length; m++)
{
    var (msaa, note) = modes[m];
    Console.WriteLine($"Graphics: MSAA {(msaa ? "on" : "off")}{note}");
    var game = new DarkArena.ArenaGame(shot, bench, msaa, note);
    try
    {
        game.Run();
        game.Dispose();
        return 0;
    }
    catch (NoSuitableGraphicsDeviceException e) when (m < modes.Length - 1)
    {
        Console.Error.WriteLine($"Graphics: device creation failed with MSAA ({e.InnerException?.GetType().Name ?? e.Message}), retrying without MSAA");
        try { game.Dispose(); } catch { /* окно могло не создаться до конца */ }
    }
}
return 1;
