/*
 * Project: Game 2048
 * Last Modified: 2020/07/02
 * 
 * Copyright (C) 2020 Programmer-Yang_Xun@outlook.com. All Rights Reserved.
 * Welcome to visit https://GitHub.com/Hydr10n
 */

using System;
using System.Numerics;
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

        private const double PaddingScale = 0.06;
        private const string GameSaveKeyLayoutSelection = "LayoutSelection";

        private readonly ViewModel ViewModel = new ViewModel();

        private int tilesCountPerSide, gameSaveKeysIndex;

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
            coreTitleBar.LayoutMetricsChanged += (sender, e) => TitleBar.Height = sender.Height;
            Window.Current.SetTitleBar(TitleBar);
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float scale = (float)(Math.Min(Math.Max(0.6 * Math.Min(ActualHeight, ActualWidth), GameLayoutContainer.MinHeight), GameLayoutContainer.MaxHeight) / GameLayoutContainer.MinHeight);
            GameLayoutContainer.CenterPoint = new Vector3((float)GameLayoutContainer.ActualWidth / 2, 0, 0);
            GameLayoutContainer.Scale = new Vector3(scale, scale, 1);
            MainContainer.Margin = new Thickness { Bottom = GameLayoutContainer.ActualHeight * (scale - 1) };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int index = GameSave.LoadIntData(GameSaveKeyLayoutSelection, out bool hasKey);
            if (!hasKey)
                return;
            LayoutSelectionComboBox.SelectedIndex = index;
        }

        private async void HelpButton_Click(object sender, RoutedEventArgs e) => await new HelpDialog().ShowAsync();

        private void LayoutSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            gameSaveKeysIndex = index;
            tilesCountPerSide = index + 4;
            ViewModel.GameState = GameState.NotStarted;
            GameLayout.Children.Clear();
            double tileFullSideLength = GameLayout.ActualHeight / (tilesCountPerSide + PaddingScale * 2);
            GameLayout.Padding = new Thickness(tileFullSideLength * PaddingScale);
            RowDefinitionCollection rowDefinitions = GameLayout.RowDefinitions;
            rowDefinitions.Clear();
            ColumnDefinitionCollection columnDefinitions = GameLayout.ColumnDefinitions;
            columnDefinitions.Clear();
            for (int i = 0; i < tilesCountPerSide; i++)
            {
                rowDefinitions.Add(new RowDefinition { Height = new GridLength(tileFullSideLength) });
                columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileFullSideLength) });
            }
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
            }
        }

        private void GameLayout_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            const double SwipeDistanceThreshold = 30, SwipeVelocityThreshold = 0.15;
            if (e.IsInertial)
            {
                e.Complete();
                double translationX = e.Cumulative.Translation.X, translationY = e.Cumulative.Translation.Y;
                if (Math.Abs(translationX) > Math.Abs(translationY))
                {
                    if (Math.Abs(translationX) > SwipeDistanceThreshold && Math.Abs(e.Velocities.Linear.X) > SwipeVelocityThreshold)
                        MoveTiles(translationX > 0 ? Direction.Right : Direction.Left);
                }
                else if (Math.Abs(translationY) > SwipeDistanceThreshold && Math.Abs(e.Velocities.Linear.Y) > SwipeVelocityThreshold)
                    MoveTiles(translationY > 0 ? Direction.Down : Direction.Up);
            }
        }
    }
}