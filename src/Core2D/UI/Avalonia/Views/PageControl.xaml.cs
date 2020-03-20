﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactivity;
using Core2D.Editor;
using Core2D.Editor.Input;
using Core2D.UI.Avalonia.Utilities;

namespace Core2D.UI.Avalonia.Views
{
    /// <summary>
    /// Attaches <see cref="IProjectEditor"/> to a <see cref="Control"/>.
    /// </summary>
    public sealed class AttachEditorBehavior : Behavior<Control>
    {
        private AvaloniaInputSource _inputSource = null;
        private ProjectEditorInputTarget _inputTarget = null;
        private InputProcessor _inputProcessor = null;
        private bool _isLoaded = false;

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AttachEditor();
            }
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                DetachEditor();
            }
        }

        private void InvalidateChild(double zoomX, double zoomY, double offsetX, double offsetY)
        {
            var control = AssociatedObject;

            if (control.DataContext is IProjectEditor projectEditor)
            {
                var state = projectEditor.Renderers[0]?.State;
                if (state != null)
                {
                    bool invalidateCache = state.ZoomX != zoomX || state.ZoomY != zoomY;

                    state.ZoomX = zoomX;
                    state.ZoomY = zoomY;
                    state.PanX = offsetX;
                    state.PanY = offsetY;

                    if (invalidateCache)
                    {
                        projectEditor.OnInvalidateCache(isZooming: true);
                    }
                }
            }
        }

        private void AttachEditor()
        {
            var control = AssociatedObject;

            if (control.DataContext is IProjectEditor projectEditor)
            {
                var containerControlData = control.Find<ContainerControl>("containerControlData");
                var containerControlTemplate = control.Find<ContainerControl>("containerControlTemplate");
                var containerControlEditor = control.Find<ContainerControl>("containerControlEditor");
                var zoomBorder = control.Find<ZoomBorder>("zoomBorder");

                if (projectEditor.CanvasPlatform is IEditorCanvasPlatform canvasPlatform)
                {
                    canvasPlatform.Invalidate = () =>
                    {
                        containerControlData?.InvalidateVisual();
                        containerControlTemplate?.InvalidateVisual();
                        containerControlEditor?.InvalidateVisual();
                    };
                    canvasPlatform.ResetZoom = () => zoomBorder?.Reset();
                    canvasPlatform.AutoFitZoom = () => zoomBorder?.AutoFit();
                    canvasPlatform.Zoom = zoomBorder;
                }

                if (zoomBorder != null)
                {
                    zoomBorder.InvalidatedChild = InvalidateChild;
                }

                _inputSource = new AvaloniaInputSource(
                        zoomBorder,
                        containerControlEditor,
                        p => p);
                _inputTarget = new ProjectEditorInputTarget(projectEditor);
                _inputProcessor = new InputProcessor();
                _inputProcessor.Connect(_inputSource, _inputTarget);
            }
        }

        private void DetachEditor()
        {
            var control = AssociatedObject;

            if (control.DataContext is IProjectEditor projectEditor)
            {
                if (projectEditor.CanvasPlatform is IEditorCanvasPlatform canvasPlatform)
                {
                    canvasPlatform.Invalidate = null;
                    canvasPlatform.ResetZoom = null;
                    canvasPlatform.AutoFitZoom = null;
                    canvasPlatform.Zoom = null;
                }

                var _zoomBorder = control.Find<ZoomBorder>("zoomBorder");

                if (_zoomBorder != null)
                {
                    _zoomBorder.InvalidatedChild = null;
                }

                _inputProcessor.Dispose();
                _inputTarget = null;
                _inputProcessor = null;
                _inputSource = null;
            }
        }
    }

    /// <summary>
    /// Interaction logic for <see cref="PageControl"/> xaml.
    /// </summary>
    public class PageControl : UserControl
    {
        private AvaloniaInputSource _inputSource = null;
        private ProjectEditorInputTarget _inputTarget = null;
        private InputProcessor _inputProcessor = null;
        private bool _isLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageControl"/> class.
        /// </summary>
        public PageControl()
        {
            InitializeComponent();
            //AttachedToVisualTree += PageControl_Loaded;
            //DetachedFromVisualTree += PageControl_Unloaded;
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // TODO:
        //protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        //{
        //    base.OnAttachedToVisualTree(e);
        //    DetachEditor();
        //    AttachEditor();
        //}

        // TODO:
        //protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        //{
        //    base.OnDetachedFromVisualTree(e);
        //    DetachEditor();
        //}

        private void PageControl_Loaded(object sender, VisualTreeAttachmentEventArgs e)
        {
            DetachEditor();
            AttachEditor();
        }

        private void PageControl_Unloaded(object sender, VisualTreeAttachmentEventArgs e)
        {
            DetachEditor();
        }

        private void InvalidateChild(double zoomX, double zoomY, double offsetX, double offsetY)
        {
            if (DataContext is IProjectEditor projectEditor)
            {
                var state = projectEditor.Renderers[0]?.State;
                if (state != null)
                {
                    bool invalidateCache = state.ZoomX != zoomX || state.ZoomY != zoomY;

                    state.ZoomX = zoomX;
                    state.ZoomY = zoomY;
                    state.PanX = offsetX;
                    state.PanY = offsetY;

                    if (invalidateCache)
                    {
                        projectEditor.OnInvalidateCache(isZooming: true);
                    }
                }
            }
        }

        /// <summary>
        /// Attach project editor to container control.
        /// </summary>
        public void AttachEditor()
        {
            if (DataContext is IProjectEditor projectEditor)
            {
                if (_isLoaded)
                {
                    return;
                }

                var containerControlData = this.Find<ContainerControl>("containerControlData");
                var containerControlTemplate = this.Find<ContainerControl>("containerControlTemplate");
                var containerControlEditor = this.Find<ContainerControl>("containerControlEditor");
                var zoomBorder = this.Find<ZoomBorder>("zoomBorder");

                if (projectEditor.CanvasPlatform is IEditorCanvasPlatform canvasPlatform)
                {
                    canvasPlatform.Invalidate = () =>
                    {
                        containerControlData?.InvalidateVisual();
                        containerControlTemplate?.InvalidateVisual();
                        containerControlEditor?.InvalidateVisual();
                    };
                    canvasPlatform.ResetZoom = () => zoomBorder?.Reset();
                    canvasPlatform.AutoFitZoom = () => zoomBorder?.AutoFit();
                    canvasPlatform.Zoom = zoomBorder;
                }

                if (zoomBorder != null)
                {
                    zoomBorder.InvalidatedChild = InvalidateChild;
                }

                _inputSource = new AvaloniaInputSource(
                        zoomBorder,
                        containerControlEditor,
                        p => p);
                _inputTarget = new ProjectEditorInputTarget(projectEditor);
                _inputProcessor = new InputProcessor();
                _inputProcessor.Connect(_inputSource, _inputTarget);

                _isLoaded = true;
            }
        }

        /// <summary>
        /// Detach project editor from container control.
        /// </summary>
        public void DetachEditor()
        {
            if (DataContext is IProjectEditor projectEditor)
            {
                if (!_isLoaded)
                {
                    return;
                }

                if (projectEditor.CanvasPlatform is IEditorCanvasPlatform canvasPlatform)
                {
                    canvasPlatform.Invalidate = null;
                    canvasPlatform.ResetZoom = null;
                    canvasPlatform.AutoFitZoom = null;
                    canvasPlatform.Zoom = null;
                }

                var _zoomBorder = this.Find<ZoomBorder>("zoomBorder");

                if (_zoomBorder != null)
                {
                    _zoomBorder.InvalidatedChild = null;
                }

                _inputProcessor.Dispose();
                _inputTarget = null;
                _inputProcessor = null;
                _inputSource = null;

                _isLoaded = false;
            }
        }
    }
}
