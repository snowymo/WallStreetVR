using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DLCCity
{
    /// <summary>
    /// For Unity UI
    /// </summary>
    public enum Enum
    {
        Invalid = 0,
        SanFrancisco,
        NewYork,
        Chicago,
        Kiev
    }

    public static readonly DLCCity SanFrancisco = new DLCCity("san_fran", true);
    public static readonly DLCCity NewYork = new DLCCity("new_york");
    public static readonly DLCCity Chicago = new DLCCity("chicago");
    public static readonly DLCCity Kiev = new DLCCity("kiev");

    public static readonly Dictionary<Enum, DLCCity> AllCities = new Dictionary<Enum, DLCCity>
    {
        {Enum.SanFrancisco, SanFrancisco},
        {Enum.NewYork, NewYork},
        {Enum.Chicago, Chicago},
        {Enum.Kiev, Kiev },
    };

    public static readonly DLCCity Default = SanFrancisco;

    public static DLCCity GetCityForEnum(Enum cityEnum)
    {
        if (AllCities.ContainsKey(cityEnum))
        {
            return AllCities[cityEnum];
        }

        return null;
    }

    public static DLCCity GetCityForBundleName(string bundleName)
    {
        foreach (var item in AllCities)
        {
            if (item.Value.BundleName == bundleName)
            {
                return item.Value;
            }
        }

        return null;
    }

    public string BundleName { get; private set; }    

    /// <summary>
    /// Whether the city is built in the main game
    /// </summary>
    public bool IsBuiltIn { get; private set; }

    public string RootPostfix { get; private set; }

    public DLCCity(string bundleName, bool isBuiltIn = false, string rootPostfix = "0")
    {
        BundleName = bundleName;
        IsBuiltIn = isBuiltIn;
        RootPostfix = rootPostfix;
    }

    public string SceneName
    {
        get
        {
            return string.Format("{0}_scene", BundleName);
        }
    }

    public string GetSubBundleFileName(string postfix)
    {
        return string.Format("{0}_bundle_{1}", BundleName, postfix);
    }

    public string RootBundleFileName
    {
        get
        {
            return GetSubBundleFileName(RootPostfix);
        }
    }
}

public class DLCManager 
	//: Singleton<DLCManager>
{
    public enum DLCState
    {
        Invalid = 0,

        /// <summary>
        /// Purchased and downloaded, ready to run
        /// </summary>
        Ready,

        /// <summary>
        /// DLC is downloading
        /// </summary>
        Downloading,

        /// <summary>
        /// Purchased but no downloaded. 
        /// </summary>
        FileMissing,

        /// <summary>
        /// DLC is not purchased
        /// </summary>
        NotPurchased
    }

    private AssetBundleManifest manifest = null;

    void Start()
    {
//        StartCoroutine(loadManifestAsync());
    } 

    #region Bundle Management
    public string GetBundlePath(DLCCity city, string postfix)
    {
        return GetBundlePath(city.GetSubBundleFileName(postfix));
    }

    public static string GetBundlePath(string filename)
    {
#if UNITY_EDITOR
        return string.Format("Assets/StreamingAssets/AssetBundles/{0}", filename);
#else
        return string.Format("{0}/AssetBundles/{1}", Application.streamingAssetsPath, filename);
#endif
    }

    /// <returns>null if manifest is not loaded. </returns>
    public string[] GetCityDependencies(DLCCity city)
    {
        if (manifest == null)
        {
            return null;
        }

        return manifest.GetAllDependencies(city.RootBundleFileName);
    }

    public bool IsManifestLoaded
    {
        get
        {
            return manifest != null;
        }
    }
    #endregion

    #region DLC Management

    public bool IsCurrentScene(DLCCity city)
    {
        return city.SceneName == SceneManager.GetActiveScene().name;
    }

    public DLCState GetDLCState(DLCCity city)
    {
/*        if (!SteamManager.Initialized)
        {
            Debug.LogError("DLC State invalid since Steam is not initialized. ");
            return DLCState.Invalid;
        }
*/ 
        // TODO: Check Steam status

        // Check file exists
        string rootPath = GetBundlePath(city.RootBundleFileName);
        if (!File.Exists(rootPath))
        {
            return DLCState.FileMissing;
        }

        if (!IsManifestLoaded)
        {
            Debug.LogError("DLC State invalid since manifest is not loaded. ");
            return DLCState.Invalid;
        }

        string[] dependencies = GetCityDependencies(city);
        for (int i = 0; i < dependencies.Length; i++)
        {
            string path = GetBundlePath(dependencies[i]);
            if (!File.Exists(path))
            {
                return DLCState.FileMissing;
            }
        }

        return DLCState.Ready;
    }


    #endregion

    #region Private
    private IEnumerator loadManifestAsync()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(GetBundlePath("AssetBundles"));
        yield return request;

        AssetBundle bundle = request.assetBundle;
        manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        Debug.Log("DLC Manifest loaded.");

        // Check DLC
        foreach (var item in DLCCity.AllCities.Values)
        {
            Debug.Log("City State: " + item.BundleName + ", " + GetDLCState(item));
        }
    }
    #endregion
}
