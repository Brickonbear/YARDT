using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;


namespace YARDT
{
    class ImageUtils
    {
        public static void CropImage(Bitmap image, string name, int x, int y, int width, int height)
        {
            Bitmap croppedImage;

            // Here we capture the resource - image file.
            using (image)
            {
                Rectangle crop = new Rectangle(x, y, width, height);

                // Here we capture another resource.
                croppedImage = image.Clone(crop, image.PixelFormat);

            } // Here we release the original resource - bitmap in memory and file on disk.

            croppedImage = AddGradient(croppedImage, name);

            // At this point the file on disk already free - you can record to the same path.
            croppedImage.Save(name.TrimEnd('_'), ImageFormat.Png);

            // It is desirable release this resource too.
            croppedImage.Dispose();
        }

        public static Bitmap AddGradient(Bitmap image, string name)
        {
            Bitmap gradient;
            //Console.WriteLine(name.Split('\\').Last<string>().Substring(2, 2).ToLower());
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
                default:
                    gradient = new Bitmap(250, 30);
                    break;
            }

            var target = new Bitmap(250, 30, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(image, 50, 0);
            graphics.DrawImage(gradient, 0, 0);

            return target;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

    }
}
