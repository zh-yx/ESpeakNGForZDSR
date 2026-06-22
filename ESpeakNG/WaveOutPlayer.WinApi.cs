using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal partial class WaveOutPlayer
{
    public const int WAVE_MAPPER = -1;
    private const int MMSYSERR_NOERROR = 0;
    private const int CALLBACK_EVENT = 0x00050000;
    private const short WAVE_FORMAT_PCM = 1;
    private const int WHDR_DONE = 0x00000001;
    private const int WHDR_PREPARED = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormatEx
    {
        public short FormatTag;
        public short Channels;
        public int SamplesPerSec;
        public int AvgBytesPerSec;
        public short BlockAlign;
        public short BitsPerSample;
        public short CBSize;

        public WaveFormatEx(short channels, int samplesPerSec, short bitsPerSample)
        {
            if (channels != 1 && channels != 2) { throw new ArgumentOutOfRangeException(nameof(channels)); }
            if (bitsPerSample != 8 && bitsPerSample != 16) { throw new ArgumentOutOfRangeException(nameof(bitsPerSample)); }

            FormatTag = WAVE_FORMAT_PCM;
            Channels = channels;
            SamplesPerSec = samplesPerSec;
            BitsPerSample = bitsPerSample;
            BlockAlign = (short)(channels * bitsPerSample / 8);
            AvgBytesPerSec = samplesPerSec * this.BlockAlign;
            CBSize = 0; // For PCM format, the value should be 0.
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHeader
    {
        public readonly static int Size = Marshal.SizeOf<WaveHeader>();

        public IntPtr LpData;
        public int BufferLength;
        public int BytesRecorded;
        public IntPtr User;
        public int Flags;
        public int Loops;
        public IntPtr LpNext;
        public IntPtr Reserved;

        public WaveHeader(IntPtr lpData, int cbSize)
        {
            LpData = lpData;
            BufferLength = cbSize;
            BytesRecorded = 0;
            User = IntPtr.Zero;
            Flags = 0;
            Loops = 0;
            LpNext = IntPtr.Zero;
            Reserved = IntPtr.Zero;
        }

        public bool IsDone => (this.Flags & WHDR_DONE) != 0;
        public bool IsPrepared => (this.Flags & WHDR_PREPARED) != 0;
    }

    private static class NativeMethods
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutOpen(out IntPtr handleWaveOut, int deviceID, in WaveFormatEx wfx, IntPtr callback, IntPtr instance, int flagOpen);

        [DllImport("winmm.dll")]
        public static extern int waveOutPrepareHeader(IntPtr handleWaveOut, IntPtr pWaveHeader, int cbWaveHeader);

        [DllImport("winmm.dll")]
        public static extern int waveOutWrite(IntPtr handleWaveOut, IntPtr pWaveHeader, int cbWaveHeader);

        [DllImport("winmm.dll")]
        public static extern int waveOutUnprepareHeader(IntPtr handleWaveOut, IntPtr pWaveHeader, int cbWaveHeader);

        [DllImport("winmm.dll")]
        public static extern int waveOutReset(IntPtr handleWaveOut);

        [DllImport("winmm.dll")]
        public static extern int waveOutClose(IntPtr handleWaveOut);
    }
}
