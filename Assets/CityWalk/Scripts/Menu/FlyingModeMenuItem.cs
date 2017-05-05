using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlyingModeMenuItem : MonoBehaviour {

    TMPro.TextMeshProUGUI text;
    Button button;

	void Start () {
        text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
	    button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);
	}
	
	void Update () {
        if (GameModeManager.Instance.FlyingModeEnabled)
        {
            text.text = "Land";
        }
        else
        {
            text.text = "Fly";
        }

        EventSystem.current.SetSelectedGameObject(null);
	}

    public void OnClick()
    {
        InputManager.Instance.SimulateButtonDown(InputManager.Button.FlyingMode);
    }
}
