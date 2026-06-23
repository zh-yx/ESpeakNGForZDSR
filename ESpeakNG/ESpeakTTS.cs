using System;
using System.Linq;

namespace ESpeakNG;

internal static class ESpeakTTS
{
    private const int _bufferLength = 100;
    private const string _dataDirname = "espeak-ng-data";
    private static readonly object _lockObj = new object();
    private static int _sampleRate;
    private static ESpeakApi.VoiceInfo[]? _voiceInfos;
    private static WaveOutPlayer? _wavePlayer;
    private static volatile bool _stopSpeak;
    private static bool _isSpeaking;

    private static bool DataReceiver(byte[] data)
    {
        if (_stopSpeak) return false;
        _wavePlayer?.Push(data);
        return true;
    }

    private static void SpeakTextAsync(string text, int pitch)
    {
        lock (_lockObj)
        {
            bool pitchFlag = pitch >= 0 && pitch <= 100; // Out of range values indicate don't set pitch value, for example -1.
            int pitchValue = ESpeakApi.GetParameter(ESpeakApi.EspeakParameter.Pitch);
            if (pitchFlag) ESpeakApi.SetParameter(ESpeakApi.EspeakParameter.Pitch, pitch);
            _isSpeaking = true;
            _stopSpeak = false;
            ESpeakApi.Synth(text);
            _isSpeaking = false;
            if (pitchFlag) ESpeakApi.SetParameter(ESpeakApi.EspeakParameter.Pitch, pitchValue);
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
        string? dataPath = Utility.FindOnPath(_dataDirname);
        if (dataPath == null) return false;
        if (ESpeakApi.Initialize(dataPath, _bufferLength, DataReceiver, out _sampleRate))
        {
            _wavePlayer = new WaveOutPlayer(1, _sampleRate, 16); // 1 channel, 16 bit per sample.
            _wavePlayer.Open();
            return true;
        }
        return false;
    }

    public static int GetVoiceCount()
    {
        _voiceInfos = ESpeakApi.ListVoiceInfos().ToArray();
        return _voiceInfos.Length;
    }

    public static string GetVoiceName(int index)
    {
        return _voiceInfos?[index].ToString() ?? $"{nameof(ESpeakNG)} voice {index}";
    }

    public static void SetParam(int type, int value)
    {
        switch (type)
        {
            case 0:
                string id = _voiceInfos?[value].Identifier ?? ESpeakApi.ESPEAK_DEFAULT_VOICE;
                ESpeakApi.SetVoiceByName(id);
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

    public static void Speak(string text, int pitch)
    {
        _ = Task.Run(() => SpeakTextAsync(text, pitch)).ConfigureAwait(false);
    }

    public static void Stop()
    {
        _stopSpeak = true;
        _wavePlayer?.Reset();
    }

    public static int GetAudioFormat(out int bitPerSample)
    {
        bitPerSample = 16; // EspeakNG generates the audio of 16 bit  per sample.
        return _sampleRate;
    }

    public static void UnInitialize()
    {
        ESpeakApi.Terminate();
        _voiceInfos = null;
        _wavePlayer?.Dispose();
        _wavePlayer = null;
    }
}
