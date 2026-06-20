using System;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal partial class ESpeakApi
{
    private const int ESPEAK_AUDIO_OUTPUT_SYNCHRONOUS = 0x0002;
    private const int ESPEAK_INITIALIZE_DONT_EXIT = 0x8000;
    private const int ESPEAK_SYNTH_CALLBACK_CONTINUE = 0;
    private const int ESPEAK_SYNTH_CALLBACK_ABORT = 1;
    private const int ESPEAK_CHARS_UTF8 = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    public struct EspeakVoice
    {
        public IntPtr NamePtr;
        public IntPtr LanguagesPtr;
        public IntPtr IdentifierPtr;
        public byte Gender;
        public byte Age;
        public byte Variant;
        public byte Xx1;
        public int Score;
        public IntPtr Spare;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int SynthCallbackDelegate(IntPtr wavSamples, int numSamples, IntPtr events);

    private partial class NativeMethods
    {
        private const string libESpeakNGName = "libespeak-ng.dll";

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int espeak_Initialize(int audioOutput, int bufferLength, [MarshalAs(UnmanagedType.LPUTF8Str)] string dataPath, int options);

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr espeak_Info(out IntPtr dataPathPtr);

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void espeak_SetSynthCallback(SynthCallbackDelegate synthCallback);

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr espeak_ListVoices(IntPtr voiceSpec);

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int espeak_SetVoiceByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int espeak_Synth([MarshalAs(UnmanagedType.LPUTF8Str)] string text, IntPtr size, int position, int positionType, int endPosition, int flags, out int uniqueIdentifier, IntPtr userData);

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int espeak_Cancel();

        [DllImport(libESpeakNGName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int espeak_Terminate();
    }
}
