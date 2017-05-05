using UnityEngine;

public class FlyingController : MonoBehaviour
{
    public float JetForceMultiplier = 1.0f;
    public float FlyingPlayerHeight = 2.0f;

    [Header("Runtime")]
    public Vector3 jetForce;

    private new Rigidbody rigidbody;
    private Collider collider;

    private bool prevIsFlying = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        rigidbody.isKinematic = true;
    }

	void Update () {
	    if (GameModeManager.Instance.FlyingModeEnabled)
	    {
	        updateFlyingMode();
	    }
	}

    void LateUpdate()
    {
        if (!GameModeManager.Instance.FlyingModeEnabled)
        {
            // updateNormalMode();
        }
    }

    private void updateFlyingMode()
    {
        TrackingSpaceController trackingSpace = TrackingSpaceController.Instance;
        if (!prevIsFlying)
        {
            Vector3 headLocalOffset = trackingSpace.headTransform.localPosition - Vector3.zero;
            Vector3 headPosition = trackingSpace.headTransform.position;

            trackingSpace.transform.localScale = Vector3.one;
            trackingSpace.transform.position = headPosition - trackingSpace.transform.TransformVector(headLocalOffset);

            float playerHeight = headPosition.y - TrackingSpaceController.Instance.transform.position.y;

            Vector3 center = new Vector3(headPosition.x, headPosition.y - 0.5f * playerHeight, headPosition.z);
            transform.position = center;

            if (!Mathf.Approximately(playerHeight, 0))
            {
                transform.localScale = 0.5f * playerHeight * Vector3.one;
            }

            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = false;

            prevIsFlying = true;
        }

        Vector3 bottomPos = new Vector3(collider.bounds.center.x, collider.bounds.min.y, collider.bounds.center.z);
        TrackingSpaceController.Instance.transform.position = bottomPos;

        Vector3 forward = InputManager.Instance.laserController.transform.forward;
        float jet = JetForceMultiplier * InputManager.Instance.GetAxis(InputManager.Axis.FlyingJet).GetValueOrDefault().x;

        if (!Mathf.Approximately(jet, 0))
        {
            jetForce = forward * jet * JetForceMultiplier;
            rigidbody.AddForce(jetForce);
        }
    }

    private void updateNormalMode()
    {
        rigidbody.isKinematic = true;

        Vector3 headPos = TrackingSpaceController.Instance.headTransform.position;
        float playerHeight = headPos.y - TrackingSpaceController.Instance.transform.position.y;

        Vector3 center = new Vector3(headPos.x, headPos.y - 0.5f * playerHeight, headPos.z);
        transform.position = center;

        transform.localScale = 0.5f * playerHeight * Vector3.one;
    }
}
