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
        var path = ResolveShaderPath(fileName);
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

    private static string ResolveShaderPath(string fileName)
    {
        var attempted = new List<string>();

        var outputPath = Path.Combine(AppContext.BaseDirectory, "shaders", "bin", fileName);
        attempted.Add(outputPath);
        if (File.Exists(outputPath))
            return outputPath;

        var workingDirectoryPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "shaders",
            "bin",
            fileName);
        attempted.Add(workingDirectoryPath);
        if (File.Exists(workingDirectoryPath))
            return workingDirectoryPath;

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (var depth = 0; directory is not null && depth < 10; depth++, directory = directory.Parent)
        {
            var projectFile = Path.Combine(directory.FullName, "SlavicGame.csproj");
            if (!File.Exists(projectFile))
                continue;

            var projectShaderPath = Path.Combine(
                directory.FullName,
                "shaders",
                "bin",
                fileName);
            attempted.Add(projectShaderPath);
            if (File.Exists(projectShaderPath))
                return projectShaderPath;

            break;
        }

        throw new FileNotFoundException(
            $"Compiled Vulkan shader '{fileName}' is missing. " +
            "On Linux, 'dotnet build' and 'dotnet run' compile shaders automatically. " +
            "You can also run ./tools/compile-shaders.sh manually. " +
            $"Checked: {string.Join(", ", attempted)}",
            outputPath);
    }
}
