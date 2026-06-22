using System;
using System.Linq;

namespace ESpeakNG;

internal static class ESpeakTTS
{
    private const int _bufferLength = 100;
    private const string _dateDirname = "espeak-ng-data";
    private static readonly object _lockObj = new object();
    private static string[]? _voiceNames;
    private static WaveOutPlayer? _wavePlayer;
    private static volatile bool _stopSpeak;
    private static bool _isSpeaking;

    private static bool DataReceiver(byte[] data)
    {
        if (_stopSpeak) return false;
        _wavePlayer?.Push(data);
        return true;
    }

    private static void SpeakTextAsync(string text)
    {
        lock (_lockObj)
        {
            _isSpeaking = true;
            _stopSpeak = false;
            ESpeakApi.Synth(text);
            _isSpeaking = false;
        }
    }

    private static void ChangeAudioOutputDevice(int deviceId)
    {
        if (_wavePlayer == null) return;
        if (_wavePlayer.IsOpen) _wavePlayer.Close();
        _wavePlayer.Open(deviceId);
    }

    public static bool Initialize()
    {
        string? dataPath = Utility.FindOnPath(_dateDirname);
        if (dataPath == null) return false;
        if (ESpeakApi.Initialize(dataPath, _bufferLength, DataReceiver, out int rate))
        {
            _wavePlayer = new WaveOutPlayer(1, rate, 16); // 1 channel, 16 bit per sample.
            _wavePlayer.Open();
            return true;
        }
        return false;
    }

    public static int GetVoiceCount()
    {
        _voiceNames = ESpeakApi.ListVoiceNames().ToArray();
        return _voiceNames.Length;
    }

    public static string GetVoiceName(int index)
    {
        if (_voiceNames == null || index < 0 || index >= _voiceNames.Length || _voiceNames[index] == null) return ESpeakApi.ESPEAK_DEFAULT_VOICE;
        return _voiceNames[index];
    }

    public static void SetParam(int type, int value)
    {
        switch (type)
        {
            case 0:
                ESpeakApi.SetVoiceByName(GetVoiceName(value));
                break;
            case 1:
                const int rateRange = ESpeakApi.ESPEAK_RATE_MAXIMUM - ESpeakApi.ESPEAK_RATE_MINIMUM;
                int rate = (int)(value * rateRange / 100.0) + ESpeakApi.ESPEAK_RATE_MINIMUM;
                ESpeakApi.SetParameter(ESpeakApi.EspeakParameter.Rate, rate);
                break;
            case 2:
                ESpeakApi.SetParameter(ESpeakApi.EspeakParameter.Volume, value);
                break;
            case 3:
                ESpeakApi.SetParameter(ESpeakApi.EspeakParameter.Pitch, value);
                break;
            case 4:
                ChangeAudioOutputDevice(value);
                break;
            case 8:
                ESpeakApi.SetParameter(ESpeakApi.EspeakParameter.Range, value);
                break;
        }
    }

    public static bool IsSpeaking()
    {
        return _isSpeaking;
    }

    public static void Speak(string text)
    {
        _ = Task.Run(() => SpeakTextAsync(text)).ConfigureAwait(false);
    }

    public static void Stop()
    {
        _stopSpeak = true;
        _wavePlayer?.Reset();
    }

    public static void UnInitialize()
    {
        ESpeakApi.Terminate();
        _voiceNames = null;
        _wavePlayer?.Dispose();
        _wavePlayer = null;
    }
}
