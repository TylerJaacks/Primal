using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace PrimalEditor.Components;

[DataContract]
public class Script : Component
{
    private string _name;

    [DataMember]
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;

            _name = value;

            OnPropertyChanged(nameof(Name));
        }
    }


    public Script(GameEntity owner) : base(owner)
    {

    }

    public override IMsComponent GetMultliselectionComponent(MsEntity msEntity) => new MSScript(msEntity);
    public override void WriteToBinary(BinaryWriter bw)
    {
        var nameBytes = Encoding.UTF8.GetBytes(Name);

        bw.Write(nameBytes.Length);
        bw.Write(nameBytes);
    }
}

internal sealed class MSScript : MsComponent<Script>
{
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;

            _name = value;

            OnPropertyChanged(nameof(Name));
        }
    }


    public MSScript(MsEntity msEntity) : base(msEntity)
    {
        Refresh();
    }

    protected override bool UpdateComponents(string propertyName)
    {
        if (propertyName != nameof(Name)) return false;

        SelectedComponents.ForEach(c => c.Name = _name);

        return true;

    }

    protected override bool UpdateMsComponent()
    {
        Name = MsEntity.GetMixedValue(SelectedComponents, new Func<Script, string>(x => x.Name));

        return true;
    }
}