using System;
using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundIn;
using CSCore.Streams;
using System.Windows.Forms;
using CSCore.CoreAudioAPI;
using CSCore.Win32;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Ten2Five.Utils
{
    public class MP3Recorder : IDisposable
    {
        private readonly ISoundIn
            wasapiCapture_;

        private readonly IWaveSource
            stereoSource_;

        private readonly MediaFoundationEncoder
            writer_;

//        private MMDevice _selectedDevice;
        private ISoundIn _soundIn;
//        private IWriteable _writer;
//        private readonly GraphVisualization _graphVisualization = new GraphVisualization();
//        private IWaveSource _finalSource;

        private void RefreshDevices()
        {
            //deviceList.Items.Clear();


            using (var deviceEnumerator = new MMDeviceEnumerator())
            using (var deviceCollection = deviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
            {
                foreach (var device in deviceCollection)
                {
                    var deviceFormat = WaveFormatFromBlob(device.PropertyStore[
                        new PropertyKey(new Guid(0xf19f064d, 0x82c, 0x4e27, 0xbc, 0x73, 0x68, 0x82, 0xa1, 0xbb, 0x8e, 0x4c), 0)].BlobValue);

                    var item = new ListViewItem(device.FriendlyName) { Tag = device };
                    item.SubItems.Add(deviceFormat.Channels.ToString(CultureInfo.InvariantCulture));

                    //deviceList.Items.Add(item);
                }
            }
        }

//        private void StartCapture(string fileName)
//        {
//            if (SelectedDevice == null)
//                return;
//
//            if (CaptureMode == CaptureMode.Capture)
//                _soundIn = new WasapiCapture();
//            else
//                _soundIn = new WasapiLoopbackCapture();
//
//            _soundIn.Device = SelectedDevice;
//            _soundIn.Initialize();
//
//            var soundInSource = new SoundInSource(_soundIn);
//            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ToSampleSource());
//            _finalSource = singleBlockNotificationStream.ToWaveSource();
//            _writer = new WaveWriter(fileName, _finalSource.WaveFormat);
//
//            byte[] buffer = new byte[_finalSource.WaveFormat.BytesPerSecond / 2];
//            soundInSource.DataAvailable += (s, e) =>
//            {
//                int read;
//                while ((read = _finalSource.Read(buffer, 0, buffer.Length)) > 0)
//                    _writer.Write(buffer, 0, read);
//            };
//
//            singleBlockNotificationStream.SingleBlockRead += SingleBlockNotificationStreamOnSingleBlockRead;
//
//            _soundIn.Start();
//        }
//
//        private void SingleBlockNotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs e)
//        {
//            _graphVisualization.AddSamples(e.Left, e.Right);
//        }

        private static WaveFormat WaveFormatFromBlob(Blob blob)
        {
            if (blob.Length == 40)
                return (WaveFormat)Marshal.PtrToStructure(blob.Data, typeof(WaveFormatExtensible));
            return (WaveFormat)Marshal.PtrToStructure(blob.Data, typeof(WaveFormat));
        }

//        private void btnRefreshDevices_Click(object sender, EventArgs e)
//        {
//            RefreshDevices();
//        }
//
//        private void btnStart_Click(object sender, EventArgs e)
//        {
//            SaveFileDialog sfd = new SaveFileDialog
//            {
//                Filter = "WAV (*.wav)|*.wav",
//                Title = "Save",
//                FileName = String.Empty
//            };
//            if (sfd.ShowDialog(this) == DialogResult.OK)
//            {
//                StartCapture(sfd.FileName);
//                btnStart.Enabled = false;
//                btnStop.Enabled = true;
//            }
//        }
//
//        private void btnStop_Click(object sender, EventArgs e)
//        {
//            StopCapture();
//        }

        private void StopCapture()
        {
            if (_soundIn != null)
            {
                _soundIn.Stop();
                _soundIn.Dispose();
                _soundIn = null;
//                _finalSource.Dispose();
//
//                if (_writer is IDisposable)
//                    ((IDisposable)_writer).Dispose();

//                btnStop.Enabled = false;
//                btnStart.Enabled = true;
            }
        }

//        private void deviceList_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            if (deviceList.SelectedItems.Count > 0)
//            {
//                SelectedDevice = (MMDevice)deviceList.SelectedItems[0].Tag;
//            }
//            else
//            {
//                SelectedDevice = null;
//            }
//        }
//
//        private void timer1_Tick(object sender, EventArgs e)
//        {
//            var image = pictureBox1.Image;
//            pictureBox1.Image = _graphVisualization.Draw(pictureBox1.Width, pictureBox1.Height);
//            if (image != null)
//                image.Dispose();
//        }
//
//        protected override void OnClosing(CancelEventArgs e)
//        {
//            base.OnClosing(e);
//            StopCapture();
//        }

        public MP3Recorder(string filename)
        {
            //RefreshDevices();

            wasapiCapture_ = new WasapiCapture();

            //wasapiCapture_ = new WasapiLoopbackCapture();
            wasapiCapture_.Initialize();
            var
                wasapiCaptureSource = new SoundInSource(wasapiCapture_);
            stereoSource_ = wasapiCaptureSource.ToStereo();
            writer_ = MediaFoundationEncoder.CreateMP3Encoder(stereoSource_.WaveFormat, filename);
            byte []
                buffer = new byte[stereoSource_.WaveFormat.BytesPerSecond];
            wasapiCaptureSource.DataAvailable += (s, e) =>
                {
                    int
                        read = stereoSource_.Read(buffer, 0, buffer.Length);
                    writer_.Write(buffer, 0, read);
                };
            wasapiCapture_.Start();
        }

        public void Dispose()
        {
            wasapiCapture_.Stop();
            writer_.Dispose();
            stereoSource_.Dispose();
            wasapiCapture_.Dispose();
        }
    }
}

