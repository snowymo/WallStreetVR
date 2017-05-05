using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleGuideMenuItem : MonoBehaviour {

    TMPro.TextMeshProUGUI text;
    Button button;

	void Start () {
        text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
	    button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);
	}
	
	void Update () {
        if (GameModeManager.Instance.TutorialEnabled)
        {
            text.text = "Dismiss Guide";
        }
        else
        {
            text.text = "Show Guide";
        }

        EventSystem.current.SetSelectedGameObject(null);
	}

    public void OnClick()
    {
        InputManager.Instance.SimulateButtonDown(InputManager.Button.ToggleTutorial);
    }
}
