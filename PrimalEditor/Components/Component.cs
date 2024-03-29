﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimalEditor.Components
{
    public interface IMsComponent { }

    [DataContract]
    public abstract class Component : ViewModelBase
    {
        public abstract IMsComponent GetMultliselectionComponent(MsEntity msEntity);
        public abstract void WriteToBinary(BinaryWriter bw);

        [DataMember]
        public GameEntity Owner { get; private set; }

        protected Component(GameEntity owner)
        {
            Debug.Assert(owner != null);

            Owner = owner;
        }
    }

    internal abstract class MsComponent<T> : ViewModelBase, IMsComponent where T : Component
    {
        public List<T> SelectedComponents { get; }

        private bool _enableUpdates = true;

        protected abstract bool UpdateComponents(string propertyName);
        protected abstract bool UpdateMsComponent();

        public void Refresh()
        {
            _enableUpdates = false;

            UpdateMsComponent();

            _enableUpdates = true;
        }

        protected MsComponent(MsEntity msEntity)
        {
            Debug.Assert(msEntity?.SelectedEntities?.Any() == true);

            SelectedComponents = msEntity.SelectedEntities.Select(entity => entity.GetComponent<T>()).ToList();

            PropertyChanged += (s, e) =>
            {
                if (_enableUpdates) UpdateComponents(e.PropertyName);
            };
        }
    }
}
