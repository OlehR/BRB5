using Android.Graphics;
using Android.Hardware.Lights;
using BRB5.Model;
using System;
using System.IO;

namespace BRB5.Droid
{
    public class Native : NativeBase
    {
        public override byte[] ResizeImage(byte[] imageData, float max, int compress = 90)
        {
            // Load the bitmap            
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            int imageMax = Math.Max(originalImage.Width, originalImage.Height);
            bool IsHeightMax = originalImage.Width < originalImage.Height;
            var coef = max / imageMax;

            var height = (int)(IsHeightMax ? max : originalImage.Height * coef);
            var width = (int)(!IsHeightMax ? max : originalImage.Width * coef);

            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, width, height, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, compress, ms);
                return ms.ToArray();
            }
        }
    }
}