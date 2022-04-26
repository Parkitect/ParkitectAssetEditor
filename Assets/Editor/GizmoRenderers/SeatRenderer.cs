namespace ParkitectAssetEditor.GizmoRenderers
{
    /// <summary>
    /// Renders guests on a bench
    /// </summary>
    /// <seealso cref="IGizmoRenderer" />
    class SeatRenderer : IGizmoRenderer
    {
        /// <inheritdoc />
        /// <summary>
        /// Determines whether this instance can render the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>
        ///   <c>true</c> if this instance can render the specified asset; otherwise, <c>false</c>.
        /// </returns>
        public bool CanRender(Asset asset)
        {
            return asset.Type == AssetType.Bench || asset.Type == AssetType.FlatRide;
        }

        /// <inheritdoc />
        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public void Render(Asset asset)
        {
            SittingType sittingType = SittingType.Bench;
            if (asset.Type != AssetType.Bench)
            {
                sittingType = asset.SittingType;
            }

            Utility.Utility.renderSeatGizmo(asset.GameObject, sittingType);
        }
    }
}
