using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowIP : MonoBehaviour {

    public SendAndReceiveClient srClient;

	// Use this for initialization
	void Start () {
        this.GetComponent<Text>().text = "Server address " + srClient.getIP();
	}
	
}
