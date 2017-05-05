using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DLCCityMenuItem : MonoBehaviour
{
    public DLCCity.Enum CityEnum;

    private DLCCity city;
    private DLCManager.DLCState dlcState = DLCManager.DLCState.Invalid;

    private Button button;

	void Awake ()
    {
        city = DLCCity.GetCityForEnum(CityEnum);
	}

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
	
	void Update () {
	    if (dlcState == DLCManager.DLCState.Invalid)
	    {
            dlcState = DLCManager.Instance.GetDLCState(city);
	    }

	    if (dlcState == DLCManager.DLCState.Invalid)
	    {
	        return;
	    }

	    bool isReady = (dlcState == DLCManager.DLCState.Ready);
	    bool isCurrentScene = (DLCManager.Instance.IsCurrentScene(city));
	    gameObject.SetActive(isReady && !isCurrentScene);
	}

    public void OnClick()
    {
        Debug.Log("Loading City: " + city.BundleName);
        SceneSwitcher.Instance.CityToLoad = city;
        SceneManager.LoadScene(0);
    }
}
