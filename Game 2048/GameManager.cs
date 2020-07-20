using Hydr10n.Utils;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Game_2048
{
    enum Direction { Left, Up, Right, Down }

    sealed class GameManager
    {
        private const double GameLayoutPaddingScale = 0.06;

        private readonly Grid GameLayout;
        private readonly ViewModel ViewModel;

        private bool isLayoutReady, isMovementFinished = true;
        private int tilesCountPerSide;
        private Tile[,] tiles;

        private string GameSaveKeyScore { get => nameof(GameSaveKeyScore) + tilesCountPerSide; }
        private string GameSaveKeyBestScore { get => nameof(GameSaveKeyBestScore) + tilesCountPerSide; }
        private string GameSaveKeyTiles { get => nameof(GameSaveKeyTiles) + tilesCountPerSide; }

        public GameManager(Grid gameLayout, ViewModel viewModel)
        {
            GameLayout = gameLayout;
            ViewModel = viewModel;
        }

        private void SaveGameProgress(bool reset)
        {
            int score = 0;
            int[,] tilesNumbers = null;
            if (!reset)
            {
                score = ViewModel.Score;
                tilesNumbers = new int[tilesCountPerSide, tilesCountPerSide];
                for (int i = 0; i < tilesCountPerSide; i++)
                    for (int j = 0; j < tilesCountPerSide; j++)
                        if (tiles[i, j] != null)
                            tilesNumbers[i, j] = tiles[i, j].Number;
            }
            AppData<int>.Save(GameSaveKeyScore, score);
            AppData<int>.Save(GameSaveKeyBestScore, ViewModel.BestScore);
            AppData2D<int>.Save(GameSaveKeyTiles, tilesNumbers);
        }

        private bool LoadGameProgress()
        {
            ViewModel.Score = AppData<int>.Load(GameSaveKeyScore, out _);
            ViewModel.BestScore = AppData<int>.Load(GameSaveKeyBestScore, out _);
            int[][] tilesNumbers = AppData2D<int>.Load(GameSaveKeyTiles, out _);
            if (tilesNumbers != null)
                for (int i = 0; i < tilesCountPerSide; i++)
                    for (int j = 0; j < tilesCountPerSide; j++)
                        if (tilesNumbers[i][j] != 0)
                            tiles[i, j] = AddTile(i, j, tilesNumbers[i][j]);
            return tilesNumbers != null;
        }

        private Tile AddTile(int row, int column, int tileNumber) => new Tile(GameLayout, row, column, tileNumber);

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

        private void MoveTile(Cell fromCell, Cell toCell)
        {
            Tile tile = tiles[fromCell.Row, fromCell.Column];
            tile.MoveTo(toCell.Row, toCell.Column);
            tiles[fromCell.Row, fromCell.Column] = null;
            tiles[toCell.Row, toCell.Column] = tile;
        }

        private void MergeTiles(Cell fromCell1, Cell fromCell2, Cell toCell)
        {
            Tile tile = tiles[fromCell2.Row, fromCell2.Column];
            tile.MergeTo(tiles[fromCell1.Row, fromCell1.Column], toCell.Row, toCell.Column);
            tiles[fromCell1.Row, fromCell1.Column] = tiles[fromCell2.Row, fromCell2.Column] = null;
            tiles[toCell.Row, toCell.Column] = tile;
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

        private bool IsGameOver
        {
            get
            {
                int[,] directions = { { 0, 1 }, { 1, 0 } };
                for (int i = 0; i < tilesCountPerSide; i++)
                    for (int j = 0; j < tilesCountPerSide; j++)
                    {
                        if (tiles[i, j] == null)
                            return false;
                        for (int n = 0; n < directions.GetLength(0); n++)
                        {
                            int newI = i + directions[n, 0], newJ = j + directions[n, 1];
                            if (newI >= 0 && newI < tilesCountPerSide && newJ >= 0 && newJ < tilesCountPerSide &&
                                (tiles[newI, newJ] == null || tiles[i, j].Number == tiles[newI, newJ].Number))
                                return false;
                        }
                    }
                return true;
            }
        }

        public void MoveTiles(Direction direction, EventHandler onTilesMerged = null)
        {
            if (!isMovementFinished || ViewModel.GameState != GameState.Started)
                return;
            Tile.Storyboard = new Storyboard();
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
            bool moved = false, won = false, merged = false;
            int count = 0;
            for (int row = 0; row < tilesCountPerSide; row++)
            {
                int next = 0;
                for (int a = 0; a < tilesCountPerSide; a++)
                {
                    Cell cell1 = RotateCell(row, a, angle);
                    if (tiles[cell1.Row, cell1.Column] != null)
                    {
                        bool foundSecond = false, onlyMoved = false;
                        for (int b = a + 1; b < tilesCountPerSide; b++)
                        {
                            Cell cell2 = RotateCell(row, b, angle);
                            if (tiles[cell2.Row, cell2.Column] != null)
                            {
                                foundSecond = true;
                                int tileNumber = tiles[cell1.Row, cell1.Column].Number;
                                if (tileNumber == tiles[cell2.Row, cell2.Column].Number)
                                {
                                    MergeTiles(cell1, cell2, RotateCell(row, next, angle));
                                    moved = merged = true;
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
                                        MoveTile(cell1, RotateCell(row, next, angle));
                                        moved = true;
                                    }
                                    if (b != next + 1)
                                    {
                                        MoveTile(cell2, RotateCell(row, next + 1, angle));
                                        moved = true;
                                    }
                                    a = next;
                                    onlyMoved = true;
                                }
                                next++;
                                break;
                            }
                        }
                        if (!foundSecond && a > 0)
                        {
                            if (onlyMoved)
                                next++;
                            if (a != next)
                            {
                                MoveTile(cell1, RotateCell(row, next, angle));
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
            if (ViewModel.Score > ViewModel.BestScore)
                ViewModel.BestScore = ViewModel.Score;
            Tile.Storyboard.Completed += delegate
            {
                Tile.Storyboard.Stop();
                if (won)
                    ViewModel.GameState = GameState.Won;
                else
                {
                    if (count < tilesCountPerSide)
                        AddRandomTile();
                    if (count >= tilesCountPerSide - 1 && IsGameOver)
                        ViewModel.GameState = GameState.Over;
                }
                isMovementFinished = true;
                if (merged)
                    onTilesMerged?.Invoke(this, EventArgs.Empty);
                SaveGameProgress(ViewModel.GameState != GameState.Started);
            };
            Tile.Storyboard.Begin();
            isMovementFinished = false;
        }

        public void SetGameLayout(int gameLayoutSelectedIndex)
        {
            tilesCountPerSide = gameLayoutSelectedIndex + 4;
            ViewModel.GameState = GameState.NotStarted;
            GameLayout.Children.Clear();
            double tileFullSideLength = GameLayout.ActualHeight / (tilesCountPerSide + GameLayoutPaddingScale * 2);
            GameLayout.Padding = new Thickness(tileFullSideLength * GameLayoutPaddingScale);
            RowDefinitionCollection rowDefinitions = GameLayout.RowDefinitions;
            rowDefinitions.Clear();
            ColumnDefinitionCollection columnDefinitions = GameLayout.ColumnDefinitions;
            columnDefinitions.Clear();
            for (int i = 0; i < tilesCountPerSide; i++)
            {
                rowDefinitions.Add(new RowDefinition { Height = new GridLength(tileFullSideLength) });
                columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileFullSideLength) });
                for (int j = 0; j < tilesCountPerSide; j++)
                    AddTile(i, j, 0);
            }
            tiles = new Tile[tilesCountPerSide, tilesCountPerSide];
            if (LoadGameProgress())
                ViewModel.GameState = GameState.Started;
            isLayoutReady = true;
        }

        public void StartNewGame()
        {
            if (!isLayoutReady)
                return;
            SaveGameProgress(true);
            RemoveAllTiles();
            AddRandomTile();
            AddRandomTile();
            ViewModel.Score = 0;
            ViewModel.GameState = GameState.Started;
        }
    }
}