using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///   calculate the root mean square error between the fiducials pre-placed on a virtual object (eg. head)
/// and the fiducials placed manually in the real world with a controller
/// </summary>

public class calcAlignmentRMS : MonoBehaviour {

    private double rms = 0.0f;

    private Vector3[] headFidsArr = new Vector3[5];

    [SerializeField, Tooltip("The text field that will display the RMS.")]
    private GameObject disp;

    [SerializeField, Tooltip("The Audio clips played after registration calculation.")]
    private AudioClip rmsGood, rmsBad;

    [SerializeField, Tooltip("The desired alignment accuracy in mm.")]
    private int rms_acc = 8;


    // the method for calculating the RMS, takes both fiducial containers as input
    public double measureRMS(GameObject fidContainer, GameObject headContainer)
    {
        // fill array with positions of headContainer
        List<Vector3> headFids = new List<Vector3>();
        foreach (Transform fidTrans in headContainer.GetComponentInChildren<Transform>())
        {
            headFids.Add(fidTrans.position);
        }
        headFidsArr = headFids.ToArray();

        // measure distance of each placed fiducial to headFiducials
        int i = 0;
        foreach (Transform fidTrans in fidContainer.GetComponentInChildren<Transform>())
        {
            rms += Vector3.Distance(fidTrans.position, headFidsArr[i])*1000;
            i++;
        }

        // round the average rms to 3 digits and display on text field
        rms = System.Math.Round(rms/5, 1);
        Text rmsDisp = disp.GetComponent<Text>();
        rmsDisp.text = "Alignment accuracy: RMS = " + rms.ToString() + " mm";

        // change color 
        if (rms < rms_acc)
        {
            rmsDisp.color = Color.green;
            if (rmsGood)
             headContainer.GetComponent<AudioSource>().PlayOneShot(rmsGood, 0.5f);
        }
        else
        {
            rmsDisp.color = Color.red;
            if (rmsBad)
                headContainer.GetComponent<AudioSource>().PlayOneShot(rmsBad, 0.5f);
        }
        return rms;
    }

}
