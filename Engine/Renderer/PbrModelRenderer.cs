using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class PbrModelRenderer : IDisposable
{
    private readonly List<Renderable> _renderables = [];
    private readonly List<Texture> _ownedTextures = [];
    private readonly List<TextureView> _ownedTextureViews = [];
    private readonly List<DeviceBuffer> _ownedMaterialBuffers = [];
    private readonly List<ResourceSet> _ownedMaterialSets = [];

    private GraphicsDevice? _graphicsDevice;
    private Pipeline? _pipeline;
    private ResourceLayout? _materialLayout;
    private Shader[]? _shaders;
    private Texture? _whiteTexture;
    private Texture? _flatNormalTexture;
    private Texture? _defaultMrTexture;
    private TextureView? _whiteView;
    private TextureView? _flatNormalView;
    private TextureView? _defaultMrView;
    private bool _disposed;

    public int RenderableCount => _renderables.Count;
    public int DrawCallCount => _renderables.Sum(renderable => renderable.Draws.Length);

    public void Initialize(
        GraphicsDevice graphicsDevice,
        ResourceLayout cameraLayout,
        OutputDescription outputDescription,
        WorldState world,
        string assetsRoot)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(cameraLayout);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetsRoot);

        _graphicsDevice = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;

        _materialLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "Material", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "BaseColorTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "NormalTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "MetallicRoughnessTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "MaterialSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _shaders = factory.CreateFromSpirv(
            new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexShader), "main"),
            new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentShader), "main"));

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(new[] { vertexLayout }, _shaders),
            new[] { cameraLayout, _materialLayout },
            outputDescription));

        CreateFallbackTextures(graphicsDevice, factory);

        foreach (var instance in world.Models)
        {
            var relative = instance.AssetPath.Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(assetsRoot, relative);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Required PBR world asset '{instance.Id}' was not found.", path);

            var model = GlbModel.Load(path);
            var transform =
                Matrix4x4.CreateScale(instance.Scale) *
                Matrix4x4.CreateRotationY(instance.YawRadians) *
                Matrix4x4.CreateTranslation(instance.Position);
            var mesh = model.BuildPbrMesh(
                transform,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: instance.SourceIsZUp);

            var vertexBuffer = factory.CreateBuffer(new BufferDescription(
                PbrVertex.SizeInBytes * checked((uint)mesh.Vertices.Length),
                BufferUsage.VertexBuffer));
            var indexBuffer = factory.CreateBuffer(new BufferDescription(
                sizeof(uint) * checked((uint)mesh.Indices.Length),
                BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(vertexBuffer, 0, mesh.Vertices);
            graphicsDevice.UpdateBuffer(indexBuffer, 0, mesh.Indices);

            var materialSets = new ResourceSet[mesh.Materials.Length];
            for (var materialIndex = 0; materialIndex < mesh.Materials.Length; materialIndex++)
                materialSets[materialIndex] = CreateMaterialSet(graphicsDevice, factory, mesh.Materials[materialIndex]);

            var draws = mesh.DrawRanges
                .Select(range => new DrawBatch(
                    range.IndexStart,
                    range.IndexCount,
                    materialSets[Math.Clamp(range.MaterialIndex, 0, materialSets.Length - 1)]))
                .ToArray();

            _renderables.Add(new Renderable(instance.Id, vertexBuffer, indexBuffer, draws));
        }

        EngineLog.Info(
            $"PBR model renderer initialized: {_renderables.Count} models, {DrawCallCount} material draws.");
    }

    public void Render(CommandList commandList, ResourceSet cameraSet)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pipeline is null)
            throw new InvalidOperationException("PBR model renderer has not been initialized.");

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);

        foreach (var renderable in _renderables)
        {
            commandList.SetVertexBuffer(0, renderable.VertexBuffer);
            commandList.SetIndexBuffer(renderable.IndexBuffer, IndexFormat.UInt32);

            foreach (var draw in renderable.Draws)
            {
                commandList.SetGraphicsResourceSet(1, draw.MaterialSet);
                commandList.DrawIndexed(
                    draw.IndexCount,
                    instanceCount: 1,
                    indexStart: draw.IndexStart,
                    vertexOffset: 0,
                    instanceStart: 0);
            }
        }
    }

    private ResourceSet CreateMaterialSet(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        GlbMaterialData material)
    {
        if (_materialLayout is null ||
            _whiteView is null ||
            _flatNormalView is null ||
            _defaultMrView is null)
        {
            throw new InvalidOperationException("PBR material resources are not initialized.");
        }

        var materialBuffer = factory.CreateBuffer(new BufferDescription(
            MaterialUniform.SizeInBytes,
            BufferUsage.UniformBuffer));
        var uniform = new MaterialUniform(
            material.BaseColorFactor,
            new Vector4(
                Math.Clamp(material.MetallicFactor, 0f, 1f),
                Math.Clamp(material.RoughnessFactor, 0.04f, 1f),
                0f,
                0f));
        graphicsDevice.UpdateBuffer(materialBuffer, 0, ref uniform);
        _ownedMaterialBuffers.Add(materialBuffer);

        var baseColorView = material.BaseColorImage is { Length: > 0 }
            ? CreateTextureView(graphicsDevice, factory, material.BaseColorImage, srgb: true)
            : _whiteView;
        var normalView = material.NormalImage is { Length: > 0 }
            ? CreateTextureView(graphicsDevice, factory, material.NormalImage, srgb: false)
            : _flatNormalView;
        var mrView = material.MetallicRoughnessImage is { Length: > 0 }
            ? CreateTextureView(graphicsDevice, factory, material.MetallicRoughnessImage, srgb: false)
            : _defaultMrView;

        var set = factory.CreateResourceSet(new ResourceSetDescription(
            _materialLayout,
            materialBuffer,
            baseColorView,
            normalView,
            mrView,
            graphicsDevice.Aniso4xSampler));
        _ownedMaterialSets.Add(set);
        return set;
    }

    private TextureView CreateTextureView(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        byte[] imageBytes,
        bool srgb)
    {
        using var stream = new MemoryStream(imageBytes, writable: false);
        var image = new ImageSharpTexture(stream, mipmap: true, srgb: srgb);
        var texture = image.CreateDeviceTexture(graphicsDevice, factory);
        var view = factory.CreateTextureView(texture);
        _ownedTextures.Add(texture);
        _ownedTextureViews.Add(view);
        return view;
    }

    private void CreateFallbackTextures(GraphicsDevice graphicsDevice, ResourceFactory factory)
    {
        _whiteTexture = CreateSolidTexture(graphicsDevice, factory, 255, 255, 255, 255, srgb: true);
        _flatNormalTexture = CreateSolidTexture(graphicsDevice, factory, 128, 128, 255, 255, srgb: false);
        // glTF metallic-roughness convention: G=roughness, B=metallic.
        _defaultMrTexture = CreateSolidTexture(graphicsDevice, factory, 0, 255, 255, 255, srgb: false);

        _whiteView = factory.CreateTextureView(_whiteTexture);
        _flatNormalView = factory.CreateTextureView(_flatNormalTexture);
        _defaultMrView = factory.CreateTextureView(_defaultMrTexture);
    }

    private static Texture CreateSolidTexture(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        byte r,
        byte g,
        byte b,
        byte a,
        bool srgb)
    {
        var format = srgb
            ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb
            : PixelFormat.R8_G8_B8_A8_UNorm;
        var texture = factory.CreateTexture(TextureDescription.Texture2D(
            1, 1, 1, 1, format, TextureUsage.Sampled));
        graphicsDevice.UpdateTexture(
            texture,
            new byte[] { r, g, b, a },
            0, 0, 0,
            1, 1, 1,
            0, 0);
        return texture;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var renderable in _renderables)
        {
            renderable.VertexBuffer.Dispose();
            renderable.IndexBuffer.Dispose();
        }
        _renderables.Clear();

        foreach (var set in _ownedMaterialSets) set.Dispose();
        foreach (var buffer in _ownedMaterialBuffers) buffer.Dispose();
        foreach (var view in _ownedTextureViews) view.Dispose();
        foreach (var texture in _ownedTextures) texture.Dispose();

        _whiteView?.Dispose();
        _flatNormalView?.Dispose();
        _defaultMrView?.Dispose();
        _whiteTexture?.Dispose();
        _flatNormalTexture?.Dispose();
        _defaultMrTexture?.Dispose();

        _pipeline?.Dispose();
        _materialLayout?.Dispose();
        if (_shaders is not null)
            foreach (var shader in _shaders) shader.Dispose();

        _ownedMaterialSets.Clear();
        _ownedMaterialBuffers.Clear();
        _ownedTextureViews.Clear();
        _ownedTextures.Clear();
        _pipeline = null;
        _materialLayout = null;
        _shaders = null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct MaterialUniform
    {
        public const uint SizeInBytes = 32;

        public readonly Vector4 BaseColorFactor;
        public readonly Vector4 MaterialFactors;

        public MaterialUniform(Vector4 baseColorFactor, Vector4 materialFactors)
        {
            BaseColorFactor = baseColorFactor;
            MaterialFactors = materialFactors;
        }
    }

    private sealed record Renderable(
        string Id,
        DeviceBuffer VertexBuffer,
        DeviceBuffer IndexBuffer,
        DrawBatch[] Draws);

    private sealed record DrawBatch(
        uint IndexStart,
        uint IndexCount,
        ResourceSet MaterialSet);

    private const string VertexShader = @"
#version 450
layout(set = 0, binding = 0) uniform ProjectionBuffer { mat4 Projection; };
layout(set = 0, binding = 1) uniform ViewBuffer { mat4 View; };

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;

layout(location = 0) out vec3 fsin_ViewPosition;
layout(location = 1) out vec3 fsin_ViewNormal;
layout(location = 2) out vec2 fsin_TexCoord;
layout(location = 3) out float fsin_Distance;

void main()
{
    vec4 viewPosition = View * vec4(Position, 1.0);
    gl_Position = Projection * viewPosition;
    fsin_ViewPosition = viewPosition.xyz;
    fsin_ViewNormal = normalize(mat3(View) * Normal);
    fsin_TexCoord = TexCoord;
    fsin_Distance = length(viewPosition.xyz);
}";

    private const string FragmentShader = @"
#version 450
const float PI = 3.14159265359;

layout(set = 0, binding = 2) uniform AtmosphereBuffer
{
    vec4 FogColorDensity;
    vec4 Lighting;
};

layout(set = 1, binding = 0) uniform MaterialBuffer
{
    vec4 BaseColorFactor;
    vec4 MaterialFactors;
};

layout(set = 1, binding = 1) uniform texture2D BaseColorTexture;
layout(set = 1, binding = 2) uniform texture2D NormalTexture;
layout(set = 1, binding = 3) uniform texture2D MetallicRoughnessTexture;
layout(set = 1, binding = 4) uniform sampler MaterialSampler;

layout(location = 0) in vec3 fsin_ViewPosition;
layout(location = 1) in vec3 fsin_ViewNormal;
layout(location = 2) in vec2 fsin_TexCoord;
layout(location = 3) in float fsin_Distance;

layout(location = 0) out vec4 fsout_Color;

mat3 CotangentFrame(vec3 normal, vec3 position, vec2 uv)
{
    vec3 dp1 = dFdx(position);
    vec3 dp2 = dFdy(position);
    vec2 duv1 = dFdx(uv);
    vec2 duv2 = dFdy(uv);

    vec3 dp2perp = cross(dp2, normal);
    vec3 dp1perp = cross(normal, dp1);
    vec3 tangent = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 bitangent = dp2perp * duv1.y + dp1perp * duv2.y;

    float invmax = inversesqrt(max(dot(tangent, tangent), dot(bitangent, bitangent)) + 1e-8);
    return mat3(tangent * invmax, bitangent * invmax, normal);
}

float DistributionGGX(vec3 n, vec3 h, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float ndoth = max(dot(n, h), 0.0);
    float ndoth2 = ndoth * ndoth;
    float denom = ndoth2 * (a2 - 1.0) + 1.0;
    return a2 / max(PI * denom * denom, 0.0001);
}

float GeometrySchlickGGX(float ndotv, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    return ndotv / max(ndotv * (1.0 - k) + k, 0.0001);
}

float GeometrySmith(vec3 n, vec3 v, vec3 l, float roughness)
{
    return GeometrySchlickGGX(max(dot(n, v), 0.0), roughness)
         * GeometrySchlickGGX(max(dot(n, l), 0.0), roughness);
}

vec3 FresnelSchlick(float cosTheta, vec3 f0)
{
    return f0 + (1.0 - f0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main()
{
    vec4 baseSample = texture(sampler2D(BaseColorTexture, MaterialSampler), fsin_TexCoord);
    vec3 albedo = max(baseSample.rgb * BaseColorFactor.rgb, vec3(0.0));
    float alpha = baseSample.a * BaseColorFactor.a;

    vec3 normal = normalize(fsin_ViewNormal);
    vec3 sampledNormal = texture(sampler2D(NormalTexture, MaterialSampler), fsin_TexCoord).xyz * 2.0 - 1.0;
    if (length(sampledNormal.xy) > 0.001)
        normal = normalize(CotangentFrame(normal, fsin_ViewPosition, fsin_TexCoord) * sampledNormal);

    vec3 mr = texture(sampler2D(MetallicRoughnessTexture, MaterialSampler), fsin_TexCoord).rgb;
    float metallic = clamp(MaterialFactors.x * mr.b, 0.0, 1.0);
    float roughness = clamp(MaterialFactors.y * mr.g, 0.06, 1.0);

    vec3 viewDirection = normalize(-fsin_ViewPosition);
    vec3 lightDirection = normalize(vec3(-0.35, 0.82, 0.28));
    vec3 halfway = normalize(viewDirection + lightDirection);

    float ndotl = max(dot(normal, lightDirection), 0.0);
    float ndotv = max(dot(normal, viewDirection), 0.0);

    vec3 f0 = mix(vec3(0.04), albedo, metallic);
    vec3 f = FresnelSchlick(max(dot(halfway, viewDirection), 0.0), f0);
    float d = DistributionGGX(normal, halfway, roughness);
    float g = GeometrySmith(normal, viewDirection, lightDirection, roughness);

    vec3 specular = (d * g * f) / max(4.0 * ndotv * ndotl, 0.001);
    vec3 kd = (vec3(1.0) - f) * (1.0 - metallic);
    vec3 diffuse = kd * albedo / PI;

    float lightStrength = max(Lighting.x, 0.18);
    vec3 ambient = albedo * (0.045 + 0.035 * (1.0 - metallic));
    vec3 color = ambient + (diffuse + specular) * ndotl * (1.7 * lightStrength);

    float fogFactor = 1.0 - exp(-FogColorDensity.w * fsin_Distance);
    fogFactor = clamp(fogFactor, 0.0, 0.94);
    color = mix(color, FogColorDensity.rgb, fogFactor);

    color = color / (color + vec3(1.0));
    color = pow(max(color, vec3(0.0)), vec3(1.0 / 2.2));
    fsout_Color = vec4(color, alpha);
}";
}
