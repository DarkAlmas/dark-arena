using System;
using System.Globalization;

// Запуск: DarkArena [--screenshot путь.png] [--bench [N]]
//   --screenshot — автотест: 90 кадров и снимок; --bench — замер FPS N секунд (по умолчанию 10) в bench.txt
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
using var game = new DarkArena.ArenaGame(shot, bench);
game.Run();
