using UnityEngine;
using System.Collections;

public class SkywandDisabler : MonoBehaviour {
	void Start () {
#if !SKYWAND
        gameObject.SetActive(false);
#endif
    }
}
