namespace ParkitectAssetEditor.GizmoRenderers
{
    interface IHandleRenderer
    {
        /// <summary>
        /// Renders unity handles
        /// </summary>
        /// <param name="asset">The asset.</param>
        void Handle(Asset asset);   
    }
}