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
    /// </summary>
    [RequireComponent(typeof(Core.MLImageTrackerBehavior))]
    public class ImageTrackingVisualizer_Attach : MonoBehaviour
    {
        #region Private Variables
        private Core.MLImageTrackerBehavior _trackerBehavior;
        private bool _targetFound = false;
        private bool _isAttached = false;
        //private bool _isLost = false; // to detach Head when tracker is lost, currently not used

        //private Vector3 _tmp_Position;
        //private Quaternion _tmp_Rotation;

        // List of positions that will be used to attach the marker
        //private List<Vector3> _posList = new List<Vector3>();
        //private List<Vector3> _rotList = new List<Vector3>();

        // when attaching the head use the first $_filterCounter frames to set position
        //private int _filterCounter = 0;
        //private int _filterMax = 20;
        #endregion

        #region Serialized Variables
        [SerializeField, Tooltip("Game Object showing the tracking cube")]
        private GameObject _trackingCube;
        [SerializeField, Tooltip("The tracking Manager")]
        private TrackingManager attachMan;
        //[SerializeField, Tooltip("The Head object that will be attached to the Image Tracker")]
        //private GameObject _trackingHead;
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

            // check if HEad already attached to ImageTracker, if not do attach, to get relative coords, then unattach
            if (_isAttached == false)
            {
                //StartCoroutine(InitialAttachHead());
                _isAttached = attachMan.InitialAttachHead();
                _isAttached = true;
            }

            //if (_isLost == true)
            //{
            //    _trackingHead.transform.parent = this.transform;
            //    _trackingHead.transform.localRotation = _tmp_Rotation;
            //    _trackingHead.transform.localPosition = _tmp_Position;
            //    _isLost = false;
            //}
        }

        /// <summary>
        /// Callback for when image tracked is lost
        /// </summary>
        private void OnTargetLost(MLImageTracker.Target target, MLImageTracker.Target.Result result)//()
        {
            _targetFound = false;
            _trackingCube.SetActive(false);

            // if tracking is lost, save local pos and rot and deattach from ImageTracker
            //if (_isAttached == true && _isLost == false)
            //{
            //    _trackingHead.transform.parent = null;
            //    _isLost = true;
            //}
        }


        // Coroutine that runs every frame once the target is found
        // if no Coroutine is used here, this is just run once when TargetFound
        //IEnumerator InitialAttachHead()
        //{
        //    // //old, before filtering
        //    // attach head to imagetracker to get localPosition and localRotation
        //    _trackingHead.transform.parent = this.transform;
           
        //    //_tmp_Position = _trackingHead.transform.localPosition;
        //    //_tmp_Rotation = _trackingHead.transform.localRotation;
        //    //_isAttached = true;
        //    //_trackingHead.transform.parent = null;



        //    // new, with filtering
        //    while (_filterCounter < _filterMax)
        //    {
        //        // get current frame pose
        //        // save pose in List<Vector3>
        //        _posList.Add(_trackingHead.transform.localPosition);
        //        _rotList.Add(_trackingHead.transform.localRotation.eulerAngles);
        //        _filterCounter += 1;
        //        yield return null;
        //    }

        //    // measure mean location and rotation of list
        //    _tmp_Position = meanLocation(_posList);
        //    _tmp_Rotation = Quaternion.Euler(meanLocation(_rotList));
        //    _isAttached = true;
        //    Debug.Log("_isAttached");
        //    _trackingCube.GetComponent<Renderer>().material.color = Color.white;
        //    _trackingHead.transform.parent = null;
        //    yield return null;

        //}

        //static Vector3 meanLocation(List<Vector3> positionList)
        //{
        //    Vector3 avgPos = Vector3.zero;
        //    foreach (Vector3 vec in positionList)
        //    {
        //        avgPos += vec;
        //    }
        //    avgPos = avgPos / positionList.Count;
        //    return avgPos;
        //}

        //public void AttachHead()
        //{
        //    if (_trackingHead.transform.parent == null)
        //    {
        //        _trackingHead.transform.parent = this.transform;
        //        _trackingHead.transform.localPosition = _tmp_Position;
        //        _trackingHead.transform.localRotation = _tmp_Rotation;
        //    }
        //    else
        //    {
        //        _trackingHead.transform.parent = null;
        //    }
        //}

        //public void DetachHead()
        //{
        //    _trackingHead.transform.parent = null;
        //    _isAttached = false;
        //    _filterCounter = 0;
        //    _posList.Clear();
        //    _rotList.Clear();
        //    _trackingCube.GetComponent<Renderer>().material.color = Color.red;
        //}
        #endregion
    }
}
