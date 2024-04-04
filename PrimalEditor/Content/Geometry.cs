using PrimalEditor.Common;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PrimalEditor.Content;

public enum PrimitiveMeshType
{
    Plane,
    Cube,
    UvSphere,
    IcoSphere,
    Cylinder,
    Capsule
};

public class Mesh : ViewModelBase
{
    private int _vertexSize;

    public int VertexSize
    {
        get => _vertexSize;
        set
        {
            if (_vertexSize != value)
            {
                _vertexSize = value;

                OnPropertyChanged(nameof(VertexSize));
            }
        }
    }

    private int _vertexCount;

    public int VertexCount
    {
        get => _vertexCount;
        set
        {
            if (_vertexCount != value)
            {
                _vertexCount = value;

                OnPropertyChanged(nameof(VertexCount));
            }
        }
    }

    private int _indexSize;

    public int IndexSize
    {
        get => _indexSize;
        set
        {
            if (_indexSize != value)
            {
                _indexSize = value;

                OnPropertyChanged(nameof(IndexSize));
            }
        }
    }

    private int _indexCount;

    public int IndexCount
    {
        get => _indexCount;
        set
        {
            if (_indexCount != value)
            {
                _indexCount = value;

                OnPropertyChanged(nameof(IndexCount));
            }
        }
    }

    public byte[] Vertices { get; set; }
    public byte[] Indices { get; set; }
}

public class MeshLOD : ViewModelBase
{
    private string _name;

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

    private float _lodThreshold;

    public float LodThreshold
    {
        get => _lodThreshold;
        set
        { 
            if (value != _lodThreshold)
            {
                _lodThreshold = value;

                OnPropertyChanged(nameof(LodThreshold));
            }
        }
    }

    public ObservableCollection<Mesh> Meshes { get; } = new ObservableCollection<Mesh>();
}

public class LODGroup : ViewModelBase
{
    private string _name;

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

    public ObservableCollection<MeshLOD> LODs { get; } = new ObservableCollection<MeshLOD>();
}

public class GeometryImportSettings : ViewModelBase
{
    private float _smoothingAngle;

    public float SmoothingAngle
    {
        get => _smoothingAngle;
        set
        {
            if (_smoothingAngle != value)
            {
                _smoothingAngle = value;

                OnPropertyChanged(nameof(SmoothingAngle));
            }
        }
    }

    private bool _calculateNormals;

    public bool CalculateNormals
    {
        get => _calculateNormals;
        set
        {
            if (_calculateNormals != value)
            {
                _calculateNormals = value;

                OnPropertyChanged(nameof(CalculateNormals));
            }
        }
    }

    private bool _calculateTangents;

    public bool CalculateTangents
    {
        get => _calculateTangents;
        set
        {
            if (_calculateTangents != value)
            {
                _calculateTangents = value;

                OnPropertyChanged(nameof(CalculateTangents));
            }
        }
    }

    private bool _reverseHandednesss;

    public bool ReverseHandedness
    {
        get => _reverseHandednesss;
        set
        {
            if (_reverseHandednesss != value)
            {
                _reverseHandednesss = value;

                OnPropertyChanged(nameof(ReverseHandedness));
            }
        }
    }

    private bool _importEmbeddedTextures;

    public bool ImportEmbeddedTextures
    {
        get => _importEmbeddedTextures;
        set
        {
            if (_importEmbeddedTextures != value)
            {
                _importEmbeddedTextures = value;

                OnPropertyChanged(nameof(ImportEmbeddedTextures));
            }
        }
    }

    private bool _importAnimation;

    public bool ImportAnimation
    {
        get => _importAnimation;
        set
        {
            if (_importAnimation != value)
            {
                _importAnimation = value;

                OnPropertyChanged(nameof(ImportAnimation));
            }
        }
    }

    public GeometryImportSettings()
    {
        CalculateNormals = false;
        CalculateTangents = false;
        SmoothingAngle = 178f;
        ReverseHandedness = false;
        ImportEmbeddedTextures = true;
        ImportAnimation = true;
    }
}

public class Geometry : Asset
{
    private readonly List<LODGroup> _lodGroup = new();

    public GeometryImportSettings ImportSettings { get; } = new GeometryImportSettings();

    public LODGroup GetLODGroup(int lodGroup = 0)
    {
        Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroup.Count);

        return _lodGroup.Any() ? _lodGroup[lodGroup] : null;
    }

    internal void FromRawData(byte[] data)
    {
        Debug.Assert(data?.Length > 0);

        _lodGroup.Clear();

        using var reader = new BinaryReader(new MemoryStream(data));

        var s = reader.ReadInt32();
        reader.BaseStream.Position += s;

        var numLodGroups = reader.ReadInt32();
        Debug.Assert(numLodGroups > 0);

        for (int i = 0; i < numLodGroups; ++i)
        {
            s = reader.ReadInt32();

            string loadGroupName;

            if (s > 0)
            {
                var nameBytes = reader.ReadBytes(s);

                loadGroupName = Encoding.UTF8.GetString(nameBytes);
            }
            else
            {
                loadGroupName = $"lod_{ContentHelper.GetRandomString()}";
            }

            var numMeshes = reader.ReadInt32();

            Debug.Assert(numMeshes > 0);

            List<MeshLOD> lods = ReadMeshLODs(numMeshes, reader);

            var lodGroups = new LODGroup() {  Name = loadGroupName };

            lods.ForEach(l => lodGroups.LODs.Add(l));

            _lodGroup.Add(lodGroups);
        }
    }

    private static List<MeshLOD> ReadMeshLODs(int numMeshes, BinaryReader reader)
    {
        var lodIds = new List<int>();
        var lodList = new List<MeshLOD>();

        for (int i = 0; i < numMeshes; i++)
        {
            ReadMeshes(reader, lodIds, lodList);
        }

        return lodList;
    }

    private static void ReadMeshes(BinaryReader reader, List<int> lodIds, List<MeshLOD> lodList)
    {
        var s = reader.ReadInt32();

        string meshName;

        if (s > 0)
        {
            var nameBytes = reader.ReadBytes(s);

            meshName = Encoding.UTF8.GetString(nameBytes);
        }
        else
        {
            // TODO: Fix this
            //meshName = $"mesh_{ContentHelper.GetRandomString()}";
            meshName = "Test";
        }

        var mesh = new Mesh();

        var lodId = reader.ReadInt32();

        mesh.VertexSize = reader.ReadInt32();
        mesh.VertexCount = reader.ReadInt32();
        mesh.IndexSize = reader.ReadInt32();
        mesh.IndexCount = reader.ReadInt32();

        var lodThreshold = reader.ReadSingle();

        var vertexBufferSize = mesh.VertexSize * mesh.VertexCount;
        var indexBufferSize = mesh.IndexSize * mesh.IndexCount;

        mesh.Vertices = reader.ReadBytes(vertexBufferSize);
        mesh.Indices = reader.ReadBytes(indexBufferSize);

        MeshLOD lod;

        if (ID.IsValid(lodId) && lodIds.Contains(lodId))
        {
            lod = lodList[lodIds.IndexOf(lodId)];

            Debug.Assert(lod != null);
        }
        else
        {
            lodIds.Add(lodId);

            lod = new MeshLOD() { Name = meshName, LodThreshold = lodThreshold };

            lodList.Add(lod);
        }

        lod.Meshes.Add(mesh);
    }

    public override IEnumerable<string> Save(string file)
    {
        Debug.Assert(_lodGroup.Any());

        var savedFiles = new List<string>();

        if (!_lodGroup.Any()) return savedFiles;

        var path = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
        var fileName = Path.GetFileNameWithoutExtension(file);

        try
        {
            foreach (var lodGroup in _lodGroup)
            {
                Debug.Assert(lodGroup.LODs.Any());

                var meshFileName = ContentHelper.SanitizeFileName(path + fileName + "_" + lodGroup.LODs[0].Name + AssetFileExtension);

                Guid = Guid.NewGuid();

                byte[] data = null;

                using (var writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write(lodGroup.Name);
                    writer.Write(lodGroup.LODs.Count);

                    var hashes = new List<byte>();

                    foreach (var lod in lodGroup.LODs)
                    {
                        LODtoBinary(lod, writer, out var hash);

                        hashes.AddRange(hash);
                    }

                    Hash = ContentHelper.ComputeHash(hashes.ToArray());
                    data = (writer.BaseStream as MemoryStream).ToArray();
                    Icon = GenerateIcon(lodGroup.LODs[0]);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);

            Logger.Log(MessageType.Error, $"Failed to save geometry to {file}");
        }

        return savedFiles;
    }

    private void LODtoBinary(MeshLOD lod, BinaryWriter writer, out byte[] hash)
    {
        writer.Write(lod.Name);
        writer.Write(lod.LodThreshold);
        writer.Write(lod.Meshes.Count);

        var meshDataBegin = writer.BaseStream.Position;

        foreach(var mesh in lod.Meshes)
        {
            writer.Write(mesh.VertexSize);
            writer.Write(mesh.VertexCount);
            writer.Write(mesh.IndexSize);
            writer.Write(mesh.IndexCount);
            writer.Write(mesh.Vertices);
            writer.Write(mesh.Indices);
        }

        var meshDataSize = writer.BaseStream.Position - meshDataBegin;

        Debug.Assert(meshDataSize > 0);

        var buffer = (writer.BaseStream as MemoryStream).ToArray();

        hash = ContentHelper.ComputeHash(buffer, (int) meshDataBegin,  (int) meshDataSize);
    }

    public Geometry() : base(AssetType.Mesh) { }
}
