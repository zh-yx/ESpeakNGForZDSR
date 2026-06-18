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
    private readonly object _lockObj = new object();

    private IntPtr _handleWaveOut;
    private WaveDataBlock? _currentData;
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

    public void Push(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_handleWaveOut == IntPtr.Zero)
            throw new InvalidOperationException("Device is not open.");

        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        if (data.Length % _blockAlign != 0)
            throw new ArgumentException($"Data length ({data.Length} bytes) is not aligned to block size ({_blockAlign} bytes). For {_channels}-channel {_bitsPerSample}-bit audio, each frame is {_blockAlign} bytes.", nameof(data));

        lock (_lockObj)
        {
            var dataBlock = this.PrepareAndWriteData(data);
            if (_currentData != null)
            {
                this.Synchronize(_currentData);
                _currentData.Dispose();
            }
            _currentData = dataBlock;
        }
    }

    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_handleWaveOut == IntPtr.Zero)
            throw new InvalidOperationException("Device is not open.");

        int result = NativeMethods.waveOutReset(_handleWaveOut);
        if (result != MMSYSERR_NOERROR)
            throw new InvalidOperationException($"waveOutReset failed with error code {result}");

        lock (_lockObj)
        {
            if (_currentData != null)
            {
                this.Synchronize(_currentData);
                _currentData.Dispose();
                _currentData = null;
            }
        }
    }

    public void Close()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_handleWaveOut == IntPtr.Zero)
            throw new InvalidOperationException("Device is not open.");

        this.Reset();
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

    private WaveDataBlock PrepareAndWriteData(byte[] data)
    {
        var dataBlock = new WaveDataBlock(data);

        int result = NativeMethods.waveOutPrepareHeader(_handleWaveOut, dataBlock.WaveHeaderPointer, WaveHeader.Size);
        if (result != MMSYSERR_NOERROR)
        {
            dataBlock.Dispose();
            throw new InvalidOperationException($"waveOutPrepareHeader failed with error code {result}");
        }

        result = NativeMethods.waveOutWrite(_handleWaveOut, dataBlock.WaveHeaderPointer, WaveHeader.Size);
        if (result != MMSYSERR_NOERROR)
        {
            this.UnprepareData(dataBlock);
            dataBlock.Dispose();
            throw new InvalidOperationException($"waveOutWrite failed with error code {result}");
        }
        return dataBlock;
    }

    private void Synchronize(WaveDataBlock dataBlock)
    {
        while (!dataBlock.IsDone)
        {
            _playbackCompleteEvent.WaitOne();
        }
        this.UnprepareData(dataBlock);
    }

    private void UnprepareData(WaveDataBlock dataBlock)
    {
        int result = NativeMethods.waveOutUnprepareHeader(_handleWaveOut, dataBlock.WaveHeaderPointer, WaveHeader.Size);
        if (result != MMSYSERR_NOERROR)
            throw new InvalidOperationException($"waveOutUnprepareHeader failed with error code {result}");
    }

    ~WaveOutPlayer()
    {
        this.Dispose(false);
    }
}
