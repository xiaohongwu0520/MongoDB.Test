using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Example1
{
    public static class ImageExtentions
    {
        /// <summary>
        /// Image转byte[]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(this Image image)
        {
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Image转Steam
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Stream ImageToStream(this Image image)
        {
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return new MemoryStream(buffer);
            }
        }

        /// <summary>
        /// Image转Base64字符串
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string ImageToBase64String(this Image image)
        {
            return Convert.ToBase64String(image.ImageToBytes());
        }
        /// <summary>
        /// Base64字符串转Image
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static Image Base64StringToImage(this string base64)
        {
            byte[] buffer = Convert.FromBase64String(base64);
            return buffer.BytesToImage();
        }

        /// <summary>
        /// Byte[]转Image
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Image BytesToImage(this byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            return image;

        }
    }
}
