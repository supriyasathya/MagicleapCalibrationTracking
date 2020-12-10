using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap
{

    public class ControllerScriptManager : MonoBehaviour
    {

        [SerializeField, Tooltip("The Magic Leap controller")]
        private GameObject controllerObject;
        [SerializeField, Tooltip("Streaming on of off")]
        private bool externalTracking;


        void Start()
        {
            if (!externalTracking)
                changeController();
        }

        // Update is called once per frame
        public void changeController()
        {
                controllerObject.GetComponent<BasicClick>().enabled = false;

                controllerObject.GetComponent<PlaceMarker>().enabled = true;

                controllerObject.GetComponent<ContentDragController>().enabled = true;
        }
    }
}