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
            UnregisterBackgroundTasks();
            RegisterBackgroundTask(UserPresentTaskName, SystemTriggerType.UserPresent);
            RegisterBackgroundTask(UserAwayTaskName, SystemTriggerType.UserAway);
            RegisterBackgroundTask(SessionConnectedTaskName, SystemTriggerType.SessionConnected);

            var app = Application.Current as App;
            await app.StartRecording();

            //MediaPlayer player = new MediaPlayer();
            //player.AutoPlay = true;
            //player.Source = MediaSource.CreateFromUri(new Uri("http://live-aacplus-64.kexp.org/kexp64.aac"));
        }


        private void RegisterBackgroundTask(string name, SystemTriggerType trigger)
        {
            var requestTask = BackgroundExecutionManager.RequestAccessAsync();
            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            //builder.TaskEntryPoint = TimezoneTriggerTaskEntryPoint;
            builder.SetTrigger(new SystemTrigger(trigger, false));
            var task = builder.Register();
        }

        private void UnregisterBackgroundTasks()
        {
            var count = BackgroundTaskRegistration.AllTasks.Count;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                Debug.WriteLine(task.Value.Name);
                if (task.Value.Name == UserPresentTaskName || task.Value.Name == UserAwayTaskName || task.Value.Name == SessionConnectedTaskName || task.Value.Name == "timezoneTrigger")
                {
                    task.Value.Unregister(true);
                }
            }
        }

    }
}
