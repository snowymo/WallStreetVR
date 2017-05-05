using UnityEngine;
using System.Collections;

public class FlightLineLoader : MonoBehaviour {

    public FlightLine flightLinePrefab;

	void Start () {
        loadRandomLines();
	}

    private void loadRandomLines()
    {
        for (int i = 0; i < 100; i++)
        {
            float height = 400 + Random.Range(-200f, 200f);
            float xOsc = Random.Range(-20f, 20f);

            Vector3 start = new Vector3(i * 40 + xOsc - 2000, height, -3000);
            Vector3 end = new Vector3(i * 40 + xOsc - 2000, height, 3000);

            if (Random.value > 0.5f)
            {
                float t = end.z;
                end.z = start.z;
                start.z = t;
            }

            LinearLineGenerator lineGenerator = new LinearLineGenerator(start, end);        // ***

            FlightLine flightLine = Instantiate(flightLinePrefab);                          // ***
            flightLine.transform.parent = transform;
            flightLine.LineGenerator = lineGenerator;                                       // ***

            float startTime = Random.Range(0f, 8f);
            float endTime = startTime + Random.Range(0f, 12f - startTime);
            flightLine.startTime = startTime;                                               // ***
            flightLine.endTime = endTime;
        }
    }
}
