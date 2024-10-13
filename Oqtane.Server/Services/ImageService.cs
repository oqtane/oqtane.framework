using Oqtane.Enums;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System;
using SixLabors.ImageSharp;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogManager _logger;

        public ImageService(ILogManager logger)
        {
            _logger = logger;
        }

        public string CreateImage(string filepath, int width, int height, string mode, string position, string background, string rotate, string imagepath)
        {
            try
            {
                // params validation
                if (!Enum.TryParse(mode, true, out ResizeMode _)) mode = "crop";
                if (!Enum.TryParse(position, true, out AnchorPositionMode _)) position = "center";
                if (!Color.TryParseHex("#" + background, out _)) background = "transparent";
                if (!int.TryParse(rotate, out _)) rotate = "0";
                rotate = (int.Parse(rotate) < 0 || int.Parse(rotate) > 360) ? "0" : rotate;

                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.Position = 0;
                    using (var image = Image.Load(stream))
                    {
                        int.TryParse(rotate, out int angle);
                        Enum.TryParse(mode, true, out ResizeMode resizemode);
                        Enum.TryParse(position, true, out AnchorPositionMode anchorpositionmode);

                        PngEncoder encoder;

                        if (background != "transparent")
                        {
                            image.Mutate(x => x
                                .AutoOrient() // auto orient the image
                                .Rotate(angle)
                                .Resize(new ResizeOptions
                                {
                                    Mode = resizemode,
                                    Position = anchorpositionmode,
                                    Size = new Size(width, height),
                                    PadColor = Color.ParseHex("#" + background)
                                }));

                            encoder = new PngEncoder();
                        }
                        else
                        {
                            image.Mutate(x => x
                                .AutoOrient() // auto orient the image
                                .Rotate(angle)
                                .Resize(new ResizeOptions
                                {
                                    Mode = resizemode,
                                    Position = anchorpositionmode,
                                    Size = new Size(width, height)
                                }));

                            encoder = new PngEncoder
                            {
                                ColorType = PngColorType.RgbWithAlpha,
                                TransparentColorMode = PngTransparentColorMode.Preserve,
                                BitDepth = PngBitDepth.Bit8,
                                CompressionLevel = PngCompressionLevel.BestSpeed
                            };
                        }

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
    }
}
