using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackingManager : MonoBehaviour {

    private Vector3 _meanPosition;
    private Quaternion _meanRotation;
    private Vector3 _meanProjPos = new Vector3();

    // List of positions that will be used to attach the marker
    private List<Vector3> _posList = new List<Vector3>();
    private List<Vector3> _projPosList = new List<Vector3>();
    private List<Quaternion> _rotList = new List<Quaternion>();

    // when attaching the head use the first no. of _filterCounter frames to set position
    private int _filterCounter = 0;
    private int _filterMax = 30;
    private int _filterMin = 5;

    private bool isAttached = false;
    private GameObject _headCopy;

    // Joint
    JointDrive _drive = new JointDrive();
    ConfigurableJoint _cj; 

    #region Serialized Variables
    [SerializeField, Tooltip("Game Object showing the tracking cube")]
    private GameObject _trackingCube;
    [SerializeField, Tooltip("The Head object that will be attached to the Image Tracker")]
    private GameObject _trackingHead;
    #endregion


    [System.Obsolete]
    private void Start()
    {
        _drive.mode = JointDriveMode.Position;
        _drive.positionSpring = 100;
        _drive.maximumForce = 3.402823e+38f;
        _cj = _trackingHead.GetComponent<ConfigurableJoint>();
    }

    public bool InitialAttachHead() 
    {
        _headCopy = new GameObject();
        _headCopy.transform.SetPositionAndRotation(_trackingHead.transform.position, _trackingHead.transform.rotation);
       // Debug.Log("starting Coroutine"); //changed this line - commented
		//   StartCoroutine(InitialAttachHeadCoRout()); //changed this line - commented
        //StartCoroutine(SimpleAttach());
        return true;
    }

    // not used, no filtering
    IEnumerator SimpleAttach()
    {
        while (_filterCounter < _filterMin)
        {
            _filterCounter += 1;
            //Debug.Log("counter " + _filterCounter.ToString());
            yield return null;
        }
        _trackingHead.transform.parent = _trackingCube.transform;
        Debug.Log("isAttached");
        _trackingCube.GetComponent<Renderer>().material.color = Color.white;
        yield return null;
    }


    // Coroutine that runs every frame once the target is found
    // if no Coroutine is used here, this is just run once when TargetFound
    IEnumerator InitialAttachHeadCoRout()
    {
        // new, with filtering
        while (_filterCounter < _filterMax)
        {
            if (_filterCounter > _filterMin)
            {
                // get current frame pose
                // save pose in List<Vector3>
                _headCopy.transform.parent = _trackingCube.transform;
                _posList.Add(_headCopy.transform.localPosition);
                _rotList.Add(_headCopy.transform.localRotation);
                _projPosList.Add(_headCopy.transform.localPosition + _headCopy.transform.localRotation * new Vector3(0.1f, 0.1f, 0.1f));
                _headCopy.transform.parent = null;
                _headCopy.transform.SetPositionAndRotation(_trackingHead.transform.position, _trackingHead.transform.rotation);
            }
            _filterCounter += 1;
            yield return null;
        }
        // measure mean projected position (includes position and rotation) of list
        _meanProjPos = meanLocation(_projPosList);

        filterListProjPos();

        Debug.Log("mean Position " + _meanPosition.ToString() + " elements " + _posList.Count.ToString());
        Debug.Log("mean Rotation " + _meanRotation.ToString() + " rotelements " + _rotList.Count.ToString());

        isAttached = true;
        Debug.Log("isAttached");
        _trackingCube.GetComponent<Renderer>().material.color = Color.white;
        _trackingHead.transform.parent = _trackingCube.transform;
        _trackingHead.transform.localPosition = _meanPosition;
        _trackingHead.transform.localRotation = _meanRotation;
        _trackingHead.transform.parent = null;

         //attach configurable joint
        _cj.connectedBody = _trackingCube.GetComponent<Rigidbody>();

        _cj.xDrive = _drive;
        _cj.yDrive = _drive;
        _cj.zDrive = _drive;
        _cj.angularXDrive = _drive;
        _cj.angularYZDrive = _drive;

        // attach spring joints
        //Component[] springJoints = _trackingHead.GetComponents(typeof(SpringJoint));

        //GameObject placeHolder = new GameObject();
        //foreach (SpringJoint joint in springJoints)
        //{
        //    joint.spring = 1000;
        //    for (int i = 0; i < 5; i++)
        //    {
        //        GameObject anchorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        anchorCube.transform.parent = null;
        //        anchorCube.transform.SetPositionAndRotation(_trackingCube.transform.position, _trackingCube.transform.rotation);
        //        anchorCube.transform.SetParent(_trackingCube.transform);
        //        anchorCube.transform.localScale = Vector3.one;
        //            Debug.Log("local Scale on cube " + anchorCube.transform.localScale.ToString());
        //        if (i == 1)
        //        {
        //            anchorCube.transform.localPosition += new Vector3(0f, 0f, 5f);
        //            anchorCube.GetComponent<Renderer>().material.color = Color.white;
        //        }
        //        else if (i == 2)
        //        {
        //            anchorCube.transform.localPosition += new Vector3(0f, 0f, -5f);
        //            anchorCube.GetComponent<Renderer>().material.color = Color.white;
        //        }
        //        else if (i == 3)
        //        {
        //            anchorCube.transform.localPosition += new Vector3(5f, 0f, 0f);
        //            anchorCube.GetComponent<Renderer>().material.color = Color.white;
        //        }
        //        else if (i == 4)
        //        {
        //            anchorCube.transform.localPosition += new Vector3(-5f, 0f, 0f);
        //            anchorCube.GetComponent<Renderer>().material.color = Color.white;
        //        }

        //            anchorCube.transform.SetParent(_trackingCube.transform);
        //            anchorCube.transform.localScale = Vector3.one * 0.2f;
        //            Debug.Log("local Scale on head " + anchorCube.transform.localScale.ToString());
        //            joint.anchor.Set(anchorCube.transform.localPosition.x, anchorCube.transform.localPosition.y, anchorCube.transform.localPosition.z);
        //    }
        //    joint.connectedBody = _trackingCube.GetComponent<Rigidbody>();
        //}

        yield return null;

    }


    private void filterListProjPos()
    {
        // sort list by distance from projected points from their mean
        List<Vector3> sortedProjPos = _projPosList.OrderBy(x => Vector3.Distance(_meanProjPos, x)).ToList();
        float maxDist = Vector3.Distance(_meanProjPos, sortedProjPos[(int)Mathf.Round(sortedProjPos.Count * 0.5f)]);
        Debug.Log("maxProjDist" + maxDist.ToString());
        Debug.Log("mean ProjPosition " + _meanProjPos.ToString("F2") + " elements " + _projPosList.Count.ToString());

        // remove any points that have a higher distants than 50% of the points
        for (int i = _projPosList.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(_projPosList[i], _meanProjPos) > maxDist)
            {
                Debug.Log(i + " projPos" + Vector3.Distance(_projPosList[i], _meanProjPos));
                _projPosList.RemoveAt(i);
                _posList.RemoveAt(i);
                _rotList.RemoveAt(i);

            }
        }

        // take remaining points and look for the one closest to the average (sortedProjPosfil[0])
        _meanProjPos = meanLocation(_projPosList);
        List<Vector3> sortedProjPosfil = _projPosList.OrderBy(x => Vector3.Distance(_meanProjPos, x)).ToList();
        for (int i = sortedProjPosfil.Count - 1; i >= 0; i--)
        {
            if (_projPosList[i] == sortedProjPosfil[0])
            {
                Debug.Log(i + " meanProjPos " + _projPosList[i].ToString("F2"));
                Debug.Log(i + " SortedProjPos0 " + sortedProjPosfil[0].ToString("F2"));
                _meanProjPos = sortedProjPosfil[0];
                _meanPosition = _posList[i];
                _meanRotation = _rotList[i];
            }
        }
        Debug.Log("mean ProjPosition " + _meanProjPos.ToString("F2") + " elements " + _projPosList.Count.ToString());
    }

    // calculates the mean of a list of vector3
    static Vector3 meanLocation(List<Vector3> positionList)
    {
        Vector3 avgPos = Vector3.zero;
        foreach (Vector3 vec in positionList)
        {
            avgPos += vec;
        }
        avgPos = avgPos / positionList.Count;
        return avgPos;
    }

    public void AttachHead()
    {
        if (_drive.maximumForce < 0.01f)
        {
            _trackingHead.transform.parent = _trackingCube.transform;
            _trackingHead.transform.localPosition = _meanPosition;
            _trackingHead.transform.localRotation = _meanRotation;
            //foreach (SpringJoint joint in _trackingHead.GetComponents(typeof(SpringJoint)))
            //{
            //    joint.spring = 1000;
            //}
            //drive.maximumForce = 3.402823e+38f;
            //_trackingHead.transform.parent = null;
        }
        else
        {
            _trackingHead.transform.parent = null;
            //foreach (SpringJoint joint in _trackingHead.GetComponents(typeof(SpringJoint)))
            //{
            //    joint.spring = 0;
            //}
            _drive.maximumForce = 0f;
        }
    }

    public void DetachHead()
    {
        _trackingHead.transform.parent = null;
        isAttached = false;
        _filterCounter = 0;
        _posList.Clear();
        _rotList.Clear();
        _trackingCube.GetComponent<Renderer>().material.color = Color.red;
    }

    public bool getAttachStat()
    {
        return isAttached;
    }
}
