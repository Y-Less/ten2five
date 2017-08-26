using System;
using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundIn;
using CSCore.Streams;
using System.Collections.Generic;
using CSCore.Codecs;
using System.IO;
using System.Timers;

namespace Ten2Five.Utils
{
    public class AppendingMixer : ISampleSource
    {
        private readonly WaveFormat waveFormat_;
        private readonly List<ISampleSource> sampleSources_ = new List<ISampleSource>();
        private readonly object lockObj_ = new object();
        private float[] mixerBuffer_;

        private int idx_ = 0;
        private long position_ = 0;

        public bool CanSeek => false;

        public WaveFormat WaveFormat => waveFormat_;

        public long Position { get => 0; set => throw new NotSupportedException(); }

        public long Length => 0;

        public int Read(float[] buffer, int offset, int count)
        {
            Array.Clear(buffer, offset, count);
            int
                numberOfStoredSamples = 0,
                num = sampleSources_.Count;
            while (count > 0 && num > idx_)
            {
                lock (lockObj_)
                {
                    mixerBuffer_ = mixerBuffer_.CheckBuffer(count);
                    var
                        sampleSource = sampleSources_[idx_];
                    int
                        remaining = (int)(sampleSource.Length - position_);
                    if (remaining <= 0)
                    {
                        position_ = 0;
                        ++idx_;
                        continue;
                    }
                    sampleSource.Position = position_;
                    int
                        read = sampleSource.Read(mixerBuffer_, 0, Math.Min(count, remaining));
                    numberOfStoredSamples += read;
                    position_ += read;
                    count -= read;
                    //Array.Copy(mixerBuffer_, 0, buffer, offset, read);
                    for (int i = 0, j = offset; i != read; ++i, ++j)
                    {
                        buffer[j] = mixerBuffer_[i];
                    }
                    if (remaining <= read)
                    {
                        position_ = 0;
                        ++idx_;
                        continue;
                    }
                    //foreach (var sampleSource in sampleSources_)
                    //{
                    //    // Each time we read from the source, it advances the
                    //    // internal pointer.
                    //    for (int read; (read = sampleSource.Read(mixerBuffer_, 0, count - numberOfStoredSamples)) > 0; )
                    //    {
                    //        Array.Copy(mixerBuffer_, 0, buffer, offset, read);
                    //        numberOfStoredSamples += read;
                    //        if (numberOfStoredSamples >= count)
                    //            return numberOfStoredSamples;
                    //    }
                    //}
                }
            }
            return numberOfStoredSamples;
        }

        public AppendingMixer(int channelCount, int sampleRate)
        {
            if (channelCount < 1)
                throw new ArgumentOutOfRangeException("channelCount");
            if (sampleRate < 1)
                throw new ArgumentOutOfRangeException("sampleRate");

            waveFormat_ = new WaveFormat(sampleRate, 32, channelCount, AudioEncoding.IeeeFloat);
        }

        public void Dispose()
        {
            lock (lockObj_)
            {
                foreach (var sampleSource in sampleSources_.ToArray())
                {
                    sampleSource.Dispose();
                    sampleSources_.Remove(sampleSource);
                }
            }
        }

        public void AddSource(ISampleSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (source.WaveFormat.Channels != WaveFormat.Channels ||
               source.WaveFormat.SampleRate != WaveFormat.SampleRate)
                throw new ArgumentException("Invalid format.", "source");

            lock (lockObj_)
            {
                if (!Contains(source))
                    sampleSources_.Add(source);
            }
        }

        public void RemoveSource(ISampleSource source)
        {
            //don't throw null ex here
            lock (lockObj_)
            {
                if (Contains(source))
                    sampleSources_.Remove(source);
            }
        }

        public bool Contains(ISampleSource source)
        {
            if (source == null)
                return false;
            return sampleSources_.Contains(source);
        }
    }

    public class Silence : ISampleSource
    {
        private readonly WaveFormat waveFormat_;
        private readonly List<ISampleSource> sampleSources_ = new List<ISampleSource>();
        private readonly object lockObj_ = new object();
        private readonly int time_;

        public bool CanSeek => true;

        public WaveFormat WaveFormat => waveFormat_;

        public long Position { get; set; }
        
        public long Length => WaveFormat.SampleRate * time_ / 1000;

        public int Read(float[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                lock (lockObj_)
                {
                    long
                        read = Math.Min(Length - Position, count);
                    if (read <= 0)
                        return 0;
                    Position += read;
                    Array.Clear(buffer, offset, (int)read);
                    return (int)read;
                }
            }
            return 0;
        }

        public Silence(int timeInMs, int channelCount, int sampleRate)
        {
            if (channelCount < 1)
                throw new ArgumentOutOfRangeException("channelCount");
            if (sampleRate < 1)
                throw new ArgumentOutOfRangeException("sampleRate");

            waveFormat_ = new WaveFormat(sampleRate, 32, channelCount, AudioEncoding.IeeeFloat);
            time_ = timeInMs;
        }

        public void Dispose()
        {
        }
    }

    public class MP3Recorder
    {
        private readonly ISoundIn
            wasapiCapture_;

        private readonly IWaveSource
            stereoSource_;

        private readonly MediaFoundationEncoder
            writer_;

        public bool Disposed { get; private set; }

        public MP3Recorder(ISoundIn src, string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            wasapiCapture_ = src;
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
            Disposed = false;
        }

        public void Clean(bool dispose)
        {
            var
                trueEnd = new Timer(500);
            trueEnd.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                wasapiCapture_.Stop();
                writer_.Dispose();
                stereoSource_.Dispose();
                if (dispose)
                    wasapiCapture_.Dispose();
                Disposed = true;
            };
            trueEnd.Start();
        }

        public static void Mix(string filename, string f0, string f1)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            const int mixerSampleRate = 48000;
            AppendingMixer
                mixer = new AppendingMixer(2, mixerSampleRate);
            mixer.AddSource(
                CodecFactory.Instance.GetCodec(f0)
//                .ChangeSampleRate(mixerSampleRate)
                .ToStereo()
                .ToSampleSource());
            mixer.AddSource(
                new Silence(5000, 2, mixerSampleRate));
            mixer.AddSource(
                CodecFactory.Instance.GetCodec(f1)
//                .ChangeSampleRate(mixerSampleRate)
                .ToStereo()
                .ToSampleSource());
            mixer.AddSource(
                new Silence(500, 2, mixerSampleRate));
            mixer.ToWaveSource().WriteToFile(filename);
            mixer.Dispose();
        }
    }
}

