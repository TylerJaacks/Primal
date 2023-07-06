using PrimalEditor.Components;
using PrimalEditor.EngineAPIStructs;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PrimalEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new(1, 1, 1);
    }

    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public TransformComponent Transform = new TransformComponent();
    }
}

namespace PrimalEditor
{

    static class EngineAPI
    {
        private const string _ddlName = "EngineDll.dll";

        [DllImport(_ddlName)]
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

        [DllImport(_ddlName)]
        private static extern int RemoveGameEntity(int id);

        public static void RemoveGameEntity(GameEntity gameEntity)
        {
            _ = RemoveGameEntity(gameEntity.EntityId);
        }
    }
}