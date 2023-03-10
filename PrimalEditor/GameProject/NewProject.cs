using PrimalEditor.Common;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimalEditor.GameProject;

[DataContract]
public class ProjectTemplate
{
    [DataMember]
    public string ProjectType { get; set; }

    [DataMember]
    public string ProjectFile { get; set; }

    [DataMember]
    public List<string> Folders { get; set; }

    public byte[] Icon { get; set; }

    public byte[] Screenshot { get; set; }

    public string IconFilePath { get; set; }

    public string ScreenshotFilePath { get; set; }

    public string ProjectFilePath { get; set; }
}

public class NewProject : ViewModelBase
{
    // TODO: Change this from hard coded to dynamic.
    private readonly string _templatePath = @"..\..\PrimalEditor\ProjectTemplates";

    private static string _projectName = "NewProject";

    private static string _projectPath =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\Projects\PrimalProjects\";

    public string ProjectName
    {
        get => _projectName;
        set
        {
            if (value == null || _projectName == value) return;

            _projectName = value;

            ValidateProjectPath();

            OnPropertyChanged(nameof(ProjectName));
        }
    }

    public string ProjectPath
    {
        get => _projectPath;
        set
        {
            if (value == null || _projectPath == value) return;

            _projectPath = value;

            ValidateProjectPath();

            OnPropertyChanged(nameof(ProjectPath));
        }
    }

    private readonly ObservableCollection<ProjectTemplate> _projectTemplates = new();

    public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates { get; }

    private bool _isValid = false;

    public bool IsValid
    {
        get => _isValid;
        set
        {
            if (_isValid == value) return;
            _isValid = value;

            OnPropertyChanged(nameof(IsValid));
        }
    }


    private bool ValidateProjectPath()
    {
        var path = ProjectPath;

        if (!Path.EndsInDirectorySeparator(path))
        {
            path += @"\";
        }

        path += $@"{ProjectName}\";

        IsValid = false;

        if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
        {
            ErrorMessage = "Type in a project name.";
        }
        else if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
        {
            ErrorMessage = "Invalid character(s) used in project name.";
        }
        else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
        {
            ErrorMessage = "Select a valid project folder.";
        }
        else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            ErrorMessage = "Invalid character(s) used in the project path.";
        }
        else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
        {
            ErrorMessage = "Selected project folder already exists and is not empty";
        }
        else
        {
            ErrorMessage = string.Empty;
            IsValid = true;
        }

        return IsValid;
    }

    public string CreateProject(ProjectTemplate projectTemplate)
    {
        ValidateProjectPath();

        if (!IsValid)
        {
            return string.Empty;
        }

        if (!Path.EndsInDirectorySeparator(ProjectPath))
        {
            ProjectPath += @"\";
        }

        var path = $@"{ProjectPath}{ProjectName}\";

        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var folder in projectTemplate.Folders)
            {
                Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path) ?? throw new InvalidOperationException(), folder)));
            }

            var dirInfo = new DirectoryInfo(path + @".Primal\");

            dirInfo.Attributes |= FileAttributes.Hidden;

            File.Copy(projectTemplate.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Icon.png")));
            File.Copy(projectTemplate.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Screenshot.png")));

            var projectXml = File.ReadAllText(projectTemplate.ProjectFilePath);

            projectXml = string.Format(projectXml, ProjectName, ProjectPath);

            var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));

            File.WriteAllText(projectPath, projectXml);

            return path;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);

            Logger.Log(MessageType.Error, $"Failed to create {ProjectName}.");

            throw;
        }
    }

    private string _errorMessage;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }


    public NewProject()
    {
        ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);

        try
        {
            var listOfTemplates = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);

            Debug.Assert(listOfTemplates.Any());

            foreach (var file in listOfTemplates)
            {
                var template = Serializer.FromFile<ProjectTemplate>(file);

                template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, "Icon.png"));
                template.Icon = File.ReadAllBytes(template.IconFilePath);

                template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, "Screenshot.png"));
                template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);


                template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, template.ProjectFile));

                _projectTemplates.Add(template);
            }

            ValidateProjectPath();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);

            Logger.Log(MessageType.Error, $"Failed to read project templates.");

            throw;
        }
    }
}