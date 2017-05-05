using UnityEngine;
using System.Collections;

public class CameraLayerBasedCulling : MonoBehaviour {

	void Start () {
        float[] layerCullDistances = new float[32];
	    layerCullDistances[LayerMask.NameToLayer("Model")] = 3000;
	    // layerCullDistances[LayerMask.NameToLayer("Ground")] = 10000;

	    Camera thisCam = GetComponent<Camera>();
	    thisCam.layerCullDistances = layerCullDistances;
	    thisCam.layerCullSpherical = true;
	}
	
	void Update () {
	
	}
}
