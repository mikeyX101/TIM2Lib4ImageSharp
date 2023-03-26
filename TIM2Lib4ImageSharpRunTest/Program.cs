using TIM2Lib4ImageSharp;

SixLabors.ImageSharp.Configuration.Default.Configure(new TIM2ConfigurationModule());

Image img = Image.Load(args[0]);
img.SaveAsPng(args[1]);