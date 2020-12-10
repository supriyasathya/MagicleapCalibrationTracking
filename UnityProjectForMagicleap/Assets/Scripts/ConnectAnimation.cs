using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectAnimation : MonoBehaviour {

    public AudioClip buttonSound;
    public SendAndReceiveClient SRClient;

    public void toggleConnectButton()
    {
        //if (this.GetComponent<Animator>().GetBool("isConnected") == true)
        if (!SRClient.checkConnection())
        {
            SRClient.Connect2Server();
            this.GetComponent<AudioSource>().PlayOneShot(buttonSound, 0.5f);
            this.GetComponent<Animator>().SetBool("isConnected", true);
        }
        else
        {
            SRClient.DisconnectClient();
            this.GetComponent<AudioSource>().PlayOneShot(buttonSound, 0.5f);
            this.GetComponent<Animator>().SetBool("isConnected", false);
        }

    }
}
