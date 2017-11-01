using UnityEditor;

namespace ParkitectAssetEditor.Utility
{
	internal sealed class AssetImporter : AssetPostprocessor
	{
		private void OnPreprocessModel()
		{
			var importer = assetImporter as ModelImporter;
			
			importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
			importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
		}
	}
}
