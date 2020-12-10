using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSplitButton : MonoBehaviour {

    public Animator anim;
	
	// Update is called once per frame
	public void ClickSplitButton ()
    {
        anim.SetBool("isClicked", true);
    }
}
