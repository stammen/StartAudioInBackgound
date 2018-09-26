//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************


using System;
using Windows.System;
using Windows.UI.Input.Preview.Injection;

namespace Utils
{
    sealed class KeyboardInput
    {
        public static void MinimizeApp()
        {
            // Minimize the app window
            InputInjector inputInjector = InputInjector.TryCreate();
            var windowsKey = new InjectedInputKeyboardInfo();
            windowsKey.VirtualKey = (ushort)VirtualKey.LeftWindows;
            var downKey = new InjectedInputKeyboardInfo();
            downKey.VirtualKey = (ushort)VirtualKey.Down;
            inputInjector.InjectKeyboardInput(new[] { windowsKey, downKey });
            windowsKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
            downKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
            inputInjector.InjectKeyboardInput(new[] { windowsKey, downKey });
        }

        public static void CloseApp()
        {
            // Minimize the app window
            InputInjector inputInjector = InputInjector.TryCreate();
            var f4Key = new InjectedInputKeyboardInfo();
            f4Key.VirtualKey = (ushort)VirtualKey.F4;
            var altKey = new InjectedInputKeyboardInfo();
            altKey.VirtualKey = (ushort)VirtualKey.Menu;
            inputInjector.InjectKeyboardInput(new[] { altKey, f4Key });
            f4Key.KeyOptions = InjectedInputKeyOptions.KeyUp;
            altKey.KeyOptions = InjectedInputKeyOptions.KeyUp;
            inputInjector.InjectKeyboardInput(new[] { altKey, f4Key });
        }
    }
}

