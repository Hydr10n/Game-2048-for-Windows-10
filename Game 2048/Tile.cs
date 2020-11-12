using System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Game_2048
{
    struct Cell
    {
        public int Row, Column;

        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static bool operator ==(Cell a, Cell b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return a.Row == b.Row && a.Column == b.Column;
        }

        public static bool operator !=(Cell a, Cell b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return this == (Cell)obj;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    sealed class Tile : Button
    {
        private const int TileForegroundIndex = 0, TileBackgroundIndex = 1;
        private const double MinSizeScale = 0.5, MaxSizeScale = 1.2, ScaleDuration = 150, Layout4MovementDurationPerCell = 50;

        private static readonly Color[,] TileColors = new Color[,] {    // [0]: text color; [1]: background color
            { new Color(), Color.FromArgb(0xff, 0xcd, 0xc1, 0xb4) },                               // empty
            { Color.FromArgb(0xff, 0x77, 0x6e, 0x65), Color.FromArgb(0xff, 0xee, 0xe4, 0xda) },    // 2
            { Color.FromArgb(0xff, 0x77, 0x6e, 0x65), Color.FromArgb(0xff, 0xed, 0xe0, 0xc8) },    // 4
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xf2, 0xb1, 0x79) },    // 8
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xf5, 0x95, 0x63) },    // 16
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xf6, 0x7c, 0x5f) },    // 32
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xf6, 0x5e, 0x3b) },    // 64
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xed, 0xcf, 0x72) },    // 128
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xed, 0xcc, 0x61) },    // 256
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xed, 0xc8, 0x50) },    // 512
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xed, 0xc5, 0x3f) },    // 1024
            { Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2), Color.FromArgb(0xff, 0xed, 0xc2, 0x2e) }     // 2048
        };

        private readonly double fullSideLength, movementDurationPerCell;
        private readonly TextBlock textBlock;

        private int number;
        public int Number
        {
            get => number;
            private set
            {
                number = value;
                if (textBlock != null)
                    textBlock.Text = value.ToString();
                int tileColorIndex = GetTileColorIndex(value);
                Background = new SolidColorBrush(TileColors[tileColorIndex, TileBackgroundIndex]);
                Foreground = new SolidColorBrush(TileColors[tileColorIndex, TileForegroundIndex]);
            }
        }

        private Cell cell;

        public static Storyboard Storyboard { get; set; }

        public Tile(Grid parent, int row, int column, int number)
        {
            Style = (Style)Resources["ButtonRevealStyle"];
            Opacity = 0.85;
            IsTabStop = false;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Padding = new Thickness(0, 0, 0, 6);
            fullSideLength = parent.RowDefinitions[0].Height.Value;
            Width = Height = fullSideLength - parent.Padding.Top * 2;
            CornerRadius = new CornerRadius(fullSideLength * 0.07);
            if (number != 0)
            {
                textBlock = new TextBlock() { FontSize = fullSideLength * 0.6, FontWeight = FontWeights.Bold, };
                Content = new Viewbox() { Child = textBlock };
            }
            parent.Children.Add(this);
            SetCell(row, column);
            cell = new Cell(row, column);
            Number = number;
            movementDurationPerCell = Layout4MovementDurationPerCell * 4 / parent.RowDefinitions.Count;
            AnimateScale(MinSizeScale, 1, false);
        }

        private int GetTileColorIndex(int number)
        {
            if (number == 0)
                return 0;
            return (int)(Math.Log(Math.Abs(number)) / Math.Log(2));
        }

        private void SetCell(int row, int column)
        {
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
        }

        private void AnimateScale(double fromScale, double toScale, bool autoReverse)
        {
            RenderTransform = new ScaleTransform { CenterX = Width / 2, CenterY = Height / 2 };
            Duration duration = new Duration(TimeSpan.FromMilliseconds(autoReverse ? ScaleDuration / 2 : ScaleDuration));
            DoubleAnimation doubleAnimation = new DoubleAnimation { From = fromScale, To = toScale, Duration = duration, EnableDependentAnimation = true }, doubleAnimation2 = new DoubleAnimation { From = fromScale, To = toScale, Duration = duration, EnableDependentAnimation = true };
            Storyboard.SetTargetProperty(doubleAnimation, "ScaleX");
            Storyboard.SetTargetProperty(doubleAnimation2, "ScaleY");
            Storyboard.SetTarget(doubleAnimation, RenderTransform);
            Storyboard.SetTarget(doubleAnimation2, RenderTransform);
            Storyboard storyboard = new Storyboard { AutoReverse = autoReverse };
            storyboard.Children.Add(doubleAnimation);
            storyboard.Children.Add(doubleAnimation2);
            storyboard.Begin();
        }

        public void RemoveSelf() => (Parent as Panel).Children.Remove(this);

        private void MoveTo(int row, int column, EventHandler<object> animationCompleted)
        {
            RenderTransform = new TranslateTransform();
            int rowDistance = row - cell.Row, columnDistance = column - cell.Column;
            cell = new Cell(row, column);
            Duration duration = new Duration(TimeSpan.FromMilliseconds(movementDurationPerCell * Math.Max(Math.Abs(rowDistance), Math.Abs(columnDistance))));
            DoubleAnimation doubleAnimation = new DoubleAnimation { To = fullSideLength * columnDistance, Duration = duration, EnableDependentAnimation = true }, doubleAnimation2 = new DoubleAnimation { To = fullSideLength * rowDistance, Duration = duration };
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            Storyboard.SetTargetProperty(doubleAnimation2, "Y");
            Storyboard.SetTarget(doubleAnimation, RenderTransform);
            Storyboard.SetTarget(doubleAnimation2, RenderTransform);
            Storyboard.Completed += delegate
            {
                SetCell(row, column);
                animationCompleted?.Invoke(null, null);
            };
            Storyboard.Children.Add(doubleAnimation);
            Storyboard.Children.Add(doubleAnimation2);
        }

        public void MoveTo(int row, int column) => MoveTo(row, column, null);

        public void MergeTo(Tile tile, int row, int column)
        {
            number <<= 1;
            tile.MoveTo(row, column, null);
            MoveTo(row, column, delegate
            {
                tile.RemoveSelf();
                Number = number;
                AnimateScale(1, MaxSizeScale, true);
            });
        }
    }
}