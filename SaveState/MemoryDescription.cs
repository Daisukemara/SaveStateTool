using System;

namespace SaveState
{
    class MemoryDescription
    {
        private String addressDescription;
        private int numberOfBytes;

        public MemoryDescription(String addressDescription, int numberOfBytes)
        {
            this.addressDescription = addressDescription;
            this.numberOfBytes = numberOfBytes;

        }

        public string AddressDescription
        {
            get => addressDescription;
            set => addressDescription = value;
        }

        public int NumberOfBytes
        {
            get => numberOfBytes;
            set => numberOfBytes = value;
        }
    }
}
