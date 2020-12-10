using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeRed : MonoBehaviour {

    public void makeButtonRed()
    {
        if (this.GetComponent<Renderer>())
            this.GetComponent<Renderer>().material.color = Color.red;
    }

    public void makeButtonYellow()
    {
        if (this.GetComponent<Renderer>())
            this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void makeButtonWhite()
    {
        if (this.GetComponent<Renderer>())
            this.GetComponent<Renderer>().material.color = Color.white;
    }
}
