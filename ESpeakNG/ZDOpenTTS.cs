using System;
using System.Runtime.InteropServices;

namespace ESpeakNG;

public static class ZDOpenTTS
{
    [UnmanagedCallersOnly(EntryPoint = "GetOpenTTSInfo")]
    public static int GetOpenTTSInfo(IntPtr pReserved1, IntPtr pReserved2, IntPtr pReserved3)
    {
        return 2; //version 2.
    }

    [UnmanagedCallersOnly(EntryPoint = "Initial")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static bool Initial()
    {
        // TODO: Path strings should not be hard coding.
        string workPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "zdsr", "common", "opentts", nameof(ESpeakNG));
        Utility.EnsurePath(workPath);
        return ESpeakTTS.Initialize();
    }

    [UnmanagedCallersOnly(EntryPoint = "GetVoiceCount")]
    public static int GetVoiceCount()
    {
        return ESpeakTTS.GetVoiceCount();
    }

    [UnmanagedCallersOnly(EntryPoint = "GetVoiceName")]
    public static void GetVoiceName(int vid, IntPtr voiceNameBuffer)
    {
        string name = ESpeakTTS.GetVoiceName(vid);
        Marshal.Copy(name.ToCharArray(), 0, voiceNameBuffer, name.Length);
        Marshal.WriteInt16(voiceNameBuffer + name.Length * sizeof(char), '\0');
    }

    [UnmanagedCallersOnly(EntryPoint = "SetResponseSpeed")]
    public static void SetResponseSpeed(int value)
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "SetParam")]
    public static void SetParam(int type, int value)
    {
        ESpeakTTS.SetParam(type, value);
    }

    [UnmanagedCallersOnly(EntryPoint = "IsSpeaking")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static bool IsSpeaking()
    {
        return ESpeakTTS.IsSpeaking();
    }

    [UnmanagedCallersOnly(EntryPoint = "IsControlNumricType")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static bool IsControlNumricType()
    {
        return false;
    }

    [UnmanagedCallersOnly(EntryPoint = "Speak")]
    public static void Speak(IntPtr textPtr, int pitch)
    {
        string? text = Marshal.PtrToStringUni(textPtr);
        if (text == null) return;
        ESpeakTTS.Speak(text, pitch);
    }

    [UnmanagedCallersOnly(EntryPoint = "Stop")]
    public static void Stop()
    {
        ESpeakTTS.Stop();
    }

    [UnmanagedCallersOnly(EntryPoint = "Pause")]
    public static void Pause()
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "Resume")]
    public static void Resume()
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "GetAudioFormat")]
    public static int GetAudioFormat(IntPtr ptrBitsPerSample)
    {
        int sampleRate = ESpeakTTS.GetAudioFormat(out int bitPerSample);
        Marshal.WriteInt32(ptrBitsPerSample, bitPerSample);
        return sampleRate;
    }

    [UnmanagedCallersOnly(EntryPoint = "TextToAudio")]
    public static int TextToAudio(IntPtr textPtr, IntPtr bufferPtr)
    {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "OpenSettings")]
    public static void OpenSettings()
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "UnInitial")]
    public static void UnInitial()
    {
        ESpeakTTS.UnInitialize();
    }
}
