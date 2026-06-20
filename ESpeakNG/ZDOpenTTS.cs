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
        return true;
    }

    [UnmanagedCallersOnly(EntryPoint = "GetVoiceCount")]
    public static int GetVoiceCount()
    {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "GetVoiceName")]
    public static void GetVoiceName(int vid, IntPtr voiceNameBuffer)
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "SetResponseSpeed")]
    public static void SetResponseSpeed(int value)
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "SetParam")]
    public static void SetParam(int type, int value)
    {
    }

    [UnmanagedCallersOnly(EntryPoint = "IsSpeaking")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static bool IsSpeaking()
    {
        return false;
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
    }

    [UnmanagedCallersOnly(EntryPoint = "Stop")]
    public static void Stop()
    {
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
        return 0;
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
    }
}
