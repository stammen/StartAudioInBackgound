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

namespace StartAudioInTheBackground
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private SuspendingDeferral suspendDeferral = null;
        private ExtendedExecutionSession session = null;
        private ExtendedExecutionForegroundSession mediaSession = null;
        private BackgroundTaskDeferral deferral = null;
        private bool startedInbackground = false;
        private Audio.AudioOutput m_audioOutput;
        private Audio.AudioInput m_audioInput;
        private MediaPlayer m_localMediaPlayer = null;
        private MediaBinder m_localMediaBinder = null;
        private MediaSource m_localMediaSource = null;
        private Deferral m_deferral = null;

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

            ShowToast("OnBackgroundActivated: " + name);

            if (name == "userAwayTrigger")
            {
                ShowToast("userAwayTrigger: Audio stopped");
                StopRecording();
                if(deferral != null)
                {
                    deferral.Complete();
                    deferral = null;
                }
                return;
            }

            startedInbackground = true;
            base.OnBackgroundActivated(args);
            deferral = args.TaskInstance.GetDeferral();
            args.TaskInstance.Canceled += TaskInstance_Canceled;

            mediaSession = new ExtendedExecutionForegroundSession();
            mediaSession.Reason = ExtendedExecutionForegroundReason.BackgroundAudio;
            mediaSession.Revoked += MediaSession_Revoked;
            var result = await mediaSession.RequestExtensionAsync();
            if (result != ExtendedExecutionForegroundResult.Allowed)
                ShowToast("Audio EE denied");

            await StartRecording();
            ShowToast("userPresentTrigger: Audio started");
        }

        public async Task StartRecording()
        {
            //BookNetworkForBackground();

            StopRecording();

            m_audioOutput = new Audio.AudioOutput();
            m_audioInput = new Audio.AudioInput();
            m_audioInput.OnAudioInput += OnAudioInput;
            await m_audioOutput.Start();
            await m_audioInput.Start();
        }


        public void StopRecording()
        {
            //BookNetworkForBackground();

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

        public bool BookNetworkForBackground()
        {
            bool result = false;
            try
            {
                var smtc = SystemMediaTransportControls.GetForCurrentView();
                smtc.IsPlayEnabled = true;
                smtc.IsPauseEnabled = true;

                if (m_localMediaBinder == null)
                {
                    m_localMediaBinder = new Windows.Media.Core.MediaBinder();
                    if (m_localMediaBinder != null)
                    {
                        m_localMediaBinder.Binding += LocalMediaBinder_Binding;
                    }
                }
                if (m_localMediaSource == null)
                {
                    m_localMediaSource = Windows.Media.Core.MediaSource.CreateFromMediaBinder(m_localMediaBinder);
                }
                if (m_localMediaPlayer == null)
                {
                    m_localMediaPlayer = new Windows.Media.Playback.MediaPlayer();
                    if (m_localMediaPlayer != null)
                    {
                        m_localMediaPlayer.CommandManager.IsEnabled = false;
                        m_localMediaPlayer.Source = m_localMediaSource;
                        result = true;
                        Debug.WriteLine("Booking network for Background task successful");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while booking network for Background task: Exception: " + ex.Message);
            }
            Debug.WriteLine("Booking network for Background task failed");
            return result;
        }

        // Method used to keep the network on while the application is in background
        private void LocalMediaBinder_Binding(Windows.Media.Core.MediaBinder sender, Windows.Media.Core.MediaBindingEventArgs args)
        {
            m_deferral = args.GetDeferral();
            Debug.WriteLine("Booking network for Background task running...");
        }

        private void OnAudioInput(NAudio.Wave.IWaveBuffer data)
        {
            m_audioOutput.Send(data.ByteBuffer);
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            StopRecording();
            deferral.Complete();
            ShowToast("TaskInstance_Canceled");
        }

        private void MediaSession_Revoked(object sender, ExtendedExecutionForegroundRevokedEventArgs args)
        {
            StopRecording();
            ShowToast("Audio EE revoked");
        }

        private void ShowToast(string message)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("UpdateTask"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(message));

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            startedInbackground = false;
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
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            suspendDeferral = e.SuspendingOperation.GetDeferral();
            if (!startedInbackground)
            {
                suspendDeferral.Complete();
                ShowToast("Exiting ...");
                return;
            }
            session = new ExtendedExecutionSession();
            session.Reason = ExtendedExecutionReason.SavingData;
            session.Revoked += Session_Revoked;
            session.Description = "";

            ExtendedExecutionResult result = await session.RequestExtensionAsync();
            if (result != ExtendedExecutionResult.Allowed)
            {
                ShowToast("SavingData EE denied");
                suspendDeferral.Complete();
            }
        }

        private void Session_Revoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            ShowToast("SavingData EE revoked");
        }
    }
}
