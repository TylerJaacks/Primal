using PrimalEditor.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using Project = PrimalEditor.GameProject.Project;

namespace PrimalEditor.GameDev;

public static class VisualStudio
{
    private static EnvDTE80.DTE2 _vsInstance = null;
    private const string ProgramId = "VisualStudio.DTE.17.0";

    public static bool BuildSucceeded { get; private set; } = true;
    public static bool BuildFinished { get; private set; } = true;

    [DllImport("ole32.dll")]
    private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pObjectTable);

    [DllImport("ole32.dll")]
    private static extern int CreateBindCtx(uint reserved, out IBindCtx pBindCtx);

    public static void OpenVisualStudio(string solutionPath)
    {
        IRunningObjectTable runningObjectTable = null;
        IEnumMoniker monikerTable = null;
        IBindCtx bindCtx = null;

        try
        {
            if (_vsInstance != null) return;

            var hResult = GetRunningObjectTable(0, out runningObjectTable);

            if (hResult < 0 || runningObjectTable == null) throw new COMException($"GetRunningObjectTable() returned HRESULT: {hResult:X8}");

            runningObjectTable.EnumRunning(out monikerTable);
            monikerTable.Reset();

            hResult = CreateBindCtx(0, out bindCtx);

            if (hResult < 0 || bindCtx == null) throw new COMException($"CreateBindCtx() returned HRESULT: {hResult:X8}");

            var currentMoniker = new IMoniker[1];

            while (monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
            {
                var name = string.Empty;

                currentMoniker[0]?.GetDisplayName(bindCtx, null, out name);

                if (!name.Contains(ProgramId)) continue;

                hResult = runningObjectTable.GetObject(currentMoniker[0], out var obj);
                        
                if (hResult < 0 || obj == null) throw new COMException($"Running object table's GetObject() returned HRESULT: {hResult:X8}");

                var dte = obj as EnvDTE80.DTE2;

                if (dte != null)
                {
                    var solutionName = dte.Solution.FullName;

                    if (solutionName != solutionPath) continue;
                }

                _vsInstance = dte;

                break;
            }

            if (_vsInstance != null) return;

            var visualStudioType = Type.GetTypeFromProgID(ProgramId, true);

            if (visualStudioType != null)
                _vsInstance = Activator.CreateInstance(visualStudioType) as EnvDTE80.DTE2;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            Logger.Log(MessageType.Error, "Failed to start Visual Studio.");
        }
        finally
        {
            if (monikerTable != null) Marshal.ReleaseComObject(monikerTable);
            if (runningObjectTable != null) Marshal.ReleaseComObject(runningObjectTable);
            if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
        }
    }

    public static void CloseVisualStudio()
    {
        if (_vsInstance?.Solution.IsOpen == true)
        {
            _vsInstance.ExecuteCommand("File.SaveAll");
            _vsInstance.Solution.Close();
        }

        try
        {
            _vsInstance?.Quit();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            Logger.Log(MessageType.Error, "Failed to close Visual Studio.");
        }
    }

    public static bool AddFilesToSolution(string solution, string projectName, string[] files)
    {
        Debug.Assert(files?.Length > 0);
        OpenVisualStudio(solution);
        try
        {
            if (_vsInstance != null)
            {
                if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                else _vsInstance.ExecuteCommand("File.SaveAll");

                foreach (EnvDTE.Project project in _vsInstance.Solution.Projects)
                {
                    if (!project.UniqueName.Contains(projectName)) continue;

                    foreach (var file in files)
                    {
                        try
                        {
                            project.ProjectItems.AddFromFile(file);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine("failed to add files to Visual Studio project");

                            return false;
                        }
                    }
                }

                var cpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".cpp");
                
                if (!string.IsNullOrEmpty(cpp))
                {
                    _vsInstance.ItemOperations.OpenFile(cpp, EnvDTE.Constants.vsViewKindTextView).Visible = true;
                }
                
                _vsInstance.MainWindow.Activate();
                _vsInstance.MainWindow.Visible = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine("failed to add files to Visual Studio project");

            return false;
        }
        return true;
    }

    public static bool IsDebugging()
    {
        var result = false;

        for (var i = 0; i < 3; i++)
        {
            try
            {
                result = _vsInstance != null &&
                         (_vsInstance.Debugger.CurrentProgram != null ||
                          _vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                if (!result) System.Threading.Thread.Sleep(1000);
            }
        }

        return result;
    }

    internal static void BuildSolution(Project project, string configName, bool showWindow = true)
    {
        if (IsDebugging())
        {
            Logger.Log(MessageType.Error, "Visual Studio is currently debugging.");

            return;
        }

        OpenVisualStudio(project.Solution);

        BuildSucceeded = false;
        BuildFinished = false;

        for (var i = 0; i < 3; i++)
        {
            try
            {
                if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(project.Solution);

                _vsInstance.MainWindow.Visible = showWindow;

                _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildProjConfigBegin;
                _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;

                try
                {
                    foreach (var pdbFile in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64\{configName}"), ".pdb"))
                    {
                        File.Delete(pdbFile);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(configName).Activate();
                _vsInstance.ExecuteCommand("Build.BuildSolution");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine($"Attempt {i}: failed to build project {project.Name}.");

                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    private static void OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
    {
        if (BuildFinished) return;

        if (success) Logger.Log(MessageType.Info, $"Building {projectConfig} configuration succeed.");
        else Logger.Log(MessageType.Error, $"Building {projectConfig} configuration failed.");

        BuildFinished = true;
        BuildSucceeded = success;
    }

    private static void OnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
    {
        Logger.Log(MessageType.Info, $"Building {project}, {projectConfig}, {platform}, {solutionConfig}");
    }
}