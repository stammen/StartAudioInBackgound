using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core.Preview;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace StartAudioInTheBackground
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string UserPresentTaskName = "userPresentTrigger";
        private string UserAwayTaskName = "userAwayTrigger";
        private string SessionConnectedTaskName = "sessionConnectedTrigger";
        private bool m_isRecording = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            bool permissionGained = await Speech.AudioCapturePermissions.RequestMicrophonePermission();

            // these are no longer needed
            Utils.BackGroundTask.UnregisterBackgroundTask(UserPresentTaskName);
            Utils.BackGroundTask.UnregisterBackgroundTask(UserAwayTaskName);
            Utils.BackGroundTask.UnregisterBackgroundTask(SessionConnectedTaskName);

            //Utils.BackGroundTask.RegisterSystemBackgroundTask(SessionConnectedTaskName, SystemTriggerType.SessionConnected);

            // start the recording task
            //await Utils.BackGroundTask.TriggerApplicationBackgroundTask("applicationBackgroundTask");

            await Utils.JumpListMenu.Clear();
            await Utils.JumpListMenu.Add("/Exit", "Exit Application", "ms-appx:///Assets/Square44x44Logo.altform-unplated_targetsize-256.png");

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
        }

        // prevents the windows from being close by the user until they select the Exit option
        public void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs args)
        {
            args.Handled = true;

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

        private async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if(m_isRecording)
            {
                Utils.BackGroundTask.UnregisterBackgroundTask("applicationBackgroundTask");
                m_isRecording = false;
                recordButton.Content = "Record";
            }
            else
            {
                // start the recording task
                await Utils.BackGroundTask.TriggerApplicationBackgroundTask("applicationBackgroundTask");
                m_isRecording = true;
                recordButton.Content = "Stop";
            }
        }
    }
}
