using PrimalEditor.Common;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimalEditor.Components;

[DataContract]
[KnownType(typeof(Transform))]
public class GameEntity : ViewModelBase
{
    private string _name;

    [DataMember]
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }


    [DataMember]
    public Scene ParentScene { get; private set; }


    [DataMember(Name = nameof(Components))]
    private ObservableCollection<Component> _components = new();
    public ReadOnlyObservableCollection<Component> Components { get; private set; }


    private bool _isEnabled;

    [DataMember]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;

            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }


    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_components != null)
        {
            Components = new ReadOnlyObservableCollection<Component>(_components);

            OnPropertyChanged(nameof(Components));
        }
    }

    public GameEntity(Scene scene)
    {
        Debug.Assert(scene != null);

        ParentScene = scene;

        _components.Add(new Transform(this));

        OnDeserialized(new StreamingContext());
    }

    public GameEntity()
    {

    }
}

abstract class MSEntity : ViewModelBase
{
    private bool _enableUpdates = true;

    private bool? _isEnabled;

    public bool? IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;

            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;

            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private readonly ObservableCollection<IMSComponent> _components = new();

    public ReadOnlyObservableCollection<IMSComponent> Components { get; }

    public List<GameEntity> SelectedEntities { get; }


    private static float? GetMixedValue(IReadOnlyCollection<GameEntity> entities, Func<GameEntity, float> getProperty)
    {
        var value = getProperty(entities.First());

        if (entities.Skip(1).Any(entity => !value.IsTheSameAs(getProperty(entity))))
        {
            return null;
        }

        return value;
    }

    private static bool? GetMixedValue(IReadOnlyCollection<GameEntity> entities, Func<GameEntity, bool> getProperty)
    {
        var value = getProperty(entities.First());

        if (entities.Skip(1).Any(entity => value != getProperty(entity)))
        {
            return null;
        }

        return value;
    }

    private static string GetMixedValue(IReadOnlyCollection<GameEntity> entities, Func<GameEntity, string> getProperty)
    {
        var value = getProperty(entities.First());

        return entities.Skip(1).Any(entity => value != getProperty(entity)) ? null : value;
    }


    protected virtual bool UpdateGameEntities(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(IsEnabled):
                SelectedEntities.ForEach(x => x.IsEnabled = IsEnabled.Value);

                return true;
            case nameof(Name):
                SelectedEntities.ForEach(x => x.Name = Name);

                return true;
        }

        return false;
    }

    protected virtual bool UpdateMSGameEntity()
    {
        IsEnabled = GetMixedValue(SelectedEntities, new Func<GameEntity, bool>(x => x.IsEnabled));
        Name = GetMixedValue(SelectedEntities, new Func<GameEntity, string>(x => x.Name));

        return true;
    }

    public void Refresh()
    {
        _enableUpdates = false;

        UpdateMSGameEntity();

        _enableUpdates = true;
    }

    protected MSEntity(List<GameEntity> entities)
    {
        Debug.Assert(entities?.Any() == true);

        Components = new ReadOnlyObservableCollection<IMSComponent>(_components);

        SelectedEntities = entities;

        PropertyChanged += (s, e) =>
        {
            if (_enableUpdates) UpdateGameEntities(e.PropertyName);
        };
    }
}

class MSGameEntity : MSEntity
{
    public MSGameEntity(List<GameEntity> entities) : base(entities)
    {
        Refresh();
    }

    public MSGameEntity() : base(new List<GameEntity>())
    {

    }
}