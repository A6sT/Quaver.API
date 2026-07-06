using System;
using System.IO;

namespace Quaver.API.Helpers
{
    public static class ImageHelper
    {
        private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        public static bool TryGetDimensions(string path, out int width, out int height)
        {
            width = 0;
            height = 0;

            try
            {
                using (var stream = File.OpenRead(path))
                    return TryGetDimensions(stream, out width, out height);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryGetDimensions(Stream stream, out int width, out int height)
        {
            width = 0;
            height = 0;

            var header = new byte[26];
            var read = stream.Read(header, 0, header.Length);

            if (read >= 24 && IsPng(header))
            {
                width = ReadInt32BigEndian(header, 16);
                height = ReadInt32BigEndian(header, 20);
                return IsValidDimension(width, height);
            }

            if (read >= 10 && IsGif(header))
            {
                width = ReadUInt16LittleEndian(header, 6);
                height = ReadUInt16LittleEndian(header, 8);
                return IsValidDimension(width, height);
            }

            if (read >= 26 && IsBmp(header))
            {
                width = Math.Abs(ReadInt32LittleEndian(header, 18));

                var signedHeight = ReadInt32LittleEndian(header, 22);
                height = signedHeight == int.MinValue ? int.MaxValue : Math.Abs(signedHeight);

                return IsValidDimension(width, height);
            }

            stream.Position = 0;
            return TryGetJpegDimensions(stream, out width, out height);
        }

        private static bool TryGetJpegDimensions(Stream stream, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8)
                return false;

            while (stream.Position < stream.Length)
            {
                var markerPrefix = stream.ReadByte();

                while (markerPrefix != -1 && markerPrefix != 0xFF)
                    markerPrefix = stream.ReadByte();

                if (markerPrefix == -1)
                    return false;

                var marker = stream.ReadByte();

                while (marker == 0xFF)
                    marker = stream.ReadByte();

                if (marker == -1 || marker == 0xD9 || marker == 0xDA)
                    return false;

                if (marker == 0x01 || marker >= 0xD0 && marker <= 0xD7)
                    continue;

                var length = ReadUInt16BigEndian(stream);

                if (length < 2)
                    return false;

                if (IsJpegStartOfFrame(marker))
                {
                    if (length < 7 || stream.ReadByte() == -1)
                        return false;

                    height = ReadUInt16BigEndian(stream);
                    width = ReadUInt16BigEndian(stream);
                    return IsValidDimension(width, height);
                }

                stream.Seek(length - 2, SeekOrigin.Current);
            }

            return false;
        }

        private static bool IsPng(byte[] header)
        {
            for (var i = 0; i < PngSignature.Length; i++)
            {
                if (header[i] != PngSignature[i])
                    return false;
            }

            return true;
        }

        private static bool IsGif(byte[] header) =>
            header[0] == 'G' && header[1] == 'I' && header[2] == 'F' &&
            header[3] == '8' && (header[4] == '7' || header[4] == '9') && header[5] == 'a';

        private static bool IsBmp(byte[] header) => header[0] == 'B' && header[1] == 'M';

        private static bool IsJpegStartOfFrame(int marker)
        {
            switch (marker)
            {
                case 0xC0:
                case 0xC1:
                case 0xC2:
                case 0xC3:
                case 0xC5:
                case 0xC6:
                case 0xC7:
                case 0xC9:
                case 0xCA:
                case 0xCB:
                case 0xCD:
                case 0xCE:
                case 0xCF:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsValidDimension(int width, int height) => width > 0 && height > 0;

        private static int ReadUInt16BigEndian(Stream stream)
        {
            var upper = stream.ReadByte();
            var lower = stream.ReadByte();

            if (upper == -1 || lower == -1)
                return 0;

            return upper << 8 | lower;
        }

        private static int ReadUInt16LittleEndian(byte[] bytes, int offset) => bytes[offset] | bytes[offset + 1] << 8;

        private static int ReadInt32BigEndian(byte[] bytes, int offset) =>
            bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 3];

        private static int ReadInt32LittleEndian(byte[] bytes, int offset) =>
            bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16 | bytes[offset + 3] << 24;
    }
}
