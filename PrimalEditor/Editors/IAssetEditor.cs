using PrimalEditor.Content;

namespace PrimalEditor.Editors;

interface IAssetEditor
{
    Asset Asset { get; }

    public void SetAsset(Asset asset);
}