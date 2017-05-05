using System.Collections.Generic;
using System.IO;
using CitySceneLoader;
using UnityEditor;
using UnityEngine;

public static class CityBundlesBuildPipeline {

    private const string BUNDLE_PATH = "Assets/StreamingAssets/AssetBundles";

    [MenuItem("City Bundles/Rebuild Bundles")]
    public static AssetBundleManifest BuildBundles() {
        if (!Directory.Exists(BUNDLE_PATH))
        {
            Directory.CreateDirectory(BUNDLE_PATH);
        }

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            BUNDLE_PATH, 
            BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, 
            EditorUserBuildSettings.activeBuildTarget);

        string[] allBundles = manifest.GetAllAssetBundles();
        List<string> bundlesToEncrypt = new List<string>();

        for (int i = 0; i < allBundles.Length; i++) {
            if (StaticCitySceneLoader.IsEncrypted(allBundles[i])) {
                bundlesToEncrypt.Add(allBundles[i]);
            }            
        }

        try {
            for (int i = 0; i < bundlesToEncrypt.Count; i++) {
                string bundleName = bundlesToEncrypt[i];
                EditorUtility.DisplayProgressBar("Encrypting...", bundlesToEncrypt[i], ((float) i / bundlesToEncrypt.Count));

                string path = Path.Combine(BUNDLE_PATH, bundleName);
                byte[] bundleData = File.ReadAllBytes(path);
                unsafe {
                    fixed (byte* bundleDataPtr = bundleData) {
                        CityVRUtilsWrapper.JRDYUTIUSH((ulong)bundleDataPtr, bundleData.Length);
                    }
                }

                File.WriteAllBytes(path, bundleData);
            }
        } finally {
            EditorUtility.ClearProgressBar();
        }

        return manifest;
    }
}
