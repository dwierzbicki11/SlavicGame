using System.Buffers.Binary;
using System.Numerics;

namespace SlavicGame.Engine.Assets;

public enum AssetRealismTier
{
    Incomplete,
    Prototype,
    ProductionCandidate
}

public sealed record AssetRealismIssue(string Code, string Message);

public sealed record AssetRealismReport(
    string Path,
    AssetRealismTier Tier,
    bool HasAuthoredNormals,
    bool HasTextureCoordinates,
    int MaterialCount,
    int MinimumTextureEdge,
    IReadOnlyList<AssetRealismIssue> Issues)
{
    public bool IsProductionCandidate => Tier == AssetRealismTier.ProductionCandidate;
}

public static class AssetRealismAudit
{
    public const int MinimumProductionTextureEdge = 1024;

    public static AssetRealismReport InspectGlb(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var model = GlbModel.Load(path);
        var mesh = model.BuildPbrMesh(Matrix4x4.Identity, sourceIsZUp: true);
        var issues = new List<AssetRealismIssue>();

        var hasNormals = model.HasAuthoredNormals;
        var hasUvs = model.HasTextureCoordinates;
        if (!hasNormals)
            issues.Add(new("AUTHORED_NORMALS_MISSING", "Mesh relies on generated normals."));
        if (!hasUvs)
            issues.Add(new("UV_MISSING", "Mesh has no authored TEXCOORD_0 data."));

        var minimumTextureEdge = int.MaxValue;
        for (var i = 0; i < model.Materials.Count; i++)
        {
            var material = model.Materials[i];

            CheckRequiredImage(material.BaseColorImage, "BASECOLOR_MISSING", i);
            CheckRequiredImage(material.NormalImage, "NORMAL_MAP_MISSING", i);
            CheckRequiredImage(material.MetallicRoughnessImage, "METALROUGH_MISSING", i);

            InspectResolution(material.BaseColorImage, "baseColor", i);
            InspectResolution(material.NormalImage, "normal", i);
            InspectResolution(material.MetallicRoughnessImage, "metallicRoughness", i);
        }

        if (minimumTextureEdge == int.MaxValue)
            minimumTextureEdge = 0;

        var incomplete = issues.Any(issue =>
            issue.Code is "AUTHORED_NORMALS_MISSING" or "UV_MISSING" or
            "BASECOLOR_MISSING" or "NORMAL_MAP_MISSING" or "METALROUGH_MISSING");

        var tier = incomplete
            ? AssetRealismTier.Incomplete
            : minimumTextureEdge >= MinimumProductionTextureEdge
                ? AssetRealismTier.ProductionCandidate
                : AssetRealismTier.Prototype;

        return new AssetRealismReport(
            path,
            tier,
            hasNormals,
            hasUvs,
            model.Materials.Count,
            minimumTextureEdge,
            issues);

        void CheckRequiredImage(byte[]? image, string code, int materialIndex)
        {
            if (image is not { Length: > 0 })
                issues.Add(new(code, $"Material {materialIndex} is missing a required texture."));
        }

        void InspectResolution(byte[]? image, string channel, int materialIndex)
        {
            if (image is not { Length: > 0 })
                return;

            if (!TryReadImageSize(image, out var width, out var height))
            {
                issues.Add(new(
                    "TEXTURE_SIZE_UNKNOWN",
                    $"Material {materialIndex} {channel} image dimensions could not be read."));
                return;
            }

            var edge = Math.Min(width, height);
            minimumTextureEdge = Math.Min(minimumTextureEdge, edge);
            if (edge < MinimumProductionTextureEdge)
            {
                issues.Add(new(
                    "TEXTURE_RESOLUTION_LOW",
                    $"Material {materialIndex} {channel} is {width}x{height}; production minimum is " +
                    $"{MinimumProductionTextureEdge}px on the shortest edge."));
            }
        }
    }

    public static bool TryReadImageSize(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (bytes.Length >= 24 &&
            bytes[0] == 0x89 &&
            bytes[1] == (byte)'P' &&
            bytes[2] == (byte)'N' &&
            bytes[3] == (byte)'G')
        {
            width = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4)));
            height = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4)));
            return width > 0 && height > 0;
        }

        // Minimal JPEG SOF parser so external production assets can be audited too.
        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8)
        {
            var offset = 2;
            while (offset + 9 < bytes.Length)
            {
                if (bytes[offset] != 0xFF)
                {
                    offset++;
                    continue;
                }

                while (offset < bytes.Length && bytes[offset] == 0xFF)
                    offset++;
                if (offset >= bytes.Length)
                    break;

                var marker = bytes[offset++];
                if (marker is 0xD8 or 0xD9)
                    continue;
                if (offset + 2 > bytes.Length)
                    break;

                var length = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
                if (length < 2 || offset + length > bytes.Length)
                    break;

                if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or
                    0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or
                    0xCD or 0xCE or 0xCF)
                {
                    if (length < 7)
                        break;
                    height = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 3, 2));
                    width = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + 5, 2));
                    return width > 0 && height > 0;
                }

                offset += length;
            }
        }

        return false;
    }
}
