using System;
using System.Diagnostics;

namespace PrimalEditor.Content;

public enum AssetType
{
    Unknown,
    Animation,
    Audio,
    Material,
    Mesh,
    Skeleton,
    Texture,
}

public abstract class Asset : ViewModelBase
{
    public AssetType Type { get; set; }

    public Asset(AssetType type)
    {
        Debug.Assert(type != AssetType.Unknown);

        Type = type;
    }
}
