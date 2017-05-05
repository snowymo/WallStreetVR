using UnityEngine;

public struct DelayedLerp {
    public float MaxSpeed;
    public float CatchUpRatio;

    public float GetValue(float timeDelta, float currentValue, float targetValue)
    {
        float delta = Mathf.Lerp(currentValue, targetValue, CatchUpRatio) - currentValue;
        float maxDelta = MaxSpeed * timeDelta;
        delta = Mathf.Clamp(delta, -maxDelta, maxDelta);
        return currentValue + delta;
    }
}
