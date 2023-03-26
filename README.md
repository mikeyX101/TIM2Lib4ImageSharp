# TIM2Lib4ImageSharp
**This was only tested with textures from one PS2 game for a niche use, it will probably not work with other TIM2 formats!**

Adds an ImageSharp decoder and configuration to decode TIM2 images. Encoder is not implemented and there is missing code that would also need to be ported.

Very lazily ported to .NET 7. Some code is not used and could be cleaned up. This code should be modified for your own uses or cleanly redone for a general usage.

This code was originally from [Marco Calautti's Rainbow ImgLib](https://github.com/marco-calautti/Rainbow/). Only the code for TIM2 images and pixel formats have been ported, other formats supported originally are not supported here.