using Veldrid;

namespace SlavicGame.Engine.Renderer;

public static class ShaderLibrary
{
    public static Shader[] LoadPair(ResourceFactory factory, string name)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return
        [
            Load(factory, $"{name}.vert.spv", ShaderStages.Vertex),
            Load(factory, $"{name}.frag.spv", ShaderStages.Fragment)
        ];
    }

    private static Shader Load(ResourceFactory factory, string fileName, ShaderStages stage)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "shaders", "bin", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Compiled Vulkan shader '{fileName}' is missing. " +
                "Run ./tools/compile-shaders.sh or ./run.sh before starting SlavicGame.",
                path);
        }

        var spirv = File.ReadAllBytes(path);
        if (spirv.Length < 4 ||
            spirv[0] != 0x03 ||
            spirv[1] != 0x02 ||
            spirv[2] != 0x23 ||
            spirv[3] != 0x07)
        {
            throw new InvalidDataException(
                $"Shader '{path}' is not valid SPIR-V (bad magic number).");
        }

        return factory.CreateShader(new ShaderDescription(stage, spirv, "main"));
    }
}
