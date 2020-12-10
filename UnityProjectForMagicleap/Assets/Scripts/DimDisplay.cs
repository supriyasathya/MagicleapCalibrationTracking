using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimDisplay : MonoBehaviour {

    public GameObject manCanvas;
    public AudioClip buttonSound;

    public void toggleAlpha()
    {
        if (this.GetComponent<Animator>().GetBool("isBright") == true)
        {
            manCanvas.SetActive(false);
            this.GetComponent<AudioSource>().PlayOneShot(buttonSound, 0.5f);
            this.GetComponent<Animator>().SetBool("isBright", false);
        }
        else
        {
            manCanvas.SetActive(true);
            this.GetComponent<AudioSource>().PlayOneShot(buttonSound, 0.5f);
            this.GetComponent<Animator>().SetBool("isBright", true);
        }

    }

}
