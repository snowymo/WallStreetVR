using UnityEngine;
using System.Collections;
//using TMPro;

public class CitywalkCameraPreviewBehavior : MonoBehaviour {

    public static int MAX_FOLDER_TEXT_TIMES = 5;
    private static int folderTextDisplayedTimes = 0;

    public float StayTime = 0.5f;
    public float MaxFallDuration = 10;
    public float GroundDuration = 2;
    public float TorqueRange = 5;

    //private TextMeshPro savedLocationText;

    void Start()
    {
        StartCoroutine(delayedFall());

        //savedLocationText = GetComponentInChildren<TextMeshPro>();

        //bool tutorialEnabled = (folderTextDisplayedTimes < MAX_FOLDER_TEXT_TIMES) && GameModeManager.Instance.TutorialEnabled;

        //savedLocationText.gameObject.SetActive(tutorialEnabled);

        //if (tutorialEnabled) {
        //    folderTextDisplayedTimes++;
        //}
    }

    public void OnCollisionEnter(Collision collision) {
        StartCoroutine(delayedGrounded());
    }

    private IEnumerator delayedFall()
    {
        yield return new WaitForSeconds(StayTime);

//        Destroy(savedLocationText.gameObject);

        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;

        Vector3 torque = new Vector3(Random.value, Random.value, Random.value) * TorqueRange;
        rigidBody.AddTorque(torque);

        yield return new WaitForSeconds(MaxFallDuration);

        Destroy(gameObject);
    }

    private IEnumerator delayedGrounded() {
        yield return new WaitForSeconds(GroundDuration); 
        Destroy(gameObject);
    }
}
