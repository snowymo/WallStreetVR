using UnityEngine;
using System.Collections;

public class WorldTime : MonoBehaviour {

    public static WorldTime Instance;

    public float maxTime = 10;
    public float speed = 0;

    [Range(0,10)]
    public float normalizedTime;

    private float prevNormalizedTime;

    public bool timeChanged { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        normalizedTime += Time.deltaTime * speed;
        if (normalizedTime > maxTime)
        {
            normalizedTime = 0;
        }


        timeChanged = !Mathf.Approximately(prevNormalizedTime, normalizedTime);
        prevNormalizedTime = normalizedTime;
    }

    public void SetNormalizedTime(float t)
    {
        normalizedTime = t;
    }
}
