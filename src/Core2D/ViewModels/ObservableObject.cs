﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Core2D
{
    [DataContract(IsReference = true)]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        private bool _isDirty;
        private ObservableObject _owner = null;
        private string _name = "";

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public virtual ObservableObject Owner
        {
            get => _owner;
            set => RaiseAndSetIfChanged(ref _owner, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public virtual string Name
        {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public virtual bool IsDirty()
        {
            return _isDirty;
        }

        public virtual void Invalidate()
        {
            _isDirty = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract object Copy(IDictionary<object, object> shared);

        public void RaisePropertyChanged([CallerMemberName] string propertyName = default)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = default)
        {
            if (!Equals(field, value))
            {
                field = value;
                _isDirty = true;
                RaisePropertyChanged(propertyName);
                return true;
            }
            return false;
        }
    }
}
