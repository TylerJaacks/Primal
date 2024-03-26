using System;
using System.Diagnostics;

namespace PrimalEditor.Content;

enum AssetType
{
    Unknown,
    Animation,
    Audio,
    Material,
    Mesh,
    Skeleton,
    Texture,
}

abstract class Asset : ViewModelBase
{
    public AssetType Type { get; set; }

    public Asset(AssetType type)
    {
        Debug.Assert(type != AssetType.Unknown);

        Type = type;
    }
}
