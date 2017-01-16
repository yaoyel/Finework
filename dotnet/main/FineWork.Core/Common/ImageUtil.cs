using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace FineWork.Common
{
    public static class ImageUtil
    {
        public static Stream CompressImage(Stream fileStream, int width,int height=0)
        {
            if (fileStream == null) throw new ArgumentException(nameof(fileStream));

            var ms = new MemoryStream();
            if (height==0)
                height = width;

            var originBitmap = Image.FromStream(fileStream);  

            using (var newImage = new Bitmap(width, height))
            {
                
                using (var graphic = GetGraphic(originBitmap, newImage))
                {
                    graphic.Clear(Color.White);
                    graphic.DrawImage(originBitmap, 0, 0,width, height); 
                }

                using (var encoderParameters = new EncoderParameters(1))
                {
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                    newImage.Save(ms, ImageCodecInfo
                        .GetImageEncoders()
                        .FirstOrDefault(x => x.FilenameExtension.Contains("JPG")), encoderParameters);
                }  

                return ms;
            }
        }

        public static Stream CutFromCenter(Stream fileStream, int? width, int? height,bool zoomByShortSide=true)
        {
            if (!width.HasValue &&!height.HasValue) return fileStream;

            if (fileStream == null) throw new ArgumentException(nameof(fileStream));

            var ms = new MemoryStream();

            var originBitmap = Image.FromStream(fileStream);
            var originWidth = originBitmap.Size.Width;
            var originHeight = originBitmap.Size.Height;
            var scaledWidth = 0;
            var scaledHeight = 0;
            var cutSize = 0;
            Image pickdImage = null;
            Graphics pickedg = null;
            Rectangle from = new Rectangle();
            var  scale=1d;

            if (zoomByShortSide)
            {
                if (originWidth > originHeight)
                    scale =(double) (height??originHeight)/originHeight;
                else
                    scale = (double)(width??originWidth)/originWidth;
            }

            scaledWidth = Convert.ToInt32(originWidth*scale);
            scaledHeight = Convert.ToInt32(originHeight*scale);

            var compressImage = Image.FromStream(CompressImage(fileStream, scaledWidth, scaledHeight));


            if (scaledWidth > scaledHeight)
            {
                cutSize = scaledHeight;
                from = new Rectangle((scaledWidth - scaledHeight)/2, 0, cutSize, cutSize);
            }
            else
            {
                cutSize = scaledWidth;
                from = new Rectangle( 0,(scaledHeight - scaledWidth) / 2, cutSize, cutSize);
            } 

            pickdImage = new Bitmap(cutSize, cutSize,PixelFormat.Format32bppArgb);
            pickedg = Graphics.FromImage(pickdImage); 
            pickedg.InterpolationMode= InterpolationMode.HighQualityBicubic;
            pickedg.SmoothingMode = SmoothingMode.HighQuality; 
            pickedg.Clear(Color.White);

            Rectangle to = new Rectangle(0, 0, cutSize, cutSize); 
            pickedg.DrawImage(compressImage, to, from, GraphicsUnit.Pixel); 
         
            pickdImage.Save(ms,ImageFormat.Jpeg);
            pickdImage.Dispose();
            pickedg.Dispose();
            
            return ms;
        }

        private static Graphics GetGraphic(Image originImage, Bitmap newImage)
        {
            newImage.SetResolution(originImage.HorizontalResolution, originImage.VerticalResolution);
            var graphic = Graphics.FromImage(newImage);
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            return graphic;
        } 
    }
}
