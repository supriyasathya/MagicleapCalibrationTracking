using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;

/// <summary>
/// On bumper press, raycast and instantiate a cube 
/// This uses Physics.Raycast + MLSpatialMapper
/// </summary>

public class CubeRaycast : MonoBehaviour {

    private MLInput.Controller _controller;

    public GameObject raycastButton;

    public Text buttonText;

    // Use this for initialization
    void Start () {
        MLInput.Start(); // to start receiving input from the controller
        MLInput.OnControllerButtonDown += OnButtonDown; // a listener function that listens for the button input.
        _controller = MLInput.GetController(MLInput.Hand.Left); //left or right it doesn’t really matter
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = _controller.Position;       // a Vector3 quantity
        transform.rotation = _controller.Orientation;    // a Quaternion quantity
    }

    void OnButtonDown(byte controller_id, MLInput.Controller.Button button)
    {
        RaycastHit hit;
        if (Physics.Raycast(_controller.Position, transform.forward, out hit))
        {
            //if (button == MLInputControllerButton.Bumper && hit.collider == raycastButton.GetComponent<Collider>())
            if (button == MLInput.Controller.Button.Bumper)
            {
                buttonText.text = "Found an object - distance: " + hit.distance.ToString("F2") + " " + hit.collider.GetComponentInParent<Transform>().name;
            }
        }
        else
        {
            if (button == MLInput.Controller.Button.Bumper)
            {
                buttonText.text = "Found no object";
            }
        }

    }


    private void OnDestroy()
    {
        MLInput.Stop();
        MLInput.OnControllerButtonDown -= OnButtonDown;
    }
}
