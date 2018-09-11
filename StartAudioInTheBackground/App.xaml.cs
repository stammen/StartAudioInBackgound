using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Utils;

namespace StartAudioInTheBackground
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private ExtendedExecutionForegroundSession m_mediaSession = null;
        private BackgroundTaskDeferral m_deferral = null;
        private Audio.AudioOutput m_audioOutput;
        private Audio.AudioInput m_audioInput;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected async override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var name = args.TaskInstance.Task.Name;

            Toasts.ShowToast("OnBackgroundActivated: " + name);

            StopRecording();

            if (m_deferral != null)
            {
                m_deferral.Complete();
                m_deferral = null;
            }

            if (name == "userAwayTrigger")
            {
                return;
            }

            base.OnBackgroundActivated(args);
            m_deferral = args.TaskInstance.GetDeferral();
            args.TaskInstance.Canceled += TaskInstance_Canceled;

            m_mediaSession = new ExtendedExecutionForegroundSession();
            m_mediaSession.Reason = ExtendedExecutionForegroundReason.Unconstrained;
            m_mediaSession.Revoked += MediaSession_Revoked;
            var result = await m_mediaSession.RequestExtensionAsync();
            if (result != ExtendedExecutionForegroundResult.Allowed)
            {
                Toasts.ShowToast("Audio EE denied");
            }

            await StartRecording();
        }

        public async Task StartRecording()
        {
            StopRecording();

            m_audioOutput = new Audio.AudioOutput();
            m_audioInput = new Audio.AudioInput();
            m_audioInput.OnAudioInput += OnAudioInput;
            await m_audioOutput.Start();
            await m_audioInput.Start();
        }

        public void StopRecording()
        {
            if (m_audioInput != null)
            {
                m_audioInput.Stop();
                m_audioInput = null;
            }

            if (m_audioOutput != null)
            {
                m_audioOutput.Stop();
                m_audioOutput = null;
            }
        }
 
        private void OnAudioInput(NAudio.Wave.IWaveBuffer data)
        {
            if(m_audioOutput != null)
            {
                m_audioOutput.Send(data.ByteBuffer);
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            StopRecording();
            m_deferral.Complete();
            ///Toasts.ShowToast("TaskInstance_Canceled");
        }

        private void MediaSession_Revoked(object sender, ExtendedExecutionForegroundRevokedEventArgs args)
        {
            StopRecording();
            //Toasts.ShowToast("Audio EE revoked");
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.Kind == ActivationKind.Launch && e.Arguments == "/Exit")
            {
                Utils.BackGroundTask.UnregisterBackgroundTask("applicationBackgroundTask");
                await Utils.JumpListMenu.Clear();
                Application.Current.Exit();
                return;
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            // uncomment this code to restart the recording background task on app exit
#if false
            var suspendDeferral = e.SuspendingOperation.GetDeferral();
            var appTrigger = new ApplicationTrigger();
            var requestStatus = await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync();
            await Utils.BackGroundTask.TriggerApplicationBackgroundTask("applicationBackgroundTask");
            suspendDeferral.Complete();
#endif
        }

        private void Session_Revoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            //Toasts.ShowToast("SavingData EE revoked");
        }
    }
}
