using UnityEngine;

public class CameraNearPlaneHeightBased : MonoBehaviour {

    public TrackingSpaceController TrackingSpaceController;
    public AnimationCurve HeightToNearPlaneCurve;

    private new Camera camera;
    private float initialFarPlane;

	void Start () {
	    camera = GetComponent<Camera>();
	    initialFarPlane = camera.farClipPlane;
	}
	
	void Update () {
	    camera.nearClipPlane = GameModeManager.Instance.FlyingModeEnabled ? 0.1f : HeightToNearPlaneCurve.Evaluate(TrackingSpaceController.PlayerHeight);
	    camera.farClipPlane = camera.nearClipPlane < 1 ? 10000 : initialFarPlane;
	}
}
