using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgApi;

public static class ImgUtils
{
    public static async Task<string> ResizeAsync(string path, int width, int height, bool forceFormat, string[]? formats = null)
    {
        formats ??= new[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{Guid.NewGuid()}.{formats[0]}";
        using (var img = await Image.LoadAsync(path))
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

    public static async Task<string> ResizeFitAsync(string path, int width, int height, bool forceFormat, string[]? formats = null)
    {
        formats ??= new[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{Guid.NewGuid()}.{formats[0]}";
        using (var img = await Image.LoadAsync(path))
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

    public static async Task<string> ResizeFitWithAlphaBorderAsync(string path, int width, int height, bool forceFormat, string[]? formats = null)
    {
        throw new NotImplementedException(); //TODO implement this!!
    }

    public static async Task<string> EncodeToFormat(string path, string output)
	{
        using (var img = await Image.LoadAsync(path))
        {
            await img.SaveAsync(output);
        }
        ReleaseImageSharpMemory();
        return output;
    }

    public static void ReleaseImageSharpMemory() => Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
}

