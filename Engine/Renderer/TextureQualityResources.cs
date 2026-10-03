using SlavicGame.Engine.Settings;
using Veldrid;
using Veldrid.ImageSharp;

namespace SlavicGame.Engine.Renderer;

public static class TextureQualityResources
{
    public static Texture CreateTexture(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        Stream stream,
        bool srgb,
        TextureQuality quality,
        out uint uploadedWidth,
        out uint uploadedHeight)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(stream);

        var source = new ImageSharpTexture(stream, mipmap: true, srgb: srgb);
        ImageSharpTexture? reduced = null;

        try
        {
            var maximumDimension =
                GraphicsQualityCatalog.TextureMaximumDimension(quality);

            var mipIndex = 0;
            while (mipIndex + 1 < source.Images.Length)
            {
                var image = source.Images[mipIndex];
                if (Math.Max(image.Width, image.Height) <= maximumDimension)
                    break;

                mipIndex++;
            }

            var selected = source.Images[mipIndex].Clone();
            reduced = new ImageSharpTexture(
                selected,
                mipmap: true,
                srgb: srgb);

            uploadedWidth = reduced.Width;
            uploadedHeight = reduced.Height;
            return reduced.CreateDeviceTexture(graphicsDevice, factory);
        }
        finally
        {
            if (reduced is not null)
            {
                foreach (var image in reduced.Images)
                    image.Dispose();
            }

            foreach (var image in source.Images)
                image.Dispose();
        }
    }

    public static Sampler CreateSampler(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        TextureQuality quality)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(factory);

        var description = SamplerDescription.Linear;

        switch (quality)
        {
            case TextureQuality.Low:
                description.Filter = SamplerFilter.MinLinear_MagLinear_MipLinear;
                description.LodBias = 1;
                description.MaximumAnisotropy = 0;
                break;

            case TextureQuality.Medium:
                description.Filter = SamplerFilter.MinLinear_MagLinear_MipLinear;
                description.LodBias = 0;
                description.MaximumAnisotropy = 0;
                break;

            case TextureQuality.High:
                if (graphicsDevice.Features.SamplerAnisotropy)
                {
                    description = SamplerDescription.Aniso4x;
                    description.MaximumAnisotropy = 2;
                }
                break;

            case TextureQuality.Ultra:
                if (graphicsDevice.Features.SamplerAnisotropy)
                {
                    description = SamplerDescription.Aniso4x;
                    description.MaximumAnisotropy = 4;
                }
                break;
        }

        return factory.CreateSampler(description);
    }
}
