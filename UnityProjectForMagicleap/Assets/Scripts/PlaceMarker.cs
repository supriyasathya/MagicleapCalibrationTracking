// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
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
    public class PlaceMarker : MonoBehaviour
    {
        #region Private Variables
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler;
        private int _lastLEDindex = -1;

        private int m_fidnum; // the number of fiducials in the scene
        private Vector3[] m_vecArray; // array containing the positions of all markers
        private int m_markerCount = 0; // counts the number of fiducials currently placed

        private float[] floatArray = new float[16];
        //private bool m_noFids = false; // Bool that prevents marker placement when dragging or selecting buttons
        private bool m_buttonConfirmed = false; // counts whether the reset button has been pressed
        private bool m_isInteracting = false;
        private bool m_calibrationDone = false;
        private bool m_registrationDone = false;
        #endregion

        #region Public Variables
        [SerializeField, Tooltip("The smallest of the blue controller spheres")]
        private GameObject cursorSphere;
        [SerializeField, Tooltip("GameObject that contains the registration script")]
        private GameObject manager;
        //[SerializeField, Tooltip("The imageTracker that will be attached to the head")]
        [SerializeField, Tooltip("The streaming client script")]
        private SendAndReceiveClient networkManager; /// no need for registration check


        [Header("Fiducials")]
        [SerializeField, Tooltip("Empty Container that will contain all the markers")]
        private GameObject fids_RealWorld;
        [SerializeField, Tooltip("The head surface containing the fiducials")]
        private GameObject fids_VirtModel;

        [Header("Messages")]
        [SerializeField, Tooltip("The canvas with the manual")]
        //[SerializeField, Tooltip("The text field for error messages")]
        private GameObject errorBoard;
        // following statements are for IPD test scene where the IPD will be displayed at different locations where the 
        // text field is placed
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp1;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp2;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp3;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp4;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp5;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp6;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp7;
        [SerializeField, Tooltip("The text field that will display the IPD.")]
        private GameObject IPDdisp8;


        [Header("Sound effects")]
        [SerializeField, Tooltip("The sounds when placing a marker")]
        private AudioClip placeMarkSound;
        [SerializeField, Tooltip("The sounds when clicking reset")]
        private AudioClip clickReset;
        [SerializeField, Tooltip("The sounds when confirming reset")]
        private AudioClip doReset;


        #endregion
        // Initialize variables for extracting IPD.
        Vector3 leftEye;
        Vector3 rightEye;
        float interpupillaryDist;

        private static PlaceMarker instance;
        public static PlaceMarker Instance
        {
            get
            {
                return instance;
            }
        }

        #region Const Variables
        private const float TRIGGER_DOWN_MIN_VALUE = 0.2f;

        // UpdateLED - Constants
        private const float HALF_HOUR_IN_DEGREES = 15.0f;
        private const float DEGREES_PER_HOUR = 12.0f / 360.0f;

        private const int MIN_LED_INDEX = (int)(MLInput.Controller.FeedbackPatternLED.Clock12);
        private const int MAX_LED_INDEX = (int)(MLInput.Controller.FeedbackPatternLED.Clock6And12);
        private const int LED_INDEX_DELTA = MAX_LED_INDEX - MIN_LED_INDEX;
        #endregion

        #region Serialized Variables
        [SerializeField, Tooltip("Game Object showing the tracking cube")]
        private GameObject _trackingCube;
        //[SerializeField, Tooltip("The Head object that will be attached to the Image Tracker")]
       // private GameObject _trackingHead;
        #endregion


        #region public methods


        
        // waits for 5 seconds and then resets the m_buttonConfirmed to false
        IEnumerator waitConfirmTime(float waitTime)
        {
            while (waitTime < 5.1f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
            m_buttonConfirmed = false;
            yield return null;
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initialize variables, callbacks and check null references.
        /// </summary>
        void Start()
        {
            _controllerConnectionHandler = GetComponent<MLControllerConnectionHandlerBehavior>();

            MLInput.OnControllerButtonUp += HandleOnButtonUp;
            MLInput.OnControllerButtonDown += HandleOnButtonDown;
            MLInput.OnTriggerDown += HandleOnTriggerDown;
            MLInput.OnControllerTouchpadGestureStart += HandleOnTouchPadGestureStart;

            m_fidnum = fids_VirtModel.transform.childCount;
            m_vecArray = new Vector3[m_fidnum];
            MLEyes.Start();
        }

        /// <summary>
        /// Update controller input based feedback.
        /// </summary>
        void Update()
        {
            UpdateLED();
            leftEye = MLEyes.LeftEye.Center;
            rightEye = MLEyes.RightEye.Center;

            // Calculate interpupillary distance.
            interpupillaryDist = Vector3.Distance(leftEye, rightEye) * 1000; // in mm

            Text ipdtext1 = IPDdisp1.GetComponent<Text>();
            ipdtext1.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext2 = IPDdisp2.GetComponent<Text>();
            ipdtext2.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext3 = IPDdisp3.GetComponent<Text>();
            ipdtext3.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext4 = IPDdisp4.GetComponent<Text>();
            ipdtext4.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext5 = IPDdisp5.GetComponent<Text>();
            ipdtext5.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext6 = IPDdisp6.GetComponent<Text>();
            ipdtext6.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext7 = IPDdisp7.GetComponent<Text>();
            ipdtext7.text = "IPD = " + interpupillaryDist.ToString();
            Text ipdtext8 = IPDdisp8.GetComponent<Text>();
            ipdtext8.text = "IPD = " + interpupillaryDist.ToString();
        }

        /// <summary>
        /// Stop input api and unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnTriggerDown -= HandleOnTriggerDown;
                MLInput.OnControllerButtonDown -= HandleOnButtonDown;
                MLInput.OnControllerButtonUp -= HandleOnButtonUp;
                MLInput.OnControllerTouchpadGestureStart -= HandleOnTouchPadGestureStart;

            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates LED on the physical controller based on touch pad input.
        /// </summary>
        private void UpdateLED()
        {
            if (!_controllerConnectionHandler.IsControllerValid())
            {
                return;
            }

            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;
            if (controller.Touch1Active)
            {
                // Get angle of touchpad position.
                float angle = -Vector2.SignedAngle(Vector2.up, controller.Touch1PosAndForce);
                if (angle < 0.0f)
                {
                    angle += 360.0f;
                }

                // Get the correct hour and map it to [0,6]
                int index = (int)((angle + HALF_HOUR_IN_DEGREES) * DEGREES_PER_HOUR) % LED_INDEX_DELTA;

                // Pass from hour to MLInputControllerFeedbackPatternLED index  [0,6] -> [MAX_LED_INDEX, MIN_LED_INDEX + 1, ..., MAX_LED_INDEX - 1]
                index = (MAX_LED_INDEX + index > MAX_LED_INDEX) ? MIN_LED_INDEX + index : MAX_LED_INDEX;

                if (_lastLEDindex != index)
                {
                    // a duration of 0 means leave it on indefinitely
                    controller.StartFeedbackPatternLED((MLInput.Controller.FeedbackPatternLED)index, MLInput.Controller.FeedbackColorLED.BrightCosmicPurple, 0);
                    _lastLEDindex = index;
                }
            }
            else if (_lastLEDindex != -1)
            {
                controller.StopFeedbackPatternLED();
                _lastLEDindex = -1;
            }
        }

        // creates a fiducial after trigger is pressed
        private void CreateMarker()
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // make sphere
            sphere.transform.parent = fids_RealWorld.transform; // !!! set parent before changin position, otherwise collider error! Unity 2018.1.9 BUG
            sphere.transform.localScale = Vector3.one * 0.01f; // scale sphere down
            sphere.transform.position = cursorSphere.transform.position; // position sphere at cursor position
            sphere.gameObject.layer = 2; // put in ignore raycast layer

            // look for lowest (0,0,0) Vector and replace it by new vector
            for (int cnt = 0; cnt < m_fidnum; cnt++)
            {
                if (m_vecArray[cnt] == new Vector3(0.0f, 0.0f, 0.0f))
                {
                    m_vecArray[cnt] = sphere.transform.position; // put sphere coordinates in array
                    break;
                }
            }
            GetComponent<AudioSource>().PlayOneShot(placeMarkSound, 0.3f);
        }



        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>

        private void HandleOnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;

            // bumper to delete fiducials that intersect with cursor
            if (controller != null && controller.Id == controllerId &&
                button == MLInput.Controller.Button.Bumper)
            {
                errorBoard.GetComponent<Text>().text = "Came inside Bumper code";
                
                    if (networkManager != null) ////remove for only registration
                    {
                        // if still in Calibration mode
                        if (networkManager.checkConnection())
                            errorBoard.GetComponent<Text>().text = "Bumper working";
                            networkManager.sendCoordinatesMarker2();
                    }
               

            }


        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnButtonUp(byte controllerId, MLInput.Controller.Button button)
        {
            // hometap not recognized when deploying the app
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;

            if (controller != null && controller.Id == controllerId &&
                button == MLInput.Controller.Button.HomeTap)
            {
                // Debug.Log("markerCount" + m_markerCount.ToString());
                // if fiducials placed && registration not yet performed -> register
                // check if enough fiducials placed and if the registration has not been performed before
                if (m_markerCount == m_fidnum && m_registrationDone == false)
                {
                    fids_VirtModel.GetComponent<Collider>().enabled = false;
                    m_registrationDone = true;
                    errorBoard.GetComponent<Text>().text = "Before changing Parent";
                    manager.GetComponent<calcTransform>().PerformRegistration();

                 
                }
                // once registration has been performed, the button switches between imaging modalities
                // else if (m_markerCount == m_fidnum && m_registrationDone == true)
                // {
                //     manager.GetComponent<Switch3DModel>().changeBrainObject();////
                //  }

                // if both registration and calibration are done, then home tap will attach the virtual head model to the tracking parent
                else if (m_registrationDone == true && m_calibrationDone == true)
                {
                    errorBoard.GetComponent<Text>().text = "Changing Parent";
                    //trackMan.InitialAttachHead();
                    fids_VirtModel.transform.parent = _trackingCube.transform;
                    Debug.Log("Tracking Cube transform" + _trackingCube.transform.position.ToString());
                    Debug.Log("Tracking hed transform" + fids_VirtModel.transform.parent.transform.position.ToString());
                    fids_VirtModel.GetComponent<Collider>().enabled = true;

                }

                // if calibration is done but the flag is not set, then set the m_calibrationDone flag to True
                else if (!m_calibrationDone)
                {
                    if (networkManager.checkConnection())
                    {
                        print("CalibDone");
                        networkManager.CalibDone();
                        m_calibrationDone = true;
                       
                    }
                }
            }

        }

        /// <summary>
        /// Handles the event for trigger down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="value">The value of the trigger button.</param>
        private void HandleOnTriggerDown(byte controllerId, float value)
        {
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;
            if (controller != null && controller.Id == controllerId)
            {
                // if fiducial placement allowd, place a fiducial
                if (!m_isInteracting)
                {
                    // if calibrated and connected to client, place the virtual fiducials
                    if (m_calibrationDone && networkManager.checkConnection()) 
                    {
                        if (m_markerCount < m_fidnum)
                        {
                            CreateMarker();
                            m_markerCount += 1;
                            
                        }
                        else
                        {
                            if (errorBoard)
                            {
                                errorBoard.GetComponent<Text>().text = "Already " + m_fidnum + " fiducials placed!";
                            }

                        }
                    }
                    // if not calibrated but connected (collect calibration samples)
                    else if (networkManager != null) ////remove for only registration
                    {
                        // if still in Calibration mode
                        if (networkManager.checkConnection())
                        {
//                            errorBoard.GetComponent<Text>().text = "Trigger working";
                            networkManager.sendCoordinates();

                        }
                    }
                }
            }

        }
        private void HandleOnTouchPadGestureStart(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesturee)
        {
            //touchpad usage - for writing the pose of child of the tracking parent when touchpad is tapped. This is for accuracy measurement purposes: the pose of the 
            // child (the virtual rendering of the head for eg.) of the tracking parent is logged before and after it is aligned with the real-world
            // head (if alignment is needed) and the difference in pose is noted).
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;
            if (controller != null && controller.Id == controllerId)
            {                
                errorBoard.GetComponent<Text>().text = _trackingCube.transform.position.ToString();
                Matrix4x4 TRScube = Matrix4x4.TRS(_trackingCube.transform.position, _trackingCube.transform.rotation, _trackingCube.transform.localScale);
                // convert Matrix4x4 to 2D array
                floatArray = TRS2Array(TRScube);
                Matrix4x4 TRScube_tp = TRScube.transpose;
                Debug.Log("TouchPad - tracked child pose" + floatArray[0].ToString() + "," + floatArray[1].ToString() + "," + floatArray[2].ToString() + "," + floatArray[3].ToString() + "," + floatArray[4].ToString() + "," + floatArray[5].ToString() + "," + floatArray[6].ToString() + "," + floatArray[7].ToString() + "," + floatArray[8].ToString()  + "," + floatArray[9].ToString() + "," + floatArray[10].ToString() + "," + floatArray[11].ToString() + "," + floatArray[12].ToString() + "," + floatArray[13].ToString() + "," + floatArray[14].ToString() + "," + floatArray[15].ToString());
                // Once the pose of the tracked child is logged, reset the child's pose to (0,0,0) with respect to the trakcing parent.
               
                _trackingCube.transform.localPosition = new Vector3(0, -0.045f, 0);
                _trackingCube.transform.localRotation = Quaternion.Euler(0, 0, 0);
 
           }
        }
        
        //convert Matrix4x4 to 2D array
        static float[] TRS2Array(Matrix4x4 matTRS)
        {
            float[] matArr = new float[16];
            matArr[0] = matTRS.m00;
            matArr[1] = matTRS.m01;
            matArr[2] = matTRS.m02;
            matArr[3] = matTRS.m03;
            matArr[4] = matTRS.m10;
            matArr[5] = matTRS.m11;
            matArr[6] = matTRS.m12;
            matArr[7] = matTRS.m13;
            matArr[8] = matTRS.m20;
            matArr[9] = matTRS.m21;
            matArr[10] = matTRS.m22;
            matArr[11] = matTRS.m23;
            matArr[12] = matTRS.m30;
            matArr[13] = matTRS.m31;
            matArr[14] = matTRS.m32;
            matArr[15] = matTRS.m33;
            return matArr;
        }


        #endregion
    }
}