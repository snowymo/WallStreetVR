using CitySceneLoader;
using UnityEngine;

[RequireComponent(typeof(ProgressBar))]
public class LoadingProgress : MonoBehaviour {

    private ProgressBar progressBar;
    private CanvasGroup canvasGroup;

	void Start () {
	    progressBar = GetComponent<ProgressBar>();
	    canvasGroup = GetComponent<CanvasGroup>();
	}
	
	void Update ()
	{
	    float targetAlpha = StaticCitySceneLoader.Instance.IsLoading ? 1 : 0;
	    float currentAlpha = canvasGroup.alpha;

	    progressBar.progress = StaticCitySceneLoader.Instance.Progress;
	    canvasGroup.alpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.5f);
	}
}
