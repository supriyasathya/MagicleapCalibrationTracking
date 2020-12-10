using UnityEngine;

// this class copies the transform.position of another GameObject

public class copyCoords : MonoBehaviour {

    public GameObject imgTracker;

	// Update is called once per frame
	void Update () {
        this.transform.position = imgTracker.transform.position;
	}
}
