using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAnimation : MonoBehaviour {

    public Animator anim;

    // Update is called once per frame
    public void startAnimation()
    {
        anim.SetTrigger("isClicked");
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("New State"))
        {
            StartCoroutine(waitConfirmTime(0.0f));
        }
    }

    IEnumerator waitConfirmTime(float waitTime)
    {
        while (waitTime < 5.1f)
        {
            waitTime += Time.deltaTime;
            anim.SetFloat("cancelTime", waitTime);
            yield return null;
        }

    }
}
