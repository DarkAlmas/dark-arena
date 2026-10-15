using System;

// Запуск: DarkArena [--screenshot путь.png]  (второй режим — для автотеста)
string shot = null;
var a = Environment.GetCommandLineArgs();
for (int i = 0; i < a.Length - 1; i++) if (a[i] == "--screenshot") shot = a[i + 1];
using var game = new DarkArena.ArenaGame(shot);
game.Run();
