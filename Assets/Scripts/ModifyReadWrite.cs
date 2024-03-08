#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ModifyReadWrite : MonoBehaviour
{
    [MenuItem("Assets/Modify Read/Write To True")]
    static void ModifySelectedAssetsToTrue()
    {
        foreach (Object selectedObject in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }
    }

    [MenuItem("Assets/Modify Read/Write To False")]
    static void ModifySelectedAssetsToFalse()
    {
        foreach (Object selectedObject in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (importer != null && importer.isReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
