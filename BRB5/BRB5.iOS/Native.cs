using BRB5.Model;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;

namespace BRB5.iOS
{
    public class Native : NativeBase
    {
        public override byte[] ResizeImage(byte[] imageData, float max)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            UIImageOrientation orientation = originalImage.Orientation;

            var imageMax = Math.Max(originalImage.Size.Width, originalImage.Size.Height);
            bool IsHeightMax = originalImage.Size.Width < originalImage.Size.Height;
            var coef = max / imageMax;

            var height = (int)(IsHeightMax ? max : originalImage.Size.Height * coef);
            var width = (int)(!IsHeightMax ? max : originalImage.Size.Width * coef);

            //create a 24bit RGB image
            using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                                                 width, height, 8,
                                                 4 * width, CGColorSpace.CreateDeviceRGB(),
                                                 CGImageAlphaInfo.PremultipliedFirst))
            {

                RectangleF imageRect = new RectangleF(0, 0, width, height);

                // draw the image
                context.DrawImage(imageRect, originalImage.CGImage);

                UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);

                // save the image as a jpeg
                return resizedImage.AsJPEG().ToArray();
            }
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
    }
}