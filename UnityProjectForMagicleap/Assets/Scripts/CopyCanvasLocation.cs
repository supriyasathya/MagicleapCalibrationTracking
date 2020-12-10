using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCanvasLocation : MonoBehaviour {

    public GameObject statCanvas;
	
	// Update is called once per frame
	void Update () {
        transform.position = statCanvas.transform.position;
        transform.rotation = statCanvas.transform.rotation;
	}
}
