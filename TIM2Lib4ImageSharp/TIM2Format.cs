using SixLabors.ImageSharp.Formats;

namespace TIM2Lib4ImageSharp;

public class TIM2Format : IImageFormat
{
    private TIM2Format() { }
    
    /// <summary>
    /// Gets the shared instance.
    /// </summary>
    public static TIM2Format Instance { get; } = new();
    
    /// <inheritdoc/>
    public string Name => "TIM2";
    
    /// <inheritdoc/>
    public string DefaultMimeType => "image/tim2";
    
    /// <inheritdoc/>
    public IEnumerable<string> MimeTypes => new[] { "image/tim2" };
    
    /// <inheritdoc/>
    public IEnumerable<string> FileExtensions => new[] { "tm2" };
}