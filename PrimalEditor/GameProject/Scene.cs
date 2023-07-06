using PrimalEditor.Common;
using PrimalEditor.Components;
using PrimalEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace PrimalEditor.GameProject;

[DataContract()]
public class Scene : ViewModelBase
{
    private string _name;

    [DataMember()]
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

    [DataMember()]
    public Project Project { get; private set; }

    [DataMember(Name = nameof(GameEntities))]
    private readonly ObservableCollection<GameEntity> _gameEntities = new();

    public ReadOnlyObservableCollection<GameEntity> GameEntities { get; private set; }

    private bool _isActive;

    [DataMember]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;

                OnPropertyChanged(nameof(IsActive));
            }
        }
    }


    private void AddGameEntity(GameEntity gameEntity)
    {
        Debug.Assert(!_gameEntities.Contains(gameEntity));

        _gameEntities.Add(gameEntity);
    }

    private void RemoveGameEntity(GameEntity gameEntity)
    {
        Debug.Assert(_gameEntities.Contains(gameEntity));

        _gameEntities.Remove(gameEntity);
    }


    public ICommand AddGameEntityCommand { get; set; }
    public ICommand RemoveGameEntityCommand { get; set; }


    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_gameEntities != null)
        {
            GameEntities = new ReadOnlyObservableCollection<GameEntity>(_gameEntities);

            OnPropertyChanged(nameof(GameEntities));
        }

        AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
        {
            AddGameEntity(x);

            var gameEntityIndex = _gameEntities.IndexOf(x);

            Project.UndoRedo.Add(new UndoRedoAction(
                $"Add {x.Name} to {Name}",
                () => RemoveGameEntity(x),
                () => _gameEntities.Insert(gameEntityIndex, x)));

        });

        RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
        {
            var gameEntityIndex = _gameEntities.IndexOf(x);

            RemoveGameEntity(x);

            Project.UndoRedo.Add(new UndoRedoAction(
                $"Remove {x.Name} to {Name}",
                () => _gameEntities.Insert(gameEntityIndex, x),
                () => RemoveGameEntity(x)));

        });
    }

    public Scene(Project project, string name)
    {
        Debug.Assert(project != null);

        Project = project;
        Name = name;

        OnDeserialized(new StreamingContext());
    }
}