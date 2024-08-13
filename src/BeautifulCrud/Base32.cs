using System.Text;

namespace BeautifulCrud;

public class Base32
{
    private static readonly char[] _base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
    private static readonly int[] _base32Lookup = new int[256];

    static Base32()
    {
        for (int i = 0; i < _base32Lookup.Length; i++)
        {
            _base32Lookup[i] = -1;
        }
        for (int i = 0; i < _base32Chars.Length; i++)
        {
            _base32Lookup[_base32Chars[i]] = i;
        }
    }

    public static string ToBase32String(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder((bytes.Length * 8 + 4) / 5);

        for (int i = 0; i < bytes.Length;)
        {
            int currentByte = bytes[i] & 0xFF;
            int digit;

            // Is the current digit going to span a byte boundary?
            if (i + 1 < bytes.Length)
            {
                int nextByte = bytes[i + 1] & 0xFF;
                digit = currentByte >> 3;
                sb.Append(_base32Chars[digit]);
                digit = (currentByte & 0x07) << 2 | nextByte >> 6;
                sb.Append(_base32Chars[digit]);
                digit = (nextByte & 0x3F) >> 1;
                sb.Append(_base32Chars[digit]);
                digit = (nextByte & 0x01) << 4;

                if (i + 2 < bytes.Length)
                {
                    nextByte = bytes[i + 2] & 0xFF;
                    digit |= nextByte >> 4;
                    sb.Append(_base32Chars[digit]);
                    digit = (nextByte & 0x0F) << 1;

                    if (i + 3 < bytes.Length)
                    {
                        nextByte = bytes[i + 3] & 0xFF;
                        digit |= nextByte >> 7;
                        sb.Append(_base32Chars[digit]);
                        digit = (nextByte & 0x7F) >> 2;
                        sb.Append(_base32Chars[digit]);
                        digit = (nextByte & 0x03) << 3;

                        if (i + 4 < bytes.Length)
                        {
                            nextByte = bytes[i + 4] & 0xFF;
                            digit |= nextByte >> 5;
                            sb.Append(_base32Chars[digit]);
                            digit = nextByte & 0x1F;
                            sb.Append(_base32Chars[digit]);
                        }
                        else
                        {
                            sb.Append(_base32Chars[digit]);
                        }
                    }
                    else
                    {
                        sb.Append(_base32Chars[digit]);
                    }
                }
                else
                {
                    sb.Append(_base32Chars[digit]);
                }
            }
            else
            {
                digit = currentByte >> 3;
                sb.Append(_base32Chars[digit]);
                digit = (currentByte & 0x07) << 2;
                sb.Append(_base32Chars[digit]);
            }

            i += 5;
        }

        return sb.ToString();
    }

    public static byte[] FromBase32String(string base32String)
    {
        int numBytes = base32String.Length * 5 / 8;
        byte[] bytes = new byte[numBytes];

        int buffer = 0;
        int bitsLeft = 0;
        int byteIndex = 0;

        foreach (char c in base32String)
        {
            if (_base32Lookup[c] == -1)
            {
                throw new ArgumentException("Invalid character found in base32 string.");
            }

            buffer <<= 5;
            buffer |= _base32Lookup[c] & 0x1F;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bytes[byteIndex++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }

        return bytes;
    }
}