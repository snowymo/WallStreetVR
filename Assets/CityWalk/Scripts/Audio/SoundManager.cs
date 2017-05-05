using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    [Header("Player Height")] 
    public float PlayerHeightMaxSpeed = 200;
    public float PlayerHeightCatchUpRatio = 0.8f;

    [Header("Hour of Day")]
    public float HourOfDayMaxSpeed = 10;
    public float HourOfDayCatchUpRatio = 0.8f;

    [Header("Runtime")]
    public float PlayerHeight;
    public float DelayedPlayerHeight = float.NaN;

    public float HourOfDay;
    public float DelayedHourOfDay = float.NaN;

    public void UpdatePlayerHeight(float playerHeight) {
        PlayerHeight = playerHeight;
        if (float.IsNaN(DelayedPlayerHeight)) {
            DelayedPlayerHeight = playerHeight;
        } else {
            DelayedPlayerHeight = delayedLerp(Time.deltaTime,
                DelayedPlayerHeight,
                playerHeight,
                PlayerHeightMaxSpeed,
                PlayerHeightCatchUpRatio);
        }

	    AkSoundEngine.SetRTPCValue("PlayerHeight", DelayedPlayerHeight);
    }

    public void UpdateHourOfDay(float hourOfDay) {
        HourOfDay = hourOfDay;

        if (float.IsNaN(DelayedHourOfDay)) {
            DelayedHourOfDay = hourOfDay;
        } else {
            float deltaHour = hourOfDay - DelayedHourOfDay;
            if (Mathf.Abs(deltaHour) > 12) {
                // 23 -> 1 ==> (1 - 23) + 24 = 2
                // 1 -> 23 ==> (23 - 1) - 24 = -2
                deltaHour = deltaHour - Mathf.Sign(deltaHour) * 24;
                hourOfDay = DelayedHourOfDay + deltaHour;
            }

            DelayedHourOfDay = delayedLerp(Time.deltaTime,
                DelayedHourOfDay,
                hourOfDay,
                HourOfDayMaxSpeed,
                HourOfDayCatchUpRatio);

            if (DelayedHourOfDay > 24) {
                DelayedHourOfDay -= 24;
            } else if (DelayedHourOfDay < 0) {
                DelayedHourOfDay += 24;
            }

            AkSoundEngine.SetRTPCValue("HourOfDay", DelayedHourOfDay);
        }
    }

    private float delayedLerp(float timeDelta, float currentValue, float targetValue, float maxSpeed, float catchUpRatio) {
        float delta = Mathf.Lerp(currentValue, targetValue, catchUpRatio) - currentValue;
        float maxDelta = maxSpeed * timeDelta;
        delta = Mathf.Clamp(delta, -maxDelta, maxDelta);
        return currentValue + delta;
    }
} 