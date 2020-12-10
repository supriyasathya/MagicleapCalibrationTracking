using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkClosestBrainRegion : MonoBehaviour {

    public GameObject networkAS, networkDM, networkLECN, networkRECN;
    private GameObject networkActive, networkOld;
    private float minDistance;

    void Start()
    {
        networkOld = networkDM;
    }

    public GameObject measureDistance()
    {
        //Vector3 closestPointDM = networkDM.GetComponent<Collider>().ClosestPoint(this.transform.position);
        float distanceAS = Vector3.Distance(networkAS.GetComponent<Collider>().ClosestPoint(this.transform.position), this.transform.position);
        float distanceDM = Vector3.Distance(networkDM.GetComponent<Collider>().ClosestPoint(this.transform.position), this.transform.position);
        float distanceRECN = Vector3.Distance(networkRECN.GetComponent<Collider>().ClosestPoint(this.transform.position), this.transform.position);
        float distanceLECN = Vector3.Distance(networkLECN.GetComponent<Collider>().ClosestPoint(this.transform.position), this.transform.position);
        //Debug.Log(this.transform.position.ToString("F4")); // display 4 digit accuracy
        //Debug.Log(distanceAS.ToString("F4") + " " + distanceDM.ToString("F4") + " " + distanceRECN.ToString("F4") + " " + distanceLECN.ToString("F4"));
        minDistance = Mathf.Min(distanceAS, distanceDM, distanceRECN, distanceLECN);
        Debug.Log("distance" + minDistance);
        if (minDistance == distanceAS)
        {
            return networkAS;
        }
        else if (minDistance == distanceDM)
        {
            return networkDM;
        }
        else if (minDistance == distanceRECN)
        {
            return networkRECN;
        }
        else
        {
            return networkLECN;
        }
    }

    // Update is called once per frame
    void Update () {
        if (this.GetComponent<MeshRenderer>().enabled == true)
        {
            Debug.Log("Renderer enabled!!!!!!!!!!!!1");
            networkActive = measureDistance();
            if (minDistance < 0.03f)
            {
                Debug.Log("CLOSE ENOUGH!!!!!!!!!!!!!!!");
                if (networkActive == networkOld)
                {
                    return;
                }
                else
                {
                    //Debug.Log("else");
                    networkActive.GetComponent<MeshRenderer>().enabled = true;
                    networkOld.GetComponent<MeshRenderer>().enabled = false;
                    networkOld = networkActive;
                }
            }
        }
        else
        {
            networkOld.GetComponent<MeshRenderer>().enabled = false;
        }
	}
}
