using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetBundles;

[Obsolete("Not supported yet. Use StaticCitySceneLoader")]
public class DynamicCityLoader : MonoBehaviour {

    public string CityBundleName;

    /* 
     Bundle Type
        Scene = 0,
        BuildingModel = 1,
        BuildingTexture = 2,
        BuildingMaterial = 3,
        GroundModel = 4, 
        GroundTexture = 5,
        GroundaMaterial = 6,
        Manifest 
    */

    private AssetBundleManifest manifest;

	void Start () {
	    StartCoroutine(loadCityAsync());
	}
	
	void Update () {
	
	}

    public void LoadCity() {
        if (string.IsNullOrEmpty(CityBundleName)) {
            Debug.LogError("CityBundleName is missing.");
            return; 
        }

        StartCoroutine(loadCityAsync());
    }

    private IEnumerator loadCityAsync() {
        yield return StartCoroutine(loadManifest());

        // Load ground
        yield return StartCoroutine(loadAssetBundleWithDependencies(string.Format("{0}_bundle_4", CityBundleName),
            (loadedBundle) => {
                var groundPrefab = loadedBundle.LoadAsset<GameObject>(string.Format("{0}_ground", CityBundleName));

                var ground = (GameObject)Instantiate(groundPrefab, transform);

                ground.transform.localPosition = Vector3.zero;
                ground.transform.localRotation = Quaternion.identity;
                ground.transform.localScale = Vector3.one;
            }
        ));

        // Load buildings
        yield return StartCoroutine(loadAssetBundleWithDependencies(string.Format("{0}_bundle_1", CityBundleName),
            (loadedBundle) => {
                var buildingsPrefab = loadedBundle.LoadAsset<GameObject>(string.Format("{0}_buildings", CityBundleName));

                var buildings = (GameObject)Instantiate(buildingsPrefab, transform);

                buildings.transform.localPosition = Vector3.zero;
                buildings.transform.localRotation = Quaternion.identity;
                buildings.transform.localScale = Vector3.one;

                StaticBatchingUtility.Combine(buildings);
            }
        ));
    }

    private IEnumerator loadAssetBundleWithDependencies(string bundleName, Action<AssetBundle> onBundleLoaded) {
        List<string> bundlesToLoad = new List<string>(manifest.GetAllDependencies(bundleName));
        bundlesToLoad.Add(bundleName);
        
        AssetBundleCreateRequest[] bundleCreateRequests = new AssetBundleCreateRequest[bundlesToLoad.Count];
        for (int i = 0; i < bundlesToLoad.Count; i++) {
            bundleCreateRequests[i] = AssetBundle.LoadFromFileAsync(getBundlePath(bundlesToLoad[i]));
        }

        for (int i = 0; i < bundleCreateRequests.Length; i++) {
            yield return bundleCreateRequests[i];

            if (isEncrypted(bundlesToLoad[i])) {
                // TODO
            }

            Debug.Log("Bundle loaded: " + bundlesToLoad[i]);
        }

        AssetBundleRequest[] assetLoadRequests = new AssetBundleRequest[bundleCreateRequests.Length];
        for (int i = 0; i < bundleCreateRequests.Length; i++) {
            assetLoadRequests[i] = bundleCreateRequests[i].assetBundle.LoadAllAssetsAsync();
        }

        for (int i = 0; i < assetLoadRequests.Length; i++) {
            yield return assetLoadRequests[i];
            Debug.Log("All assets loaded for bundle: " + bundlesToLoad[i]);
        }

        onBundleLoaded(bundleCreateRequests.Last().assetBundle);
    }

    private IEnumerator loadManifest() {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(getBundlePath("Windows"));
        yield return request;

        AssetBundle bundle = request.assetBundle;
        manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    private string getBundlePath(string bundleName) {
#if UNITY_EDITOR
        return string.Format("AssetBundles/Windows/{0}", bundleName);
#else
        return string.Format("{0}/{1}", Application.streamingAssetsPath, bundleName);
#endif
    }

    private bool isEncrypted(string bundleName) {
        return false;
    }
}
