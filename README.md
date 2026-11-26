# 井中井 · Nested Tic-Tac-Toe

简体中文 · [日本語](README.ja.md) · [English](README.en.md)

《井中井》是我高中时和同学一起设计、玩过的一种井字棋。棋盘上有九个大区，每个大区都是一盘小井字棋。我用 Unity 6 把它做成了 Windows 游戏，可以和朋友共用一台电脑下，也可以和电脑下。

## 下载

从 [最新 Release](https://github.com/Resuty/Nested-Tic-Tac-Toe/releases/latest) 下载 Windows x64 ZIP，解压整个压缩包后双击 `NestedTicTacToe.exe`。

用鼠标落子，游戏可以离线运行。和电脑下时可选先手或后手。「设置」里可以调音量、静音，也可以切换简体中文、日本語和 English。

## 怎么玩

1. O 先下，第一手任选空格。之后，你下在哪个编号的小格，对手就要去哪一个大区。比如下在 8 号格，对手下一手就去八号大区。
2. 在一个大区里横、竖或斜连成三个，就拿下这个大区。下满还没三连就算和棋。两种情况都封盘。要去的大区封盘了，就另选一个还能下的。
3. 拿下的大区连成三个就赢。九个大区都封盘还没人连成线，这局就是和棋。

## 打开工程

用 **Unity 6000.6.0f1** 打开仓库根目录，打开 [WellWithinWell.unity](Assets/Scenes/WellWithinWell.unity) 后点 Play。

在 `Window → General → Test Runner` 可以运行 Edit Mode 和 Play Mode 测试。安装 Windows Build Support 后，用 `井中井 → 构建 Windows 游戏` 生成 `Build/Windows`。

- [中文技术开发说明](技术开发说明.md)
- [测试与构建记录](Verification/测试记录.md)

## 资源

木纹、棋子和木棋敲击声由项目内的 [ArtWorkshop.cs](Assets/Editor/ArtWorkshop.cs) 生成。界面字体使用 Noto Serif SC，随附 [SIL Open Font License 1.1](NotoSerifSC-OFL.txt)。
