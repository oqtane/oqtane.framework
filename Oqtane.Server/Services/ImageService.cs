using Oqtane.Enums;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System;
using SixLabors.ImageSharp;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using System.Linq;

namespace Oqtane.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogManager _logger;
        private static readonly string[] _formats = ["png", "webp"];

        public ImageService(ILogManager logger)
        {
            _logger = logger;
        }

        public string[] GetAvailableFormats()
        {
            return _formats;
        }

        public string CreateImage(string filepath, int width, int height, string mode, string position, string background, string rotate, string format, string imagepath)
        {
            try
            {
                // params validation
                if (!Enum.TryParse(mode, true, out ResizeMode _)) mode = "crop";
                if (!Enum.TryParse(position, true, out AnchorPositionMode _)) position = "center";
                if (!Color.TryParseHex("#" + background, out _)) background = "transparent";
                if (!int.TryParse(rotate, out _)) rotate = "0";
                rotate = (int.Parse(rotate) < 0 || int.Parse(rotate) > 360) ? "0" : rotate;
                if (!_formats.Contains(format)) format = "png";

                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.Position = 0;
                    using (var image = Image.Load(stream))
                    {
                        int.TryParse(rotate, out int angle);
                        Enum.TryParse(mode, true, out ResizeMode resizemode);
                        Enum.TryParse(position, true, out AnchorPositionMode anchorpositionmode);

                        if (width == 0 && height == 0)
                        {
                            width = image.Width;
                            height = image.Height;
                        }

                        IImageEncoder encoder;
                        var resizeOptions = new ResizeOptions
                        {
                            Mode = resizemode,
                            Position = anchorpositionmode,
                            Size = new Size(width, height)
                        };

                        if (background != "transparent")
                        {
                            resizeOptions.PadColor = Color.ParseHex("#" + background);
                            encoder = GetEncoder(format, transparent: false);
                        }
                        else
                        {
                            encoder = GetEncoder(format, transparent: true);
                        }

                        image.Mutate(x => x
                                .AutoOrient() // auto orient the image
                                .Rotate(angle)
                                .Resize(resizeOptions));

                        image.Save(imagepath, encoder);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, ex, "Error Creating Image For File {FilePath} {Width} {Height} {Mode} {Rotate} {Error}", filepath, width, height, mode, rotate, ex.Message);
                imagepath = "";
            }

            return imagepath;
        }

        private static IImageEncoder GetEncoder(string format, bool transparent)
        {
            return format switch
            {
                "png" => GetPngEncoder(transparent),
                "webp" => GetWebpEncoder(transparent),
                _ => GetPngEncoder(transparent),
            };
        }

        private static PngEncoder GetPngEncoder(bool transparent)
        {
            return new PngEncoder()
            {
                ColorType = transparent ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
                TransparentColorMode = transparent ? PngTransparentColorMode.Preserve : PngTransparentColorMode.Clear,
                BitDepth = PngBitDepth.Bit8,
                CompressionLevel = PngCompressionLevel.BestSpeed
            };
        }

        private static WebpEncoder GetWebpEncoder(bool transparent)
        {
            return new WebpEncoder()
            {
                FileFormat = WebpFileFormatType.Lossy,
                Quality = 60,
                TransparentColorMode = transparent ? WebpTransparentColorMode.Preserve : WebpTransparentColorMode.Clear,
            };
        }
    }
}
