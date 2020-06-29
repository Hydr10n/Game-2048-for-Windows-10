/*
 * Project: Game 2048
 * Last Modified: 2020/06/29
 * 
 * Copyright (C) 2020 Programmer-Yang_Xun@outlook.com. All Rights Reserved.
 * Welcome to visit https://GitHub.com/Hydr10n
 */

using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Game_2048
{
    public sealed partial class MainPage : Page
    {
        private enum Direction { Left, Up, Right, Down }

        private const double PaddingScale = 0.06, TileScale = 1 - 2 * PaddingScale, Layout4RepositionAnimationDurationUnit = 40;
        private const string GameSaveKeyLayoutSelection = "LayoutSelection";

        private readonly ViewModel ViewModel = new ViewModel();

        private int tilesCountPerSide, gameSaveKeysIndex;
        private double tileFullSideLength, repositionAnimationDurationUnit;

        public MainPage()
        {
            DataContext = ViewModel;
            InitializeComponent();
            CustomizeTitleBar();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private void CustomizeTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            Window.Current.SetTitleBar(TitleBar);
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => TitleBar.Height = sender.Height;

        private void SetGameLayoutGrid()
        {
            tileFullSideLength = GameLayout.Height / (tilesCountPerSide + PaddingScale * 2);
            GameLayout.Padding = new Thickness(tileFullSideLength * PaddingScale);
            RowDefinitionCollection rowDefinitions = GameLayout.RowDefinitions;
            rowDefinitions.Clear();
            ColumnDefinitionCollection columnDefinitions = GameLayout.ColumnDefinitions;
            columnDefinitions.Clear();
            for (int i = 0; i < tilesCountPerSide; i++)
            {
                rowDefinitions.Add(new RowDefinition() { Height = new GridLength(tileFullSideLength) });
                columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileFullSideLength) });
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double sideLength = Math.Min(Math.Max(0.6 * Math.Min(ActualHeight, ActualWidth), GameLayout.MinHeight), GameLayout.MaxHeight);
            GameLayout.CornerRadius = new CornerRadius(sideLength * 0.023);
            GameLayout.Width = GameLayout.Height = sideLength;
            GameStateText.FontSize = 45 * GameLayout.Height / GameLayout.MinHeight;
            if (!ViewModel.LayoutReady)
                return;
            SetGameLayoutGrid();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int index = GameSave.LoadIntData(GameSaveKeyLayoutSelection, out bool hasKey);
            if (!hasKey)
                return;
            LayoutSelectionComboBox.SelectedIndex = index;
        }

        private async void HelpButton_Click(object sender, RoutedEventArgs e) => await new HelpDialog().ShowAsync();

        private void Layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int count, index = (sender as ComboBox).SelectedIndex;
            switch (index)
            {
                case 0:
                    count = 4;
                    gameSaveKeysIndex = 0;
                    repositionAnimationDurationUnit = Layout4RepositionAnimationDurationUnit;
                    break;
                case 1:
                    count = 5;
                    gameSaveKeysIndex = 1;
                    repositionAnimationDurationUnit = Layout4RepositionAnimationDurationUnit * 4 / 5;
                    break;
                case 2:
                    count = 6;
                    gameSaveKeysIndex = 2;
                    repositionAnimationDurationUnit = Layout4RepositionAnimationDurationUnit * 4 / 6;
                    break;
                default: return;
            }
            if (tilesCountPerSide == count)
                return;
            tilesCountPerSide = count;
            GameLayout.Children.Clear();
            SetGameLayoutGrid();
            if (InitializeGameLayout())
                ViewModel.GameState = GameState.Started;
            ViewModel.LayoutReady = true;
            GameSave.SaveData(GameSaveKeyLayoutSelection, index);
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GameState = GameState.NotStarted;
            SaveGameProgress();
            StartNewGame();
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case VirtualKey.Left: MoveTiles(Direction.Left); break;
                case VirtualKey.Up: MoveTiles(Direction.Up); break;
                case VirtualKey.Right: MoveTiles(Direction.Right); break;
                case VirtualKey.Down: MoveTiles(Direction.Down); break;
                default: return;
            }
            SaveGameProgress();
        }

        private void GameLayout_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            const double SwipeDistanceThreshold = 30, SwipeVelocityTheshold = 0.15;
            if (e.IsInertial)
            {
                e.Complete();
                double translationX = e.Cumulative.Translation.X, translationY = e.Cumulative.Translation.Y;
                if (Math.Abs(translationX) > Math.Abs(translationY) && Math.Abs(e.Velocities.Linear.X) > SwipeVelocityTheshold)
                {
                    if (Math.Abs(translationX) > SwipeDistanceThreshold)
                        if (translationX > 0)
                            MoveTiles(Direction.Right);
                        else
                            MoveTiles(Direction.Left);
                }
                else if (Math.Abs(translationY) > SwipeDistanceThreshold && Math.Abs(e.Velocities.Linear.Y) > SwipeVelocityTheshold)
                {
                    if (translationY > 0)
                        MoveTiles(Direction.Down);
                    else
                        MoveTiles(Direction.Up);
                }
                else
                    return;
                SaveGameProgress();
            }
        }
    }
}