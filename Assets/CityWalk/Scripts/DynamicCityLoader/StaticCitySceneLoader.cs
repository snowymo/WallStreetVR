using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
namespace CitySceneLoader {
    using System;
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEngine.SceneManagement;

    internal class StaticCitySceneLoader : MonoBehaviour {
        internal static StaticCitySceneLoader Instance;

        // TODO: Don't use static like this!!!
        private static readonly List<AssetBundle> assetBundles = new List<AssetBundle>();

        private enum BundleType {
            Scene = 0,
            BuildingModel = 1,
            BuildingTexture = 2,
            BuildingMaterial = 3,
            GroundModel = 4,
            GroundTexture = 5,
            GroundaMaterial = 6,
            Manifest
        }

        private enum LoadingStep {
            Idle = 0,
            LoadingRequested,
            LoadPlainBundles,
            LoadEncryptedBundles,
            LoadAssets,
            LoadScene,
            Finished
        }

        private LoadingStep currentStep;
        private readonly List<AsyncOperation> currentAsyncOperations = new List<AsyncOperation>();

        internal void Awake() {
            Instance = this;
        }

        internal void Update() {
            if (!SteamManager.Initialized) {
                return;
            }

            if (!DLCManager.Instance.IsManifestLoaded)
            {
                return;
            }

            if (!IsLoading)
            {
                DLCCity cityToLoad = SceneSwitcher.Instance.CityToLoad;
                if (cityToLoad == null ||
                    DLCManager.Instance.GetDLCState(cityToLoad) != DLCManager.DLCState.Ready)
                {
                    cityToLoad = DLCCity.Default;
                }

                LoadCity(cityToLoad);
            }
        }

        internal void LoadCity(DLCCity city) {
            currentStep = LoadingStep.LoadingRequested;

            StartCoroutine(
                loadAssetBundleWithDependencies(
                    city,
                    () =>
                    {
                        currentStep = LoadingStep.LoadScene;

                        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(city.SceneName);
                        loadSceneOperation.allowSceneActivation = true;

                        currentAsyncOperations.Clear();
                        currentAsyncOperations.Add(loadSceneOperation);
                    }
            ));
        }

        internal float Progress {
            get {
                if ((int) currentStep < (int) LoadingStep.LoadPlainBundles) {
                    return 0;
                }

                float stepStartProgress = (float) ((int) currentStep - (int) LoadingStep.LoadPlainBundles) /
                                          ((int) LoadingStep.Finished - (int) LoadingStep.LoadPlainBundles);
                float stepTotalProgress = 1.0f / ((int) LoadingStep.Finished - (int) LoadingStep.LoadPlainBundles);

                if (currentAsyncOperations.Count == 0) {
                    return stepStartProgress;
                }

                float asyncAverageProgress = 0;
                for (int i = 0; i < currentAsyncOperations.Count; i++) {
                    asyncAverageProgress += currentAsyncOperations[i].progress;
                }
                asyncAverageProgress /= currentAsyncOperations.Count;

                return stepStartProgress + asyncAverageProgress * stepTotalProgress;
            }
        }

        internal bool IsLoading {
            get { return currentStep != LoadingStep.Idle; }
        }

        private IEnumerator loadAssetBundleWithDependencies(DLCCity city, Action onBundleLoaded) {
            for (int i = 0; i < assetBundles.Count; i++)
            {
                assetBundles[i].Unload(true);
            }
            assetBundles.Clear();

            List<string> plainBundlesToLoad = new List<string>();
            List<string> encryptedBundlesToLoad = new List<string>();

            string[] dependencies = DLCManager.Instance.GetCityDependencies(city);
            for (int i = 0; i < dependencies.Length; i++) {
                string bundleName = dependencies[i];
                if (IsEncrypted(bundleName)) {
                    encryptedBundlesToLoad.Add(bundleName);
                } else {
                    plainBundlesToLoad.Add(bundleName);
                }
            }

            if (IsEncrypted(city.RootBundleFileName)) {
                encryptedBundlesToLoad.Add(city.RootBundleFileName);
            } else {
                plainBundlesToLoad.Add(city.RootBundleFileName);
            }

            // Load plain bundles
            currentStep = LoadingStep.LoadPlainBundles;

            List<AssetBundleCreateRequest> bundleCreateRequests =
                new List<AssetBundleCreateRequest>(plainBundlesToLoad.Count);
            currentAsyncOperations.Clear();
            currentAsyncOperations.Capacity = plainBundlesToLoad.Count;

            for (int i = 0; i < plainBundlesToLoad.Count; i++) {
                var request = AssetBundle.LoadFromFileAsync(DLCManager.GetBundlePath(plainBundlesToLoad[i]));
                bundleCreateRequests.Add(request);
                currentAsyncOperations.Add(request);
            }

            for (int i = 0; i < bundleCreateRequests.Count; i++) {
                yield return bundleCreateRequests[i];
                assetBundles.Add(bundleCreateRequests[i].assetBundle);
                Debug.Log("Bundle loaded: " + bundleCreateRequests[i].assetBundle.name);
            }

            // Load encrypted bundles
            currentStep = LoadingStep.LoadEncryptedBundles;
            List<AssetBundleCreateRequest> encryptedBundleCreateRequests =
                new List<AssetBundleCreateRequest>(encryptedBundlesToLoad.Count);
            currentAsyncOperations.Clear();
            currentAsyncOperations.Capacity = encryptedBundlesToLoad.Count;
            for (int i = 0; i < encryptedBundlesToLoad.Count; i++) {
                byte[] rawData = File.ReadAllBytes(DLCManager.GetBundlePath(encryptedBundlesToLoad[i]));
                decrypt(rawData);
                var request = AssetBundle.LoadFromMemoryAsync(rawData);
                encryptedBundleCreateRequests.Add(request);
                currentAsyncOperations.Add(request);
            }

            for (int i = 0; i < encryptedBundleCreateRequests.Count; i++) {
                yield return encryptedBundleCreateRequests[i];
                assetBundles.Add(encryptedBundleCreateRequests[i].assetBundle);
                Debug.Log("Bundle loaded: " + encryptedBundleCreateRequests[i].assetBundle.name);
            }

            // Load assets
            currentStep = LoadingStep.LoadAssets;

            List<AssetBundleRequest> assetLoadRequests = new List<AssetBundleRequest>(assetBundles.Count);
            currentAsyncOperations.Clear();
            for (int i = 0; i < assetBundles.Count; i++) {
                if (!assetBundles[i].isStreamedSceneAssetBundle) {
                    var request = assetBundles[i].LoadAllAssetsAsync();
                    assetLoadRequests.Add(request);
                    currentAsyncOperations.Add(request);
                }
            }

            for (int i = 0; i < assetLoadRequests.Count; i++) {
                yield return assetLoadRequests[i];
                Debug.Log("All assets loaded for bundle: " + assetBundles[i].name);
            }

            onBundleLoaded();
        }

        private void decrypt(byte[] rawData) {
            unsafe {
                fixed (byte* rawDataPtr = rawData) {
                    CityVRUtilsWrapper.VULBICVIOO((ulong)rawDataPtr, rawData.Length);
                }
            }
        }

        internal static bool IsEncrypted(string bundleName) {
            Regex regex = new Regex("^.+_(\\d+)(-\\d+)?");
            Match match = regex.Match(bundleName);
            if (!match.Success) {
                return false;
            }

            string typeStr = match.Groups[1].Value;

            int type;
            if (!int.TryParse(typeStr, out type)) {
                return false;
            }

            BundleType bundleType;
            try {
                bundleType = (BundleType) type;
            } catch (Exception) {
                return false;
            }

            switch (bundleType) {
                case BundleType.Scene:
                case BundleType.BuildingModel:
                case BundleType.BuildingTexture:
                case BundleType.GroundModel:
                case BundleType.GroundTexture:
                    return true;

                default:
                    return false;
            }
        }
    }
}
