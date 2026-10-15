# Dark Arena

Небольшая 3D-игра на арене: собери 30 кристаллов за 3 минуты и не наступай на красные зоны.
C# / .NET 8 / MonoGame 3.8 (DesktopGL) — один проект собирается и на Windows, и на Linux.

## Открыть в Visual Studio 2022
1. В Visual Studio Installer должна быть рабочая нагрузка **«Разработка классических приложений .NET»** (.NET desktop development) и .NET 8 SDK (подойдёт и SDK 10 с установленным runtime 8).
2. Откройте `DarkArena.sln`.
3. Нажмите **F5** (или «Запуск»). При первой сборке скачиваются пакеты NuGet и инструмент `mgcb` (прописан в `DarkArena/.config/dotnet-tools.json`) — это может занять несколько минут.

## Сборка из командной строки
```
dotnet build
dotnet run --project DarkArena
```

## Бенчмарк (`--bench`)
```
DarkArena.exe --bench        # 10 секунд
DarkArena.exe --bench 30     # 30 секунд
dotnet run --project DarkArena -c Release -- --bench 10
```
Игра сама запускает раунд и N секунд (по умолчанию 10) рендерит обычную сцену: игрок идёт по кругу, камера облетает его, вылетают частицы. VSync и фиксированный шаг времени на время замера выключены, первая секунда (прогрев) не учитывается. Потом игра закрывается, а результат печатается в консоль и записывается в `bench.txt` рядом с exe (`bin/<Конфигурация>/net8.0/bench.txt`):
```
Dark Arena bench
duration_s: 10.00
frames: 6240
avg_fps: 624.0          средний FPS
min_fps: 45.2           FPS самого медленного кадра
low1_fps: 78.8          1% low: средний FPS по 1% самых медленных кадров
frame_time_max_ms: 22.12  самый долгий кадр, мс
gpu: Intel(R) UHD Graphics 610   видеокарта
resolution: 1280x720
msaa: 4x
vsync: off, fixed_timestep: off
```
Переменная окружения `DARKARENA_NO_MSAA=1` выключает сглаживание (нужно для запуска без видеокарты, например в Xvfb).

## Управление
| Клавиша | Действие |
|---|---|
| W A S D | движение |
| Мышь | вращение камеры |
| Пробел | прыжок |
| Esc | пауза (Продолжить / Настройки / В главное меню) |
| F3 | показать/скрыть FPS |
| Стрелки/Enter | навигация по меню |

## Структура
```
DarkArena/
  Program.cs               точка входа (--screenshot путь.png — автотест: 90 кадров и снимок; --bench N — замер FPS)
  Source/ArenaGame.cs      состояния игры, цикл, отрисовка
  Source/Player.cs         игрок: движение, прыжок, гравитация, коллизии AABB
  Source/ThirdPersonCamera.cs  камера от третьего лица
  Source/Level.cs          арена, стены, препятствия, опасные зоны
  Source/Collectible.cs    кристаллы
  Source/Particles.cs      частицы при подборе
  Source/Primitives.cs     low-poly меши (куб, икосаэдр, призма) + BasicEffect
  Source/Palette.cs        цвета от Design Bot
  Source/UI/               меню, HUD, отрисовка 2D
  Content/Fonts/           шрифт Inter (SIL OFL 1.1, лицензия рядом)
```
Модели строятся в коде, текстур и сторонних моделей нет.

## Лицензии
Шрифт Inter © The Inter Project Authors, SIL Open Font License 1.1 — `DarkArena/Content/Fonts/Inter-OFL-LICENSE.txt`.
