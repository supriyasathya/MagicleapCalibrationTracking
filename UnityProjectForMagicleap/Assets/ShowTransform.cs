using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTransform : MonoBehaviour
{
	Matrix4x4 matTRS = new Matrix4x4();
    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown("space"))
		{
		matTRS = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
		Debug.Log("MarkerTransform " + matTRS.ToString());
		}
    }
}
