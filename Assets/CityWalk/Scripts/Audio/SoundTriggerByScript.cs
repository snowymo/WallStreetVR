using UnityEngine;

public class SoundTriggerByScript : AkTriggerBase {

    public void Play() {
        if (triggerDelegate != null) {
            triggerDelegate(gameObject);
        }
    }

}
