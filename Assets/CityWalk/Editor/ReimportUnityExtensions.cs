using UnityEngine;
using UnityEditor;
using System.IO;
 
public class ReimportUnityEngineUI
{
    [MenuItem( "Assets/Reimport UI Assemblies", false, 100 )]
    public static void ReimportUI() {
        var path = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity";
        var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
        var version = string.Empty;

        for (int i = 0; i < files.Length; i++) {
            ReimportDll(files[i]);
        }

    }
    static void ReimportDll(string path )
    {
        if (File.Exists(path)) {
            Debug.Log("Reimport " + path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
        } else {
            Debug.LogError(string.Format("DLL not found {0}", path));
        }
    }
}
