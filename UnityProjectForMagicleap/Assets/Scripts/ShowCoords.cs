using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles visibility on image tracking, displaying and hiding prefabs
    /// when images are detected or lost.
    /// </summary>
    [RequireComponent(typeof(Core.MLImageTrackerBehavior))]
    public class ShowCoords : MonoBehaviour
    {
        #region Private Variables
        private Core.MLImageTrackerBehavior _trackerBehavior;
        private bool _targetFound = false;
        private bool isAttached = false;
        private bool isLost = false;
        //private Vector3 tmp_Position;
        //private Quaternion tmp_Rotation;

        [SerializeField, Tooltip("Game Object showing the tracking cube")]
        private GameObject _trackingCube;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate inspector variables
        /// </summary>
        void Awake()
        {
            if (null == _trackingCube)
            {
                Debug.LogError("Error: ImageTrackingVisualizer._trackingCube is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Initializes variables and register callbacks
        /// </summary>
        void Start()
        {
            _trackerBehavior = GetComponent<Core.MLImageTrackerBehavior>();
            _trackerBehavior.OnTargetFound += OnTargetFound;
            _trackerBehavior.OnTargetLost += OnTargetLost;
        }

        private void Update()
        {

        }
        /// <summary>
        /// Unregister calbacks
        /// </summary>
        void OnDestroy()
        {
            _trackerBehavior.OnTargetFound -= OnTargetFound;
            _trackerBehavior.OnTargetLost -= OnTargetLost;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Callback for when tracked image is found
        /// </summary>
        /// <param name="isReliable"> Contains if image found is reliable </param>
        private void OnTargetFound(MLImageTracker.Target target, MLImageTracker.Target.Result result)//(bool isReliable)
        {
            _targetFound = true;
            _trackingCube.SetActive(true);
        }

        /// <summary>
        /// Callback for when image tracked is lost
        /// </summary>
        private void OnTargetLost(MLImageTracker.Target target, MLImageTracker.Target.Result result)//()
        {
            _targetFound = false;
            _trackingCube.SetActive(false);
        }
        #endregion

        // For Debugging, show TRS matrix
        public void GiveTransform()
        {
            Debug.Log(this.transform.position.ToString("F2"));
            Debug.Log(this.transform.rotation.eulerAngles.ToString("F2"));
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(this.transform.position, this.transform.rotation, this.transform.localScale);
            Debug.Log("TRS " + m.ToString("F2"));
        }
    }
}
