using System;
using System.Runtime.InteropServices;

namespace ESpeakNG;

internal partial class WaveOutPlayer
{
    private sealed class WaveDataBlock : IDisposable
    {
        private readonly byte[] _data;
        private GCHandle _dataHandle;
        private GCHandle _headerHandle;
        private IntPtr _headerPtr;

        public WaveDataBlock(byte[] data)
        {
            _data = data;
            this.AllocData();
        }

        public bool IsDone => _headerHandle.Target is WaveHeader hdr && hdr.IsDone;
        public bool IsPrepared => _headerHandle.Target is WaveHeader hdr && hdr.IsPrepared;
        public IntPtr WaveHeaderPointer => _headerPtr;

        public void Dispose()
        {
            this.FreeData();
            GC.SuppressFinalize(this);
        }

        private void AllocData()
        {
            _dataHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);

            var header = new WaveHeader(_dataHandle.AddrOfPinnedObject(), _data.Length);
            _headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
            _headerPtr = _headerHandle.AddrOfPinnedObject();
        }

        private void FreeData()
        {
            if (_headerHandle.IsAllocated)
                _headerHandle.Free();

            if (_dataHandle.IsAllocated)
                _dataHandle.Free();

            _headerPtr = IntPtr.Zero;
        }

        ~WaveDataBlock()
        {
            this.FreeData();
        }
    }
}
