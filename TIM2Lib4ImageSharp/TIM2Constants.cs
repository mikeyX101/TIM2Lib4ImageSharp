namespace TIM2Lib4ImageSharp;

internal static class TIM2Constants
{
    /// <summary>
    /// The list of mimetypes that equate to a Tim2.
    /// </summary>
    public static readonly IEnumerable<string> MimeTypes = new[] { "image/tim2" };

    /// <summary>
    /// The list of file extensions that equate to a Tim2.
    /// </summary>
    public static readonly IEnumerable<string> FileExtensions = new[] { "tm2" };
    
    public static ulong HeaderValue => 0x54494D3204000100;
    
    /// <summary>
    /// Gets the header bytes identifying a Tim2.
    /// </summary>
    public static ReadOnlySpan<byte> HeaderBytes => new byte[]
    {
        0x54, // T 
        0x49, // I
        0x4D, // M
        0x32, // 2
        0x04, 
        0x00, 
        0x01, 
        0x00
    };
}