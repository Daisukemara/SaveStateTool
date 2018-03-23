using System;

namespace SaveState
{
    class Pointer : MemoryDescription
    {
        private IntPtr ptr;
        private int[] offsets;

        public Pointer(String addressDescription, int numberOfBytes, IntPtr ptr, int[] offset) : base(addressDescription, numberOfBytes)
        {
            this.ptr = ptr;
            this.offsets = offset;
        }

        public IntPtr Ptr
        {
            get => ptr;
            set => ptr = value;
        }

        public int[] Offsets
        {
            get => offsets;
            set => offsets = value;
        }
    }
}
