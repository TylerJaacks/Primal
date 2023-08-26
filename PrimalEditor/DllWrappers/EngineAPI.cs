// ReSharper disable InconsistentNaming

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
    internal class GameEntityDescriptor
    {
        public TransformComponent Transform = new();
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

        public static class EntityAPI
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