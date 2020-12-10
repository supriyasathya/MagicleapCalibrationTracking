using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;


namespace MagicLeap
{
    /// <summary>
    /// This class provides examples of how you can use haptics and LEDs
    /// on the Control.
    /// </summary>
    [RequireComponent(typeof(MLControllerConnectionHandlerBehavior))]
    public class testChangeParent : MonoBehaviour
    {
       // public GameObject cubeobj;
        // Start is called before the first frame update
        #region Private Variables
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler;
        [SerializeField, Tooltip("The text field for error messages")]
        private GameObject errorBoard;
        #endregion

        void Start()
        {
            //transform.parent = cubeobj.transform;
            errorBoard.GetComponent<Text>().text = "Bumper not working";
            _controllerConnectionHandler = GetComponent<MLControllerConnectionHandlerBehavior>();

            MLInput.OnControllerButtonDown += HandleOnButtonDown;
            MLInput.OnControllerButtonUp += HandleOnButtonUp;


        }

        // Update is called once per frame
        void Update()
        {

        }
        private void HandleOnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;

            // bumper to delete fiducials that intersect with cursor
            if (controller != null && controller.Id == controllerId &&
                button == MLInput.Controller.Button.Bumper)
            {
                errorBoard.GetComponent<Text>().text = "Bumper working";
                //transform.parent = cubeobj.transform;
            }
        }

        private void HandleOnButtonUp(byte controllerId, MLInput.Controller.Button button)
        {
            // hometap not recognized when deploying the app
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;

            if (controller != null && controller.Id == controllerId &&
                button == MLInput.Controller.Button.HomeTap)
            {
                errorBoard.GetComponent<Text>().text = "Home tap working";
                //transform.parent = cubeobj.transform;
            }



        }
    }
}
