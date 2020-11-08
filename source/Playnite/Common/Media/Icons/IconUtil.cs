/*
 *  IconExtractor/IconUtil for .NET
 *  Copyright (C) 2014 Tsuda Kageyu. All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *   1. Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *   2. Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 *  TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 *  PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER
 *  OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 *  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 *  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 *  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 *  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 *  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Playnite.Common.Media.Icons
{
    public static class IconUtil
    {
        private delegate byte[] GetIconDataDelegate(Icon icon);

        static GetIconDataDelegate getIconData;

        static IconUtil()
        {
            // Create a dynamic method to access Icon.iconData private field.

            var dm = new DynamicMethod(
                "GetIconData", typeof(byte[]), new Type[] { typeof(Icon) }, typeof(Icon));
            var fi = typeof(Icon).GetField(
                "iconData", BindingFlags.Instance | BindingFlags.NonPublic);
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, fi);
            gen.Emit(OpCodes.Ret);

            getIconData = (GetIconDataDelegate)dm.CreateDelegate(typeof(GetIconDataDelegate));
        }

        /// <summary>
        /// Split an Icon consists of multiple icons into an array of Icon each
        /// consists of single icons.
        /// </summary>
        /// <param name="icon">A System.Drawing.Icon to be split.</param>
        /// <returns>An array of System.Drawing.Icon.</returns>
        public static Icon[] Split(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // Get an .ico file in memory, then split it into separate icons.

            var src = GetIconData(icon);

            var splitIcons = new List<Icon>();
            {
                int count = BitConverter.ToUInt16(src, 4);

                for (int i = 0; i < count; i++)
                {
                    int length = BitConverter.ToInt32(src, 6 + 16 * i + 8);    // ICONDIRENTRY.dwBytesInRes
                    int offset = BitConverter.ToInt32(src, 6 + 16 * i + 12);   // ICONDIRENTRY.dwImageOffset

                    using (var dst = new BinaryWriter(new MemoryStream(6 + 16 + length)))
                    {
                        // Copy ICONDIR and set idCount to 1.

                        dst.Write(src, 0, 4);
                        dst.Write((short)1);

                        // Copy ICONDIRENTRY and set dwImageOffset to 22.

                        dst.Write(src, 6 + 16 * i, 12); // ICONDIRENTRY except dwImageOffset
                        dst.Write(22);                   // ICONDIRENTRY.dwImageOffset

                        // Copy a picture.

                        dst.Write(src, offset, length);

                        // Create an icon from the in-memory file.

                        dst.BaseStream.Seek(0, SeekOrigin.Begin);
                        splitIcons.Add(new Icon(dst.BaseStream));
                    }
                }
            }

            return splitIcons.ToArray();
        }

        /// <summary>
        /// Converts an Icon to a GDI+ Bitmap preserving the transparent area.
        /// </summary>
        /// <param name="icon">An System.Drawing.Icon to be converted.</param>
        /// <returns>A System.Drawing.Bitmap Object.</returns>
        public static Bitmap ToBitmap(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // Quick workaround: Create an .ico file in memory, then load it as a Bitmap.

            using (var ms = new MemoryStream())
            {
                icon.Save(ms);
                using (var bmp = (Bitmap)Image.FromStream(ms))
                {
                    return new Bitmap(bmp);
                }
            }
        }

        /// <summary>
        /// Gets the bit depth of an Icon.
        /// </summary>
        /// <param name="icon">An System.Drawing.Icon object.</param>
        /// <returns>Bit depth of the icon.</returns>
        /// <remarks>
        /// This method takes into account the PNG header.
        /// If the icon has multiple variations, this method returns the bit
        /// depth of the first variation.
        /// </remarks>
        public static int GetBitCount(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            // Get an .ico file in memory, then read the header.

            var data = GetIconData(icon);
            if (data.Length >= 51
                && data[22] == 0x89 && data[23] == 0x50 && data[24] == 0x4e && data[25] == 0x47
                && data[26] == 0x0d && data[27] == 0x0a && data[28] == 0x1a && data[29] == 0x0a
                && data[30] == 0x00 && data[31] == 0x00 && data[32] == 0x00 && data[33] == 0x0d
                && data[34] == 0x49 && data[35] == 0x48 && data[36] == 0x44 && data[37] == 0x52)
            {
                // The picture is PNG. Read IHDR chunk.

                switch (data[47])
                {
                    case 0:
                        return data[46];
                    case 2:
                        return data[46] * 3;
                    case 3:
                        return data[46];
                    case 4:
                        return data[46] * 2;
                    case 6:
                        return data[46] * 4;
                    default:
                        // NOP
                        break;
                }
            }
            else if (data.Length >= 22)
            {
                // The picture is not PNG. Read ICONDIRENTRY structure.

                return BitConverter.ToUInt16(data, 12);
            }

            throw new ArgumentException("The icon is corrupt. Couldn't read the header.", "icon");
        }

        private static byte[] GetIconData(Icon icon)
        {
            var data = getIconData(icon);
            if (data != null)
            {
                return data;
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    icon.Save(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
