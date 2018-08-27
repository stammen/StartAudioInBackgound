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

            Utils.BackGroundTask.RegisterSystemBackgroundTask(SessionConnectedTaskName, SystemTriggerType.SessionConnected);
            await Utils.BackGroundTask.TriggerApplicationBackgroundTask("applicationBackgroundTask");
        }
    }
}
