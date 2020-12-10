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
    public class PlaceMarkerAlignOnly : MonoBehaviour
    {
        #region Private Variables
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler;
        private int _lastLEDindex = -1;

        private int m_fidnum; // the number of fiducials in the scene
        private Vector3[] m_vecArray; // array containing the positions of all markers
        private int m_markerCount = 0; // counts the number of fiducials currently placed

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
        //private GameObject rsCube;
        //[SerializeField, Tooltip("The streaming client script")]
        //private SendAndReceiveClient networkManager; /// no need for registration check


        [Header("Fiducials")]
        [SerializeField, Tooltip("Empty Container that will contain all the markers")]
        private GameObject fids_RealWorld;
        [SerializeField, Tooltip("The head surface containing the fiducials")]
        private GameObject fids_VirtModel;

        [Header("Messages")]
        [SerializeField, Tooltip("The canvas with the manual")]
        //private GameObject canvasManual; //// delete this and stuff that depends
        //[SerializeField, Tooltip("The text field for error messages")]
        private GameObject errorBoard;

        [Header("Sound effects")]
        [SerializeField, Tooltip("The sounds when placing a marker")]
        private AudioClip placeMarkSound;
        [SerializeField, Tooltip("The sounds when clicking reset")]
        private AudioClip clickReset;
        [SerializeField, Tooltip("The sounds when confirming reset")]
        private AudioClip doReset;

        [Header("Tracking")]
        [SerializeField, Tooltip("The Tracking Manager")]
        private TrackingManager trackMan; ////
        //[SerializeField, Tooltip("The Tracking Manager")]
        //private GameObject trackingObj;

        #endregion

        #region Const Variables
        private const float TRIGGER_DOWN_MIN_VALUE = 0.2f;

        // UpdateLED - Constants
        private const float HALF_HOUR_IN_DEGREES = 15.0f;
        private const float DEGREES_PER_HOUR = 12.0f / 360.0f;

        private const int MIN_LED_INDEX = (int)(MLInput.Controller.FeedbackPatternLED.Clock12);
        private const int MAX_LED_INDEX = (int)(MLInput.Controller.FeedbackPatternLED.Clock6And12);
        private const int LED_INDEX_DELTA = MAX_LED_INDEX - MIN_LED_INDEX;
        #endregion

        #region public methods


        // changes the reset button to confirm or resets the scene (remove fiducials) after pressing reset button
        public void ResetScene()
        {
            // check if reset button has been already pressed
            if (m_buttonConfirmed)
            {
                // this scene reload unfortunately leads to errors such as the button being not clickable after one or more reloads
                //Scene scene = SceneManager.GetActiveScene();
                //SceneManager.LoadScene(scene.name);

                //// delete all fiducials
                foreach (Transform child in fids_RealWorld.transform)
                {
                    Destroy(child.gameObject);
                }
                m_markerCount = 0;
                for (int cnt = 0; cnt < m_fidnum; cnt++)
                {
                    m_vecArray[cnt] = new Vector3(0.0f, 0.0f, 0.0f);
                }
                fids_VirtModel.transform.parent = null;
                //if (imgTracker != null)
                //imgTracker.SetActive(false);
                //imgTracker.GetComponent<ImageTrackingVisualizer_Attach>().DetachHead();
                trackMan.DetachHead();

               // Component[] springJoints = fids_VirtModel.GetComponents(typeof(SpringJoint)); ////

                //GameObject placeHolder = new GameObject();
               // foreach (SpringJoint joint in springJoints) ////
               // {
                  //  joint.spring = 0;
                    //joint.connectedBody = null;
                //}
                m_buttonConfirmed = false;
                m_registrationDone = false;

                GetComponent<AudioSource>().PlayOneShot(doReset, 0.5f);

                // reset onscreen manual
                //canvasManual.transform.Find("PlaceFids").gameObject.SetActive(true);////
                //canvasManual.transform.Find("Register").gameObject.SetActive(false);////
                //canvasManual.transform.Find("SwitchModel").gameObject.SetActive(false);////

                errorBoard.GetComponent<Text>().text = "";
            }
            else
            {
                m_buttonConfirmed = true;
                GetComponent<AudioSource>().PlayOneShot(clickReset, 0.5f);

                // start coroutine to wait for 5 seconds before reset button will be returned to default state
                StartCoroutine(waitConfirmTime(0.0f));
            }
        }

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
            m_fidnum = fids_VirtModel.transform.childCount;
            m_vecArray = new Vector3[m_fidnum];
        }

        /// <summary>
        /// Update controller input based feedback.
        /// </summary>
        void Update()
        {
            UpdateLED();
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
                //if (imgTracker.activeInHierarchy == false)
                //if (m_calibrationDone == true && m_registrationDone == false)
                //{
                  //  RemoveMarker(); ////
                    //if (m_markerCount < m_fidnum)////
                    //{
                      //  canvasManual.transform.Find("PlaceFids").gameObject.SetActive(true);////
                        //canvasManual.transform.Find("Register").gameObject.SetActive(false);////
                    //}
                //}
                // once registration is done, the bumper attaches the head model to the imageTracker
                if (m_calibrationDone == true && m_registrationDone == true && trackMan.getAttachStat() == true)
                {
                    //imgTracker.GetComponent<ImageTrackingVisualizer_Attach>().AttachHead();
                    trackMan.AttachHead();
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
                Debug.Log("markerCount" + m_markerCount.ToString());
                // if fiducials placed && registration not yet performed -> register
                // check if enough fiducials placed and if the registration has not been performed before
                if (m_markerCount == m_fidnum && m_registrationDone == false)
                {
                    fids_VirtModel.GetComponent<Collider>().enabled = false;
                    manager.GetComponent<calcTransform>().PerformRegistration();
                    trackMan.InitialAttachHead();
                    m_registrationDone = true;
                //    canvasManual.transform.Find("Register").gameObject.SetActive(false);////
                  //  canvasManual.transform.Find("SwitchModel").gameObject.SetActive(true);////
                }
                // once registration has been performed, the button switches between imaging modalities
                else if (m_markerCount == m_fidnum && m_registrationDone == true)
                {
                    manager.GetComponent<Switch3DModel>().changeBrainObject();////
                }
                // if calibration not yet done -> start receiving
                //else if (!m_calibrationDone)
                //{
                    //if (networkManager.checkConnection())
                    //{
                      //  Debug.Log("CalibDone");
                        //networkManager.CalibDone();
                        //m_calibrationDone = true;
                        //imgTracker.SetActive(false);
                        //if (trackingObj != null)
                        //trackMan.AttachHead(trackingObj);
                    //}
                //}
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
                    // if Attached (nothing more to do)
                    // if calibrated or no networkmanager (place fiducials)
                    //if (m_calibrationDone == null)// || networkManager == null)
                    {
                        if (m_markerCount < m_fidnum)
                        {
                            CreateMarker();
                            m_markerCount += 1;
                            //if (m_markerCount == m_fidnum)
                            //{
                               // canvasManual.transform.Find("PlaceFids").gameObject.SetActive(false);/////
                                //canvasManual.transform.Find("Register").gameObject.SetActive(true);////
                           // }
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
                  //  else if (networkManager != null) ////remove for only registration
                    //{
                        // if still in Calibration mode
                      //  if (networkManager.checkConnection())
                        //    networkManager.sendCoordinates();
                   // }
                }
            }
        }

        public void TriggerUI()
        {
            m_isInteracting = true;
        }

        public void TriggerNoUI()
        {
            m_isInteracting = false;
        }


        #endregion
    }
}