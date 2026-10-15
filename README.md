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
  Program.cs               точка входа (--screenshot путь.png — автотест: 90 кадров и снимок)
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
