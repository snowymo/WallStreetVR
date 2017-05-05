using UnityEngine;

public class MenuController : MonoBehaviour {

    private Canvas canvas;

    public static MenuController Instance { get; private set; }

    [Header("Runtime")]
    public SteamVR_ControllerManager controllerManager;
    public GameObject leftLaserPointer;
    public GameObject rightLaserPointer;

    void Awake()
    {
        Instance = this;
    }

	void Start () {
        canvas = GetComponent<Canvas>();
        controllerManager = GetComponentInParent<SteamVR_ControllerManager>();
    }
	
	void Update () {
        bool leftLaserPointerMissing = (leftLaserPointer == null);
        bool rightLaserPointerMissing = (rightLaserPointer == null);
        bool leftControllerFound = (controllerManager.left != null);
        bool rightControllerFound = (controllerManager.right != null);

        if (leftControllerFound && rightControllerFound &&
            (leftLaserPointerMissing || rightLaserPointerMissing))
        {
            leftLaserPointer = controllerManager.left.transform.Find("LaserPointer").gameObject;
            rightLaserPointer = controllerManager.right.transform.Find("LaserPointer").gameObject;

            GameModeManager.Instance.MenuEnabled = false;
        }

	    bool menuEnabled = GameModeManager.Instance.MenuEnabled;

	    canvas.enabled = menuEnabled;
	    if (leftLaserPointer != null)
	    {
	        leftLaserPointer.SetActive(!menuEnabled);
	    }

	    if (rightLaserPointer != null)
	    {
	        rightLaserPointer.SetActive(menuEnabled);
	    }
	}
}
