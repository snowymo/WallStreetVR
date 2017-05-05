using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class FlightLine : MonoBehaviour
{
    public float width;
    public int sampleCount;
    public float startTime;
    public float endTime;

    public LineGenerator LineGenerator = new LinearLineGenerator(new Vector3(0, 0, 0), new Vector3(1000, 0, 0));

    private LineRenderer lineRenderer;

    private float t;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        loadData();
    }

    private void loadData()
    {
        lineRenderer.SetVertexCount(sampleCount);

        for (int i = 0; i < sampleCount; i++)
        {
            float ts = (float)i / sampleCount;
            lineRenderer.SetPosition(i, transform.TransformPoint(LineGenerator.sample(ts)));
        }

        lineRenderer.material.SetFloat("_TimeOffset", Random.Range(0f, 1f));
    }

    void Update()
    {
        if (WorldTime.Instance.timeChanged)
        {
            float delayedTime = (WorldTime.Instance.normalizedTime - startTime) / (endTime - startTime);        
            if (delayedTime < 0 || delayedTime > 1)
            {
                delayedTime = 0;
            }
            lineRenderer.material.SetFloat("_Ratio", delayedTime);
        }
      
        Vector3 refPos = transform.TransformPoint(LineGenerator.sample(0.5f));
        float distance = (Camera.main.transform.position - refPos).magnitude;

        var scaledWidth = width * distance / 10000;
        lineRenderer.SetWidth(scaledWidth, scaledWidth);
    }
}
