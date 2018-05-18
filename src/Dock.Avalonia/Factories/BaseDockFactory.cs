﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dock.Model;

namespace Dock.Avalonia.Factories
{
    /// <summary>
    /// Dock factory base.
    /// </summary>
    public abstract class BaseDockFactory : IDockFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initialize new instance of <see cref="BaseDockFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public BaseDockFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public virtual void UpdateWindows(IList<IDockWindow> windows, IList<IDock> views, object context)
        {
            foreach (var window in windows)
            {
                var host = (IDockHost)_serviceProvider.GetService(typeof(IDockHost));

                window.Host = host;
                window.Context = context;

                UpdateLayout(window.Layout, views, context);
            }
        }

        /// <inheritdoc/>
        public virtual void UpdateViews(IList<IDock> target, IList<IDock> views, object context)
        {
            for (int i = 0; i < target.Count; i++)
            {
                var original = target[i];
                target[i] = views.FirstOrDefault(v => v.Title == original.Title);

                if (original.Windows != null)
                {
                    target[i].Windows = original.Windows;

                    UpdateWindows(original.Windows, views, context);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void UpdateLayout(IDock layout, IList<IDock> views, object context)
        {
            UpdateViews(layout.Views, views, context);

            layout.CurrentView = views.FirstOrDefault(v => v.Title == layout.CurrentView?.Title);
            layout.Factory = this;

            if (layout.Views != null)
            {
                foreach (var view in layout.Views)
                {
                    if (view is IDock childLayout)
                    {
                        UpdateLayout(childLayout, views, context);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual IDockWindow CreateDockWindow(IDock layout, object context, IDock container, int viewIndex, double x, double y)
        {
            var view = container.Views[viewIndex];

            layout.RemoveView(container, viewIndex);

            var dockLayout = new DockLayout
            {
                Dock = "",
                CurrentView = view,
                Views = new ObservableCollection<IDock>
                {
                    new DockLayout
                    {
                        Dock = "",
                        Views = new ObservableCollection<IDock> { view },
                        CurrentView = view,
                        Factory = this
                    }
                },
                Factory = this
            };

            var host = (IDockHost)_serviceProvider.GetService(typeof(IDockHost));

            var dockWindow = new DockWindow()
            {
                X = x,
                Y = y,
                Width = 300,
                Height = 400,
                Title = "Dock",
                Context = context,
                Layout = dockLayout,
                Host = host
            };

            if (layout.CurrentView.Windows == null)
            {
                layout.CurrentView.Windows = new ObservableCollection<IDockWindow>();
            }
            layout.CurrentView.AddWindow(dockWindow);

            return dockWindow;
        }

        /// <inheritdoc/>
        public abstract IDock CreateDefaultLayout(IList<IDock> views);

        /// <inheritdoc/>
        public abstract void CreateOrUpdateLayout();
    }
}
