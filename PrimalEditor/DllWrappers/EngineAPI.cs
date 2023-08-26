// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
using System;
using PrimalEditor.Components;
using PrimalEditor.EngineAPIStructs;

using System.Numerics;
using System.Runtime.InteropServices;


namespace PrimalEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    internal class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new(1, 1, 1);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class ScriptComponent
    {
        public IntPtr ScriptCreator;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class GameEntityDescriptor
    {
        public TransformComponent Transform = new();
        public ScriptComponent Script = new ScriptComponent();
    }
}

namespace PrimalEditor
{

    public static class EngineAPI
    {
        private const string _engineDll = "EngineDll.dll";

        [DllImport(_engineDll, CharSet=CharSet.Ansi)]
        public static extern int LoadGameCodeDll(string dllPath);

        [DllImport(_engineDll, CharSet = CharSet.Ansi)]
        public static extern int UnloadGameCodeDll();

        [DllImport(_engineDll)]
        public static extern IntPtr GetScriptCreator(string name);

        [DllImport(_engineDll)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();

        internal static class EntityAPI
        {
            [DllImport(_engineDll)]
            private static extern int CreateGameEntity(GameEntityDescriptor descriptor);

            public static int CreateGameEntity(GameEntity gameEntity)
            {
                GameEntityDescriptor descriptor = new();

                {
                    var c = gameEntity.GetComponent<Transform>();

                    descriptor.Transform.Position = c.Position;
                    descriptor.Transform.Rotation = c.Rotation;
                    descriptor.Transform.Scale = c.Scale;
                }

                {
                    // var c = gameEntity.GetComponent<Script>();
                }

                return CreateGameEntity(descriptor);
            }

            [DllImport(_engineDll)]
            private static extern int RemoveGameEntity(int id);

            public static void RemoveGameEntity(GameEntity gameEntity)
            {
                _ = RemoveGameEntity(gameEntity.EntityId);
            }
        }
    }
}