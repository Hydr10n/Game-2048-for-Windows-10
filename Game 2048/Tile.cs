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
    }

    class Tile : Button
    {
        private const int TileForegroundIndex = 0, TileBackgroundIndex = 1;
        private const double MinScale = 0.3, MaxScale = 1.2, ScaleAnimationDuration = 150;

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

        private readonly double initialParentSideLength, initialFullSideLength, repositionAnimationDurationUnit, scale;
        private readonly Grid parent;

        private double fullSideLength, maxFontSize, fontScale = 1;

        private int number;
        public int Number
        {
            get => number;
            private set
            {
                number = value;
                if (value != 0)
                    Content = value;
                if (value > 1000)
                    fontScale = 0.55;
                else if (value > 100)
                    fontScale = 0.75;
                int tileColorIndex = GetTileColorIndex(Number);
                Background = new SolidColorBrush(TileColors[tileColorIndex, TileBackgroundIndex]);
                Foreground = new SolidColorBrush(TileColors[tileColorIndex, TileForegroundIndex]);
            }
        }

        public Tile(Grid parent, int row, int column, int number, double fullSideLength, double repositionAnimationDurationUnit, double scale)
        {
            RequestedTheme = ElementTheme.Dark;
            Style = (Style)Resources["ButtonRevealStyle"];
            Opacity = 0.85;
            IsTabStop = false;
            Padding = new Thickness();
            CornerRadius = new CornerRadius(fullSideLength * scale * 0.08);
            FontWeight = FontWeights.Bold;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            parent.Children.Add(this);
            SetCell(row, column);
            initialParentSideLength = Math.Min(parent.ActualWidth, parent.ActualHeight);
            this.parent = parent;
            Number = number;
            this.fullSideLength = initialFullSideLength = fullSideLength;
            maxFontSize = fullSideLength * 0.7;
            this.repositionAnimationDurationUnit = repositionAnimationDurationUnit;
            this.scale = scale;
            BeginScaleAnimation(MinScale, scale, false);
            parent.SizeChanged += Parent_SizeChanged;
        }

        private void Parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            fullSideLength = initialFullSideLength * e.NewSize.Height / initialParentSideLength;
            maxFontSize = fullSideLength * 0.6;
            FontSize = fontScale * maxFontSize;
            Width = Height = fullSideLength * scale;
            CornerRadius = new CornerRadius(Height * 0.08);
        }

        private int GetTileColorIndex(int tileValue)
        {
            if (tileValue == 0)
                return 0;
            else if (tileValue < 0)
                throw new ArgumentException();
            int index = (int)(Math.Log(Math.Abs(tileValue)) / Math.Log(2));
            if (index >= TileColors.GetLength(0))
                throw new ArgumentException();
            return index;
        }

        private void SetCell(int row, int column)
        {
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
        }

        private void BeginScaleAnimation(double fromScale, double toScale, bool autoReverse)
        {
            double fromSideLength = fullSideLength * fromScale, toSideLength = fullSideLength * toScale;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(autoReverse ? ScaleAnimationDuration / 2 : ScaleAnimationDuration));
            DoubleAnimation doubleAnimation = new DoubleAnimation() { From = fromSideLength, To = toSideLength, Duration = duration, EnableDependentAnimation = true }, doubleAnimation2 = new DoubleAnimation() { From = fromSideLength, To = toSideLength, Duration = duration, EnableDependentAnimation = true }, doubleAnimation3 = new DoubleAnimation() { From = fromScale * (fontScale * maxFontSize), To = toScale * (fontScale * maxFontSize), Duration = duration, EnableDependentAnimation = true };
            Storyboard.SetTargetProperty(doubleAnimation, "Width");
            Storyboard.SetTargetProperty(doubleAnimation2, "Height");
            Storyboard.SetTargetProperty(doubleAnimation3, "FontSize");
            Storyboard.SetTarget(doubleAnimation, this);
            Storyboard.SetTarget(doubleAnimation2, this);
            Storyboard.SetTarget(doubleAnimation3, this);
            Storyboard storyboard = new Storyboard() { AutoReverse = autoReverse };
            storyboard.Children.Add(doubleAnimation);
            storyboard.Children.Add(doubleAnimation2);
            storyboard.Children.Add(doubleAnimation3);
            storyboard.Begin();
        }

        public void RemoveSelf() => parent.Children.Remove(this);

        private void MoveTo(int row, int column, Storyboard storyboard, EventHandler<object> animationCompleted)
        {
            int rowDistance = row - Grid.GetRow(this), columnDistance = column - Grid.GetColumn(this);
            TranslateTransform translateTransform = new TranslateTransform();
            RenderTransform = translateTransform;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(repositionAnimationDurationUnit * Math.Max(Math.Abs(rowDistance), Math.Abs(columnDistance))));
            DoubleAnimation doubleAnimation = new DoubleAnimation() { To = fullSideLength * columnDistance, Duration = duration, EnableDependentAnimation = true }, doubleAnimation2 = new DoubleAnimation() { To = fullSideLength * rowDistance, Duration = duration };
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            Storyboard.SetTargetProperty(doubleAnimation2, "Y");
            Storyboard.SetTarget(doubleAnimation, translateTransform);
            Storyboard.SetTarget(doubleAnimation2, translateTransform);
            storyboard.Completed += delegate
            {
                SetCell(row, column);
                animationCompleted?.Invoke(null, null);
            };
            storyboard.Children.Add(doubleAnimation);
            storyboard.Children.Add(doubleAnimation2);
        }

        public void MoveTo(int row, int column, Storyboard storyboard) => MoveTo(row, column, storyboard, null);

        public void Merge(Tile tile, Cell toCell, Storyboard storyboard)
        {
            tile.MoveTo(toCell.Row, toCell.Column, storyboard, null);
            MoveTo(toCell.Row, toCell.Column, storyboard, delegate
            {
                tile.RemoveSelf();
                Number <<= 1;
                BeginScaleAnimation(scale, MaxScale, true);
            });
        }
    }
}