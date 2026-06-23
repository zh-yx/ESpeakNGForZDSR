using System;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal partial class ESpeakApi
{
    public sealed class VoiceInfo
    {
        public required string Identifier { get; set; }
        public required string Name { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Identifier);
        }

        public static VoiceInfo? ParseFromPtr(IntPtr voicePtr)
        {
            if (voicePtr == IntPtr.Zero) return null;
            EspeakVoice voice = Marshal.PtrToStructure<EspeakVoice>(voicePtr);
            string? identifier = Marshal.PtrToStringUTF8(voice.IdentifierPtr);
            if (identifier == null) return null;
            string name = Marshal.PtrToStringUTF8(voice.NamePtr) ?? string.Empty;
            VoiceInfo info = new VoiceInfo() { Identifier = identifier, Name = name };
            info.Identifier = identifier;
            info.Name = name;
            return info;
        }
    }
}
