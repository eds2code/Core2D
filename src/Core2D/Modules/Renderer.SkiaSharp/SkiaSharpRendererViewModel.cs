﻿using System;
using Core2D.ViewModels.Renderer;

namespace Core2D.Renderer.SkiaSharp
{
    public class SkiaSharpRendererViewModel : NodeRendererViewModel
    {
        public SkiaSharpRendererViewModel(IServiceProvider serviceProvider)
            : base(serviceProvider, new SkiaSharpDrawNodeFactory())
        {
        }
    }
}
