using System.Collections;
using UnityEngine;

public class OpeningController : MonoBehaviour {

    public static OpeningController Instance { get; private set; } 

    public enum GameState {
        Waiting = 0,
        Opening,
        Running,
    }

    public GameState State = GameState.Waiting;
    public float minTitleVisibleTime = 3f;
    public float endingHour;
    public float endingWindSpeed;

    private VisibilityTimeCounter titleVisibilityTimeCounter;
    private Animator animator;
    private TimeOfDayController timeOfDayController;

    void Awake()
    {
        Instance = this;
    }

	void Start () {
        State = GameState.Waiting;

	    titleVisibilityTimeCounter = GetComponentInChildren<VisibilityTimeCounter>();
	    animator = GetComponent<Animator>();
	    timeOfDayController = GetComponentInChildren<TimeOfDayController>();
	}
	
	void Update () {
	    if (State == GameState.Waiting &&
            titleVisibilityTimeCounter.VisibleTime > minTitleVisibleTime) {
            State = GameState.Opening;	        
            animator.SetTrigger("OpeningStart");
	        AkSoundEngine.PostEvent("Play_CityWalk_Opening_TimeOfDay", gameObject);

            UnityAnalytics.Sunrise();
	    }
    }

    public void OnOpeningFinished() {
        titleVisibilityTimeCounter.gameObject.SetActive(false);
        timeOfDayController.Hour = endingHour;
        timeOfDayController.WindSpeed = endingWindSpeed;

        StartCoroutine(delayedSwitchingToRunning());
    }

    private IEnumerator delayedSwitchingToRunning() {
        // Delay 2 frames so that timeOfDayController can update the ending value
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        State = GameState.Running;
    }
}
