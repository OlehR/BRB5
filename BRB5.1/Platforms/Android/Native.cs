using Android.Graphics;
using BRB5.Model;

namespace BRB6
{
    public class Native : NativeBase
    {
        public override byte[] ResizeImage(byte[] imageData, float max, int compress = 90)
        {
            // Load the bitmap            
            Bitmap resizedImage,originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            int imageMax = Math.Max(originalImage.Width, originalImage.Height);
            if (max < imageMax)
            {
                bool IsHeightMax = originalImage.Width < originalImage.Height;
                var coef = max / imageMax;

                var height = (int)(IsHeightMax ? max : originalImage.Height * coef);
                var width = (int)(!IsHeightMax ? max : originalImage.Width * coef);

                resizedImage = Bitmap.CreateScaledBitmap(originalImage, width, height, false);
            }
            else
                resizedImage = originalImage;
            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, compress, ms);
                return ms.ToArray();
            }
        }
    }
}