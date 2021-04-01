using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.UI.AssetHandlers
{
    public class FenceAssetHandler : WallAssetHandler
    {
        public override void DrawDetailsSection(Asset selectedAsset)
        {
            // Game object drop box
            GUILayout.Label("Category in the deco window:", EditorStyles.boldLabel);
            selectedAsset.Category = EditorGUILayout.TextField("Category:", selectedAsset.Category);
            selectedAsset.SubCategory = EditorGUILayout.TextField("Sub category:", selectedAsset.SubCategory);

            var post = EditorGUILayout.ObjectField("Post:", selectedAsset.FencePost, typeof(GameObject), true) as GameObject;

            if (post != selectedAsset.FencePost)
            {
                post.transform.SetParent(selectedAsset.GameObject.transform);
                post.transform.localPosition = new Vector3(0.5f, 0, 0);

                if (selectedAsset.FencePost != null)
                {
                    GameObject.DestroyImmediate(selectedAsset.FencePost);
                }

                selectedAsset.FencePost = post;
                post.name = "Post";
            }
            selectedAsset.HasMidPost = EditorGUILayout.Toggle("Has mid post: ", selectedAsset.HasMidPost);


            base.DrawDetailsSection(selectedAsset);
        }
    }
}
