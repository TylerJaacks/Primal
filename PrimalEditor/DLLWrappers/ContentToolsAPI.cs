﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

using PrimalEditor.Content;
using PrimalEditor.ContentToolsAPIStructs;
using PrimalEditor.Utilities;

namespace PrimalEditor.ContentToolsAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public class GeometryImportSettings
    {
        public float SmoothingAngle = 178f;
        public byte CalculateNormals = 0;
        public byte CalculateTangents = 1;
        public byte ReverseHandedness = 0;
        public byte ImportEmbeddedTextures = 1;
        public byte ImportAnimations = 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    class SceneData : IDisposable
    {
        public IntPtr Data;
        public int DataSize;
        public GeometryImportSettings ImportSettings = new();

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(Data);

            GC.SuppressFinalize(this);
        }

        ~SceneData()
        {
            Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    class PrimitiveInitInfo
    {
        public PrimitiveMeshType Type;
        public int SegmentX = 1;
        public int SegmentY = 1;
        public int SegmentZ = 1;
        public Vector3 Size = new Vector3(1f);
        public int LOD = 0;
    }
}

namespace PrimalEditor.DLLWrappers
{
    static class ContentToolsAPI
    {
        private const string _toolsDLL = "ContentTools.dll";

        [DllImport(_toolsDLL)]
        private static extern void CreatePrimitiveMesh([In, Out] SceneData data, PrimitiveInitInfo info);

        public static void CreatePrimitiveMesh(Geometry geometry, PrimitiveInitInfo info)
        {
            Debug.Assert(geometry != null);

            using var sceneData = new SceneData();

            try
            {
                CreatePrimitiveMesh(sceneData, info);

                Debug.Assert(sceneData.Data != IntPtr.Zero && sceneData.DataSize > 0);

                var data = new byte[sceneData.DataSize];

                Marshal.Copy(sceneData.Data, data, 0, sceneData.DataSize);

                geometry.FromRawData(data);
            }
            catch (Exception e)
            {
                Logger.Log(MessageType.Error, $"Failed to create {info.Type} primitive mesh.");

                Debug.WriteLine(e.Message);
            }
        }
    }
}