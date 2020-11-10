/*
 * Project: Game 2048
 * Last Modified: 2020/11/10
 * 
 * Copyright (C) 2020 Programmer-Yang_Xun@outlook.com. All Rights Reserved.
 * Welcome to visit https://GitHub.com/Hydr10n
 */

using Hydr10n.Utils;
using System;
using System.Numerics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using DropDownButtonAutomationPeer = Microsoft.UI.Xaml.Automation.Peers.DropDownButtonAutomationPeer;

namespace Game_2048
{
    public sealed partial class MainPage : Page
    {
        private readonly GameManager GameManager;
        private readonly ViewModelEx ViewModelEx;

        public MainPage()
        {
            InitializeComponent();
            CustomizeTitleBar();
            ViewModelEx = DataContext as ViewModelEx;
            GameManager = new GameManager(GameLayout, ViewModelEx);
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            GamepadUtil.GamepadAdded += async (sender, e) => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ViewModelEx.IsGamepadActive = true);
            Gamepad.GamepadRemoved += async (sender, e) => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ViewModelEx.IsGamepadActive = GamepadUtil.Gamepads.Count != 0);
        }

        private void CustomizeTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += (sender, e) => TitleBar.Height = sender.Height;
            Window.Current.SetTitleBar(TitleBar);
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonForegroundColor = Colors.Black;
        }

        private string Version
        {
            get
            {
                PackageVersion packageVersion = Package.Current.Id.Version;
                return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
            }
        }

        private double GamepadVibrationIntensity
        {
            get
            {
                AppData.Load(nameof(GamepadVibrationIntensity), out double data, out bool hasKey);
                return hasKey ? data : 50;
            }
            set => AppData.Save(nameof(GamepadVibrationIntensity), value);
        }

        private int GameLayoutSelectedIndex
        {
            get
            {
                AppData.Load(nameof(GameLayoutSelectedIndex), out int data, out _);
                return data;
            }
            set => AppData.Save(nameof(GameLayoutSelectedIndex), value);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float scale = (float)(Math.Min(Math.Max(0.6 * Math.Min(ActualHeight, ActualWidth), GameLayoutPanel.MinHeight), GameLayoutPanel.MaxHeight) / GameLayoutPanel.MinHeight);
            GameLayoutPanel.CenterPoint = new Vector3((float)GameLayoutPanel.ActualWidth / 2, 0, 0);
            GameLayoutPanel.Scale = new Vector3(scale, scale, 1);
            MainPanel.Margin = new Thickness { Bottom = GameLayoutPanel.ActualHeight * (scale - 1) };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GamepadVibrationIntensitySlider.Value = GamepadVibrationIntensity;
            (new RadioButtonAutomationPeer(LayoutOptionsPanel.Children[GameLayoutSelectedIndex] as RadioButton).GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider).Select();
        }

        private void GamepadVibrationIntensitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => GamepadVibrationIntensity = (sender as Slider).Value;

        private void LayoutOptionsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            LayoutOptionsFlyout.Hide();
            GameManager.SetGameLayout(GameLayoutSelectedIndex = LayoutOptionsPanel.Children.IndexOf(sender as RadioButton));
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e) => GameManager.StartNewGame();

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            const string GamepadName = "Gamepad";
            string virtualKeyName = e.VirtualKey.ToString();
            int gamepadKeyIndex = virtualKeyName.IndexOf(GamepadName);
            bool isGamepadActive = gamepadKeyIndex != -1;
            ViewModelEx.IsGamepadActive = isGamepadActive;
            object focusedElement = FocusManager.GetFocusedElement();
            if (focusedElement != null && !(focusedElement is ScrollViewer scrollViewer && scrollViewer.Parent == null))
                return;
            Gamepad activeGamepad = null;
            if (isGamepadActive)
                lock (GamepadUtil.Gamepads)
                    foreach (Gamepad gamepad in GamepadUtil.Gamepads)
                        try
                        {
                            if (gamepad.GetCurrentReading().Buttons.ToString().Contains(virtualKeyName.Substring(gamepadKeyIndex + GamepadName.Length)))
                            {
                                activeGamepad = gamepad;
                                break;
                            }
                        }
                        catch { }
            double actualGamepadVibrationIntensity = GamepadVibrationIntensity / 200, leftTrigger = 0, rightTrigger = 0, leftMotor = 0, rightMotor = 0;
            void onTilesMerged(object _, EventArgs _2)
            {
                if (activeGamepad != null)
                    GamepadUtil.StartVibration(activeGamepad, new GamepadVibration() { LeftTrigger = leftTrigger, RightTrigger = rightTrigger, LeftMotor = leftMotor, RightMotor = rightMotor }, 300);
            }
            switch (e.VirtualKey)
            {
                case VirtualKey.F1:
                case VirtualKey.GamepadView: SettingsButton.Flyout.ShowAt(SettingsButton); break;
                case VirtualKey.Application:
                case VirtualKey.GamepadMenu: (new DropDownButtonAutomationPeer(LayoutOptionsDropDownButton).GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider).Expand(); break;
                case VirtualKey.GamepadA:
                case VirtualKey.Space: (new ButtonAutomationPeer(NewGameButton).GetPattern(PatternInterface.Invoke) as IInvokeProvider).Invoke(); break;
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.Left:
                    {
                        leftMotor = actualGamepadVibrationIntensity;
                        leftTrigger = leftMotor / 4;
                        GameManager.MoveTiles(Direction.Left, onTilesMerged);
                    }
                    break;
                case VirtualKey.GamepadDPadUp:
                case VirtualKey.Up:
                    {
                        leftTrigger = rightTrigger = actualGamepadVibrationIntensity / 4;
                        GameManager.MoveTiles(Direction.Up, onTilesMerged);
                    }
                    break;
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.Right:
                    {
                        rightMotor = actualGamepadVibrationIntensity;
                        rightTrigger = rightMotor / 4;
                        GameManager.MoveTiles(Direction.Right, onTilesMerged);
                    }
                    break;
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.Down:
                    {
                        leftMotor = rightMotor = actualGamepadVibrationIntensity;
                        GameManager.MoveTiles(Direction.Down, onTilesMerged);
                    }
                    break;
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
                        GameManager.MoveTiles(translationX > 0 ? Direction.Right : Direction.Left);
                }
                else if (Math.Abs(translationY) > SwipeDistanceThreshold && Math.Abs(e.Velocities.Linear.Y) > SwipeVelocityThreshold)
                    GameManager.MoveTiles(translationY > 0 ? Direction.Down : Direction.Up);
            }
        }
    }
}