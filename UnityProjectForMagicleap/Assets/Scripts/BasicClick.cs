using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Controller script that allows to set certain GameObjects active/inactive (and in turn connect to different servers)
    /// and record coordinates
    /// </summary>
    [RequireComponent(typeof(MLControllerConnectionHandlerBehavior))]
    public class BasicClick : MonoBehaviour
    {
        #region Private Variables
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler;

        private int _lastLEDindex = -1;
        #endregion

        public SendAndReceiveClient networkManager;
        public GameObject imgTracker;
        public GameObject rsCube;

        #region Const Variables
        private const float TRIGGER_DOWN_MIN_VALUE = 0.2f;

        // UpdateLED - Constants
        private const float HALF_HOUR_IN_DEGREES = 15.0f;
        private const float DEGREES_PER_HOUR = 12.0f / 360.0f;

        private const int MIN_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock12);
        private const int MAX_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock6And12);
        private const int LED_INDEX_DELTA = MAX_LED_INDEX - MIN_LED_INDEX;

        private bool isConnected = false;

        private bool isInteracting = false; // checks whether the cursor is over an UI object like a button
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
                    controller.StartFeedbackPatternLED((MLInputControllerFeedbackPatternLED)index, MLInputControllerFeedbackColorLED.BrightCosmicPurple, 0);
                    _lastLEDindex = index;
                }
            }
            else if (_lastLEDindex != -1)
            {
                controller.StopFeedbackPatternLED();
                _lastLEDindex = -1;
            }
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

            // bumper to connect to calibration server if not yet connected
            if (controller != null && controller.Id == controllerId &&
                button == MLInput.Controller.Button.Bumper)
            {
                // currently unused 
            }


        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnButtonUp(byte controllerId, MLInput.Controller.Button button)
        {
            // hometap to disconnect from calibration server if connected
            // and to connect to sending server if not connected
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;

            if (controller != null && controller.Id == controllerId &&
                button == MLInput.Controller.Button.HomeTap)
            {
                if (networkManager.checkConnection())
                {
                    networkManager.CalibDone();
                    imgTracker.SetActive(false);
                    rsCube.SetActive(true);
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
            // measure pose of ImageTracker and send to calibration server
            MLInput.Controller controller = _controllerConnectionHandler.ConnectedController;
            if (controller != null && controller.Id == controllerId)
            {
                if (!isInteracting)
                {
                    // if still in Calibration mode
                    if (networkManager.checkConnection())
                        networkManager.sendCoordinates();
                }
            }
        }


        public void TriggerUI()
        {
                isInteracting = true;
        }

        public void TriggerNoUI()
        {
                isInteracting = false;
        }



        #endregion
    }
}
