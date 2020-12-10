using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaycastButton : MonoBehaviour
{

    public GameObject buttonHalo;

    public Image buttonFrame;


    private void OnTriggerEnter(Collider other)
    {
        HighlightButton();
        buttonFrame.color = Color.red;
    }

    private void OnTriggerExit(Collider other)
    {
        UnHighlightButton();
        buttonFrame.color = Color.white;
    }

    void HighlightButton()
    {
        buttonHalo.SetActive(true);
    }

    void UnHighlightButton()
    {
        buttonHalo.SetActive(false);
    }

}

