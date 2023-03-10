using PrimalEditor.Common;
using PrimalEditor.Utilities;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace PrimalEditor.GameProject;

[DataContract(Name = "Game")]
public class Project : ViewModelBase
{
    public static string Extension { get; } = ".primal";

    [DataMember] public string Name { get; private set; } = "New Project";

    [DataMember]
    public string Path { get; private set; }

    private string FullPath => $@"{Path}{Name}\{Name}{Extension}";

    [DataMember(Name = "Scenes")]
    private ObservableCollection<Scene> _scenes = new();

    public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

    private Scene _activeScene;

    public Scene ActiveScene

    {
        get => _activeScene;
        set
        {
            if (_activeScene != value)
            {
                _activeScene = value;
                OnPropertyChanged(nameof(ActiveScene));
            }

        }
    }

    public static Project Current => Application.Current.MainWindow?.DataContext as Project;


    public static UndoRedo UndoRedo { get; } = new UndoRedo();


    public static Project Load(string file)
    {
        Debug.Assert(File.Exists(file));

        Logger.Log(MessageType.Info, $"Loading {file}");

        return Serializer.FromFile<Project>(file);
    }

    public void Unload()
    {
        UndoRedo.Reset();
    }

    private static void Save(Project project)
    {
        Serializer.ToFile<Project>(project, project.FullPath);

        Logger.Log(MessageType.Info, $"Saved {project.Name} to {project.FullPath}");
    }


    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_scenes != null)
        {
            Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);

            OnPropertyChanged(nameof(Scenes));
        }

        ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

        AddSceneCommand = new RelayCommand<object>(x =>
        {
            AddSceneInternal($"Scene {Scenes.Count}");

            var newScene = _scenes.Last();
            var newSceneIndex = _scenes.IndexOf(newScene);

            UndoRedo.Add(new UndoRedoAction(
                $"Add {newScene.Name}",
                () => { RemoveSceneInternal(newScene); },
                () => { _scenes.Insert(newSceneIndex, newScene); }
            ));
        });

        RemoveSceneCommand = new RelayCommand<Scene>(x =>
        {
            var sceneIndex = _scenes.IndexOf(x);

            RemoveSceneInternal(x);

            UndoRedo.Add(new UndoRedoAction(
                $"Remove {x.Name}",
                () => { _scenes.Insert(sceneIndex, x); },
                () => { RemoveSceneInternal(x); }
            ));
        }, x => !x.IsActive);


        UndoCommand = new RelayCommand<object>(x => { UndoRedo.Undo(); });
        RedoCommand = new RelayCommand<object>(x => { UndoRedo.Redo(); });

        SaveCommand = new RelayCommand<object>(x => { Save(this); });
    }


    private void AddSceneInternal(string sceneName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(sceneName.Trim()));

        _scenes.Add(new Scene(this, sceneName));
    }

    private void RemoveSceneInternal(Scene scene)
    {
        Debug.Assert(_scenes.Contains(scene));

        _scenes.Remove(scene);
    }


    public ICommand AddSceneCommand { get; private set; }

    public ICommand RemoveSceneCommand { get; private set; }

    public ICommand UndoCommand { get; private set; }

    public ICommand RedoCommand { get; private set; }

    public ICommand SaveCommand { get; private set; }


    public Project(string name, string path)
    {
        Name = name;
        Path = path;


        OnDeserialized(new StreamingContext());
    }

    public Project()
    {

    }
}