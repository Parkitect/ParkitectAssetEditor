namespace ParkitectAssetEditor.UI.AssetHandlers
{
    /// <summary>
    /// Defines an interface that deals with a certain (new) asset type
    /// </summary>
    public interface IAssetTypeHandler
    {
        /// <summary>
        /// Called by <see cref="AssetEditorWindow.DrawAssetDetailSection"/>
        /// and used to draw the detail asset settings.
        /// </summary>
        /// <param name="selectedAsset"></param>
        void DrawDetailsSection(Asset selectedAsset);
    }
}
