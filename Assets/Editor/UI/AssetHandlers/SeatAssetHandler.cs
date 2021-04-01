using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public abstract class SeatAssetHandler : IAssetTypeHandler
    {
        public virtual void DrawDetailsSection(Asset selectedAsset)
            => Draw(selectedAsset.GameObject);

        protected virtual void Draw(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            GUILayout.Label("Seats", "PreToolbar");

            var seats = gameObject
                .GetComponentsInChildren<Transform>(true)
                .Where(transform => transform.name.StartsWith("Seat", System.StringComparison.OrdinalIgnoreCase));

            if (seats.Count() == 0)
            {
                EditorGUILayout.HelpBox("There are no seats yet", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Seats found", seats.Count().ToString());
            }

            foreach (Transform seat in seats)
            {
                if (GUILayout.Button(seat.name))
                {
                    EditorGUIUtility.PingObject(seat.gameObject.GetInstanceID());
                    Selection.activeGameObject = seat.gameObject;
                }
            }

            if (GUILayout.Button("Add seat"))
            {
                var seat = new GameObject("Seat");

                seat.transform.SetParent(gameObject.transform);

                seat.transform.localPosition = Vector3.zero;

                Selection.activeGameObject = seat.gameObject;
            }
        }
    }
}
