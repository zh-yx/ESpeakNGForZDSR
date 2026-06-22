using System;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal partial class ESpeakApi
{
    private static readonly SynthCallbackDelegate _synthCallback = OnSynthCallback;
    private static Func<byte[], bool>? _dataReceiver;

    private static int OnSynthCallback(IntPtr waveSample, int numSample, IntPtr events)
    {
        if (numSample > 0 && waveSample != IntPtr.Zero)
        {
            byte[] data = new byte[numSample * 2]; // 16 bit per sample.
            Marshal.Copy(waveSample, data, 0, data.Length);
            bool allowContinue = _dataReceiver?.Invoke(data) ?? true;
            if (!allowContinue) return ESPEAK_SYNTH_CALLBACK_ABORT;
        }
        return ESPEAK_SYNTH_CALLBACK_CONTINUE;
    }

    public static bool Initialize(string dataPath, int bufferLength, Func<byte[], bool> synthDataReceiver, out int sampleRate)
    {
        sampleRate = NativeMethods.espeak_Initialize(ESPEAK_AUDIO_OUTPUT_SYNCHRONOUS, bufferLength, dataPath, ESPEAK_INITIALIZE_DONT_EXIT);
        if (sampleRate == 0) return false;

        NativeMethods.espeak_SetSynthCallback(_synthCallback);
        _dataReceiver = synthDataReceiver;
        return true;
    }

    public static string? GetVersionAndDataPath(out string? dataPath)
    {
        IntPtr versionPtr = NativeMethods.espeak_Info(out IntPtr pathPtr);
        string? version = Marshal.PtrToStringUTF8(versionPtr);
        dataPath = Marshal.PtrToStringUTF8(pathPtr);
        return version;
    }

    public static IEnumerable<string> ListVoiceNames()
    {
        IntPtr voicesPtr = NativeMethods.espeak_ListVoices(IntPtr.Zero);
        if (voicesPtr == IntPtr.Zero) yield break;
        int i = 0;
        while (true)
        {
            IntPtr voicePtr = Marshal.ReadIntPtr(voicesPtr, IntPtr.Size * i);
            if (voicePtr == IntPtr.Zero) yield break;

            EspeakVoice voice = Marshal.PtrToStructure<EspeakVoice>(voicePtr);
            string? name = Marshal.PtrToStringUTF8(voice.NamePtr);
            if (name == null) continue;
            yield return name;
            i++;
        }
    }

    public static void SetVoiceByName(string voiceName)
    {
        NativeMethods.espeak_SetVoiceByName(voiceName);
    }

    public static int GetParameter(EspeakParameter paramType)
    {
        return NativeMethods.espeak_GetParameter(paramType, true);
    }

    public static void SetParameter(EspeakParameter paramType, int value)
    {
        NativeMethods.espeak_SetParameter(paramType, value, false);
    }

    public static void Synth(string text)
    {
        NativeMethods.espeak_Synth(text, IntPtr.Zero, 0, 0, 0, ESPEAK_CHARS_UTF8, out int _, IntPtr.Zero);
    }

    public static void Terminate()
    {
        NativeMethods.espeak_Terminate();
    }
}
