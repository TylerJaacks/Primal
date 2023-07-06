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
public class ProjectData
{
    [DataMember]
    public string ProjectName { get; set; }

    [DataMember]
    public string ProjectPath { get; set; }

    [DataMember]
    public DateTime Date { get; set; }


    public string FullPath => $"{ProjectPath}{ProjectName}{Project.Extension}";

    public byte[] Icon { get; set; }

    public byte[] Screenshot { get; set; }
}

[DataContract]
public class ProjectDataList
{
    [DataMember]
    public List<ProjectData> Projects { get; set; }
}

public class OpenProject
{
    private static readonly string ApplicationDataPath =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\PrimalEditor\";

    private static readonly string ProjectDataPath;

    private static readonly ObservableCollection<ProjectData> _projects = new();

    public static ReadOnlyObservableCollection<ProjectData> Projects { get; }

    static OpenProject()
    {
        try
        {
            if (!Directory.Exists(ApplicationDataPath)) Directory.CreateDirectory(ApplicationDataPath);

            ProjectDataPath = $@"{ApplicationDataPath}ProjectData.xml";

            Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);

            ReadProjectData();

            Debug.WriteLine("");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);

            Logger.Log(MessageType.Error, $"Failed to read project data.");

            throw;
        }
    }

    private static void ReadProjectData()
    {
        if (File.Exists(ProjectDataPath))
        {
            var projects = Serializer.FromFile<ProjectDataList>(ProjectDataPath).Projects
                .OrderByDescending(x => x.Date);

            _projects.Clear();

            foreach (var project in projects)
            {
                if (File.Exists(project.FullPath))
                {
                    project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Icon.png");
                    project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Screenshot.png");

                    _projects.Add(project);
                }
            }
        }
    }

    private static void WriteProjectData()
    {
        var projects = _projects.OrderBy(x => x.Date).ToList();

        Serializer.ToFile(new ProjectDataList() { Projects = projects }, ProjectDataPath);
    }

    public static Project Open(ProjectData data)
    {
        ReadProjectData();

        var project = _projects.FirstOrDefault(x => x.FullPath == data.FullPath);

        if (project != null)
        {
            project.Date = DateTime.Now;
        }
        else
        {
            project = data;

            project.Date = DateTime.Now;

            _projects.Add(project);
        }

        WriteProjectData();

        return Project.Load(project.FullPath);
    }
}