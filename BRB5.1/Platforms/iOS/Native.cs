using BRB5.Model;
using BRB6.Platforms.iOS;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;

namespace BRB6
{
    public class Native : NativeBase
    {

        public override byte[] ResizeImage(byte[] imageData, float width, int compress = 90)
        {
            float height = width;
            UIImage resizedImage, originalImage = ImageFromByteArray(imageData);

            var originalHeight = originalImage.Size.Height;
            var originalWidth = originalImage.Size.Width;


            if (width < originalHeight && width < originalWidth)
            {
                nfloat newHeight = 0;
                nfloat newWidth = 0;
                if (originalHeight > originalWidth)
                {
                    newHeight = height;
                    nfloat ratio = originalHeight / height;
                    newWidth = originalWidth / ratio;
                }
                else
                {
                    newWidth = width;
                    nfloat ratio = originalWidth / width;
                    newHeight = originalHeight / ratio;
                }

                width = (float)newWidth;
                height = (float)newHeight;

                UIGraphics.BeginImageContext(new System.Drawing.SizeF(width, height));
                originalImage.Draw(new RectangleF(0, 0, width, height));
                resizedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
            }
            else
                resizedImage = originalImage;

            var bytesImagen = resizedImage.AsJPEG(compress).ToArray();
            resizedImage.Dispose();
            return bytesImagen;
        }
        public static UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            UIKit.UIImage image;
            try
            {
                image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }

        public override async Task<List<FilePickerResult>> PickPhotos() 
        {
            PhotoService photoPickerService = new PhotoService();
            var res = await photoPickerService.PickPhotos();
            return res;
        }
    }
}