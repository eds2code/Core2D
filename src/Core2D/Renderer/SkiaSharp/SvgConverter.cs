﻿using System;
using System.Collections.Generic;
using System.IO;
using Core2D.Shapes;
using Svg.Skia;

namespace Core2D.Renderer.SkiaSharp
{
    /// <summary>
    /// Svg converter.
    /// </summary>
    public class SvgConverter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgConverter"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public SvgConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Converts svg file to shapes.
        /// </summary>
        /// <param name="path">The svg path.</param>
        /// <returns>The converted shapes.</returns>
        public IList<IBaseShape> Convert(string path)
        {
            var document = SKSvg.Open(path);

            if (document == null)
            {
                return null;
            }

            var drawable = SKSvg.ToDrawable(document);
            if (drawable == null)
            {
                return null;
            }

            var shapes = new List<IBaseShape>();

            switch (drawable)
            {
                case AnchorDrawable anchorDrawable:
                    {
                        // TODO:
                    }
                    break;
                case FragmentDrawable fragmentDrawable:
                    {
                        // TODO:
                    }
                    break;
                case ImageDrawable imageDrawable:
                    {
                        // TODO:
                    }
                    break;
                case SwitchDrawable switchDrawable:
                    {
                        // TODO:
                    }
                    break;
                case UseDrawable useDrawable:
                    {
                        // TODO:
                    }
                    break;
                case CircleDrawable circleDrawable:
                    {
                        // TODO:
                    }
                    break;
                case EllipseDrawable ellipseDrawable:
                    {
                        // TODO:
                    }
                    break;
                case RectangleDrawable rectangleDrawable:
                    {
                        // TODO:
                    }
                    break;
                case GroupDrawable groupDrawable:
                    {
                        // TODO:
                    }
                    break;
                case LineDrawable lineDrawable:
                    {
                        // TODO:
                    }
                    break;
                case PathDrawable pathDrawable:
                    {
                        // TODO:
                    }
                    break;
                case PolylineDrawable polylineDrawable:
                    {
                        // TODO:
                    }
                    break;
                case PolygonDrawable polygonDrawable:
                    {
                        // TODO:
                    }
                    break;
                case TextDrawable textDrawable:
                    {
                        // TODO:
                    }
                    break;
            }

            return shapes;
        }
    }
}
