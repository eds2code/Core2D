﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TestSIM;

namespace Test2d
{
    /// <summary>
    ///
    /// </summary>
    public class EditorContext : ObservableObject, IDisposable
    {
        private EditorCommands _commands;
        private Editor _editor;
        private IView _view;
        private IRenderer[] _renderers;
        private IProjectFactory _projectFactory;
        private ISimulationTimer _simulationTimer;
        private ITextClipboard _textClipboard;
        private ISerializer _serializer;
        private IScriptEngine _scriptEngine;
        private ICodeEngine _codeEngine;
        private IFileWriter _pdfWriter;
        private IFileWriter _dxfWriter;
        private ITextFieldReader<Database> _csvReader;
        private ITextFieldWriter<Database> _csvWriter;
        private ImmutableArray<RecentProject> _recentProjects = ImmutableArray.Create<RecentProject>();
        private string _rootScriptsPath;
        private ImmutableArray<ScriptDirectory> _scriptDirectories;
        private ScriptFile _currentScript;
        private bool _isSimulationPaused;
        private Clock _clock = default(Clock);
        private System.IO.FileSystemWatcher _watcher = default(System.IO.FileSystemWatcher);
        private Container _containerToCopy = default(Container);
        private Document _documentToCopy = default(Document);

        /// <summary>
        ///
        /// </summary>
        public EditorCommands Commands
        {
            get { return _commands; }
            set { Update(ref _commands, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public Editor Editor
        {
            get { return _editor; }
            set { Update(ref _editor, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public IView View
        {
            get { return _view; }
            set { Update(ref _view, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public IRenderer[] Renderers
        {
            get { return _renderers; }
            set { Update(ref _renderers, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public IProjectFactory ProjectFactory
        {
            get { return _projectFactory; }
            set { Update(ref _projectFactory, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ISimulationTimer SimulationTimer
        {
            get { return _simulationTimer; }
            set { Update(ref _simulationTimer, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ITextClipboard TextClipboard
        {
            get { return _textClipboard; }
            set { Update(ref _textClipboard, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ISerializer Serializer
        {
            get { return _serializer; }
            set { Update(ref _serializer, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public IScriptEngine ScriptEngine
        {
            get { return _scriptEngine; }
            set { Update(ref _scriptEngine, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ICodeEngine CodeEngine
        {
            get { return _codeEngine; }
            set { Update(ref _codeEngine, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public IFileWriter PdfWriter
        {
            get { return _pdfWriter; }
            set { Update(ref _pdfWriter, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public IFileWriter DxfWriter
        {
            get { return _dxfWriter; }
            set { Update(ref _dxfWriter, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ITextFieldReader<Database> CsvReader
        {
            get { return _csvReader; }
            set { Update(ref _csvReader, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ITextFieldWriter<Database> CsvWriter
        {
            get { return _csvWriter; }
            set { Update(ref _csvWriter, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ImmutableArray<RecentProject> RecentProjects
        {
            get { return _recentProjects; }
            set { Update(ref _recentProjects, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public string RootScriptsPath
        {
            get { return _rootScriptsPath; }
            set { Update(ref _rootScriptsPath, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ImmutableArray<ScriptDirectory> ScriptDirectories
        {
            get { return _scriptDirectories; }
            set { Update(ref _scriptDirectories, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public ScriptFile CurrentScript
        {
            get { return _currentScript; }
            set { Update(ref _currentScript, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public Clock Clock
        {
            get { return _clock; }
            set { Update(ref _clock, value); }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsSimulationPaused
        {
            get { return _isSimulationPaused; }
            set { Update(ref _isSimulationPaused, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnNew(object item)
        {
            if (item is Container)
            {
                var selected = item as Container;
                var document = _editor.Project.Documents.FirstOrDefault(d => d.Containers.Contains(selected));
                if (document != null)
                {
                    var container = _projectFactory.GetContainer(_editor.Project, "Container");

                    var previous = document.Containers;
                    var next = document.Containers.Add(container);
                    _editor.History.Snapshot(previous, next, (p) => document.Containers = p);
                    document.Containers = next;

                    _editor.Project.CurrentContainer = container;
                }
            }
            else if (item is Document)
            {
                var selected = item as Document;
                var container = _projectFactory.GetContainer(_editor.Project, "Container");

                var previous = selected.Containers;
                var next = selected.Containers.Add(container);
                _editor.History.Snapshot(previous, next, (p) => selected.Containers = p);
                selected.Containers = next;

                _editor.Project.CurrentContainer = container;
            }
            else if (item is Project)
            {
                var document = _projectFactory.GetDocument(_editor.Project, "Document");

                var previous = _editor.Project.Documents;
                var next = _editor.Project.Documents.Add(document);
                _editor.History.Snapshot(previous, next, (p) => _editor.Project.Documents = p);
                _editor.Project.Documents = next;

                _editor.Project.CurrentDocument = document;
                _editor.Project.CurrentContainer = document.Containers.FirstOrDefault();
            }
            else if (item is EditorContext || item == null)
            {
                New();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnClose()
        {
            Close();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void OnExit()
        {
            _view.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUndo()
        {
            try
            {
                if (_editor.History.CanUndo())
                {
                    _editor.Deselect();
                    _editor.History.Undo();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRedo()
        {
            try
            {
                if (_editor.History.CanRedo())
                {
                    _editor.Deselect();
                    _editor.History.Redo();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnCut()
        {
            try
            {
                if (CanCopy())
                {
                    OnCopy();

                    _editor.DeleteSelected();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnCopy()
        {
            try
            {
                if (CanCopy())
                {
                    if (_editor.Renderers[0].State.SelectedShape != null)
                    {
                        Copy(Enumerable.Repeat(_editor.Renderers[0].State.SelectedShape, 1).ToList());
                    }

                    if (_editor.Renderers[0].State.SelectedShapes != null)
                    {
                        Copy(_editor.Renderers[0].State.SelectedShapes.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnPaste()
        {
            try
            {
                if (CanPaste())
                {
                    var json = _textClipboard.GetText();
                    if (!string.IsNullOrEmpty(json))
                    {
                        Paste(json);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnCut(object item)
        {
            if (item is Container)
            {
                var container = item as Container;
                _containerToCopy = container;
                _documentToCopy = default(Document);
                _editor.Delete(container);
            }
            else if (item is Document)
            {
                var document = item as Document;
                _containerToCopy = default(Container);
                _documentToCopy = document;
                _editor.Delete(document);
            }
            else if (item is EditorContext || item == null)
            {
                OnCut();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnCopy(object item)
        {
            if (item is Container)
            {
                var container = item as Container;
                _containerToCopy = container;
                _documentToCopy = default(Document);
            }
            else if (item is Document)
            {
                var document = item as Document;
                _containerToCopy = default(Container);
                _documentToCopy = document;
            }
            else if (item is EditorContext || item == null)
            {
                OnCopy();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnPaste(object item)
        {
            if (item is Container)
            {
                if (_containerToCopy != null)
                {
                    var container = item as Container;
                    var document = _editor.Project.Documents.FirstOrDefault(d => d.Containers.Contains(container));
                    if (document != null)
                    {
                        int index = document.Containers.IndexOf(container);
                        var clone = Clone(_containerToCopy);

                        var builder = document.Containers.ToBuilder();
                        builder[index] = clone;
                        document.Containers = builder.ToImmutable();

                        var previous = document.Containers;
                        var next = builder.ToImmutable();
                        _editor.History.Snapshot(previous, next, (p) => document.Containers = p);
                        document.Containers = next;

                        _editor.Project.CurrentContainer = clone;
                    }
                }
            }
            else if (item is Document)
            {
                if (_containerToCopy != null)
                {
                    var document = item as Document;
                    var clone = Clone(_containerToCopy);

                    var previous = document.Containers;
                    var next = document.Containers.Add(clone);
                    _editor.History.Snapshot(previous, next, (p) => document.Containers = p);
                    document.Containers = next;

                    _editor.Project.CurrentContainer = clone;
                }
                else if (_documentToCopy != null)
                {
                    var document = item as Document;
                    int index = _editor.Project.Documents.IndexOf(document);
                    var clone = Clone(_documentToCopy);

                    var builder = _editor.Project.Documents.ToBuilder();
                    builder[index] = clone;

                    var previous = _editor.Project.Documents;
                    var next = builder.ToImmutable();
                    _editor.History.Snapshot(previous, next, (p) => _editor.Project.Documents = p);
                    _editor.Project.Documents = next;

                    _editor.Project.CurrentDocument = clone;
                }
            }
            else if (item is EditorContext || item == null)
            {
                OnPaste();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnDelete(object item)
        {
            if (item is Container)
            {
                var container = item as Container;
                _editor.Delete(container);
            }
            else if (item is Document)
            {
                var document = item as Document;
                _editor.Delete(document);
            }
            else if (item is EditorContext || item == null)
            {
                _editor.DeleteSelected();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnSelectAll()
        {
            try
            {
                _editor.Deselect(_editor.Project.CurrentContainer);
                _editor.Select(
                    _editor.Project.CurrentContainer,
                    ImmutableHashSet.CreateRange<BaseShape>(_editor.Project.CurrentContainer.CurrentLayer.Shapes));
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnDeselectAll()
        {
            try
            {
                _editor.Deselect(_editor.Project.CurrentContainer);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnClearAll()
        {
            try
            {
                // TODO: Add history snapshot.
                _editor.Project.CurrentContainer.Clear();
                _editor.Project.CurrentContainer.Invalidate();
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddDatabase()
        {
            if (_editor.Project == null)
                return;
            
            var builder = ImmutableArray.CreateBuilder<Column>();
            builder.Add(Column.Create("Column0"));
            builder.Add(Column.Create("Column1"));

            var db = Database.Create("New", builder.ToImmutable());

            var previous = _editor.Project.Databases;
            var next = _editor.Project.Databases.Add(db);
            _editor.History.Snapshot(previous, next, (p) => _editor.Project.Databases = p);
            _editor.Project.Databases = next;

            _editor.Project.CurrentDatabase = db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        public void OnRemoveDatabase(object db)
        {
            if (_editor.Project == null)
                return;
            
            if (db != null && db is Database)
            {
                var previous = _editor.Project.Databases;
                var next = _editor.Project.Databases.Remove(db as Database);
                _editor.History.Snapshot(previous, next, (p) => _editor.Project.Databases = p);
                _editor.Project.Databases = next;

                _editor.Project.CurrentDatabase = _editor.Project.Databases.FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public void OnAddColumn(object owner)
        {
            if (owner != null && owner is Database)
            {
                var db = owner as Database;
                if (db.Columns == null)
                {
                    db.Columns = ImmutableArray.Create<Column>();
                }

                var previous = db.Columns;
                var next = db.Columns.Add(Column.Create("Column" + db.Columns.Length));
                _editor.History.Snapshot(previous, next, (p) => db.Columns = p);
                db.Columns = next;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void OnRemoveColumn(object parameter)
        {
            if (parameter != null && parameter is ColumnParameter)
            {
                var owner = (parameter as ColumnParameter).Owner;
                var column = (parameter as ColumnParameter).Column;

                if (owner is Database)
                {
                    var db = owner as Database;
                    if (db.Columns != null)
                    {
                        var previous = db.Columns;
                        var next = db.Columns.Remove(column);
                        _editor.History.Snapshot(previous, next, (p) => db.Columns = p);
                        db.Columns = next;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddRecord()
        {
            if (_editor.Project == null || _editor.Project.CurrentDatabase == null)
                return;

            var db = _editor.Project.CurrentDatabase;

            var values = Enumerable.Repeat("<empty>", db.Columns.Length).Select(c => Value.Create(c));
            var record = Record.Create(
                db.Columns,
                ImmutableArray.CreateRange<Value>(values));

            var previous = db.Records;
            var next = db.Records.Add(record);
            _editor.History.Snapshot(previous, next, (p) => db.Records = p);
            db.Records = next;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveRecord()
        {
            if (_editor.Project == null || _editor.Project.CurrentDatabase == null)
                return;
            
            var db = _editor.Project.CurrentDatabase;
            if (db.CurrentRecord != null)
            {
                var record = db.CurrentRecord;

                var previous = db.Records;
                var next = db.Records.Remove(record);
                _editor.History.Snapshot(previous, next, (p) => db.Records = p);
                db.Records = next;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public void OnResetRecord(object owner)
        {
            if (owner != null && owner is BaseShape)
            {
                var shape = owner as BaseShape;
                var record = shape.Record;

                if (record != null)
                {
                    var previous = record;
                    var next = default(Record);
                    _editor.History.Snapshot(previous, next, (p) => shape.Record = p);
                    shape.Record = next;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public void OnAddBinding(object owner)
        {
            if (owner != null && owner is BaseShape)
            {
                var shape = owner as BaseShape;
                if (shape.Bindings == null)
                {
                    shape.Bindings = ImmutableArray.Create<ShapeBinding>();
                }

                _editor.AddWithHistory(shape, ShapeBinding.Create("", ""));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void OnRemoveBinding(object parameter)
        {
            if (parameter != null && parameter is ShapeBindingParameter)
            {
                var owner = (parameter as ShapeBindingParameter).Owner;
                var binding = (parameter as ShapeBindingParameter).Binding;

                if (owner != null && owner is BaseShape)
                {
                    var shape = owner as BaseShape;
                    if (shape.Bindings != null)
                    {
                        var previous = shape.Bindings;
                        var next = shape.Bindings.Remove(binding);
                        _editor.History.Snapshot(previous, next, (p) => shape.Bindings = p);
                        shape.Bindings = next;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public void OnAddProperty(object owner)
        {
            if (owner != null)
            {
                if (owner is BaseShape)
                {
                    var shape = owner as BaseShape;
                    if (shape.Properties == null)
                    {
                        shape.Properties = ImmutableArray.Create<ShapeProperty>();
                    }
                    
                    _editor.AddWithHistory(shape, ShapeProperty.Create("New", ""));
                }
                else if (owner is Container)
                {
                    var container = owner as Container;
                    if (container.Properties == null)
                    {
                        container.Properties = ImmutableArray.Create<ShapeProperty>();
                    }
                    
                    _editor.AddWithHistory(container, ShapeProperty.Create("New", ""));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        public void OnRemoveProperty(object parameter)
        {
            if (parameter != null && parameter is ShapePropertyParameter)
            {
                var owner = (parameter as ShapePropertyParameter).Owner;
                var property = (parameter as ShapePropertyParameter).Property;

                if (owner is BaseShape)
                {
                    var shape = owner as BaseShape;
                    if (shape.Properties != null)
                    {
                        var previous = shape.Properties;
                        var next = shape.Properties.Remove(property);
                        _editor.History.Snapshot(previous, next, (p) => shape.Properties = p);
                        shape.Properties = next;
                    }
                }
                else if (owner is Container)
                {
                    var container = owner as Container;
                    if (container.Properties != null)
                    {
                        var previous = container.Properties;
                        var next = container.Properties.Remove(property);
                        _editor.History.Snapshot(previous, next, (p) => container.Properties = p);
                        container.Properties = next;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddGroupLibrary()
        {
            if (_editor.Project == null || _editor.Project.GroupLibraries == null)
                return;
            
            var gl = GroupLibrary.Create("New");

            var previous = _editor.Project.GroupLibraries;
            var next = _editor.Project.GroupLibraries.Add(gl);
            _editor.History.Snapshot(previous, next, (p) => _editor.Project.GroupLibraries = p);
            _editor.Project.GroupLibraries = next;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveGroupLibrary()
        {
            _editor.RemoveCurrentGroupLibrary();
        }
 
        /// <summary>
        /// 
        /// </summary>
        public void OnAddGroup()
        {
            if (_editor.Project == null || _editor.Project.CurrentGroupLibrary == null)
                return;
            
            var group = _editor.Renderers[0].State.SelectedShape;
            if (group != null && group is XGroup)
            {
                if (_editor.Project.CurrentGroupLibrary != null)
                {
                    var clone = Clone(group as XGroup);
                    if (clone != null)
                    {
                        var gl = _editor.Project.CurrentGroupLibrary;
                        var previous = gl.Groups;
                        var next = gl.Groups.Add(clone);
                        _editor.History.Snapshot(previous, next, (p) => gl.Groups = p);
                        gl.Groups = next;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveGroup()
        {
            _editor.RemoveCurrentGroup();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddLayer()
        {
            if (_editor.Project == null || _editor.Project.CurrentContainer == null)
                return;

            var container = _editor.Project.CurrentContainer;
            var previous = container.Layers;
            var next = container.Layers.Add(Layer.Create("New", container));
            _editor.History.Snapshot(previous, next, (p) => container.Layers = p);
            container.Layers = next;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveLayer()
        {
            _editor.RemoveCurrentLayer();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddStyleLibrary()
        {
            if (_editor.Project == null || _editor.Project.StyleLibraries == null)
                return;
            
            var sg = StyleLibrary.Create("New");

            var previous = _editor.Project.StyleLibraries;
            var next = _editor.Project.StyleLibraries.Add(sg);
            _editor.History.Snapshot(previous, next, (p) => _editor.Project.StyleLibraries = p);
            _editor.Project.StyleLibraries = next;
        }
 
        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveStyleLibrary()
        {
            _editor.RemoveCurrentStyleLibrary();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddStyle()
        {
            if (_editor.Project == null || _editor.Project.CurrentStyleLibrary == null)
                return;
            
            var sg = _editor.Project.CurrentStyleLibrary;
            var previous = sg.Styles;
            var next = sg.Styles.Add(ShapeStyle.Create("New"));
            _editor.History.Snapshot(previous, next, (p) => sg.Styles = p);
            sg.Styles = next;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveStyle()
        {
            _editor.RemoveCurrentStyle();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveShape()
        {
            _editor.RemoveCurrentShape();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAddTemplate()
        {
            if (_editor.Project == null)
                return;
            
            var previous = _editor.Project.Templates;
            var next = _editor.Project.Templates.Add(_projectFactory.GetTemplate(_editor.Project, "Empty"));
            _editor.History.Snapshot(previous, next, (p) => _editor.Project.Templates = p);
            _editor.Project.Templates = next;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRemoveTemplate()
        {
            _editor.RemoveCurrentTemplate();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnEditTemplate()
        {
            if (_editor.Project == null || _editor.Project.CurrentTemplate == null)
                return;
            
            var template = _editor.Project.CurrentTemplate;
            if (template != null)
            {
                _editor.Project.CurrentContainer = template;
                _editor.Project.CurrentContainer.Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnApplyTemplate(object item)
        {
            if (_editor.Project == null || _editor.Project.CurrentContainer == null)
                return;
            
            if (item is Container)
            {
                var template = item as Container;

                var previous = _editor.Project.CurrentContainer.Template;
                var next = template;
                _editor.History.Snapshot(previous, next, (p) => _editor.Project.CurrentContainer.Template = p);
                _editor.Project.CurrentContainer.Template = next;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnSelectedItemChanged(object item)
        {
            if (_editor.Project == null)
                return;
            
            if (item is Container)
            {
                var selected = item as Container;
                var document = _editor.Project.Documents.FirstOrDefault(d => d.Containers.Contains(selected));
                if (document != null)
                {
                    _editor.Project.CurrentDocument = document;
                    _editor.Project.CurrentContainer = selected;
                    _editor.Project.CurrentContainer.Invalidate();
                }
            }
            else if (item is Document)
            {
                var selected = item as Document;
                _editor.Project.CurrentDocument = selected;
            }
        }
  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnAddContainer(object item)
        {
            if (_editor.Project == null || _editor.Project.CurrentDocument == null)
                return;
            
            var container = _projectFactory.GetContainer(_editor.Project, "Container");

            var document = _editor.Project.CurrentDocument;
            var previous = document.Containers;
            var next = document.Containers.Add(container);
            _editor.History.Snapshot(previous, next, (p) => document.Containers = p);
            document.Containers = next;

            _editor.Project.CurrentContainer = container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnInsertContainerBefore(object item)
        {
            if (_editor.Project == null || _editor.Project.CurrentDocument == null)
                return;
            
            if (item is Container)
            {
                var selected = item as Container;
                int index = _editor.Project.CurrentDocument.Containers.IndexOf(selected);
                var container = _projectFactory.GetContainer(_editor.Project, "Container");

                var document = _editor.Project.CurrentDocument;
                var previous = document.Containers;
                var next = document.Containers.Insert(index, container);
                _editor.History.Snapshot(previous, next, (p) => document.Containers = p);
                document.Containers = next;

                _editor.Project.CurrentContainer = container;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnInsertContainerAfter(object item)
        {
            if (_editor.Project == null || _editor.Project.CurrentDocument == null)
                return;
            
            if (item is Container)
            {
                var selected = item as Container;
                int index = _editor.Project.CurrentDocument.Containers.IndexOf(selected);
                var container = _projectFactory.GetContainer(_editor.Project, "Container");

                var document = _editor.Project.CurrentDocument;
                var previous = document.Containers;
                var next = document.Containers.Insert(index + 1, container);
                _editor.History.Snapshot(previous, next, (p) => document.Containers = p);
                document.Containers = next;

                _editor.Project.CurrentContainer = container;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnAddDocument(object item)
        {
            if (_editor.Project == null)
                return;
            
            var document = _projectFactory.GetDocument(_editor.Project, "Document");

            var previous = _editor.Project.Documents;
            var next = _editor.Project.Documents.Add(document);
            _editor.History.Snapshot(previous, next, (p) => _editor.Project.Documents = p);
            _editor.Project.Documents = next;

            _editor.Project.CurrentDocument = document;
            _editor.Project.CurrentContainer = document.Containers.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnInsertDocumentBefore(object item)
        {
            if (_editor.Project == null)
                return;
            
            if (item is Document)
            {
                var selected = item as Document;
                int index = _editor.Project.Documents.IndexOf(selected);
                var document = _projectFactory.GetDocument(_editor.Project, "Document");

                var previous = _editor.Project.Documents;
                var next = _editor.Project.Documents.Insert(index, document);
                _editor.History.Snapshot(previous, next, (p) => _editor.Project.Documents = p);
                _editor.Project.Documents = next;

                _editor.Project.CurrentDocument = document;
                _editor.Project.CurrentContainer = document.Containers.FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void OnInsertDocumentAfter(object item)
        {
            if (_editor.Project == null)
                return;
            
            if (item is Document)
            {
                var selected = item as Document;
                int index = _editor.Project.Documents.IndexOf(selected);
                var document = _projectFactory.GetDocument(_editor.Project, "Document");

                var previous = _editor.Project.Documents;
                var next = _editor.Project.Documents.Insert(index + 1, document);
                _editor.History.Snapshot(previous, next, (p) => _editor.Project.Documents = p);
                _editor.Project.Documents = next;

                _editor.Project.CurrentDocument = document;
                _editor.Project.CurrentContainer = document.Containers.FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolNone()
        {
            _editor.CurrentTool = Tool.None;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolSelection()
        {
            _editor.CurrentTool = Tool.Selection;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolGroup()
        {
            _editor.CurrentTool = Tool.Group;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolPoint()
        {
            _editor.CurrentTool = Tool.Point;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolLine()
        {
            if (_editor.CurrentTool == Tool.Path)
            {
                _editor.CurrentPathTool = PathTool.Line;
            }
            else
            {
                _editor.CurrentTool = Tool.Line;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolArc()
        {
            if (_editor.CurrentTool == Tool.Path)
            {
                _editor.CurrentPathTool = PathTool.Arc;
            }
            else
            {
                _editor.CurrentTool = Tool.Arc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolBezier()
        {
            if (_editor.CurrentTool == Tool.Path)
            {
                _editor.CurrentPathTool = PathTool.Bezier;
            }
            else
            {
                _editor.CurrentTool = Tool.Bezier;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolQBezier()
        {
            if (_editor.CurrentTool == Tool.Path)
            {
                _editor.CurrentPathTool = PathTool.QBezier;
            }
            else
            {
                _editor.CurrentTool = Tool.QBezier;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolPath()
        {
            _editor.CurrentTool = Tool.Path;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolRectangle()
        {
            _editor.CurrentTool = Tool.Rectangle;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolEllipse()
        {
            _editor.CurrentTool = Tool.Ellipse;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolText()
        {
            _editor.CurrentTool = Tool.Text;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToolImage()
        {
            _editor.CurrentTool = Tool.Image;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToggleDefaultIsStroked()
        {
            if (_editor.Project == null || _editor.Project.Options == null)
                return;
            
            _editor.Project.Options.DefaultIsStroked = !_editor.Project.Options.DefaultIsStroked;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void OnToggleDefaultIsFilled()
        {
            if (_editor.Project == null || _editor.Project.Options == null)
                return;
            
            _editor.Project.Options.DefaultIsFilled = !_editor.Project.Options.DefaultIsFilled;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToggleDefaultIsClosed()
        {
            if (_editor.Project == null || _editor.Project.Options == null)
                return;
            
            _editor.Project.Options.DefaultIsClosed = !_editor.Project.Options.DefaultIsClosed;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToggleDefaultIsSmoothJoin()
        {
            if (_editor.Project == null || _editor.Project.Options == null)
                return;
            
            _editor.Project.Options.DefaultIsSmoothJoin = !_editor.Project.Options.DefaultIsSmoothJoin;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToggleSnapToGrid()
        {
            if (_editor.Project == null || _editor.Project.Options == null)
                return;
            
            _editor.Project.Options.SnapToGrid = !_editor.Project.Options.SnapToGrid;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnToggleTryToConnect()
        {
            if (_editor.Project == null || _editor.Project.Options == null)
                return;
            
            _editor.Project.Options.TryToConnect = !_editor.Project.Options.TryToConnect;
        }

        /// <summary>
        ///
        /// </summary>
        public void OnTickSimulation()
        {
            try
            {
                if (IsSimulationMode())
                {
                    _editor.Observer.IsPaused = true;
                    _codeEngine.Run();
                    _editor.Observer.IsPaused = false;
                    if (_editor.Project.CurrentContainer != null)
                    {
                        _editor.Project.CurrentContainer.Invalidate();
                    }
                    _clock.Tick();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }

                if (IsSimulationMode())
                {
                    OnStopSimulation();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnStartSimulation()
        {
            try
            {
                if (IsSimulationMode())
                {
                    return;
                }

                _clock = new Clock(
                    cycle: 0L,
                    resolution: _editor.Project.Options.CycleResolution);

                var shapes = _editor.Project.Documents
                    .SelectMany(d => d.Containers)
                    .SelectMany(c => c.Layers)
                    .SelectMany(l => l.Shapes).ToImmutableArray();
                _codeEngine.Build(shapes, this);

                IsSimulationPaused = false;

                _simulationTimer.Start(
                    () =>
                    {
                        if (!IsSimulationPaused)
                        {
                            OnTickSimulation();
                        }
                    },
                    _clock.Resolution);

                UpdateCanExecuteState();
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnStopSimulation()
        {
            try
            {
                if (IsSimulationMode())
                {
                    _simulationTimer.Stop();
                    _codeEngine.Reset();
                    IsSimulationPaused = false;
                    UpdateCanExecuteState();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void OnRestartSimulation()
        {
            OnStopSimulation();
            OnStartSimulation();
        }

        /// <summary>
        ///
        /// </summary>
        public void OnPauseSimulation()
        {
            try
            {
                if (IsSimulationMode())
                {
                    IsSimulationPaused = !IsSimulationPaused;
                    UpdateCanExecuteState();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="isZooming"></param>
        public void Invalidate(bool isZooming)
        {
            try
            {
                foreach (var renderer in _editor.Renderers)
                {
                    renderer.ClearCache(isZooming);
                }

                _editor.Project.CurrentContainer.Invalidate();
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="context"></param>
        public void Eval(string code, EditorContext context)
        {
            try
            {
                _scriptEngine.Eval(code, context);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public void Eval(string path)
        {
            try
            {
                var code = System.IO.File.ReadAllText(path);
                var context = this;
                Eval(code, context);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <param name="compress"></param>
        public string ReadUtf8Text(string path, bool compress = true)
        {
            try
            {
                if (compress)
                {
                    using (var fs = System.IO.File.OpenRead(path))
                    {
                        using (var cs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress))
                        {
                            using (var sr = new System.IO.StreamReader(cs, Encoding.UTF8))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
                else
                {
                    using (var fs = System.IO.File.OpenRead(path))
                    {
                        using (var sr = new System.IO.StreamReader(fs, Encoding.UTF8))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
            return null;
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="compress"></param>
        public void WriteUtf8Text(string path, string text, bool compress = true)
        {
            try
            {
                if (compress)
                {
                    using (var fs = System.IO.File.Create(path))
                    {
                        using (var cs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress))
                        {
                            using (var sw = new System.IO.StreamWriter(cs, Encoding.UTF8))
                            {
                                sw.Write(text);
                            }
                        }
                    }
                }
                else
                {
                    using (var fs = System.IO.File.Create(path))
                    {
                        using (var sw = new System.IO.StreamWriter(fs, Encoding.UTF8))
                        {
                            sw.Write(text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void New()
        {
            _editor.History.Reset();
            _editor.Unload();
            _editor.Load(_projectFactory.GetProject(), string.Empty);
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public void Open(string path)
        {
            try
            {
                var json = ReadUtf8Text(path);
                var project = _serializer.FromJson<Project>(json);

                //var root = new Uri(path);
                //var images = Editor.GetAllShapes<XImage>(project);
                //_editor.ToAbsoluteUri(root, images);

                _editor.History.Reset();
                _editor.Unload();
                _editor.Load(project, path);

                AddRecent(path, project.Name);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            try
            {
                //var root = new Uri(path);
                //var images = Editor.GetAllShapes<XImage>(_editor.Project);
                //_editor.ToRelativeUri(root, images);

                var json = _serializer.ToJson(_editor.Project);
                WriteUtf8Text(path, json);

                //_editor.ToAbsoluteUri(root, images);

                AddRecent(path, _editor.Project.Name);

                if (string.IsNullOrEmpty(_editor.ProjectPath))
                {
                    _editor.ProjectPath = path;
                }

                _editor.IsProjectDirty = false;
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            _editor.History.Reset();
            _editor.Unload();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        public void ExportAsPdf(string path, object item)
        {
            try
            {
                if (_pdfWriter != null)
                {
                    _pdfWriter.Save(path, item, null);
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="version"></param>
        public void ExportAsDxf(string path, Dxf.DxfAcadVer version)
        {
            try
            {
                if (_dxfWriter != null)
                {
                    _dxfWriter.Save(path, _editor.Project.CurrentContainer, version);
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        /// <param name="type"></param>
        public void ImportObject(string path, object item, ImportType type)
        {
            try
            {
                switch (type)
                {
                    case ImportType.Style:
                        {
                            var sg = item as StyleLibrary;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<ShapeStyle>(json);

                            var previous = sg.Styles;
                            var next = sg.Styles.Add(import);
                            _editor.History.Snapshot(previous, next, (p) => sg.Styles = p);
                            sg.Styles = next;
                        }
                        break;
                    case ImportType.Styles:
                        {
                            var sg = item as StyleLibrary;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<IList<ShapeStyle>>(json);

                            var builder = sg.Styles.ToBuilder();
                            foreach (var style in import)
                            {
                                builder.Add(style);
                            }

                            var previous = sg.Styles;
                            var next = builder.ToImmutable();
                            _editor.History.Snapshot(previous, next, (p) => sg.Styles = p);
                            sg.Styles = next;
                        }
                        break;
                    case ImportType.StyleLibrary:
                        {
                            var project = item as Project;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<StyleLibrary>(json);

                            var previous = project.StyleLibraries;
                            var next = project.StyleLibraries.Add(import);
                            _editor.History.Snapshot(previous, next, (p) => project.StyleLibraries = p);
                            project.StyleLibraries = next;
                        }
                        break;
                    case ImportType.StyleLibraries:
                        {
                            var project = item as Project;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<IList<StyleLibrary>>(json);

                            var builder = project.StyleLibraries.ToBuilder();
                            foreach (var sg in import)
                            {
                                builder.Add(sg);
                            }

                            var previous = project.StyleLibraries;
                            var next = builder.ToImmutable();
                            _editor.History.Snapshot(previous, next, (p) => project.StyleLibraries = p);
                            project.StyleLibraries = next;
                        }
                        break;
                    case ImportType.Group:
                        {
                            var gl = item as GroupLibrary;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<XGroup>(json);

                            var shapes = Enumerable.Repeat(import as XGroup, 1);
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToAbsoluteUri(root, images);

                            var previous = gl.Groups;
                            var next = gl.Groups.Add(import);
                            _editor.History.Snapshot(previous, next, (p) => gl.Groups = p);
                            gl.Groups = next;
                        }
                        break;
                    case ImportType.Groups:
                        {
                            var gl = item as GroupLibrary;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<IList<XGroup>>(json);

                            var shapes = import;
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToAbsoluteUri(root, images);

                            var builder = gl.Groups.ToBuilder();
                            foreach (var group in import)
                            {
                                builder.Add(group);
                            }

                            var previous = gl.Groups;
                            var next = builder.ToImmutable();
                            _editor.History.Snapshot(previous, next, (p) => gl.Groups = p);
                            gl.Groups = next;
                        }
                        break;
                    case ImportType.GroupLibrary:
                        {
                            var project = item as Project;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<GroupLibrary>(json);

                            var shapes = import.Groups;
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToAbsoluteUri(root, images);

                            var previous = project.GroupLibraries;
                            var next = project.GroupLibraries.Add(import);
                            _editor.History.Snapshot(previous, next, (p) => project.GroupLibraries = p);
                            project.GroupLibraries = next;
                        }
                        break;
                    case ImportType.GroupLibraries:
                        {
                            var project = item as Project;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<IList<GroupLibrary>>(json);

                            var shapes = import.SelectMany(x => x.Groups);
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToAbsoluteUri(root, images);

                            var builder = project.GroupLibraries.ToBuilder();
                            foreach (var library in import)
                            {
                                builder.Add(library);
                            }

                            var previous = project.GroupLibraries;
                            var next = builder.ToImmutable();
                            _editor.History.Snapshot(previous, next, (p) => project.GroupLibraries = p);
                            project.GroupLibraries = next;
                        }
                        break;
                    case ImportType.Template:
                        {
                            var project = item as Project;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<Container>(json);

                            var shapes = import.Layers.SelectMany(x => x.Shapes);
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToAbsoluteUri(root, images);

                            var previous = project.Templates;
                            var next = project.Templates.Add(import);
                            _editor.History.Snapshot(previous, next, (p) => project.Templates = p);
                            project.Templates = next;
                        }
                        break;
                    case ImportType.Templates:
                        {
                            var project = item as Project;
                            var json = ReadUtf8Text(path, false);
                            var import = _serializer.FromJson<IList<Container>>(json);

                            var shapes = import.SelectMany(x => x.Layers).SelectMany(x => x.Shapes);
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToAbsoluteUri(root, images);

                            var builder = project.Templates.ToBuilder();
                            foreach (var template in import)
                            {
                                builder.Add(template);
                            }

                            var previous = project.Templates;
                            var next = builder.ToImmutable();
                            _editor.History.Snapshot(previous, next, (p) => project.Templates = p);
                            project.Templates = next;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        /// <param name="type"></param>
        public void ExportObject(string path, object item, ExportType type)
        {
            try
            {
                switch (type)
                {
                    case ExportType.Style:
                        {
                            var json = _serializer.ToJson(item as ShapeStyle);
                            WriteUtf8Text(path, json, false);
                        }
                        break;
                    case ExportType.Styles:
                        {
                            var json = _serializer.ToJson((item as StyleLibrary).Styles);
                            WriteUtf8Text(path, json, false);
                        }
                        break;
                    case ExportType.StyleLibrary:
                        {
                            var json = _serializer.ToJson((item as StyleLibrary));
                            WriteUtf8Text(path, json, false);
                        }
                        break;
                    case ExportType.StyleLibraries:
                        {
                            var json = _serializer.ToJson((item as Project).StyleLibraries);
                            WriteUtf8Text(path, json, false);
                        }
                        break;
                    case ExportType.Group:
                        {
                            var shapes = Enumerable.Repeat(item as XGroup, 1);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToRelativeUri(root, images);

                            var json = _serializer.ToJson(item as XGroup);
                            WriteUtf8Text(path, json, false);

                            //_editor.ToAbsoluteUri(root, images);
                        }
                        break;
                    case ExportType.Groups:
                        {
                            var shapes = (item as GroupLibrary).Groups;
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToRelativeUri(root, images);

                            var json = _serializer.ToJson((item as GroupLibrary).Groups);
                            WriteUtf8Text(path, json, false);

                            //_editor.ToAbsoluteUri(root, images);
                        }
                        break;
                    case ExportType.GroupLibrary:
                        {
                            var shapes = (item as GroupLibrary).Groups;
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToRelativeUri(root, images);

                            var json = _serializer.ToJson(item as GroupLibrary);
                            WriteUtf8Text(path, json, false);

                            //_editor.ToAbsoluteUri(root, images);
                        }
                        break;
                    case ExportType.GroupLibraries:
                        {
                            var shapes = (item as Project).GroupLibraries.SelectMany(x => x.Groups);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToRelativeUri(root, images);

                            var json = _serializer.ToJson((item as Project).GroupLibraries);
                            WriteUtf8Text(path, json, false);

                            //_editor.ToAbsoluteUri(root, images);
                        }
                        break;
                    case ExportType.Template:
                        {
                            var shapes = (item as Container).Layers.SelectMany(x => x.Shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToRelativeUri(root, images);

                            var json = _serializer.ToJson(item as Container);
                            WriteUtf8Text(path, json, false);

                            //_editor.ToAbsoluteUri(root, images);
                        }
                        break;
                    case ExportType.Templates:
                        {
                            var shapes = (item as Project).Templates.SelectMany(x => x.Layers).SelectMany(x => x.Shapes);
                            var root = new Uri(path);
                            var images = Editor.GetAllShapes<XImage>(shapes);
                            //_editor.ToRelativeUri(root, images);

                            var json = _serializer.ToJson((item as Project).Templates);
                            WriteUtf8Text(path, json, false);

                            //_editor.ToAbsoluteUri(root, images);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public void ImportData(string path)
        {
            if (_editor.Project == null)
                return;
            
            try
            {
                if (_csvReader == null)
                    return;

                var db = _csvReader.Read(path);

                var previous = _editor.Project.Databases;
                var next = _editor.Project.Databases.Add(db);
                _editor.History.Snapshot(previous, next, (p) => _editor.Project.Databases = p);
                _editor.Project.Databases = next;

                _editor.Project.CurrentDatabase = db;
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="database"></param>
        public void ExportData(string path, Database database)
        {
            try
            {
                if (_csvWriter == null)
                    return;

                _csvWriter.Write(path, database);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="database"></param>
        public void UpdateData(string path, Database database)
        {
            try
            {
                if (_csvReader == null)
                    return;

                var db = _csvReader.Read(path);

                if (db.Columns.Length <= 1)
                    return;

                // check for the Id column
                if (db.Columns[0].Name != "Id")
                    return;

                // skip Id column for update
                if (db.Columns.Length - 1 != database.Columns.Length)
                    return;

                // check column names
                for (int i = 1; i < db.Columns.Length; i++)
                {
                    if (db.Columns[i].Name != database.Columns[i - 1].Name)
                        return;
                }

                bool isDirty = false;
                var recordsBuilder = database.Records.ToBuilder();

                for (int i = 0; i < database.Records.Length; i++)
                {
                    var record = database.Records[i];
                    
                    var result = db.Records.FirstOrDefault(r => r.Id == record.Id);
                    if (result != null)
                    {
                        // update existing record
                        for (int j = 1; j < result.Values.Length; j++)
                        {
                            var valuesBuilder = record.Values.ToBuilder();
                            valuesBuilder[j - 1] = result.Values[j];
                            record.Values = valuesBuilder.ToImmutable();
                        }
                        isDirty = true;
                    }
                    else
                    {
                        var r = db.Records[i];

                        // use existing columns
                        r.Columns = database.Columns;

                        // skip Id column
                        r.Values = r.Values.Skip(1).ToImmutableArray();

                        recordsBuilder.Add(r);
                        isDirty = true;
                    }
                }

                if (isDirty)
                {
                    //var previous = database.Records;
                    //var next = builder.ToImmutable();
                    //_editor.History.Snapshot(previous, next, (p) => database.Records = p);
                    //database.Records = next;

                    var builder = _editor.Project.Databases.ToBuilder();
                    var index = builder.IndexOf(database);
                    database.Records = recordsBuilder.ToImmutable();
                    builder[index] = database;

                    var previous = _editor.Project.Databases;
                    var next = builder.ToImmutable();
                    _editor.History.Snapshot(previous, next, (p) => _editor.Project.Databases = p);
                    _editor.Project.Databases = next;
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="shape"></param>
        public void ImportShapeCode(string path, BaseShape shape)
        {
            try
            {
                var json = ReadUtf8Text(path, false);
                var code = _serializer.FromJson<ShapeCode>(json);

                var previous = shape.Code;
                var next = code;
                _editor.History.Snapshot(previous, next, (p) => shape.Code = p);
                shape.Code = next;
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="shape"></param>
        public void ExportShapeCode(string path, BaseShape shape)
        {
            try
            {
                var json = _serializer.ToJson(shape.Code);
                WriteUtf8Text(path, json, false);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Add recent project file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        private void AddRecent(string path, string name)
        {
            var q = _recentProjects.Where(x => x.Path.ToLower() == path.ToLower()).ToList();
            var builder = _recentProjects.ToBuilder();

            if (q.Count() > 0)
            {
                foreach (var r in q)
                {
                    builder.Remove(r);
                }
            }

            builder.Insert(0, RecentProject.Create(name, path));

            RecentProjects = builder.ToImmutable();
        }

        /// <summary>
        /// Load recent project files.
        /// </summary>
        /// <param name="path"></param>
        public void LoadRecent(string path)
        {
            try
            {
                var json = ReadUtf8Text(path, false);
                var recent = _serializer.FromJson<ImmutableArray<RecentProject>>(json);

                if (recent != null)
                {
                    var remove = recent.Where(x => System.IO.File.Exists(x.Path) == false).ToList();
                    var builder = recent.ToBuilder();

                    foreach (var file in remove)
                    {
                        builder.Remove(file);
                    }

                    RecentProjects = builder.ToImmutable();
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Save recent project files.
        /// </summary>
        /// <param name="path"></param>
        public void SaveRecent(string path)
        {
            try
            {
                var json = _serializer.ToJson(_recentProjects);
                WriteUtf8Text(path, json, false);
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool CanCopy()
        {
            return _editor.IsSelectionAvailable();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool CanPaste()
        {
            try
            {
                return _textClipboard.ContainsText();
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapes"></param>
        private void Copy(IList<BaseShape> shapes)
        {
            try
            {
                var json = _serializer.ToJson(shapes);
                if (!string.IsNullOrEmpty(json))
                {
                    _textClipboard.SetText(json);
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="json"></param>
        public void Paste(string json)
        {
            try
            {
                var shapes = _serializer.FromJson<IList<BaseShape>>(json);
                if (shapes != null && shapes.Count() > 0)
                {
                    Paste(shapes);
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapes"></param>
        private void TryToRestoreStyles(IEnumerable<BaseShape> shapes)
        {
            try
            {
                if (_editor.Project.StyleLibraries == null)
                    return;

                var styles = _editor.Project.StyleLibraries
                    .Where(sg => sg.Styles != null && sg.Styles.Length > 0)
                    .SelectMany(sg => sg.Styles)
                    .Distinct(new StyleComparer())
                    .ToDictionary(s => s.Name);

                // reset point shape to container default
                foreach (var point in Editor.GetAllPoints(shapes, ShapeState.Connector))
                {
                    point.Shape = _editor.Project.Options.PointShape;
                }

                // try to restore shape styles
                foreach (var shape in Editor.GetAllShapes(shapes))
                {
                    if (shape.Style == null)
                        continue;

                    ShapeStyle style;
                    if (styles.TryGetValue(shape.Style.Name, out style))
                    {
                        // use existing style
                        shape.Style = style;
                    }
                    else
                    {
                        // create Imported style library
                        if (_editor.Project.CurrentStyleLibrary == null)
                        {
                            var sg = StyleLibrary.Create("Imported");
                            _editor.Project.StyleLibraries = _editor.Project.StyleLibraries.Add(sg);
                            _editor.Project.CurrentStyleLibrary = sg;
                        }

                        // add missing style
                        _editor.Project.CurrentStyleLibrary.Styles = _editor.Project.CurrentStyleLibrary.Styles.Add(shape.Style);

                        // recreate styles dictionary
                        styles = _editor.Project.StyleLibraries
                            .Where(sg => sg.Styles != null && sg.Styles.Length > 0)
                            .SelectMany(sg => sg.Styles)
                            .Distinct(new StyleComparer())
                            .ToDictionary(s => s.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapes"></param>
        private void TryToRestoreRecords(IEnumerable<BaseShape> shapes)
        {
            try
            {
                if (_editor.Project.Databases == null)
                    return;

                var records = _editor.Project.Databases
                    .Where(d => d.Records != null && d.Records.Length > 0)
                    .SelectMany(d => d.Records)
                    .ToDictionary(s => s.Id);

                // try to restore shape record
                foreach (var shape in Editor.GetAllShapes(shapes))
                {
                    if (shape.Record == null)
                        continue;

                    Record record;
                    if (records.TryGetValue(shape.Record.Id, out record))
                    {
                        // use existing record
                        shape.Record = record;
                    }
                    else
                    {
                        // create Imported database
                        if (_editor.Project.CurrentDatabase == null)
                        {
                            var db = Database.Create("Imported", shape.Record.Columns);
                            _editor.Project.Databases = _editor.Project.Databases.Add(db);
                            _editor.Project.CurrentDatabase = db;
                        }

                        // add missing data record
                        _editor.Project.CurrentDatabase.Records = _editor.Project.CurrentDatabase.Records.Add(shape.Record);

                        // recreate records dictionary
                        records = _editor.Project.Databases
                            .Where(d => d.Records != null && d.Records.Length > 0)
                            .SelectMany(d => d.Records)
                            .ToDictionary(s => s.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapes"></param>
        public void Paste(IEnumerable<BaseShape> shapes)
        {
            try
            {
                _editor.Deselect(_editor.Project.CurrentContainer);

                TryToRestoreStyles(shapes);
                TryToRestoreRecords(shapes);

                var layer = _editor.Project.CurrentContainer.CurrentLayer;

                var builder = layer.Shapes.ToBuilder();
                foreach (var shape in shapes)
                {
                    builder.Add(shape);
                }

                var previous = layer.Shapes;
                var next = builder.ToImmutable();
                _editor.History.Snapshot(previous, next, (p) => layer.Shapes = p);
                layer.Shapes = next;

                if (shapes.Count() == 1)
                {
                    _editor.Select(_editor.Project.CurrentContainer, shapes.FirstOrDefault());
                }
                else
                {
                    _editor.Select(_editor.Project.CurrentContainer, ImmutableHashSet.CreateRange<BaseShape>(shapes));
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public XGroup Clone(XGroup group)
        {
            try
            {
                var json = _serializer.ToJson(group);
                if (!string.IsNullOrEmpty(json))
                {
                    var clone = _serializer.FromJson<XGroup>(json);
                    if (clone != null)
                    {
                        var shapes = Enumerable.Repeat(clone, 1).ToList();
                        TryToRestoreStyles(shapes);
                        TryToRestoreRecords(shapes);
                        return clone;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }

            return null;
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public Container Clone(Container container)
        {
            try
            {
                var template = container.Template;
                var json = _serializer.ToJson(container);
                if (!string.IsNullOrEmpty(json))
                {
                    var clone = _serializer.FromJson<Container>(json);
                    if (clone != null)
                    {
                        var shapes = clone.Layers.SelectMany(l => l.Shapes);
                        TryToRestoreStyles(shapes);
                        TryToRestoreRecords(shapes);
                        clone.Template = template;
                        return clone;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }

            return null;
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public Document Clone(Document document)
        {
            try
            {
                var templates = document.Containers.Select(c => c.Template).ToArray();
                var json = _serializer.ToJson(document);
                if (!string.IsNullOrEmpty(json))
                {
                    var clone = _serializer.FromJson<Document>(json);
                    if (clone != null)
                    {
                        for (int i = 0; i < clone.Containers.Length; i++)
                        {
                            var container = clone.Containers[i];
                            var shapes = container.Layers.SelectMany(l => l.Shapes);
                            TryToRestoreStyles(shapes);
                            TryToRestoreRecords(shapes);
                            container.Template = templates[i];
                        }
                        return clone;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public bool Drop(string[] files)
        {
            try
            {
                if (files != null && files.Length >= 1)
                {
                    bool result = false;
                    foreach (var path in files)
                    {
                        if (string.IsNullOrEmpty(path))
                            continue;

                        string ext = System.IO.Path.GetExtension(path);

                        if (string.Compare(ext, ".project", true, CultureInfo.InvariantCulture) == 0)
                        {
                            Open(path);
                            result = true;
                        }
                        if (string.Compare(ext, ".code", true, CultureInfo.InvariantCulture) == 0)
                        {
                            var selectedShape = _editor.Renderers[0].State.SelectedShape;
                            var selectedShapes = _editor.Renderers[0].State.SelectedShapes;

                            if (selectedShape != null)
                            {
                                ImportShapeCode(path, selectedShape);
                            }

                            if (selectedShapes != null)
                            {
                                foreach (var shape in selectedShapes)
                                {
                                    ImportShapeCode(path, shape);
                                }
                            }

                            result = true;
                        }
                        else if (string.Compare(ext, ".cs", true, CultureInfo.InvariantCulture) == 0)
                        {
                            Eval(path);
                            result = true;
                        }
                        else if (string.Compare(ext, ".csv", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportData(path);
                            result = true;
                        }
                        else if (string.Compare(ext, ".style", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project.CurrentStyleLibrary, ImportType.Style);
                            result = true;
                        }
                        else if (string.Compare(ext, ".styles", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project.CurrentStyleLibrary, ImportType.Styles);
                            result = true;
                        }
                        else if (string.Compare(ext, ".StyleLibrary", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project, ImportType.StyleLibrary);
                            result = true;
                        }
                        else if (string.Compare(ext, ".StyleLibraries", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project, ImportType.StyleLibraries);
                            result = true;
                        }
                        else if (string.Compare(ext, ".group", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project.CurrentGroupLibrary, ImportType.Group);
                            result = true;
                        }
                        else if (string.Compare(ext, ".groups", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project.CurrentGroupLibrary, ImportType.Groups);
                            result = true;
                        }
                        else if (string.Compare(ext, ".grouplibrary", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project, ImportType.GroupLibrary);
                            result = true;
                        }
                        else if (string.Compare(ext, ".grouplibraries", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project, ImportType.GroupLibraries);
                            result = true;
                        }
                        else if (string.Compare(ext, ".template", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project, ImportType.Template);
                            result = true;
                        }
                        else if (string.Compare(ext, ".templates", true, CultureInfo.InvariantCulture) == 0)
                        {
                            ImportObject(path, _editor.Project, ImportType.Templates);
                            result = true;
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DropAsClone(XGroup group, double x, double y)
        {
            try
            {
                double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
                double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;

                var clone = Clone(group);
                if (clone != null)
                {
                    _editor.Deselect(_editor.Project.CurrentContainer);
                    clone.Move(sx, sy);

                    _editor.AddWithHistory(clone);

                    _editor.Select(_editor.Project.CurrentContainer, clone);

                    if (_editor.Project.Options.TryToConnect)
                    {
                        _editor.TryToConnect(clone);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="group"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DropAsReference(XGroup group, double x, double y)
        {
            try
            {
                double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
                double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;

                var origin = XPoint.Create(sx, sy, _editor.Project.Options.PointShape);
                var reference = XReference.Create(string.Concat("r-", group.Name), origin, group);
               
                _editor.Deselect(_editor.Project.CurrentContainer);
                _editor.AddWithHistory(reference);

                _editor.Select(_editor.Project.CurrentContainer, reference);

                if (_editor.Project.Options.TryToConnect)
                {
                    // TODO: Implement TryToConnect for XReference.
                    //_editor.TryToConnect(reference);
                } 
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="record"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Drop(Record record, double x, double y)
        {
            try
            {
                if (_editor.Renderers[0].State.SelectedShape != null)
                {
                    // TODO: Add history snapshot.
                    _editor.Renderers[0].State.SelectedShape.Record = record;
                }
                else if (_editor.Renderers[0].State.SelectedShapes != null && _editor.Renderers[0].State.SelectedShapes.Count > 0)
                {
                    // TODO: Add history snapshot.
                    foreach (var shape in _editor.Renderers[0].State.SelectedShapes)
                    {
                        shape.Record = record;
                    }
                }
                else
                {
                    var container = _editor.Project.CurrentContainer;
                    if (container != null)
                    {
                        var result = ShapeBounds.HitTest(container, new Vector2(x, y), _editor.Project.Options.HitTreshold);
                        if (result != null)
                        {
                            var previous = result.Record;
                            var next = record;
                            _editor.History.Snapshot(previous, next, (p) => result.Record = p);
                            result.Record = next;
                        }
                        else
                        {
                            if (_editor.CurrentTool == Tool.None
                                || _editor.CurrentTool == Tool.Selection
                                || _editor.CurrentTool == Tool.Group
                                || _editor.CurrentTool == Tool.Image
                                || _editor.CurrentTool == Tool.Path)
                            {
                                DropAsGroup(record, x, y);
                            }
                            else
                            {
                                DropAsShapeAndBind(record, x, y);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DropAsShapeAndBind(Record record, double x, double y)
        {
            switch (_editor.CurrentTool)
            {
                case Tool.Point:
                    {
                        var point = XPoint.Create(x, y, _editor.Project.Options.PointShape);
                        point.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            point.Bindings = point.Bindings.Add(ShapeBinding.Create("X", record.Columns[0].Name));
                            point.Bindings = point.Bindings.Add(ShapeBinding.Create("Y", record.Columns[1].Name));
                        }

                        _editor.AddWithHistory(point);
                    }
                    break;
                case Tool.Line:
                    {
                        var line = XLine.Create(
                            x, y, 
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape);
                        line.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            line.Bindings = line.Bindings.Add(ShapeBinding.Create("Start.X", record.Columns[0].Name));
                            line.Bindings = line.Bindings.Add(ShapeBinding.Create("Start.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            line.Bindings = line.Bindings.Add(ShapeBinding.Create("End.X", record.Columns[2].Name));
                            line.Bindings = line.Bindings.Add(ShapeBinding.Create("End.Y", record.Columns[3].Name));
                        }

                        _editor.AddWithHistory(line);
                    }
                    break;
                case Tool.Rectangle:
                    {
                        var rectangle = XRectangle.Create(
                            x, y,
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape);
                        rectangle.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            rectangle.Bindings = rectangle.Bindings.Add(ShapeBinding.Create("TopLeft.X", record.Columns[0].Name));
                            rectangle.Bindings = rectangle.Bindings.Add(ShapeBinding.Create("TopLeft.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            rectangle.Bindings = rectangle.Bindings.Add(ShapeBinding.Create("BottomRight.X", record.Columns[2].Name));
                            rectangle.Bindings = rectangle.Bindings.Add(ShapeBinding.Create("BottomRight.Y", record.Columns[3].Name));
                        }

                        _editor.AddWithHistory(rectangle);
                    }
                    break;
                case Tool.Ellipse:
                    {
                        var ellipse = XEllipse.Create(
                            x, y,
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape);
                        ellipse.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            ellipse.Bindings = ellipse.Bindings.Add(ShapeBinding.Create("TopLeft.X", record.Columns[0].Name));
                            ellipse.Bindings = ellipse.Bindings.Add(ShapeBinding.Create("TopLeft.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            ellipse.Bindings = ellipse.Bindings.Add(ShapeBinding.Create("BottomRight.X", record.Columns[2].Name));
                            ellipse.Bindings = ellipse.Bindings.Add(ShapeBinding.Create("BottomRight.Y", record.Columns[3].Name));
                        }

                        _editor.AddWithHistory(ellipse);
                    }
                    break;
                case Tool.Arc:
                    {
                        var arc = XArc.Create(
                            x, y,
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape);
                        arc.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point1.X", record.Columns[0].Name));
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point1.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point2.X", record.Columns[2].Name));
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point2.Y", record.Columns[3].Name));
                        }

                        if (record.Columns.Length >= 6)
                        {
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point3.X", record.Columns[4].Name));
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point3.Y", record.Columns[5].Name));
                        }

                        if (record.Columns.Length >= 8)
                        {
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point4.X", record.Columns[6].Name));
                            arc.Bindings = arc.Bindings.Add(ShapeBinding.Create("Point4.Y", record.Columns[7].Name));
                        }

                        _editor.AddWithHistory(arc);
                    }
                    break;
                case Tool.Bezier:
                    {
                        var bezier = XBezier.Create(
                            x, y,
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape);
                        bezier.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point1.X", record.Columns[0].Name));
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point1.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point2.X", record.Columns[2].Name));
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point2.Y", record.Columns[3].Name));
                        }

                        if (record.Columns.Length >= 6)
                        {
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point3.X", record.Columns[4].Name));
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point3.Y", record.Columns[5].Name));
                        }

                        if (record.Columns.Length >= 8)
                        {
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point4.X", record.Columns[6].Name));
                            bezier.Bindings = bezier.Bindings.Add(ShapeBinding.Create("Point4.Y", record.Columns[7].Name));
                        }

                        _editor.AddWithHistory(bezier);
                    }
                    break;
                case Tool.QBezier:
                    {
                        var qbezier = XQBezier.Create(
                            x, y,
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape);
                        qbezier.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            qbezier.Bindings = qbezier.Bindings.Add(ShapeBinding.Create("Point1.X", record.Columns[0].Name));
                            qbezier.Bindings = qbezier.Bindings.Add(ShapeBinding.Create("Point1.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            qbezier.Bindings = qbezier.Bindings.Add(ShapeBinding.Create("Point2.X", record.Columns[2].Name));
                            qbezier.Bindings = qbezier.Bindings.Add(ShapeBinding.Create("Point2.Y", record.Columns[3].Name));
                        }

                        if (record.Columns.Length >= 6)
                        {
                            qbezier.Bindings = qbezier.Bindings.Add(ShapeBinding.Create("Point3.X", record.Columns[4].Name));
                            qbezier.Bindings = qbezier.Bindings.Add(ShapeBinding.Create("Point3.Y", record.Columns[5].Name));
                        }

                        _editor.AddWithHistory(qbezier);
                    }
                    break;
                case Tool.Text:
                    {
                        var text = XText.Create(
                            x, y,
                            _editor.Project.CurrentStyleLibrary.CurrentStyle,
                            _editor.Project.Options.PointShape,
                            "Text");
                        text.Record = record;

                        if (record.Columns.Length >= 2)
                        {
                            text.Bindings = text.Bindings.Add(ShapeBinding.Create("TopLeft.X", record.Columns[0].Name));
                            text.Bindings = text.Bindings.Add(ShapeBinding.Create("TopLeft.Y", record.Columns[1].Name));
                        }

                        if (record.Columns.Length >= 4)
                        {
                            text.Bindings = text.Bindings.Add(ShapeBinding.Create("BottomRight.X", record.Columns[2].Name));
                            text.Bindings = text.Bindings.Add(ShapeBinding.Create("BottomRight.Y", record.Columns[3].Name));
                        }

                        _editor.AddWithHistory(text);
                    }
                    break;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="record"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DropAsGroup(Record record, double x, double y)
        {
            var g = XGroup.Create("g");
            g.Record = record;

            double sx = _editor.Project.Options.SnapToGrid ? Editor.Snap(x, _editor.Project.Options.SnapX) : x;
            double sy = _editor.Project.Options.SnapToGrid ? Editor.Snap(y, _editor.Project.Options.SnapY) : y;

            var length = record.Values.Length;
            double px = sx;
            double py = sy;
            double width = 150;
            double height = 15;

            for (int i = 0; i < length; i++)
            {
                var column = record.Columns[i];
                if (column.IsVisible)
                {
                    var text = XText.Create(
                        px, py,
                        px + width, py + height,
                        _editor.Project.CurrentStyleLibrary.CurrentStyle,
                        _editor.Project.Options.PointShape, "");
                    var binding = ShapeBinding.Create("Text", record.Columns[i].Name);
                    text.Bindings = text.Bindings.Add(binding);
                    g.AddShape(text);

                    py += height;
                }
            }

            var rectangle = XRectangle.Create(
                sx, sy,
                sx + width, sy + (double)length * height,
                _editor.Project.CurrentStyleLibrary.CurrentStyle,
                _editor.Project.Options.PointShape);
            g.AddShape(rectangle);

            double ptx = sx + width / 2;
            double pty = sy;

            double pbx = sx + width / 2;
            double pby = sy + (double)length * height;

            double plx = sx;
            double ply = sy + ((double)length * height) / 2;

            double prx = sx + width;
            double pry = sy + ((double)length * height) / 2;

            var pt = XPoint.Create(ptx, pty, _editor.Project.Options.PointShape);
            var pb = XPoint.Create(pbx, pby, _editor.Project.Options.PointShape);
            var pl = XPoint.Create(plx, ply, _editor.Project.Options.PointShape);
            var pr = XPoint.Create(prx, pry, _editor.Project.Options.PointShape);

            g.AddConnectorAsNone(pt);
            g.AddConnectorAsNone(pb);
            g.AddConnectorAsNone(pl);
            g.AddConnectorAsNone(pr);

            _editor.AddWithHistory(g);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="style"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Drop(ShapeStyle style, double x, double y)
        {
            try
            {
                if (_editor.Renderers[0].State.SelectedShape != null)
                {
                    // TODO: Add history snapshot.
                    _editor.Renderers[0].State.SelectedShape.Style = style;
                }
                else if (_editor.Renderers[0].State.SelectedShapes != null && _editor.Renderers[0].State.SelectedShapes.Count > 0)
                {
                    // TODO: Add history snapshot.
                    foreach (var shape in _editor.Renderers[0].State.SelectedShapes)
                    {
                        shape.Style = style;
                    }
                }
                else
                {
                    var container = _editor.Project.CurrentContainer;
                    if (container != null)
                    {
                        var result = ShapeBounds.HitTest(container, new Vector2(x, y), _editor.Project.Options.HitTreshold);
                        if (result != null)
                        {
                            // Add history snapshot.
                            result.Style = style;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return _editor.History.CanUndo();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return _editor.History.CanRedo();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsEditMode()
        {
            return !_simulationTimer.IsRunning;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsSimulationMode()
        {
            return _simulationTimer.IsRunning;
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializeEditor()
        {
            try
            {
                _editor = Editor.Create(_projectFactory.GetProject(), _renderers);
                _editor.Log = new TraceLog();
                _editor.Log.Initialize("Test.log");

                _commands = new EditorCommands();

                _commands.NewCommand =
                    new DelegateCommand<object>(
                        (item) => OnNew(item),
                        (item) => IsEditMode());
                
                _commands.CloseCommand =
                    new DelegateCommand(
                        () => OnClose(),
                        () => IsEditMode());

                _commands.ExitCommand =
                    new DelegateCommand(
                        () => OnExit(),
                        () => true);

                _commands.UndoCommand =
                    new DelegateCommand(
                        () => OnUndo(),
                        () => IsEditMode() /* && CanUndo() */);

                _commands.RedoCommand =
                    new DelegateCommand(
                        () => OnRedo(),
                        () => IsEditMode() /* && CanRedo() */);

                _commands.CutCommand =
                    new DelegateCommand<object>(
                        (item) => OnCut(item),
                        (item) => IsEditMode() /* && CanCopy() */);

                _commands.CopyCommand =
                    new DelegateCommand<object>(
                        (item) => OnCopy(item),
                        (item) => IsEditMode() /* && CanCopy() */);

                _commands.PasteCommand =
                    new DelegateCommand<object>(
                        (item) => OnPaste(item),
                        (item) => IsEditMode() /* && CanPaste() */);

                _commands.DeleteCommand =
                    new DelegateCommand<object>(
                        (item) => OnDelete(item),
                        (item) => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.SelectAllCommand =
                    new DelegateCommand(
                        () => OnSelectAll(),
                        () => IsEditMode());

                _commands.DeselectAllCommand =
                    new DelegateCommand(
                        () => OnDeselectAll(),
                        () => IsEditMode());

                _commands.ClearAllCommand =
                    new DelegateCommand(
                        () => OnClearAll(),
                        () => IsEditMode());

                _commands.GroupCommand =
                    new DelegateCommand(
                        () => _editor.GroupSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.UngroupCommand =
                    new DelegateCommand(
                        () => _editor.UngroupSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.ReferenceCommand =
                    new DelegateCommand(
                        () => _editor.ReferenceSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.BringToFrontCommand =
                    new DelegateCommand(
                        () => _editor.BringToFrontSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.SendToBackCommand =
                    new DelegateCommand(
                        () => _editor.SendToBackSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.BringForwardCommand =
                    new DelegateCommand(
                        () => _editor.BringForwardSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.SendBackwardCommand =
                    new DelegateCommand(
                        () => _editor.SendBackwardSelected(),
                        () => IsEditMode() /* && _editor.IsSelectionAvailable() */);

                _commands.ToolNoneCommand =
                    new DelegateCommand(
                        () => OnToolNone(),
                        () => IsEditMode());

                _commands.ToolSelectionCommand =
                    new DelegateCommand(
                        () => OnToolSelection(),
                        () => IsEditMode());

                _commands.ToolGroupCommand =
                    new DelegateCommand(
                        () => OnToolGroup(),
                        () => IsEditMode());

                _commands.ToolPointCommand =
                    new DelegateCommand(
                        () => OnToolPoint(),
                        () => IsEditMode());

                _commands.ToolLineCommand =
                    new DelegateCommand(
                        () => OnToolLine(),
                        () => IsEditMode());

                _commands.ToolArcCommand =
                    new DelegateCommand(
                        () => OnToolArc(),
                        () => IsEditMode());

                _commands.ToolBezierCommand =
                    new DelegateCommand(
                        () => OnToolBezier(),
                        () => IsEditMode());

                _commands.ToolQBezierCommand =
                    new DelegateCommand(
                        () => OnToolQBezier(),
                        () => IsEditMode());

                _commands.ToolPathCommand =
                    new DelegateCommand(
                        () => OnToolPath(),
                        () => IsEditMode());

                _commands.ToolRectangleCommand =
                    new DelegateCommand(
                        () => OnToolRectangle(),
                        () => IsEditMode());

                _commands.ToolEllipseCommand =
                    new DelegateCommand(
                        () => OnToolEllipse(),
                        () => IsEditMode());

                _commands.ToolTextCommand =
                    new DelegateCommand(
                        () => OnToolText(),
                        () => IsEditMode());

                _commands.ToolImageCommand =
                    new DelegateCommand(
                        () => OnToolImage(),
                        () => IsEditMode());

                _commands.EvalScriptCommand =
                    new DelegateCommand<string>(
                        (path) => Eval(path),
                        (path) => true);

                _commands.DefaultIsStrokedCommand =
                    new DelegateCommand(
                        () => OnToggleDefaultIsStroked(),
                        () => IsEditMode());

                _commands.DefaultIsFilledCommand =
                    new DelegateCommand(
                        () => OnToggleDefaultIsFilled(),
                        () => IsEditMode());

                _commands.DefaultIsClosedCommand =
                    new DelegateCommand(
                        () => OnToggleDefaultIsClosed(),
                        () => IsEditMode());

                _commands.DefaultIsSmoothJoinCommand =
                    new DelegateCommand(
                        () => OnToggleDefaultIsSmoothJoin(),
                        () => IsEditMode());

                _commands.SnapToGridCommand =
                    new DelegateCommand(
                        () => OnToggleSnapToGrid(),
                        () => IsEditMode());

                _commands.TryToConnectCommand =
                    new DelegateCommand(
                        () => OnToggleTryToConnect(),
                        () => IsEditMode());

                _commands.AddDatabaseCommand =
                    new DelegateCommand(
                        () => OnAddDatabase(),
                        () => IsEditMode());

                _commands.RemoveDatabaseCommand =
                    new DelegateCommand<object>(
                        (db) => OnRemoveDatabase(db),
                        (db) => IsEditMode());

                _commands.AddColumnCommand =
                    new DelegateCommand<object>(
                        (owner) => OnAddColumn(owner),
                        (owner) => IsEditMode());

                _commands.RemoveColumnCommand =
                    new DelegateCommand<object>(
                        (parameter) => OnRemoveColumn(parameter),
                        (parameter) => IsEditMode());

                _commands.AddRecordCommand =
                    new DelegateCommand(
                        () => OnAddRecord(),
                        () => IsEditMode());

                _commands.RemoveRecordCommand =
                    new DelegateCommand(
                        () => OnRemoveRecord(),
                        () => IsEditMode());

                _commands.ResetRecordCommand =
                    new DelegateCommand<object>(
                        (owner) => OnResetRecord(owner),
                        (owner) => IsEditMode());

                _commands.AddBindingCommand =
                    new DelegateCommand<object>(
                        (owner) => OnAddBinding(owner),
                        (owner) => IsEditMode());

                _commands.RemoveBindingCommand =
                    new DelegateCommand<object>(
                        (parameter) => OnRemoveBinding(parameter),
                        (parameter) => IsEditMode());

                _commands.AddPropertyCommand =
                    new DelegateCommand<object>(
                        (owner) => OnAddProperty(owner),
                        (owner) => IsEditMode());

                _commands.RemovePropertyCommand =
                    new DelegateCommand<object>(
                        (parameter) => OnRemoveProperty(parameter),
                        (parameter) => IsEditMode());

                _commands.AddGroupLibraryCommand =
                    new DelegateCommand(
                        () => OnAddGroupLibrary(),
                        () => IsEditMode());

                _commands.RemoveGroupLibraryCommand =
                    new DelegateCommand(
                        () => OnRemoveGroupLibrary(),
                        () => IsEditMode());

                _commands.AddGroupCommand =
                    new DelegateCommand(
                        () => OnAddGroup(),
                        () => IsEditMode());

                _commands.RemoveGroupCommand =
                    new DelegateCommand(
                        () => OnRemoveGroup(),
                        () => IsEditMode());

                _commands.AddLayerCommand =
                    new DelegateCommand(
                        () => OnAddLayer(),
                        () => IsEditMode());

                _commands.RemoveLayerCommand =
                    new DelegateCommand(
                        () => OnRemoveLayer(),
                        () => IsEditMode());

                _commands.AddStyleLibraryCommand =
                    new DelegateCommand(
                        () => OnAddStyleLibrary(),
                        () => IsEditMode());

                _commands.RemoveStyleLibraryCommand =
                    new DelegateCommand(
                        () => OnRemoveStyleLibrary(),
                        () => IsEditMode());

                _commands.AddStyleCommand =
                    new DelegateCommand(
                        () => OnAddStyle(),
                        () => IsEditMode());

                _commands.RemoveStyleCommand =
                    new DelegateCommand(
                        () => OnRemoveStyle(),
                        () => IsEditMode());

                _commands.RemoveShapeCommand =
                    new DelegateCommand(
                        () => OnRemoveShape(),
                        () => IsEditMode());

                _commands.StartSimulationCommand =
                    new DelegateCommand(
                        () => OnStartSimulation(),
                        () => IsEditMode());

                _commands.StopSimulationCommand =
                    new DelegateCommand(
                        () => OnStopSimulation(),
                        () => IsSimulationMode());

                _commands.RestartSimulationCommand =
                    new DelegateCommand(
                        () => OnRestartSimulation(),
                        () => IsSimulationMode());

                _commands.PauseSimulationCommand =
                    new DelegateCommand(
                        () => OnPauseSimulation(),
                        () => IsSimulationMode());

                _commands.TickSimulationCommand =
                    new DelegateCommand(
                        () => OnTickSimulation(),
                        () => IsSimulationMode() && IsSimulationPaused);

                _commands.AddTemplateCommand =
                    new DelegateCommand(
                        () => OnAddTemplate(),
                        () => IsEditMode());

                _commands.RemoveTemplateCommand =
                    new DelegateCommand(
                        () => OnRemoveTemplate(),
                        () => IsEditMode());

                _commands.EditTemplateCommand =
                    new DelegateCommand(
                        () => OnEditTemplate(),
                        () => IsEditMode());

                _commands.ApplyTemplateCommand =
                    new DelegateCommand<object>(
                        (item) => OnApplyTemplate(item),
                        (item) => true);

                _commands.SelectedItemChangedCommand =
                    new DelegateCommand<object>(
                        (item) => OnSelectedItemChanged(item),
                        (item) => IsEditMode() || IsSimulationMode());

                _commands.AddContainerCommand =
                    new DelegateCommand<object>(
                        (item) => OnAddContainer(item),
                        (item) => IsEditMode());

                _commands.InsertContainerBeforeCommand =
                    new DelegateCommand<object>(
                        (item) => OnInsertContainerBefore(item),
                        (item) => IsEditMode());

                _commands.InsertContainerAfterCommand =
                    new DelegateCommand<object>(
                        (item) => OnInsertContainerAfter(item),
                        (item) => IsEditMode());

                _commands.AddDocumentCommand =
                    new DelegateCommand<object>(
                        (item) => OnAddDocument(item),
                        (item) => IsEditMode());

                _commands.InsertDocumentBeforeCommand =
                    new DelegateCommand<object>(
                        (item) => OnInsertDocumentBefore(item),
                        (item) => IsEditMode());

                _commands.InsertDocumentAfterCommand =
                    new DelegateCommand<object>(
                        (item) => OnInsertDocumentAfter(item),
                        (item) => IsEditMode());

                WarmUpCSharpScript();
            }
            catch (Exception ex)
            {
                if (_editor != null && _editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
                else
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void WarmUpCSharpScript()
        {
            // NOTE: Warmup Roslyn script engine.
            try
            {
                Task.Run(
                    () =>
                    {
                        Eval("Action a = () => { };", this);
                    });
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void InitializeScripts()
        {
            try
            {
#if DEBUG
                _rootScriptsPath = "../../../Scripts";
#else
                _rootScriptsPath = "Scripts";
#endif
                ScriptDirectories = ImmutableArray.Create<ScriptDirectory>();

                Action update = () =>
                {
                    try
                    {
                        ScriptDirectories =
                            ScriptDirectory.CreateScriptDirectories(_rootScriptsPath);
                    }
                    catch (Exception ex)
                    {
                        if (_editor.Log != null)
                        {
                            _editor.Log.LogError("{0}{1}{2}",
                                ex.Message,
                                Environment.NewLine,
                                ex.StackTrace);
                        }
                    }
                };

                if (System.IO.Directory.Exists(_rootScriptsPath))
                {
                    update();

                    _watcher = new System.IO.FileSystemWatcher();
                    _watcher.Path = _rootScriptsPath;
                    _watcher.Filter = "*.*";
                    _watcher.NotifyFilter =
                        System.IO.NotifyFilters.LastAccess
                        | System.IO.NotifyFilters.LastWrite
                        | System.IO.NotifyFilters.FileName
                        | System.IO.NotifyFilters.DirectoryName;
                    _watcher.IncludeSubdirectories = true;
                    _watcher.Filter = "*.*";
                    _watcher.Changed += (s, e) => update();
                    _watcher.Created += (s, e) => update();
                    _watcher.Deleted += (s, e) => update();
                    _watcher.Renamed += (s, e) => update();
                    _watcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                if (_editor.Log != null)
                {
                    _editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdateCanExecuteState()
        {
            (_commands.NewCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.OpenCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.CloseCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.SaveCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.SaveAsCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ExportCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExitCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.ImportDataCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportDataCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.UpdateDataCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.ImportStyleCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportStylesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportStyleLibraryCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportStyleLibrariesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportGroupCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportGroupsCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportGroupLibraryCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportGroupLibrariesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportTemplateCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ImportTemplatesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportStyleCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportStylesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportStyleLibraryCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportStyleLibrariesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportGroupCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportGroupsCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportGroupLibraryCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportGroupLibrariesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportTemplateCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.ExportTemplatesCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
 
            (_commands.UndoCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RedoCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.CopyAsEmfCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.CutCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.CopyCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.PasteCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.DeleteCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.SelectAllCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DeselectAllCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ClearAllCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.GroupCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.UngroupCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ReferenceCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.BringToFrontCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.BringForwardCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.SendBackwardCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.SendToBackCommand as DelegateCommand).RaiseCanExecuteChanged();
                        
            (_commands.ToolNoneCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolSelectionCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolGroupCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolPointCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolLineCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolArcCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolBezierCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolQBezierCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolRectangleCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolEllipseCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolPathCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.ToolTextCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ToolImageCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.EvalCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.EvalScriptCommand as DelegateCommand<string>).RaiseCanExecuteChanged();

            (_commands.DefaultIsStrokedCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DefaultIsFilledCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DefaultIsClosedCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DefaultIsSmoothJoinCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.SnapToGridCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.TryToConnectCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.AddDatabaseCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveDatabaseCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.AddColumnCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.RemoveColumnCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.AddRecordCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveRecordCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.ResetRecordCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.AddBindingCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.RemoveBindingCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            
            (_commands.AddPropertyCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.RemovePropertyCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.AddGroupLibraryCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveGroupLibraryCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.AddGroupCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveGroupCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.AddLayerCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveLayerCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.AddStyleCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveStyleCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.AddStyleLibraryCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveStyleLibraryCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.RemoveShapeCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.StartSimulationCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.StopSimulationCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RestartSimulationCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.PauseSimulationCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.TickSimulationCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.ImportShapeCodeCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ExportShapeCodeCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.ZoomResetCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ZoomExtentCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.ProjectWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.OptionsWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.TemplatesWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.GroupsWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DatabasesWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DatabaseWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ScriptWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ScriptsWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            //(_commands.ContainerWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            //(_commands.DocumentWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.StylesWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.LayersWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ShapesWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.TemplateWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.PropertiesWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.StateWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.CodeWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.DataWindowCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.StyleWindowCommand as DelegateCommand).RaiseCanExecuteChanged();

            (_commands.AddTemplateCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.RemoveTemplateCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.EditTemplateCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ApplyTemplateCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            
            (_commands.SelectedItemChangedCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.AddContainerCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.InsertContainerBeforeCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.InsertContainerAfterCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.AddDocumentCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.InsertDocumentBeforeCommand as DelegateCommand<object>).RaiseCanExecuteChanged();
            (_commands.InsertDocumentAfterCommand as DelegateCommand<object>).RaiseCanExecuteChanged();

            (_commands.LoadWindowLayoutCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.SaveWindowLayoutCommand as DelegateCommand).RaiseCanExecuteChanged();
            (_commands.ResetWindowLayoutCommand as DelegateCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (_editor.Log != null)
            {
                _editor.Log.Close();
            }

            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            if (IsSimulationMode())
            {
                OnStopSimulation();
            }
        }
    }
}
