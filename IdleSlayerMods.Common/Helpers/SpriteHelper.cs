using System.Reflection;
using IdleSlayerMods.Common.Extensions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdleSlayerMods.Common.Helpers;

/// <summary>
/// Loads textures and sprites at runtime without AssetBundles.
/// <para>
/// The recommended approach for IL2CPP mods is to embed images in the mod DLL
/// (<c>&lt;EmbeddedResource Include="Assets\Icon.png" LogicalName="MyMod.Icon.png" /&gt;</c>)
/// and load them with <see cref="LoadSpriteFromEmbeddedResource"/>. The asset then ships inside the DLL,
/// so it cannot go missing and does not depend on the install folder layout.
/// </para>
/// <para>
/// PNGs are decoded in managed code and uploaded with <c>Texture2D.SetPixels32</c>.
/// <c>ImageConversion.LoadImage</c> is not used for PNGs because, in this game's Unity 6 interop assemblies,
/// every LoadImage overload routes through a ReadOnlySpan overload that throws
/// MissingMethodException (<c>Il2CppSystem.ReadOnlySpan`1.GetPinnableReference</c>).
/// </para>
/// Each stage logs one line prefixed with <c>[SpriteHelper]</c>, so a failure can be traced to the exact step.
/// </summary>
public static class SpriteHelper
{
    private const string LogPrefix = "[SpriteHelper]";

    /// <summary>
    /// Decodes image bytes into a texture that survives scene changes and Resources.UnloadUnusedAssets.
    /// PNG is decoded in managed code; other formats fall back to ImageConversion.LoadImage, which may not
    /// work on this game (see class remarks).
    /// </summary>
    /// <param name="data">Encoded image bytes.</param>
    /// <param name="name">Optional texture name (useful when debugging).</param>
    /// <param name="filterMode">Texture filtering. Use Point for crisp pixel art.</param>
    /// <returns>The texture, or null on failure.</returns>
    public static Texture2D LoadTextureFromBytes(byte[] data, string name = null, FilterMode filterMode = FilterMode.Bilinear)
    {
        name = string.IsNullOrEmpty(name) ? "Runtime Texture" : name;
        if (data == null || data.Length == 0)
        {
            Plugin.Logger?.Error($"{LogPrefix} decode '{name}': no image data");
            return null;
        }

        return PngDecoder.IsPng(data)
            ? LoadPng(data, name, filterMode)
            : LoadWithImageConversion(data, name, filterMode);
    }

    private static Texture2D LoadPng(byte[] data, string name, FilterMode filterMode)
    {
        PngDecoder.Image image;
        try
        {
            image = PngDecoder.Decode(data);
            Plugin.Logger?.Debug($"{LogPrefix} decode '{name}': OK, {image.Width}x{image.Height} ({image.Format})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"{LogPrefix} decode '{name}': FAILED - {ex.Message}");
            return null;
        }

        Texture2D texture = null;
        try
        {
            texture = new(image.Width, image.Height, TextureFormat.RGBA32, false);

            // PNG rows are top-to-bottom; Unity textures start at the bottom-left.
            var pixels = new Color32[image.Width * image.Height];
            for (var y = 0; y < image.Height; y++)
            {
                var srcRow = y * image.Width * 4;
                var dstRow = (image.Height - 1 - y) * image.Width;
                for (var x = 0; x < image.Width; x++)
                {
                    var s = srcRow + x * 4;
                    pixels[dstRow + x] = new Color32(image.Rgba[s], image.Rgba[s + 1], image.Rgba[s + 2], image.Rgba[s + 3]);
                }
            }

            texture.SetPixels32(new Il2CppStructArray<Color32>(pixels));
            texture.Apply(false, false);
            ConfigureTexture(texture, name, filterMode);
            Plugin.Logger?.Msg($"{LogPrefix} upload '{name}': OK, Texture2D {texture.width}x{texture.height} {texture.format}");
            return texture;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"{LogPrefix} upload '{name}': FAILED - {ex.GetType().Name}: {ex.Message}");
            if (texture) Object.Destroy(texture);
            return null;
        }
    }

    private static Texture2D LoadWithImageConversion(byte[] data, string name, FilterMode filterMode)
    {
        Plugin.Logger?.Warning($"{LogPrefix} decode '{name}': not a PNG, trying ImageConversion.LoadImage " +
                               "(known to throw MissingMethodException on this game; convert the image to PNG)");
        Texture2D texture = null;
        try
        {
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, new Il2CppStructArray<byte>(data)))
            {
                Plugin.Logger?.Error($"{LogPrefix} decode '{name}': FAILED - LoadImage returned false");
                Object.Destroy(texture);
                return null;
            }

            ConfigureTexture(texture, name, filterMode);
            Plugin.Logger?.Msg($"{LogPrefix} decode '{name}': OK via LoadImage, {texture.width}x{texture.height}");
            return texture;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"{LogPrefix} decode '{name}': FAILED - {ex.GetType().Name}: {ex.Message}");
            if (texture) Object.Destroy(texture);
            return null;
        }
    }

    private static void ConfigureTexture(Texture2D texture, string name, FilterMode filterMode)
    {
        texture.name = name;
        texture.filterMode = filterMode;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    /// <summary>
    /// Reads the raw bytes of a manifest resource. Matches the exact name first, then any resource
    /// whose name ends with <paramref name="resourceName"/> (case-insensitive).
    /// </summary>
    /// <returns>The bytes, or null if the resource does not exist.</returns>
    public static byte[] ReadEmbeddedResource(Assembly assembly, string resourceName)
    {
        if (assembly == null || string.IsNullOrEmpty(resourceName))
        {
            Plugin.Logger?.Error($"{LogPrefix} resource: no assembly or resource name given");
            return null;
        }

        var names = assembly.GetManifestResourceNames();
        var match = names.FirstOrDefault(n => n == resourceName)
                    ?? names.FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            Plugin.Logger?.Error($"{LogPrefix} resource '{resourceName}': NOT FOUND in {assembly.GetName().Name}. " +
                                 $"Available: [{string.Join(", ", names)}]");
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(match);
        if (stream == null)
        {
            Plugin.Logger?.Error($"{LogPrefix} resource '{match}': stream could not be opened");
            return null;
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        var bytes = memory.ToArray();
        Plugin.Logger?.Msg($"{LogPrefix} resource '{match}': OK, {bytes.Length} bytes from {assembly.GetName().Name}");
        return bytes;
    }

    /// <summary>
    /// Loads a texture from an image embedded in the given assembly.
    /// </summary>
    public static Texture2D LoadTextureFromEmbeddedResource(Assembly assembly, string resourceName, FilterMode filterMode = FilterMode.Bilinear)
    {
        var bytes = ReadEmbeddedResource(assembly, resourceName);
        return bytes == null ? null : LoadTextureFromBytes(bytes, resourceName, filterMode);
    }

    /// <summary>
    /// Wraps a whole texture in a centred sprite that survives scene changes.
    /// </summary>
    public static Sprite CreateSprite(Texture2D texture, float pixelsPerUnit = 100f)
    {
        if (texture == null) return null;

        try
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
            Plugin.Logger?.Msg($"{LogPrefix} sprite '{sprite.name}': OK, rect {sprite.rect.width}x{sprite.rect.height}");
            return sprite;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"{LogPrefix} sprite '{texture.name}': FAILED - {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads a sprite from an image embedded in the given assembly.
    /// </summary>
    /// <param name="assembly">Usually <c>typeof(SomeTypeInYourMod).Assembly</c>.</param>
    /// <param name="resourceName">The resource's LogicalName, or a suffix of its manifest name.</param>
    /// <param name="filterMode">Texture filtering. Use Point for crisp pixel art.</param>
    /// <param name="pixelsPerUnit">Sprite pixels per unit.</param>
    /// <returns>The sprite, or null on failure.</returns>
    public static Sprite LoadSpriteFromEmbeddedResource(Assembly assembly, string resourceName,
        FilterMode filterMode = FilterMode.Bilinear, float pixelsPerUnit = 100f)
        => CreateSprite(LoadTextureFromEmbeddedResource(assembly, resourceName, filterMode), pixelsPerUnit);
}
