using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class TmpAutoImporter
{
    static TmpAutoImporter()
    {
        EditorApplication.delayCall += ImportTmpEssentialsIfNeeded;
    }

    static void ImportTmpEssentialsIfNeeded()
    {
        if (Directory.Exists(Application.dataPath + "/TextMesh Pro")) return;

        string packagePath = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        string fullPath = Path.GetFullPath(packagePath);

        if (!File.Exists(fullPath))
        {
            packagePath = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
            fullPath = Path.GetFullPath(packagePath);
        }

        if (File.Exists(fullPath))
        {
            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log("[CatCatGo] TMP Essential Resources imported.");
        }
    }

    [MenuItem("CatCatGo/Import TMP Essential Resources")]
    static void ManualImportTmp()
    {
        ImportTmpEssentialsIfNeeded();
    }
}
