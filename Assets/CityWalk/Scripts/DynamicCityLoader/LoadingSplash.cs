using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadingSplash : MonoBehaviour {

	void Start ()
	{
	    Sprite sprite = null;

	    DLCCity city = SceneSwitcher.Instance.CityToLoad;
	    if (city != null)
	    {
	        string splashName = string.Format("{0}_loading", city.BundleName);
	        sprite = Resources.Load<Sprite>(splashName);

	        Text text = GetComponentInChildren<Text>(false);
	        if (text != null)
	        {
	            text.gameObject.SetActive(false);
	        }
	    }

	    if (sprite == null)
	    {
	        // fallback
	        string fallbackName = "unknown_city_loading";
	        sprite = Resources.Load<Sprite>(fallbackName);

	        Text text = GetComponentInChildren<Text>(true);
	        if (text != null)
	        {
	            if (city != null)
	            {
	                text.text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(city.BundleName.Replace("_", " "));
	                text.gameObject.SetActive(true);
	            }
	            else
	            {
	                text.gameObject.SetActive(false);
	            }
	        }
	    }

	    if (sprite != null)
	    {
	        GetComponent<Image>().sprite = sprite;
	    }
	}
}
