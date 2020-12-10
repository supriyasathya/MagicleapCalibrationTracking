using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch3DModel : MonoBehaviour {

    
    private bool m_dtiOn = false; // bool to check, which dataset is active
    private bool m_vesOn = false; // bool to check, which dataset is active

    [SerializeField, Tooltip("The DTI dataset")]
    private GameObject dtiObj;
    [SerializeField, Tooltip("The vasculature dataset")]
    private GameObject vesObj;

    public void changeBrainObject()
    {
        if (m_dtiOn == true && m_vesOn == false)
            {
                dtiObj.SetActive(false);
                m_dtiOn = false;
                vesObj.SetActive(false);
                m_vesOn = false;
            }
            else if (m_dtiOn == false && m_vesOn == true)
            {
                dtiObj.SetActive(true);
                m_dtiOn = true;
                vesObj.SetActive(false);
                m_vesOn = false;
            }
            else if (m_dtiOn == false && m_vesOn == false)
            {
                dtiObj.SetActive(false);
                m_dtiOn = false;
                vesObj.SetActive(true);
                m_vesOn = true;
            }
    }
}
