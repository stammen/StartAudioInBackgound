//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using NAudio.Wave;

namespace Audio
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public sealed class AudioInput
    {
        private AudioGraph m_audioGraph;
        private AudioDeviceInputNode m_deviceInputNode;

        ~AudioInput()
        {
            Stop();
        }

        public void Stop()
        {
            if (m_audioGraph != null)
            {
                m_audioGraph.Stop();
                m_audioGraph.Dispose();
                m_audioGraph = null;
            }
        }

        public async Task Start()
        {
            try
            {
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioCaptureSelector());
                var pcmEncoding = AudioEncodingProperties.CreatePcm(16000, 1, 16);

                // Construct the audio graph
                var result = await AudioGraph.CreateAsync(
                    new AudioGraphSettings(AudioRenderCategory.Speech)
                    {
                        DesiredRenderDeviceAudioProcessing = AudioProcessing.Raw,
                        AudioRenderCategory = AudioRenderCategory.Speech,
                        EncodingProperties = pcmEncoding
                    });

                if (result.Status != AudioGraphCreationStatus.Success)
                {
                    throw new Exception("AudioGraph creation error: " + result.Status);
                }

                m_audioGraph = result.Graph;

                var inputResult = await m_audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Speech, pcmEncoding);
                if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
                {
                    throw new Exception("AudioGraph CreateDeviceInputNodeAsync error: " + inputResult.Status);
                }

                m_deviceInputNode = inputResult.DeviceInputNode;
                m_audioGraph.QuantumStarted += Node_QuantumStarted;
                m_audioGraph.Start();

            }
            catch(Exception ex)
            {
                Utils.Toasts.ShowToast("","AudioInput Start Exception: " + ex.Message);
            }
        }

        private void Node_QuantumStarted(AudioGraph graph, object args)
        {
  
        }
    }
}