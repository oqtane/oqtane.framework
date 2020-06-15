using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Oqtane.Extensions;
using Oqtane.Shared;

namespace Oqtane.Infrastructure.ImageProcessing
{
    public class ThumbnailParameters
    {
        public ThumbnailParameters(string sizeString, string extension = null, long quality = 90, KnownColor background = KnownColor.Transparent, bool forceGeneration = false)
        {
            SizeString = sizeString;
            Extension = extension;
            Quality = quality;
            Background = background;
            ForceGeneration = forceGeneration;
        }

        public string SizeString { get; private set; }
        public string Extension { get; private set; }
        public long Quality { get; private set; }
        public KnownColor Background { get; private set; }
        public bool ForceGeneration { get; private set; }
    }

    public static class ThumbnailImage
    {
        /// <summary>
        /// Generates thumbnail image, write to output file and returns thumbnail path
        /// </summary>
        /// <returns>File path of generated thumbnail</returns>
        public static string GenerateThumbnail(string sourceImagePath, ThumbnailParameters thumbnailParameters)
        {
            return GenerateThumbnail(sourceImagePath, thumbnailParameters.SizeString, thumbnailParameters.Extension, thumbnailParameters.Quality, thumbnailParameters.Background, thumbnailParameters.ForceGeneration);
        }

        /// <summary>
        /// Generates thumbnail image, write to output file and returns thumbnail path
        /// </summary>
        /// <param name="sourceImagePath">Source image path</param>
        /// <param name="sizeString">size of thumbnail, 200x200 resize image to frame , 200x0, 0x200 restzes with aspect ratio, full -  returns same image</param>
        /// <param name="extension">Image format, default is same as source image, png,jpg,gif</param>
        /// <param name="quality">Resulting Jpeg quality</param>
        /// <param name="background">color of background when resized to frame</param>
        /// <param name="forceGeneration">force generate of thumbnail</param>
        /// <returns>File path of generated thumbnail</returns>
        public static string GenerateThumbnail(string sourceImagePath, string sizeString, string extension = null, long quality = 90, KnownColor background = KnownColor.Transparent, bool forceGeneration = false)
        {
            if (!File.Exists(sourceImagePath) || sizeString == "full") return sourceImagePath;
            var fileExtension = Path.GetExtension(sourceImagePath);
            if(!Constants.ImageFiles.Split(",").Contains(fileExtension.ToLower().Replace(".", ""))) return sourceImagePath; 
            
            var size = ParseSize(sizeString); if (size == null) return sourceImagePath;

            string thumbnailImagePath = GenerateThumbnailFilePath(sourceImagePath, size);
    
            if (extension != null && !fileExtension.Equals($".{extension}", StringComparison.OrdinalIgnoreCase))
            {
                thumbnailImagePath = Path.ChangeExtension(thumbnailImagePath, extension);
                fileExtension = $".{extension}";
            }

            if (File.Exists(thumbnailImagePath) && !forceGeneration) return thumbnailImagePath;
            var width = size.Value.Width;
            var height = size.Value.Height;

            try
            {
                Image image = Image.FromFile(sourceImagePath);

                if (width == 0) width = image.Width * height / image.Height;
                if (height == 0) height = image.Height * width / image.Width;

                Image thumbnail = new Bitmap(width, height);
                var graphic = Graphics.FromImage(thumbnail);

                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;

                double ratioX = width / (double) image.Width;
                double ratioY = height / (double) image.Height;
                double ratio = ratioX < ratioY ? ratioX : ratioY;

                int newHeight = Convert.ToInt32(image.Height * ratio);
                int newWidth = Convert.ToInt32(image.Width * ratio);

                int posX = Convert.ToInt32((width - (image.Width * ratio)) / 2);
                int posY = Convert.ToInt32((height - (image.Height * ratio)) / 2);


                graphic.Clear(Color.FromKnownColor(background));
                graphic.DrawImage(image, posX, posY, newWidth, newHeight);

                switch (fileExtension)
                {
                    case ".png":
                        SavePng(thumbnail, thumbnailImagePath);
                        break;
                    case ".gif":
                        SaveGif(thumbnail, thumbnailImagePath);
                        break;
                    default:
                        SaveJpeg(thumbnail, thumbnailImagePath, quality);
                        break;
                }

                image.Dispose();
                return thumbnailImagePath;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //fallback to source
            return sourceImagePath;
        }

        /// <summary>
        /// Parse input size string. Ex. sizes : 128x128, 120, 1, 512x512, full, 0x128, 128x0
        /// </summary>
        /// <param name="sizeString"></param>
        /// <param name="defaultSize"></param>
        /// <returns></returns>
        public static Size? ParseSize(string sizeString, Size? defaultSize = null)
        {
            var size = defaultSize ?? new Size(256, 256);

            try
            {
                if (!string.IsNullOrEmpty(sizeString))
                {
                    sizeString = sizeString.ToLower();
                    if (sizeString.Contains("x"))
                    {
                        var parts = sizeString.Split('x');
                        size.Width = int.Parse(parts[0]);
                        size.Height = int.Parse(parts[1]);
                    }
                    else if (sizeString == "full")
                    {
                        return new Size?();
                    }
                    else
                    {
                        size.Width = int.Parse(sizeString);
                        size.Height = int.Parse(sizeString);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }

            return size;
        }

        public static string GenerateThumbnailFilePath(string path, Size? size)
        {
            if (!size.HasValue) return path;
            var fileName = Path.GetFileNameWithoutExtension(path);
            var filePath = Path.GetDirectoryName(path);
            var ext = Path.GetExtension(path);

            //ex : sample.jpg -> sample_256x256.jpg
            fileName = $"{fileName}.{size.Value.Width}x{size.Value.Height}.thumb{ext}";
            return Path.Combine(filePath ?? string.Empty, fileName);
        }

        public static void SaveJpeg(this Image image, string path)
        {
            SaveJpeg(image, path, 95L);
        }

        public static void SaveJpeg(Image image, string path, long quality)
        {
            using (EncoderParameters encoderParameters = new EncoderParameters(1))
            using (EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, quality))
            {
                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageDecoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                encoderParameters.Param[0] = encoderParameter;
                image.Save(path, codecInfo, encoderParameters);
            }
        }

        public static void SaveGif(this Image image, string path)
        {
            image.Save(path, ImageFormat.Gif);
        }

        public static void SavePng(this Image image, string path)
        {
            image.Save(path, ImageFormat.Png);
        }
    }
}
