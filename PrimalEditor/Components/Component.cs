using PrimalEditor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PrimalEditor.Components;

internal interface IMSComponent { }

[DataContract]
public abstract class Component : ViewModelBase
{
    [DataMember]
    public GameEntity Owner { get; private set; }

    protected Component(GameEntity owner)
    {
        Debug.Assert(owner != null);

        Owner = owner;
    }
}

internal abstract class MSComponent<T> : ViewModelBase, IMSComponent where T : MSComponent<T>
{

}