using System;

namespace SaveState
{
    class Address : MemoryDescription
    {

        private int location;

        public Address(String addressDescription, int numberOfBytes, int location) : base(addressDescription, numberOfBytes)
        {
            this.location = location;
        }

        public int Location
        {
            get => location;
            set => location = value;
        }
    }
}

