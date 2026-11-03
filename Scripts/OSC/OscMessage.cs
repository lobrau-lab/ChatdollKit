using System;
using System.Text;

namespace ChatdollKit.OSC
{
    // A very basic OSC message builder without relying on external packages
    public class OscMessage
    {
        public string Address { get; private set; }
        public object Value { get; private set; }

        public OscMessage(string address, object value)
        {
            Address = address;
            Value = value;
        }

        public byte[] ToBytes()
        {
            // Placeholder for OSC encoding
            return new byte[0];
        }
    }
}
