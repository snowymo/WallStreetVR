using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class changePos : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var a = new List<string>(); 
		foreach (Transform child in transform){
			a.Add (child.name);
//			foreach(Transform grandchild in child)
//			{
//				if (grandchild.name.Contains ("MHN")) {
//					print (grandchild.name);
//					child.position += new Vector3 (-55, 0, -90);
//					break;
//				}
//			}	
		}
		print (string.Join (" ", a.ToArray ()));
		//transform.position += new Vector3 (-55, 0, -90);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
