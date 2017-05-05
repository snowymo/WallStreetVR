using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ControllerVibrateBehavior : MonoBehaviour
{
    public int MaxVibrateCount = 10;

//    private SteamVR_TrackedController trackedController;

    private int vibrateCount = 0;

    void Start()
    {
//        trackedController = GetComponentInParent<SteamVR_TrackedController>();
    }

    void OnTriggerEnter(Collider other) {
        vibrateCount = 0;
    }

    public void OnTriggerExit(Collider other)
    {
    }

    void OnTriggerStay(Collider other)
    {
        if (vibrateCount < MaxVibrateCount) {
            pulseVibrate();
            vibrateCount++;
        }
    }

    /// <param name="length">how long the vibration should go for</param>
    /// <param name="strength">vibration strength from 0-1</param>
    private IEnumerator longVibrate(float length, float strength)
    {
//        var device = SteamVR_Controller.Input((int)(trackedController.controllerIndex));
        for (float i = 0; i < length; i += Time.deltaTime)
        {
            //device.TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
            yield return null;
        }
    }

    private void pulseVibrate()
    {
//        var device = SteamVR_Controller.Input((int)(trackedController.controllerIndex));
//        device.TriggerHapticPulse(3999);
    }
}
