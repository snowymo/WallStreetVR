using UnityEngine;

public class TrackingSpaceController : MonoBehaviour
{
    public static TrackingSpaceController Instance { get; private set; }

    public Transform headTransform;
    public Transform skyDome;

    [Header("Controllers")]
    public Transform laserTransform;
    public Transform droneTransform;

    [Header("Teleport")]
    public Transform teleportIndicator;
    public float teleportWaitTime;
    public float maxHitDistance;
    public float offsetOnHitBuilding;

    [Header("Pinch")]
    public float maxScaleRatio;
    public float minScaleRatio = 1f;
    public float pinchScaleRatio;
    public bool pinchScale;
    public bool pinchRotation;
    public bool pinchTranslation;

    [Header("Runtime")]
    public bool teleportWaiting; // Public for instruction
    public bool teleportReady;   // Public for instruction
    public HitType hitType;         // Public for instruction
    public float scaleRatio;
    public GameObject teleportHit;

    public AnimationCurve ScaleToGravityCurve;

    public float teleportElapsedTime;

    private Vector3 pinchStartVector;
    private Vector3 pinchStartCenter;
    private Vector3 pinchStartScale;
    private Vector3 pinchStartPosition;
    private Quaternion pinchStartRotation;
    private Quaternion pinchStartSkydomeRotation;
    private Vector3 pinchPivot;
    private bool isPinching = false;

    public enum HitType {
        None = 0,
        Ground,
        Model,
        Plane
    }

	void Awake ()
	{
	    Instance = this;
	}
	
	void Update () {
        updateTeleport();
        updatePinch();

	    Physics.gravity = -Vector3.up * ScaleToGravityCurve.Evaluate(transform.localScale.x);

	    SoundManager.Instance.UpdatePlayerHeight(PlayerHeight);
	}

    public float PlayerHeight {
        get { return headTransform.position.y; }
    }

    private void updateTeleport() {
        bool buttonDown = InputManager.Instance.GetButtonDown(InputManager.Button.Teleport);
        bool buttonUp = InputManager.Instance.GetButtonUp(InputManager.Button.Teleport);

        var footPos = headTransform.position;
        footPos.y = transform.position.y;

        float hitDist = 0;
        Ray ray = new Ray(laserTransform.position, laserTransform.forward);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, maxHitDistance, LayerMask.GetMask("Ground", "Model"))) {
            var layer = hitInfo.collider.gameObject.layer;
            if (layer == LayerMask.NameToLayer("Ground")) {
                hitType = HitType.Ground;
            } else if (layer == LayerMask.NameToLayer("Model") || layer == LayerMask.NameToLayer("ModelTall")) {
                hitType = HitType.Model;
            } else {
                Debug.LogError("Teleportor hits unknown object: " + hitInfo.collider.gameObject.name);
            }
            hitDist = hitInfo.distance;
            teleportHit = hitInfo.collider.gameObject;
        } else {
            var ground = new Plane(Vector3.up, footPos);
            hitType = ground.Raycast(ray, out hitDist) ? HitType.Plane : HitType.None;
        }

        // Cancel teleport in certain condition
        if (hitDist > maxHitDistance ||
            Vector3.Dot(ray.direction, Vector3.up) > 0.9f || 
            !MapModelSwitcher.Instance.currentMapModel.BoundsContainsPoint(ray.GetPoint(hitDist))) { 
            hitType = HitType.None;
        } 

        if (buttonDown && hitType != HitType.None && !teleportWaiting) {
            teleportWaiting = true;
            teleportElapsedTime = 0;
        }

        if (teleportWaiting) {
            if (hitType != HitType.None) {
                teleportElapsedTime += Time.deltaTime;
                var t = 5.0f * Mathf.Clamp01(teleportElapsedTime / teleportWaitTime);
                teleportIndicator.position = ray.GetPoint(hitDist);

                var lens = teleportIndicator.GetComponent<LensFlare>();
                lens.brightness = t;
                // lens.color = teleportElapsedTime >= teleportWaitTime ? new Color(0.617f * 0.8f, 0.52f * 0.8f, 0.773f * 0.8f) : new Color(92/256f, 77/256f, 115/256f);

                teleportReady = teleportElapsedTime >= teleportWaitTime;
            } else {
                teleportWaiting = false;
                teleportReady = false;
            }
        }

        if (buttonUp) {
            if (teleportReady) {
                AkSoundEngine.PostEvent("Play_Whoosh", gameObject);
                UnityAnalytics.Teleport(teleportIndicator.position);

                var buildingOffset = Vector3.zero;
                if (hitType == HitType.Model) {
                    var buildingToFoot = footPos - hitInfo.point;
                    buildingToFoot.y = 0;
                    buildingOffset = offsetOnHitBuilding * transform.localScale.x * buildingToFoot.normalized;
                }

                var footOffset = teleportIndicator.position - footPos;
                // Adjust vertical offset
                switch (hitType) {
                    case HitType.Plane:
                        footOffset.y = 0;
                        break;

                    case HitType.Model:
                        // Make another hit test from the hit point to ground
                        RaycastHit groundHitInfo;
                        if (Physics.Raycast(teleportIndicator.position,
                            Vector3.down,
                            out groundHitInfo,
                            maxHitDistance,
                            LayerMask.GetMask("Ground"))) {
                            footOffset.y = groundHitInfo.point.y - footPos.y;
                        } else {
                            footOffset.y = 0;
                        }
                        break;

                    case HitType.Ground:
                        footOffset.y = hitInfo.point.y - footPos.y;
                        break;

                    case HitType.None:
                    default:
                        break;
                }

                transform.position += footOffset + buildingOffset;

                teleportReady = false;
            }

            teleportElapsedTime = 0;
            teleportWaiting = false;
        }

        teleportIndicator.gameObject.GetComponent<Renderer>().enabled = teleportWaiting;
        teleportIndicator.gameObject.GetComponent<LensFlare>().enabled = teleportWaiting;
    }

    private void updatePinch() {
        if (InputManager.Instance.GetButtonDown(InputManager.Button.ModelManipulating)) {
            pinchStartVector = laserTransform.localPosition - droneTransform.localPosition;
            pinchStartCenter = droneTransform.localPosition + 0.5f * pinchStartVector;
            pinchStartScale = transform.localScale;
            pinchStartPosition = transform.position;
            pinchStartRotation = transform.rotation;
            pinchStartSkydomeRotation = skyDome.rotation;

            pinchPivot = headTransform.position;
            pinchPivot.y = pinchStartPosition.y;

            isPinching = true;
        }

        if (isPinching && InputManager.Instance.GetButton(InputManager.Button.ModelManipulating)) {
            Vector3 pinchVector = laserTransform.localPosition - droneTransform.localPosition;
            Vector3 pinchCenter = droneTransform.localPosition + 0.5f * pinchVector;

            // Scale
            scaleRatio = 1;

            if (pinchScale) {
                scaleRatio = Mathf.Pow(pinchVector.magnitude / pinchStartVector.magnitude, -pinchScaleRatio);
                transform.localScale = scaleRatio * pinchStartScale;

                if (transform.localScale.x > maxScaleRatio) {
                    transform.localScale = maxScaleRatio * Vector3.one;
                    scaleRatio = maxScaleRatio / pinchStartScale.x;
                } else if (transform.localScale.x < minScaleRatio) {
                    transform.localScale = minScaleRatio * Vector3.one;
                    scaleRatio = minScaleRatio / pinchStartScale.x;
                }
            }

            // Rotate
            Quaternion rot = Quaternion.identity;

            if (pinchRotation) {
                Vector3 pinchStartVectorProj = pinchStartVector;
                pinchStartVectorProj.y = 0;

                Vector3 pinchVectorProj = pinchVector;
                pinchVectorProj.y = 0;

                rot = Quaternion.FromToRotation(pinchVectorProj.normalized, pinchStartVectorProj.normalized);
                transform.rotation = rot * pinchStartRotation;
                skyDome.rotation = rot * pinchStartSkydomeRotation;
            }

            // Keep pivot and translate
            Vector3 offset = Vector3.zero;

            if (pinchTranslation) {
                offset = transform.TransformPoint(pinchCenter) - transform.TransformPoint(pinchStartCenter);
                offset.y = 0;
                transform.position = scaleRatio * (rot * (pinchStartPosition - pinchPivot)) + pinchPivot - offset;
            }

            // Fix ground
            Ray ray = new Ray(headTransform.position + 500 * Vector3.up, Vector3.down);
            RaycastHit hitInfo;
            if (Physics.Raycast(headTransform.position, Vector3.down, out hitInfo, 2000, LayerMask.GetMask("Ground"))) {
                transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
            }
        }

        if (InputManager.Instance.GetButtonUp(InputManager.Button.ModelManipulating)) {
            isPinching = false;
            UnityAnalytics.Zoom(headTransform.position, transform.localScale.x);
        }
    }
}
