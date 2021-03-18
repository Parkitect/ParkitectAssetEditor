namespace ParkitectAssetEditor.UI.AssetHandlers
{
    /// <summary>
    /// An asset handler that does basically nothing.
    /// </summary>
    public class DefaultAssetHandler : IAssetTypeHandler
    {
        public DefaultAssetHandler(AssetType handledType)
        {
        }

        public void DrawDetailsSection(Asset selectedAsset)
        {
        }
    }
}
