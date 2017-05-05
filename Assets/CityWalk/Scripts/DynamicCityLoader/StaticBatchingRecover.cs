using UnityEngine;

public class StaticBatchingRecover : MonoBehaviour {
	void Start () {
        StaticBatchingUtility.Combine(gameObject);
	}
}
