using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParkitectAssetEditor.Assets.Editor.GizmoRenderers
{
    interface IGizmoRenderer
    {
        /// <summary>
        /// Determines whether this instance can render the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>
        ///   <c>true</c> if this instance can render the specified asset; otherwise, <c>false</c>.
        /// </returns>
        bool CanRender(Asset asset);

        /// <summary>
        /// Renders the specified asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        void Render(Asset asset);
    }
}
