using System;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal partial class WaveOutPlayer : IDisposable
{
    private readonly short _channels;
    private readonly int _sampleRate;
    private readonly short _bitsPerSample;
    private readonly short _blockAlign;
    private readonly AutoResetEvent _playbackCompleteEvent;
    private IntPtr _handleWaveOut;
    private bool _disposed;

    public WaveOutPlayer(short channels, int sampleRate, short bitsPerSample)
    {
        _channels = channels;
        _sampleRate = sampleRate;
        _bitsPerSample = bitsPerSample;
        _blockAlign = (short)(channels * (bitsPerSample / 8));
        _playbackCompleteEvent = new AutoResetEvent(false);
    }

    public void Open(int deviceId = WAVE_MAPPER)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_handleWaveOut != IntPtr.Zero)
            throw new InvalidOperationException("Device is already open.");

        var format = new WaveFormatEx(_channels, _sampleRate, _bitsPerSample);
        IntPtr eventHandle = _playbackCompleteEvent.SafeWaitHandle.DangerousGetHandle();
        int result = NativeMethods.waveOutOpen(out IntPtr handle, deviceId, format, eventHandle, IntPtr.Zero, CALLBACK_EVENT);
        if (result != MMSYSERR_NOERROR)
            throw new InvalidOperationException($"waveOutOpen failed with error code {result}");

        _handleWaveOut = handle;
    }

    public void Close()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_handleWaveOut == IntPtr.Zero)
            throw new InvalidOperationException("Device is not open.");

        int result = NativeMethods.waveOutClose(_handleWaveOut);
        if (result != MMSYSERR_NOERROR)
            throw new InvalidOperationException($"waveOutClose failed with error code {result}");

        _handleWaveOut = IntPtr.Zero;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (_handleWaveOut != IntPtr.Zero)
        {
            this.Close();
        }
        if (disposing)
        {
            _playbackCompleteEvent.Dispose();
        }
        _disposed = true;
    }

    ~WaveOutPlayer()
    {
        this.Dispose(false);
    }
}
