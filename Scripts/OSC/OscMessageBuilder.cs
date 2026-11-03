using System;
using System.Collections.Generic;
using System.Text;

namespace ChatdollKit.OSC
{
    public static class OscMessageBuilder
    {
        public static byte[] BuildMessage(string address, object value)
        {
            var addressBytes = Encoding.UTF8.GetBytes(address);
            var addressPaddedLength = (addressBytes.Length + 4) & ~3;

            string typeString = ",";
            byte[] valueBytes = new byte[0];

            if (value is int intValue)
            {
                typeString += "i";
                valueBytes = BitConverter.GetBytes(intValue);
                if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);
            }
            else if (value is float floatValue)
            {
                typeString += "f";
                valueBytes = BitConverter.GetBytes(floatValue);
                if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);
            }
            else if (value is string stringValue)
            {
                typeString += "s";
                var strBytes = Encoding.UTF8.GetBytes(stringValue);
                var strPaddedLength = (strBytes.Length + 4) & ~3;
                valueBytes = new byte[strPaddedLength];
                Buffer.BlockCopy(strBytes, 0, valueBytes, 0, strBytes.Length);
            }
            else if (value is bool boolValue)
            {
                typeString += boolValue ? "T" : "F";
            }

            var typeBytes = Encoding.UTF8.GetBytes(typeString);
            var typePaddedLength = (typeBytes.Length + 4) & ~3;

            var result = new byte[addressPaddedLength + typePaddedLength + valueBytes.Length];

            Buffer.BlockCopy(addressBytes, 0, result, 0, addressBytes.Length);
            Buffer.BlockCopy(typeBytes, 0, result, addressPaddedLength, typeBytes.Length);
            Buffer.BlockCopy(valueBytes, 0, result, addressPaddedLength + typePaddedLength, valueBytes.Length);

            return result;
        }
    }
}
