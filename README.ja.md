# 井中井 · Nested Tic-Tac-Toe

[简体中文](README.md) · 日本語 · [English](README.en.md)

高校のときにクラスメートと考えて遊んだ三目並べを、Unity 6 で Windows ゲームにしました。大きな盤には九つの小さな盤があります。一台の PC で友達と交互に打つことも、CPU と対局することもできます。

## ダウンロード

[最新リリース](https://github.com/Resuty/Nested-Tic-Tac-Toe/releases/latest)から Windows x64 版の ZIP をダウンロードします。ZIP をすべて展開し、`NestedTicTacToe.exe` を起動してください。

操作はマウスで、オフラインでも遊べます。CPU 戦では先手か後手を選べます。設定では音量、ミュート、表示言語（簡体字中国語、日本語、英語）を変更できます。

## 遊び方

1. O が先手です。初手は空いているマスならどこにでも打てます。次の手は、相手が打ったマスと同じ番号の盤に打ちます。たとえば 8 番のマスに打つと、相手は八番の盤に打ちます。
2. 縦・横・斜めに三つそろえると、その盤を取れます。すべて埋まってもそろわなければ引き分けです。どちらの場合もその盤は終了し、そこに打つよう指定されたときは、まだ打てる盤を一つ選びます。
3. 取った盤が縦・横・斜めに三つ並べば勝ちです。九つの盤がすべて終わっても、どちらも三つ並べられなければ引き分けです。

## Unity で開く

**Unity 6000.6.0f1** でリポジトリのルートを開き、[WellWithinWell.unity](Assets/Scenes/WellWithinWell.unity) を開いて Play を押します。

`Window → General → Test Runner` から Edit Mode と Play Mode のテストを実行できます。Windows Build Support をインストールすると、`井中井 → 构建 Windows 游戏` で `Build/Windows` にビルドできます。

- [技術メモ（中国語）](技术开发说明.md)
- [テスト記録（中国語）](Verification/测试记录.md)

## 素材

木目、駒、木の盤に駒を置く効果音は [ArtWorkshop.cs](Assets/Editor/ArtWorkshop.cs) で生成しています。画面のフォントは Noto Serif SC です。[SIL Open Font License 1.1](NotoSerifSC-OFL.txt) を同梱しています。
