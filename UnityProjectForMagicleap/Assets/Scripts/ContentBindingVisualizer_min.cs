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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This a utility class to help debug MLPersistentBehavior. This class listens to
    /// events from MLPersistentBehavior and displays them.
    /// </summary>
    [RequireComponent(typeof(MLPersistentBehavior), typeof(Collider))]
    public class ContentBindingVisualizer_min : MonoBehaviour
    {
        #region Private Variables

        [SerializeField, Tooltip("Highlight Effect")]
        GameObject _highlightEffect;

        Renderer[] _renderers;
        Collider _collider;
        ContentDragController _controllerDrag;
        #endregion

        #region Public Events
        /// <summary>
        /// Triggered when a controller touches trigger area
        /// </summary>
        public event Action OnHighlight;

        /// <summary>
        /// Triggered when a controller leaves trigger area
        /// </summary>
        public event Action OnUnhighlight;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate parameters, initialize renderers, and listen to events
        /// </summary>
        void Awake()
        {
            if (_highlightEffect == null)
            {
                Debug.LogError("Error: ContentBindingStatusText._highlightEffect is not set, disabling script");
                enabled = false;
                return;
            }
            _highlightEffect.SetActive(false);


            _renderers = GetComponentsInChildren<Renderer>();
            EnableRenderers(false);

            _collider = GetComponent<Collider>();
            _collider.enabled = false;
        }


        /// <summary>
        /// Controller touches this content
        /// </summary>
        /// <param name="other">Collider of Controller</param>
        private void OnTriggerEnter(Collider other)
        {
            ContentDragController controllerDrag = other.GetComponent<ContentDragController>();
            if (controllerDrag == null)
            {
                return;
            }

            _controllerDrag = controllerDrag;
            Highlight();
        }

        /// <summary>
        /// Controller leaves this content
        /// </summary>
        /// <param name="other">Collider of Controller</param>
        private void OnTriggerExit(Collider other)
        {
            ContentDragController controllerDrag = other.GetComponent<ContentDragController>();
            if (controllerDrag == null)
            {
                return;
            }

            if (_controllerDrag == controllerDrag)
            {
                _controllerDrag = null;
                Unhighlight();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Show visual effect when highlighting
        /// </summary>
        private void Highlight()
        {
            _highlightEffect.SetActive(true);

            if (OnHighlight != null)
            {
                OnHighlight();
            }
        }

        /// <summary>
        /// Remove highlight visual effects
        /// </summary>
        private void Unhighlight()
        {
            _highlightEffect.SetActive(false);

            if (OnUnhighlight != null)
            {
                OnUnhighlight();
            }
        }

        /// <summary>
        /// Enable/Disable Renderers
        /// </summary>
        /// <param name="enable">Toggle value</param>
        void EnableRenderers(bool enable)
        {
            foreach(Renderer r in _renderers)
            {
                r.enabled = enable;
            }
        }
        #endregion

    }

    internal class MLPersistentBehavior
    {
    }
}
