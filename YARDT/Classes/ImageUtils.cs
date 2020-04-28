using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YARDT
{
    class ImageUtils
    {
        /// <summary>
        /// Crops image from x,y to x+width,y+height
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap CropImage(Bitmap image, int x, int y, int width, int height)
        {
            Bitmap croppedImage;

            // Here we capture the resource - image file.
            using (image)
            {
                Rectangle crop = new Rectangle(x, y, width, height);

                // Here we capture another resource.
                croppedImage = image.Clone(crop, image.PixelFormat);

            } // Here we release the original resource - bitmap in memory and file on disk.
            return croppedImage;
        }

        /// <summary>
        /// Applies gradient to cropped image based on region
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Bitmap AddGradient(Bitmap image, string name)
        {
            Bitmap gradient;

            switch (name.Split('\\').Last().Substring(2, 2).ToLower())
            {
                case "de":
                    gradient = new Bitmap(Properties.Resources.GradientDemacia);
                    break;
                case "fr":
                    gradient = new Bitmap(Properties.Resources.GradientFreljord);
                    break;
                case "io":
                    gradient = new Bitmap(Properties.Resources.GradientIonia);
                    break;
                case "nx":
                    gradient = new Bitmap(Properties.Resources.GradientNoxus);
                    break;
                case "pz":
                    gradient = new Bitmap(Properties.Resources.GradientPiltoverZaun);
                    break;
                case "si":
                    gradient = new Bitmap(Properties.Resources.GradientShadowIsles);
                    break;
                case "bw":
                    gradient = new Bitmap(Properties.Resources.GradientBilgewater);
                    break;
                case "xx":
                    gradient = new Bitmap(Properties.Resources.OverlayDark);
                    break;
                default:
                    gradient = new Bitmap(250, 30);
                    break;
            }

            Bitmap target = new Bitmap(250, 30, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            if(name != "xxxx")
            {
                graphics.DrawImage(image, 50, 0);
            }
            else //Special case for darkening image
            {
                graphics.DrawImage(image, 0, 0);
            }
            graphics.DrawImage(gradient, 0, 0);

            return target;
        }

        /// <summary>
        /// Resizes image to specified width and height
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        /// <summary>
        /// Convert BitmapSource to Bitmap
        /// </summary>
        /// <param name="bitmapsource"></param>
        /// <returns></returns>
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        /// <summary>
        /// Convert Bitmap to BitmapSource
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static BitmapSource SourceFromBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
