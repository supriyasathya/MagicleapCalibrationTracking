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

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles visibility on image tracking, displaying and hiding prefabs
    /// when images are detected or lost.
    /// 
    /// cwule
    /// It attaches the head object to the imagetracker, so that imagetracker can be used to update the head position
    /// Changed to ImageTrackingVisualizer_Attach, this script ImageTrackingVisualizer_min is not used anymore.
    /// </summary>
    [RequireComponent(typeof(Core.MLImageTrackerBehavior))]
    public class ImageTrackingVisualizer_min : MonoBehaviour
    {
        #region Private Variables
        private Core.MLImageTrackerBehavior _trackerBehavior = null;
        private bool _targetFound = false;
        private bool isAttached = false;
        private bool isLost = false;

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
    }
}
