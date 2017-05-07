using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushCtrl : MonoBehaviour {

	public Transform arcam;

	public Vector3 offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = arcam.position + offset;
	}
}
