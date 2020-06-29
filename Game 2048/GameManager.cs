using System;
using Windows.UI.Xaml.Media.Animation;

namespace Game_2048
{
    partial class MainPage
    {
        private const int GameSaveKeyScoreIndex = 0, GameSaveKeyBestScoreIndex = 1, GameSaveKeyTilesNumbersIndex = 2;

        private static readonly string[,] GameSaveKeys = {
            {"Layout4Score", "Layout4BestScore", "Layout4TilesNumbers"},
            {"Layout5Score", "Layout5BestScore", "Layout5TilesNumbers"},
            {"Layout6Score", "Layout6BestScore", "Layout6TilesNumbers"}
        };

        private readonly GameSave GameSave = new GameSave();

        private bool isMovementFinished = true;
        private Tile[,] tiles;

        private Tile AddTile(int row, int column, int tileNumber) => new Tile(GameLayout, row, column, tileNumber, tileFullSideLength, repositionAnimationDurationUnit, TileScale);

        private void AddRandomTile()
        {
            Random random = new Random();
            int i, j;
            while (tiles[i = random.Next(tilesCountPerSide), j = random.Next(tilesCountPerSide)] != null)
                ;
            tiles[i, j] = AddTile(i, j, 2);
        }

        private void RemoveAllTiles()
        {
            for (int i = 0; i < tilesCountPerSide; i++)
                for (int j = 0; j < tilesCountPerSide; j++)
                    if (tiles[i, j] != null)
                    {
                        tiles[i, j].RemoveSelf();
                        tiles[i, j] = null;
                    }
        }

        private void MoveTile(Cell fromCell, Cell toCell, Storyboard storyboard)
        {
            Tile tile = tiles[fromCell.Row, fromCell.Column];
            tile.MoveTo(toCell.Row, toCell.Column, storyboard);
            tiles[fromCell.Row, fromCell.Column] = null;
            tiles[toCell.Row, toCell.Column] = tile;
        }

        private void MergeTiles(Cell fromCell1, Cell fromCell2, Cell toCell, Storyboard storyboard)
        {
            Tile fromTile2 = tiles[fromCell2.Row, fromCell2.Column];
            fromTile2.Merge(tiles[fromCell1.Row, fromCell1.Column], toCell, storyboard);
            tiles[fromCell1.Row, fromCell1.Column] = null;
            tiles[fromCell2.Row, fromCell2.Column] = null;
            tiles[toCell.Row, toCell.Column] = fromTile2;
        }

        private Cell RotateCell(int row, int column, int angle)
        {
            if (angle == 0)
                return new Cell(row, column);
            int offsetRow = tilesCountPerSide - 1, offsetColumn = offsetRow;
            if (angle == 90)
                offsetRow = 0;
            else if (angle == 270)
                offsetColumn = 0;
            double radians = Math.PI * angle / 180;
            int cos = (int)Math.Cos(radians), sin = (int)Math.Sin(radians);
            return new Cell(offsetRow + column * sin + row * cos, offsetColumn + column * cos - row * sin);
        }

        private bool IsGameOver()
        {
            int[,] directions = { { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 0 } };
            for (int i = 0; i < tilesCountPerSide; i++)
                for (int j = 0; j < tilesCountPerSide; j++)
                {
                    if (tiles[i, j] == null)
                        return false;
                    for (int n = 0; n < 4; n++)
                    {
                        int newI = i + directions[n, 0], newJ = j + directions[n, 1];
                        if (newI >= 0 && newI < tilesCountPerSide && newJ >= 0 && newJ < tilesCountPerSide &&
                            (tiles[newI, newJ] == null || tiles[i, j].Number == tiles[newI, newJ].Number))
                            return false;
                    }
                }
            return true;
        }

        private void MoveTiles(Direction direction)
        {
            if (!isMovementFinished || ViewModel.GameState != GameState.Started)
                return;
            int angle;
            switch (direction)
            {
                case Direction.Left:
                    angle = 0;
                    break;
                case Direction.Up:
                    angle = 90;
                    break;
                case Direction.Right:
                    angle = 180;
                    break;
                case Direction.Down:
                    angle = 270;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            bool moved = false, won = false;
            int count = 0;
            Storyboard storyboard = new Storyboard();
            for (int row = 0; row < tilesCountPerSide; row++)
            {
                int next = 0;
                for (int a = 0; a < tilesCountPerSide; a++)
                {
                    Cell cell1 = RotateCell(row, a, angle);
                    if (tiles[cell1.Row, cell1.Column] != null)
                    {
                        bool foundSecond = false, merged = true;
                        for (int b = a + 1; b < tilesCountPerSide; b++)
                        {
                            Cell cell2 = RotateCell(row, b, angle);
                            if (tiles[cell2.Row, cell2.Column] != null)
                            {
                                foundSecond = true;
                                int tileNumber = tiles[cell1.Row, cell1.Column].Number;
                                if (tileNumber == tiles[cell2.Row, cell2.Column].Number)
                                {
                                    MergeTiles(cell1, cell2, RotateCell(row, next, angle), storyboard);
                                    moved = true;
                                    a = b;
                                    tileNumber <<= 1;
                                    ViewModel.Score += tileNumber;
                                    if (tileNumber >= 2048)
                                        won = true;
                                }
                                else
                                {
                                    if (a != next)
                                    {
                                        MoveTile(cell1, RotateCell(row, next, angle), storyboard);
                                        moved = true;
                                    }
                                    if (b != next + 1)
                                    {
                                        MoveTile(cell2, RotateCell(row, next + 1, angle), storyboard);
                                        moved = true;
                                    }
                                    a = next;
                                    merged = false;
                                }
                                next++;
                                break;
                            }
                        }
                        if (!foundSecond && a > 0)
                        {
                            if (!merged)
                                next++;
                            if (a != next)
                            {
                                MoveTile(cell1, RotateCell(row, next, angle), storyboard);
                                moved = true;
                            }
                            break;
                        }
                    }
                }
                Cell cell = RotateCell(row, tilesCountPerSide - 1, angle);
                if (tiles[cell.Row, cell.Column] != null)
                    count++;
            }
            if (!moved)
                return;
            storyboard.Completed += delegate
            {
                storyboard.Stop();
                isMovementFinished = true;
                if (ViewModel.Score > ViewModel.BestScore)
                    ViewModel.BestScore = ViewModel.Score;
                if (won)
                {
                    ViewModel.GameState = GameState.Won;
                    return;
                }
                if (count < tilesCountPerSide)
                    AddRandomTile();
                if (count >= tilesCountPerSide - 1 && IsGameOver())
                    ViewModel.GameState = GameState.Over;
            };
            storyboard.Begin();
            isMovementFinished = false;
        }

        private void SaveGameProgress()
        {
            int score = 0;
            int[,] tilesNumbers = null;
            if (ViewModel.GameState == GameState.Started)
            {
                score = ViewModel.Score;
                tilesNumbers = new int[tilesCountPerSide, tilesCountPerSide];
                for (int i = 0; i < tilesCountPerSide; i++)
                    for (int j = 0; j < tilesCountPerSide; j++)
                        if (tiles[i, j] != null)
                            tilesNumbers[i, j] = tiles[i, j].Number;
            }
            GameSave.SaveData(GameSaveKeys[gameSaveKeysIndex, GameSaveKeyScoreIndex], score);
            GameSave.SaveData(GameSaveKeys[gameSaveKeysIndex, GameSaveKeyBestScoreIndex], Math.Max(ViewModel.Score, ViewModel.BestScore));
            GameSave.SaveData(GameSaveKeys[gameSaveKeysIndex, GameSaveKeyTilesNumbersIndex], tilesNumbers);
        }

        private bool LoadGameProgress()
        {
            ViewModel.Score = GameSave.LoadIntData(GameSaveKeys[gameSaveKeysIndex, GameSaveKeyScoreIndex], out _);
            ViewModel.BestScore = GameSave.LoadIntData(GameSaveKeys[gameSaveKeysIndex, GameSaveKeyBestScoreIndex], out _);
            int[][] tileNumbers = GameSave.LoadInt2DData(GameSaveKeys[gameSaveKeysIndex, GameSaveKeyTilesNumbersIndex]);
            if (tileNumbers != null)
                for (int i = 0; i < tilesCountPerSide; i++)
                    for (int j = 0; j < tilesCountPerSide; j++)
                        if (tileNumbers[i][j] != 0)
                            tiles[i, j] = AddTile(i, j, tileNumbers[i][j]);
            return tileNumbers != null;
        }

        private bool InitializeGameLayout()
        {
            for (int i = 0; i < tilesCountPerSide; i++)
                for (int j = 0; j < tilesCountPerSide; j++)
                    AddTile(i, j, 0);
            tiles = new Tile[tilesCountPerSide, tilesCountPerSide];
            return LoadGameProgress();
        }

        private void StartNewGame()
        {
            RemoveAllTiles();
            AddRandomTile();
            AddRandomTile();
            ViewModel.Score = 0;
            ViewModel.GameState = GameState.Started;
        }
    }
}