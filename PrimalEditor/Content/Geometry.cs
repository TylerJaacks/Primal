﻿using PrimalEditor.Common;
using PrimalEditor.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace PrimalEditor.Content;

enum PrimitiveMeshType
{
    Plane,
    Cube,
    UvSphere,
    IcoSphere,
    Cylinder,
    Capsule
};

class Mesh : ViewModelBase
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

class MeshLOD : ViewModelBase
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

class LODGroup : ViewModelBase
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

class Geometry : Asset
{
    private readonly List<LODGroup> _lodGroup = new();

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

    public Geometry() : base(AssetType.Mesh) { }
}
