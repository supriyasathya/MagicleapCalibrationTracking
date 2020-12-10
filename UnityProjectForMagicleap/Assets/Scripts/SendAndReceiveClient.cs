using MagicLeap;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script records the pose of the imageTracker tracked by the MagicLeap and 
/// sends the corresponding 4x4 transformation matrix to the server
/// After calibration it receives a 4x4 transformation matrix from the server to update the pose of a Gameobject
/// </summary>

public class SendAndReceiveClient : MonoBehaviour
{
    public TcpClient client;
    string receivedMessage;
    byte[] buf = new byte[256];

    // change to server address
    [SerializeField, Tooltip("The server IP address")]
    private string ipAddress = "192.168.1.163";//"10.0.0.184";//192.168.0.101";//"171.65.36.161";//"192.168.1.7";
    [SerializeField, Tooltip("The server port")]
    private int port = 2017;

    private bool debugOn = true	;

    private float[] floatArray1 = new float[16];

    [SerializeField, Tooltip("The GameObject that is activated if the ImageTracker is found")]
    private GameObject trackingObj;
    [SerializeField, Tooltip("The ImageTracker")]
    private GameObject imgTracker;
    [SerializeField, Tooltip("The GameObject that is activated if the second ImageTracker is found")]
    private GameObject trackingObj2;
    [SerializeField, Tooltip("The second ImageTracker")]
    private GameObject imgTracker2;
    [SerializeField, Tooltip("The GameObject that will be updated via the Realsense camera")]
    private GameObject rsCube;
    [SerializeField, Tooltip("Indicates the connection status")]
    private Text connectText;
    [SerializeField, Tooltip("The text field for error messages")]
    private GameObject errorBoard2;
    // for single GameObject (update position and rotation)
    private Matrix4x4 trans_Matrix = new Matrix4x4();

    // multiple GameObjects (only update position)
    private Vector3[] positionArray = new Vector3[5];

    private bool calibrationDone = false;
    private bool isConnected = false;

    // Connect to server. For a faster timeout check the code in ReceiveClient.cs
    public void Connect2Server()
    {
        // tries to connect for 3 seconds and then throws an error if it cannot connect
        // taken from https://social.msdn.microsoft.com/Forums/vstudio/en-us/2281199d-cd28-4b5c-95dc-5a888a6da30d/tcpclientconnect-timeout
        client = new TcpClient();
        IAsyncResult ar = client.BeginConnect(ipAddress, port, null, null);
        System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
        try
        {
            if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3), false))
            {
                client.Close();
                throw new TimeoutException();
            }

            client.EndConnect(ar);
        }
        finally
        {
            wh.Close();
        }
        print("SOCKET READY");

        connectText.GetComponent<Text>().text = "Connected";
        connectText.color = Color.green;
        isConnected = true;
    }

    public void DisconnectClient()
    {
        if (client.Connected)
        {
            client.Close();
            connectText.text = "Disconnected";
            connectText.color = Color.red;
            isConnected = false;
        }
    }


    void Message_Sent(IAsyncResult res)
    {
        if (debugOn)
        print("Array sent");
    }

    // send the coordinates of the image Tracker tracked by the HMD camera to the server
    public void sendCoordinates()
    {
        if (!client.Connected)
            return; // early out to stop the function from running if client is disconnected

        // Only send the coordinates if the MagicLeap is currently tracking the imageTracker.
        if (trackingObj.activeInHierarchy == false)
            return;

        receivedMessage = "";

        // Set up async read
        //  print ("Server: Accept() is OK...");

        // record the pose of the ImageTracker and convert to Matrix4x4.TRS (4x4 transformation matrix)
        Matrix4x4 TRS = Matrix4x4.TRS(imgTracker.transform.position, imgTracker.transform.rotation, imgTracker.transform.localScale);
        //Debug.Log("MatrixTRS " + TRS.ToString());

        // convert Matrix4x4 to 2D array
        floatArray1 = TRS2Array(TRS);

        // convert to byte array and send
        var byteArray1 = new byte[floatArray1.Length * 4];
        Buffer.BlockCopy(floatArray1, 0, byteArray1, 0, byteArray1.Length);

        var stream = client.GetStream();

        IAsyncResult asyncWrite;
        do
        {
            asyncWrite = stream.BeginWrite(byteArray1, 0, byteArray1.Length, Message_Sent, null);
            if (debugOn)
                print("array sent through write");
            stream.EndWrite(asyncWrite);
        }
        while (stream.DataAvailable);
    }

    // this function is to send co-ordinates of marker 2 during one-time calibration, called when bumper is pressed
    public void sendCoordinatesMarker2()
    {
        if (!client.Connected)
            return; // early out to stop the function from running if client is disconnected

        // Only send the coordinates if the MagicLeap is currently tracking the imageTracker.
        if (trackingObj2.activeInHierarchy == false)
            return;
        errorBoard2.GetComponent<Text>().text = "Marker 2";

        receivedMessage = "";

        // Set up async read
        //  print ("Server: Accept() is OK...");
        //  Console.WriteLine("Server: Accepted connection from: {0}", client.RemoteEndPoint.ToString());

        // record the pose of the ImageTracker and convert to Matrix4x4.TRS (4x4 transformation matrix)
        Matrix4x4 TRS = Matrix4x4.TRS(imgTracker2.transform.position, imgTracker2.transform.rotation, imgTracker2.transform.localScale);
        //Debug.Log("MatrixTRS " + TRS.ToString());

        // convert Matrix4x4 to 2D array
        floatArray1 = TRS2Array(TRS);

        // convert to byte array and send
        var byteArray1 = new byte[floatArray1.Length * 4];
        Buffer.BlockCopy(floatArray1, 0, byteArray1, 0, byteArray1.Length);

        var stream = client.GetStream();

        IAsyncResult asyncWrite;
        do
        {
            asyncWrite = stream.BeginWrite(byteArray1, 0, byteArray1.Length, Message_Sent, null);
            if (debugOn)
                print("array sent through write");
            stream.EndWrite(asyncWrite);
        }
        while (stream.DataAvailable);
    }
 
    
    // changing the calibDone bool to true activates the listening client in the update function
    public void CalibDone() 
    {
        calibrationDone = true;
        rsCube.SetActive(true);
    }


    void Update()
    {
        if (calibrationDone)
        {
            if (!client.Connected)
            {
                connectText.text = "Disconnected";
                connectText.color = Color.red;
                isConnected = false;
                return; // early out to stop the function from running if client is disconnected
            }
            connectText.text = "Connected";
            connectText.color = Color.green;

            receivedMessage = "";

            // Set up async read
            //print("Server: Accept() is OK...");
            //  Console.WriteLine("Server: Accepted connection from: {0}", client.RemoteEndPoint.ToString());

            var stream = client.GetStream();

            
            stream.BeginRead(buf, 0, buf.Length, Message_Received, null);

            // update position and rotation of single gameobject
            updateGameObjectPose();
        }
    }

    void updateGameObjectPose()
    {

        // the transform arrives transposed, so transpose again to get correct values
		Matrix4x4 trans_Matrix_tp = trans_Matrix.transpose;
        if (debugOn)
            Debug.Log("transMat" + trans_Matrix_tp.ToString());

        // Extract new local position
        Vector3 position = trans_Matrix_tp.GetColumn(3);
	
        //if (debugOn)
         //   Debug.Log("pos" + position.ToString());

        // Extract new local rotation (the up and forward vectors will depend on how the pose of the marker during calibration.
		Quaternion rotation = Quaternion.LookRotation(trans_Matrix_tp.GetColumn(2), trans_Matrix_tp.GetColumn(1));

        // update pose of this GameObject
		rsCube.transform.SetPositionAndRotation(position, rotation); // changed rscube to modelHead in this line
       // if (debugOn)
		//	Debug.Log("modelHead" + rsCube.transform.position.ToString());
    }
		


    void Message_Received(IAsyncResult res)
    {
        //print("came to Message_received code");
        if (res.IsCompleted && client.Connected)
        {
            var stream = client.GetStream();
            int bytesIn = stream.EndRead(res);
            //print("Server: Preparing to receive using Receive()...");
            receivedMessage = Encoding.ASCII.GetString(buf, 0, bytesIn);

            float tempVar;
            for (int i = 0; i < 16; i++)
            {
                tempVar = System.BitConverter.ToSingle(buf, i * 4);
                trans_Matrix[i] = tempVar;
            }
         //   if (debugOn)
                //Debug.Log("trans_Matrix " + trans_Matrix.ToString("F4"));
  
        }
    }


    public bool checkConnection()
    {
        return isConnected;
    }

    public string getIP()
    {
        return ipAddress;
    }

    // transform Matrix4x4.TRS to 2D array
    static float[] TRS2Array(Matrix4x4 matTRS)
    {
        float[] matArr = new float[16];
        matArr[0] = matTRS.m00;
        matArr[1] = matTRS.m01;
        matArr[2] = matTRS.m02;
        matArr[3] = matTRS.m03;
        matArr[4] = matTRS.m10;
        matArr[5] = matTRS.m11;
        matArr[6] = matTRS.m12;
        matArr[7] = matTRS.m13;
        matArr[8] = matTRS.m20;
        matArr[9] = matTRS.m21;
        matArr[10] = matTRS.m22;
        matArr[11] = matTRS.m23;
        matArr[12] = matTRS.m30;
        matArr[13] = matTRS.m31;
        matArr[14] = matTRS.m32;
        matArr[15] = matTRS.m33;
        return matArr;
    }

}