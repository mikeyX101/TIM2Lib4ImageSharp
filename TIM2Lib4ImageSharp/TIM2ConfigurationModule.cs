using SixLabors.ImageSharp.Formats;

namespace TIM2Lib4ImageSharp;

public class TIM2ConfigurationModule : IImageFormatConfigurationModule
{
    public void Configure(Configuration configuration)
    {
        ImageFormatManager ifm = configuration.ImageFormatsManager;
        
        ifm.AddImageFormat(TIM2Format.Instance);
        ifm.AddImageFormatDetector(TIM2ImageFormatDetector.Instance);
        
        ifm.SetDecoder(TIM2Format.Instance, TIM2Decoder.Instance);
    }
}