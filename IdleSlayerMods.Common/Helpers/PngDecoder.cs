using System.IO.Compression;

namespace IdleSlayerMods.Common.Helpers;

/// <summary>
/// Minimal managed PNG decoder.
/// <para>
/// Exists because <c>UnityEngine.ImageConversion.LoadImage</c> cannot be called from mods on this game:
/// in the Unity 6 interop assemblies every LoadImage overload forwards to a <c>ReadOnlySpan&lt;byte&gt;</c>
/// overload whose generated body calls <c>Il2CppSystem.ReadOnlySpan`1.GetPinnableReference()</c>, which fails at
/// runtime with MissingMethodException. Decoding in managed code and uploading with
/// <c>Texture2D.SetPixels32</c> avoids that path entirely.
/// </para>
/// Supports all PNG colour types (greyscale, RGB, palette, grey+alpha, RGBA), bit depths 1–16,
/// tRNS transparency, and Adam7 interlacing.
/// </summary>
internal static class PngDecoder
{
    private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    /// <summary>Decoded image: RGBA8, rows top-to-bottom.</summary>
    internal sealed class Image
    {
        internal int Width;
        internal int Height;
        internal byte[] Rgba;
        internal string Format;
    }

    internal static bool IsPng(byte[] data)
    {
        if (data == null || data.Length < Signature.Length) return false;
        for (var i = 0; i < Signature.Length; i++)
            if (data[i] != Signature[i]) return false;
        return true;
    }

    internal static Image Decode(byte[] data)
    {
        if (!IsPng(data)) throw new InvalidDataException("Not a PNG file (bad signature).");

        int width = 0, height = 0, bitDepth = 0, colorType = 0, interlace = 0;
        byte[] palette = null, paletteAlpha = null, transparentKey = null;
        using var idat = new MemoryStream();

        var pos = Signature.Length;
        while (pos + 8 <= data.Length)
        {
            var length = ReadInt32BE(data, pos);
            var type = System.Text.Encoding.ASCII.GetString(data, pos + 4, 4);
            var start = pos + 8;
            if (length < 0 || start + length > data.Length) throw new InvalidDataException($"Truncated chunk '{type}'.");

            switch (type)
            {
                case "IHDR":
                    width = ReadInt32BE(data, start);
                    height = ReadInt32BE(data, start + 4);
                    bitDepth = data[start + 8];
                    colorType = data[start + 9];
                    if (data[start + 10] != 0 || data[start + 11] != 0)
                        throw new InvalidDataException("Unsupported PNG compression or filter method.");
                    interlace = data[start + 12];
                    break;
                case "PLTE":
                    palette = Slice(data, start, length);
                    break;
                case "tRNS":
                    if (colorType == 3) paletteAlpha = Slice(data, start, length);
                    else transparentKey = Slice(data, start, length);
                    break;
                case "IDAT":
                    idat.Write(data, start, length);
                    break;
            }

            pos = start + length + 4; // skip CRC
            if (type == "IEND") break;
        }

        if (width <= 0 || height <= 0) throw new InvalidDataException("Missing or invalid IHDR.");
        if (colorType == 3 && palette == null) throw new InvalidDataException("Palette image without PLTE chunk.");

        var channels = colorType switch
        {
            0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4,
            _ => throw new InvalidDataException($"Unsupported PNG colour type {colorType}.")
        };
        var bitsPerPixel = channels * bitDepth;
        var bytesPerPixel = Math.Max(1, bitsPerPixel / 8);

        byte[] raw;
        idat.Position = 0;
        using (var z = new ZLibStream(idat, CompressionMode.Decompress))
        using (var outStream = new MemoryStream())
        {
            z.CopyTo(outStream);
            raw = outStream.ToArray();
        }

        var rgba = new byte[width * height * 4];
        var ctx = new PixelContext(colorType, bitDepth, palette, paletteAlpha, transparentKey);

        if (interlace == 0)
        {
            DecodePass(raw, 0, width, height, bitsPerPixel, bytesPerPixel, ctx, rgba, width, 0, 0, 1, 1);
        }
        else
        {
            int[] xStart = { 0, 4, 0, 2, 0, 1, 0 }, yStart = { 0, 0, 4, 0, 2, 0, 1 };
            int[] xStep = { 8, 8, 4, 4, 2, 2, 1 }, yStep = { 8, 8, 8, 4, 4, 2, 2 };
            var offset = 0;
            for (var p = 0; p < 7; p++)
            {
                var pw = (width - xStart[p] + xStep[p] - 1) / xStep[p];
                var ph = (height - yStart[p] + yStep[p] - 1) / yStep[p];
                if (pw <= 0 || ph <= 0) continue;
                offset = DecodePass(raw, offset, pw, ph, bitsPerPixel, bytesPerPixel, ctx, rgba, width,
                    xStart[p], yStart[p], xStep[p], yStep[p]);
            }
        }

        var format = $"{(colorType switch { 0 => "grey", 2 => "rgb", 3 => "palette", 4 => "grey+alpha", _ => "rgba" })} " +
                     $"{bitDepth}-bit{(interlace != 0 ? " interlaced" : "")}{(paletteAlpha != null || transparentKey != null ? " +tRNS" : "")}";
        return new Image { Width = width, Height = height, Rgba = rgba, Format = format };
    }

    private static int DecodePass(byte[] raw, int offset, int w, int h, int bitsPerPixel, int bytesPerPixel,
        PixelContext ctx, byte[] rgba, int fullWidth, int x0, int y0, int dx, int dy)
    {
        var stride = (w * bitsPerPixel + 7) / 8;
        var prev = new byte[stride];
        var cur = new byte[stride];

        for (var y = 0; y < h; y++)
        {
            if (offset + 1 + stride > raw.Length) throw new InvalidDataException("Image data is truncated.");
            var filter = raw[offset];
            Buffer.BlockCopy(raw, offset + 1, cur, 0, stride);
            offset += 1 + stride;
            Unfilter(filter, cur, prev, bytesPerPixel);

            for (var x = 0; x < w; x++)
            {
                var dst = ((y0 + y * dy) * fullWidth + (x0 + x * dx)) * 4;
                ctx.WritePixel(cur, x, rgba, dst);
            }

            (prev, cur) = (cur, prev);
        }

        return offset;
    }

    private static void Unfilter(byte filter, byte[] cur, byte[] prev, int bpp)
    {
        switch (filter)
        {
            case 0: return;
            case 1:
                for (var i = bpp; i < cur.Length; i++) cur[i] += cur[i - bpp];
                return;
            case 2:
                for (var i = 0; i < cur.Length; i++) cur[i] += prev[i];
                return;
            case 3:
                for (var i = 0; i < cur.Length; i++)
                {
                    var left = i >= bpp ? cur[i - bpp] : 0;
                    cur[i] += (byte)((left + prev[i]) >> 1);
                }
                return;
            case 4:
                for (var i = 0; i < cur.Length; i++)
                {
                    int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
                    int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
                    cur[i] += (byte)(pa <= pb && pa <= pc ? a : pb <= pc ? b : c);
                }
                return;
            default:
                throw new InvalidDataException($"Unknown PNG filter type {filter}.");
        }
    }

    private sealed class PixelContext
    {
        private readonly int _colorType, _bitDepth;
        private readonly byte[] _palette, _paletteAlpha, _key;

        internal PixelContext(int colorType, int bitDepth, byte[] palette, byte[] paletteAlpha, byte[] key)
        {
            _colorType = colorType;
            _bitDepth = bitDepth;
            _palette = palette;
            _paletteAlpha = paletteAlpha;
            _key = key;
        }

        // Sample n of the row, at the image's bit depth, as a raw integer.
        private int Sample(byte[] row, int n)
        {
            switch (_bitDepth)
            {
                case 8: return row[n];
                case 16: return (row[n * 2] << 8) | row[n * 2 + 1];
                default:
                    var bitIndex = n * _bitDepth;
                    var shift = 8 - _bitDepth - (bitIndex & 7);
                    return (row[bitIndex >> 3] >> shift) & ((1 << _bitDepth) - 1);
            }
        }

        private byte To8(int value) => _bitDepth switch
        {
            16 => (byte)(value >> 8),
            8 => (byte)value,
            _ => (byte)(value * 255 / ((1 << _bitDepth) - 1)),
        };

        private int KeySample(int index) => (_key[index * 2] << 8) | _key[index * 2 + 1];

        internal void WritePixel(byte[] row, int x, byte[] rgba, int dst)
        {
            switch (_colorType)
            {
                case 0:
                {
                    var g = Sample(row, x);
                    rgba[dst] = rgba[dst + 1] = rgba[dst + 2] = To8(g);
                    rgba[dst + 3] = (byte)(_key != null && _key.Length >= 2 && g == KeySample(0) ? 0 : 255);
                    break;
                }
                case 2:
                {
                    int r = Sample(row, x * 3), g = Sample(row, x * 3 + 1), b = Sample(row, x * 3 + 2);
                    rgba[dst] = To8(r);
                    rgba[dst + 1] = To8(g);
                    rgba[dst + 2] = To8(b);
                    var transparent = _key != null && _key.Length >= 6 && r == KeySample(0) && g == KeySample(1) && b == KeySample(2);
                    rgba[dst + 3] = (byte)(transparent ? 0 : 255);
                    break;
                }
                case 3:
                {
                    var i = Sample(row, x);
                    if (i * 3 + 2 < _palette.Length)
                    {
                        rgba[dst] = _palette[i * 3];
                        rgba[dst + 1] = _palette[i * 3 + 1];
                        rgba[dst + 2] = _palette[i * 3 + 2];
                    }
                    rgba[dst + 3] = _paletteAlpha != null && i < _paletteAlpha.Length ? _paletteAlpha[i] : (byte)255;
                    break;
                }
                case 4:
                    rgba[dst] = rgba[dst + 1] = rgba[dst + 2] = To8(Sample(row, x * 2));
                    rgba[dst + 3] = To8(Sample(row, x * 2 + 1));
                    break;
                case 6:
                    rgba[dst] = To8(Sample(row, x * 4));
                    rgba[dst + 1] = To8(Sample(row, x * 4 + 1));
                    rgba[dst + 2] = To8(Sample(row, x * 4 + 2));
                    rgba[dst + 3] = To8(Sample(row, x * 4 + 3));
                    break;
            }
        }
    }

    private static int ReadInt32BE(byte[] d, int i) => (d[i] << 24) | (d[i + 1] << 16) | (d[i + 2] << 8) | d[i + 3];

    private static byte[] Slice(byte[] d, int start, int length)
    {
        var r = new byte[length];
        Buffer.BlockCopy(d, start, r, 0, length);
        return r;
    }
}
