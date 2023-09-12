using System;
using System.Diagnostics;

namespace PrimalEditor.Components;

internal enum ComponentType
{
    Transform,
    Script,
}

internal static class ComponentFactory
{
    private static readonly Func<GameEntity, object, Component>[] Function = new Func<GameEntity, object, Component>[]
    {
        (entity, data) => new Transform(entity),
        (entity, data) => new Script(entity) { Name = (string) data }
    };

    public static Func<GameEntity, object, Component> GetCreationFunction(ComponentType componentType)
    {
        Debug.Assert((int) componentType < Function.Length);

        return Function[(int)componentType];
    }

    public static ComponentType ToEnumType(this Component component)
    {
        return component switch
        {
            Transform _ => ComponentType.Transform,
            Script _ => ComponentType.Script,
            _ => throw new ArgumentException("Unknown component type.")
        };
    }
}