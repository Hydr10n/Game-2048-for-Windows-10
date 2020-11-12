using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace Hydr10n
{
    namespace Utils
    {
        class GamepadUtil
        {
            private static readonly HashSet<Gamepad> gamepads = new HashSet<Gamepad>();
            public static IReadOnlyCollection<Gamepad> Gamepads => gamepads;
            public static event EventHandler<Gamepad> GamepadAdded, GamepadRemoved;

            static GamepadUtil()
            {
                Gamepad.GamepadAdded += (sender, e) =>
                {
                    lock (gamepads)
                    {
                        gamepads.Add(e);
                        GamepadAdded?.Invoke(null, e);
                    }
                };
                Gamepad.GamepadRemoved += (sender, e) =>
                {
                    lock (gamepads)
                    {
                        gamepads.Remove(e);
                        GamepadRemoved?.Invoke(null, e);
                    }
                };
            }

            public static void StartVibration(Gamepad gamepad, GamepadVibration vibration, int millionseconds)
            {
                new Thread(async delegate ()
                {
                    gamepad.Vibration = vibration;
                    await Task.Delay(millionseconds);
                    gamepad.Vibration = new GamepadVibration();
                }).Start();
            }
        }
    }
}