﻿using System;
using Core2D;
using Core2D.Containers;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Editor.Tools.Selection
{
    public class ToolEllipseSelection
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LayerContainer _layer;
        private readonly EllipseShape _ellipse;
        private readonly ShapeStyle _style;
        private PointShape _topLeftHelperPoint;
        private PointShape _bottomRightHelperPoint;

        public ToolEllipseSelection(IServiceProvider serviceProvider, LayerContainer layer, EllipseShape shape, ShapeStyle style)
        {
            _serviceProvider = serviceProvider;
            _layer = layer;
            _ellipse = shape;
            _style = style;
        }

        public void ToStateBottomRight()
        {
            _topLeftHelperPoint = _serviceProvider.GetService<IFactory>().CreatePointShape(0, 0);
            _bottomRightHelperPoint = _serviceProvider.GetService<IFactory>().CreatePointShape(0, 0);

            _layer.Shapes = _layer.Shapes.Add(_topLeftHelperPoint);
            _layer.Shapes = _layer.Shapes.Add(_bottomRightHelperPoint);
        }

        public void Move()
        {
            if (_topLeftHelperPoint != null)
            {
                _topLeftHelperPoint.X = _ellipse.TopLeft.X;
                _topLeftHelperPoint.Y = _ellipse.TopLeft.Y;
            }

            if (_bottomRightHelperPoint != null)
            {
                _bottomRightHelperPoint.X = _ellipse.BottomRight.X;
                _bottomRightHelperPoint.Y = _ellipse.BottomRight.Y;
            }

            _layer.InvalidateLayer();
        }

        public void Reset()
        {
            if (_topLeftHelperPoint != null)
            {
                _layer.Shapes = _layer.Shapes.Remove(_topLeftHelperPoint);
                _topLeftHelperPoint = null;
            }

            if (_bottomRightHelperPoint != null)
            {
                _layer.Shapes = _layer.Shapes.Remove(_bottomRightHelperPoint);
                _bottomRightHelperPoint = null;
            }

            _layer.InvalidateLayer();
        }
    }
}
