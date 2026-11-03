using System;
using System.Text;

namespace ChatdollKit.OSC
{
    public static class VRChatV2OscMessageBuilder
    {
        // Special method for VRChat chatbox which accepts a string and a boolean
        public static byte[] BuildChatboxMessage(string text, bool typingIndicator = true, bool bypassPlaybook = false)
        {
            string address = "/chatbox/input";
            var addressBytes = Encoding.UTF8.GetBytes(address);
            var addressPaddedLength = (addressBytes.Length + 4) & ~3;

            // For VRChat /chatbox/input, typical signature is (string text, bool typingIndicator, bool bypassPlaybook)
            // But usually just (string text, bool typingIndicator) works too.
            string typeString = ",sT"; // string, True (for typing indicator)

            var textBytes = Encoding.UTF8.GetBytes(text);
            var textPaddedLength = (textBytes.Length + 4) & ~3;
            byte[] valueBytes = new byte[textPaddedLength];
            Buffer.BlockCopy(textBytes, 0, valueBytes, 0, textBytes.Length);

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
