// ReSharper disable AsyncVoidLambda
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using PrimalEditor.Common;
using PrimalEditor.Components;
using PrimalEditor.GameDev;
using PrimalEditor.Utilities;

namespace PrimalEditor.GameProject
{
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }

        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = new();

        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public static string Extension { get; } = ".primal";

        public string FullPath => $@"{Path}{Name}{Extension}";

        public string Solution => $@"{Path}{Name}.sln";

        private static readonly string[] BuildConfigurationNames =
            { "Debug", "DebugEditor", "Release", "ReleaseEditor" };

        private int _buildConfig;

        public int BuildConfig
        {
            get => _buildConfig;
            set
            {
                if (_buildConfig == value) return;

                _buildConfig = value;

                OnPropertyChanged(nameof(BuildConfig));
            }
        }

        public BuildConfigType StandaloneBuildConfig =>
            BuildConfig == 0 ? BuildConfigType.Debug : BuildConfigType.Release;

        public BuildConfigType DllBuildConfig =>
            BuildConfig == 0 ? BuildConfigType.DebugEditor : BuildConfigType.ReleaseEditor;

        private string[] _availableScripts;

        public string[] AvailableScripts
        {
            get => _availableScripts;
            set
            {
                if (_availableScripts == value) return;

                _availableScripts = value;

                OnPropertyChanged(nameof(AvailableScripts));
            }
        }

        public enum BuildConfigType
        {
            Debug,
            DebugEditor,
            Release,
            ReleaseEditor,
        }

        private Scene _activeScene;

        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene == value) return;

                _activeScene = value;

                OnPropertyChanged(nameof(ActiveScene));
            }
        }

        public static Project Current => 
            Application.Current.MainWindow?.DataContext as Project;

        private static string GetConfigName(BuildConfigType config) => BuildConfigurationNames[(int)config];

        public static UndoRedo UndoRedo { get; } = new UndoRedo();

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand AddSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }
        public ICommand DebugStartCommand { get; private set; }
        public ICommand DebugStartWithoutDebuggingCommand { get; private set; }
        public ICommand DebugStopCommand { get; private set; }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
        }

        [OnDeserialized]
        private async void OnDeserialized(StreamingContext context)
        {
            if (_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);

                OnPropertyChanged(nameof(Scenes));
            }

            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

            Debug.Assert(ActiveScene != null);

            await BuildGameCodeDll(false);

            SetCommands();
        }

        private void SetCommands()
        {
            AddSceneCommand = new RelayCommand<object>(x =>
            {
                AddScene($"New Scene {_scenes.Count}");

                var newScene = _scenes.Last();
                var sceneIndex = _scenes.Count - 1;

                UndoRedo.Add(new UndoRedoAction(
                    () => RemoveScene(newScene),
                    () => _scenes.Insert(sceneIndex, newScene),
                    $"Add {newScene.Name}"));
            });

            RemoveSceneCommand = new RelayCommand<Scene>(x =>
            {
                var sceneIndex = _scenes.IndexOf(x);

                RemoveScene(x);

                UndoRedo.Add(new UndoRedoAction(
                    () => _scenes.Insert(sceneIndex, x),
                    () => RemoveScene(x),
                    $"Remove {x.Name}"));
            }, x => !x.IsActive);

            UndoCommand = new RelayCommand<object>(x => UndoRedo.Undo(), x => UndoRedo.UndoList.Any());
            RedoCommand = new RelayCommand<object>(x => UndoRedo.Redo(), x => UndoRedo.RedoList.Any());
            SaveCommand = new RelayCommand<object>(x => Save(this));

            BuildCommand = new RelayCommand<bool>(async x => await BuildGameCodeDll(x), x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished);

            DebugStartCommand = new RelayCommand<object>(async x => await RunGame(true), x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished);
            DebugStartWithoutDebuggingCommand = new RelayCommand<object>(async x => await RunGame(false), x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished);
            DebugStopCommand = new RelayCommand<object>(async x => await StopGame(), x => VisualStudio.IsDebugging());

            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(BuildCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
        }

        private void UnloadGameCodeDll()
        {
            ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = false);

            if (EngineAPI.UnloadGameCodeDll() == 0) return;

            AvailableScripts = null;

            Logger.Log(MessageType.Info, "Game code dll unloaded successfully.");
        }

        private void LoadGameCodeDll()
        {
            var configName = GetConfigName(DllBuildConfig);

            var dll = $@"{Path}x64\{configName}\{Name}.dll";

            AvailableScripts = null;

            if (File.Exists(dll) && EngineAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = EngineAPI.GetScriptNames();

                ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = true);


                Logger.Log(MessageType.Info, "Game code dll loaded successfully.");
            }
            else
                Logger.Log(MessageType.Warning, "Failed to load game code dll. Try to build the project first.");
        }

        private async Task BuildGameCodeDll(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDll();

                await Task.Run(() => VisualStudio.BuildSolution(this, GetConfigName(DllBuildConfig), showWindow));

                if (VisualStudio.BuildSucceeded)
                {
                    LoadGameCodeDll();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                throw;
            }
        }

        private void AddScene(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));

            _scenes.Add(new Scene(this, sceneName));
        }

        private void RemoveScene(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
        }

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));

            return Serializer.FromFile<Project>(file);
        }

        public void Unload()
        {
            UnloadGameCodeDll();

            VisualStudio.CloseVisualStudio();

            UndoRedo.Reset();
        }

        public static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(MessageType.Info, $"Project saved to {project.FullPath}");
        }

        private void SaveToBinary()
        {
            var configName = GetConfigName(StandaloneBuildConfig);
            var bin = $@"{Path}x64\{configName}\game.bin";

            using var bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write));

            bw.Write(ActiveScene.GameEntities.Count);

            foreach (var entity in ActiveScene.GameEntities)
            {
                bw.Write(0);
                bw.Write(entity.Components.Count);

                foreach (var component in entity.Components)
                {
                    bw.Write((int) component.ToEnumType());

                    component.WriteToBinary(bw);
                }
            }
        }

        private async Task RunGame(bool debug)
        {
            var configName = GetConfigName(StandaloneBuildConfig);

            await Task.Run(() => VisualStudio.BuildSolution(this, configName, debug));

            if (VisualStudio.BuildSucceeded) 
            {
                SaveToBinary();

                await Task.Run(() => VisualStudio.Run(this, configName, debug));
            }
        }

        private async Task StopGame() => await Task.Run(VisualStudio.Stop);
    }
}