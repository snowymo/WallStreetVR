using UnityEngine;
using System.Collections;
using System;

public abstract class LineGenerator
{
    public abstract Vector3 sample(float t);
}

public class LinearLineGenerator : LineGenerator
{
    private Vector3 start;
    private Vector3 end;

    public LinearLineGenerator(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;
    }

    public override Vector3 sample(float t)
    {
        float time = Mathf.Clamp01(t);
        return Vector3.Lerp(start, end, t);
    }
}

public class SpiralLineGenerator : LineGenerator {
    public float circle = 10;
    public float radius = 3;
    public float height = 10;

    public override Vector3 sample(float t) {
        float a = t * circle * 2 * Mathf.PI;

        float x = Mathf.Cos(a) * radius;
        float z = Mathf.Sin(a) * radius;
        float y = t * height;

        return new Vector3(x, y, z);
    }
}
