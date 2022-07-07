using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgApi;

/// <summary>
/// A static class containing utilities for image processing
/// </summary>
public static class ImgUtils
{
    /// <summary>
    /// Resizes an image to the specified dimensions. Does not respect aspect ratio.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="width">The desired width of the output image</param>
    /// <param name="height">The desired height of the output image</param>
    /// <param name="forceFormat">If the image doesn't need to be resized, convert the image to an allowed format</param>
    /// <param name="formats">A list of formats that are allowed. If conversion is needed, it will choose the 1st one by default</param>
    /// <returns>A string path to the output image file.</returns>
    public static async Task<string> ResizeAsync(string path, int width, int height, bool forceFormat, string[]? formats = null)
    {
        formats ??= new[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{Guid.NewGuid()}.{formats[0]}";
        using (var img = await Image.LoadAsync<Rgba32>(path))
        {
            if (img.Height != height || img.Width != width)
            {
                img.Mutate(x => x.Resize(width, height));
                await img.SaveAsync(savePath);
            }
            else if (forceFormat && !formats.Contains(Path.GetExtension(path)[1..]))
                await img.SaveAsync(savePath);
            else
                savePath = path;
        }
        ReleaseImageSharpMemory();
        return savePath;
    }

    /// <summary>
    /// Resizes the image to fit within the specified dimensions. Respects aspect ratio.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="width">The desired width of the output image</param>
    /// <param name="height">The desired height of the output image</param>
    /// <param name="forceFormat">If the image doesn't need to be resized, convert the image to an allowed format</param>
    /// <param name="formats">A list of formats that are allowed. If conversion is needed, it will choose the 1st one by default</param>
    /// <returns>A string path to the output image file.</returns>
    public static async Task<string> ResizeFitAsync(string path, int width, int height, bool forceFormat, string[]? formats = null)
    {
        formats ??= new[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{Guid.NewGuid()}.{formats[0]}";
        using (var img = await Image.LoadAsync<Rgba32>(path))
        {
            if (!(img.Width <= width && img.Height <= height && (img.Width == width || img.Height == height)))
            {
                double widthRatio = (img.Width + 0.0) / width;
                double heightRatio = (img.Height + 0.0) / height;

                bool widthPriority = widthRatio >= heightRatio;

                int newWidth = widthPriority ? width : 0;
                int newHeight = widthPriority ? 0 : height;

                if (width > img.Width || height > img.Height) img.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
                else img.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Spline));

                await img.SaveAsync(savePath);
            }
            else if (forceFormat && !formats.Contains(Path.GetExtension(path)[1..]))
                await img.SaveAsync(savePath);
            else
                savePath = path;

        }
        ReleaseImageSharpMemory();
        return savePath;
    }

    /// <summary>
    /// Resizes an image to fit within specified dimensions and pads the image with transparent pixels so that the output
    /// is the size provided. Respects aspect ratio of original image.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="width">The desired width of the output image</param>
    /// <param name="height">The desired height of the output image</param>
    /// <param name="forceFormat">If the image doesn't need to be resized, convert the image to an allowed format</param>
    /// <param name="formats">A list of formats that are allowed. If conversion is needed, it will choose the 1st one by default</param>
    /// <returns>A string path to the output image file.</returns>
    public static async Task<string> ResizePadAsync(string path, int width, int height, bool forceFormat, string[]? formats = null)
    { // TODO see if i can compress this, or have another method to containerize it all
        formats ??= new[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{Guid.NewGuid()}.{formats[0]}";
        using (var img = await Image.LoadAsync<Rgba32>(path))
        {
            if (img.Height != height || img.Width != width)
            {
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Pad
                }));
                await img.SaveAsync(savePath);
            }
            else if (forceFormat && !formats.Contains(Path.GetExtension(path)[1..]))
                await img.SaveAsync(savePath);
            else
                savePath = path;
        }
        ReleaseImageSharpMemory();
        return savePath;
    }

    /// <summary>
    /// Pads the input image with transparent pixels so that either width or height is the respective maximum
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="maxWidth">The desired width of the output image</param>
    /// <param name="maxHeight">The desired height of the output image</param>
    /// <param name="forceFormat">If the image doesn't need to be resized, convert the image to an allowed format</param>
    /// <param name="formats">A list of formats that are allowed. If conversion is needed, it will choose the 1st one by default</param>
    /// <returns>A string path to the output image file.</returns>
    public static async Task<string> ResizeFitPadWidthPriorityAsync(string path, int maxWidth, int maxHeight, bool forceFormat, string[]? formats = null)
    { // TODO see if i can compress this, or have another method to containerize it all
        formats ??= new[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{Guid.NewGuid()}.{formats[0]}";
        using (var img = await Image.LoadAsync<Rgba32>(path))
        {
            if (img.Height <= maxHeight && img.Width <= maxWidth && (img.Height == maxHeight || img.Width == maxWidth))
            {
                savePath = path;
            }
            if (img.Height >= maxHeight || img.Width >= maxWidth)
            {
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max
                }));
                await img.SaveAsync(savePath);
            }
            else
            {
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, img.Height),
                    Mode = ResizeMode.Pad,
                }));
                await img.SaveAsync(savePath);
            }
        }
        ReleaseImageSharpMemory();
        return savePath;
    }

    /// <summary>
    /// Encodes an image to a specified file format. NOTE: The output format is specified in the output file path
    /// </summary>
    /// <param name="path">The path to an input image</param>
    /// <param name="output">The desired output path</param>
    /// <returns>The path to the output</returns>
    public static async Task<string> EncodeToFormat(string path, string output)
	{
        using (var img = await Image.LoadAsync(path))
        {
            await img.SaveAsync(output);
        }
        ReleaseImageSharpMemory();
        return output;
    }

    /// <summary>
    /// Calls the ImageSharp memory allocator to release retained resources
    /// </summary>
    public static void ReleaseImageSharpMemory() => Configuration.Default.MemoryAllocator.ReleaseRetainedResources();

    /// <summary>
    /// Calls the ImageSharp memory allocator to release retained resources after a certain amount of time
    /// </summary>
    /// <param name="delay"></param>
    public static async void CollectImageSharpLater(int delay) =>
        await Task.Run(async () =>
        {
            await Task.Delay(delay);
            ReleaseImageSharpMemory();
        });
}

