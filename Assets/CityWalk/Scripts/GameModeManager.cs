using UnityEngine;

public class GameModeManager : Singleton<GameModeManager> {

    [Header("Runtime Game Modes")]
    public bool TutorialEnabled = true;

    public bool CitywalkCameraEnabled;

    public bool MenuEnabled;

    public bool FlyingModeEnabled;

    void Update()
    {
        // Tutorial
        if (InputManager.Instance.GetButtonDown(InputManager.Button.ToggleTutorial)) {
            TutorialEnabled = !TutorialEnabled;
        }

        // Menu
        if (InputManager.Instance.GetButtonDown(InputManager.Button.Menu))
        {
            MenuEnabled = !MenuEnabled;
            if (MenuEnabled)
            {
                CitywalkCameraEnabled = false;
            }
        }	
    
        // Camera
	    if (InputManager.Instance.GetButtonDown(InputManager.Button.ToggleCamera)) {
	        CitywalkCameraEnabled = !CitywalkCameraEnabled;

	        if (CitywalkCameraEnabled)
	        {
	            MenuEnabled = false;
	        }
	    }
    }
}
