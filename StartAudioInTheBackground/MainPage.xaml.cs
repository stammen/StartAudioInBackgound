using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private bool m_isRecording = false;
        private Audio.AudioOutput m_audioOutput;
        private Audio.AudioInput m_audioInput;
        private ExtendedExecutionForegroundSession m_session = null;
        private static Mutex m_mutex = new Mutex();

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            bool permissionGained = await Speech.AudioCapturePermissions.RequestMicrophonePermission();

            await Utils.JumpListMenu.Clear();
            await Utils.JumpListMenu.Add("/Exit", "Exit Application", "ms-appx:///Assets/Square44x44Logo.altform-unplated_targetsize-256.png");

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
        }

        // prevents the windows from being close by the user until they select the Exit option
        public async void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs args)
        {
            // If the ExtendedExecutionForegroundSession is active, don't allow the app to exit unless the user wants it to exit.
            if (m_session != null)
            {
                var deferral = args.GetDeferral();

                CloseAppDialog dialog = new CloseAppDialog();
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    args.Handled = true;
                    Utils.KeyboardInput.MinimizeApp();
                }
                else
                {
                    args.Handled = false;
                    await ExitApp();
                }

                deferral.Complete();
            }
        }

        private async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if(m_isRecording)
            {
                m_isRecording = false;
                recordButton.Content = "Record";
                StopRecording();  
            }
            else
            {
                // start the recording task
                m_isRecording = true;
                recordButton.Content = "Stop";
                await StartRecording();
            }
        }

        public async Task StartRecording()
        {
            StopRecording();

            m_session = new ExtendedExecutionForegroundSession();
            m_session.Reason = ExtendedExecutionForegroundReason.Unconstrained;
            m_session.Revoked += Session_Revoked;
            var result = await m_session.RequestExtensionAsync();
            if (result != ExtendedExecutionForegroundResult.Allowed)
            {
                Utils.Toasts.ShowToast("StartAudioInTheBackground", "Audio EE denied");
                return;
            }

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

            ClearExtendedExecution();
        }

        private void OnAudioInput(NAudio.Wave.IWaveBuffer data)
        {
            if (m_audioOutput != null)
            {
                // send recorded audio to speakers
                m_audioOutput.Send(data.ByteBuffer);
            }
        }

        private async void Exit_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await ExitApp();
        }

        public async Task ExitApp()
        {
            StopRecording();
            ClearExtendedExecution();
            await Utils.JumpListMenu.Clear();
            Utils.KeyboardInput.CloseApp();
        }

        private void ClearExtendedExecution()
        {
            m_mutex.WaitOne();
            if (m_session != null)
            {
                m_session.Revoked -= Session_Revoked;
                m_session.Dispose();
                m_session = null;
            }

            m_mutex.ReleaseMutex();
        }

        private void Session_Revoked(object sender, ExtendedExecutionForegroundRevokedEventArgs args)
        {
            ClearExtendedExecution();
            Utils.Toasts.ShowToast("StartAudioInTheBackground", "Session_Revoked. Reason: " + args.Reason.ToString());
        }
    }
}
