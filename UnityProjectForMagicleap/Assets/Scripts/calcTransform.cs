using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class calcTransform : MonoBehaviour
{
    #region Private Variables
    // a list of vectors containing the virtual Model fiducial positions
    private List<Vector<double>> vecListVirt = new List<Vector<double>>();
    // a list of vectors containing the virtual Model fiducial positions centered around their center of mass
    private List<Vector<double>> vecListVirtCoM = new List<Vector<double>>();

    // the center of mass of the virtual Model fiducials
    private Vector<double> coMVirt;
    #endregion


    [SerializeField, Tooltip("(Empty) container that will take up the real world fiducials")]
    private GameObject rwContainer;

    [SerializeField, Tooltip("The head container that contains virtual model fiducials")]
    private GameObject virtContainer;

    [SerializeField, Tooltip("The image tracker tracking the head")]
    private GameObject headTracker;

    [SerializeField, Tooltip("Hide the fiducials after registration")]
    private bool fidVisible = false;

    [Header("3D Models")]
    [SerializeField, Tooltip("The MNI atlas and other data (DTI,...) (will be attached to head after fiducials are placed)")]
    private GameObject mniROIs;
    [SerializeField, Tooltip("The MNI atlas and other data (DTI,...) (will be attached to head after fiducials are placed)")]
    private GameObject brainROIs;
    [SerializeField, Tooltip("The MNI atlas and other data (DTI,...) (will be attached to head after fiducials are placed)")]
    private GameObject DTIobj;





    private void Start()
    {
        // fill list of vectors with face fiducials
        vecListVirt = virtContainer.GetComponent<FillMatrixMath>().TransVecList();

        if (!fidVisible)
        {
            // turn off renderer of face fiducials
            Renderer[] rs = virtContainer.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                r.enabled = false;
            }
        }

        virtContainer.GetComponent<Renderer>().enabled = true;
        // calculate center of mass of Vector list
        coMVirt = CalcCoM(vecListVirt);
        // calculate vectors translated to center of mass
        vecListVirtCoM = CalcCoMVecs(vecListVirt, coMVirt);

        // attach MNI ROIs to head
        if (mniROIs != null)
            mniROIs.transform.parent = virtContainer.transform;
        if (brainROIs != null)
            brainROIs.transform.parent = virtContainer.transform;
        if (DTIobj != null)
            DTIobj.transform.parent = virtContainer.transform;

        // set the virtContainer to the right in peripheral view so that it can be dragged around
        virtContainer.transform.SetPositionAndRotation(Camera.main.transform.position + new Vector3(0.2f, 0.2f, 0.6f), Camera.main.transform.rotation);
        virtContainer.transform.localEulerAngles = new Vector3(0.0f, -180.0f, 0.0f);
    }

    // Use this for registration
    public void PerformRegistration()
    {
        // a list of vectors containing the manually placed fiducial positions
        List<Vector<double>> vecListRW = rwContainer.GetComponent<FillMatrixMath>().TransVecList();

        // calculate center of mass of Vector list
        Vector<double> coMRW = CalcCoM(vecListRW);
        // calculate vectors translated to center of mass
        List<Vector<double>> vecListRWCoM = CalcCoMVecs(vecListRW, coMRW);
        // calculate covariance matrix
        Matrix<double> covMatrix = CalcCov(vecListRWCoM, vecListVirtCoM);
        // perform singular value decomposition
        var svd = covMatrix.Svd(true);
        // get the rotation through R = U VT
        var rot = svd.U * svd.VT;
        var tra = coMRW - rot * coMVirt;

        // convert Math rotation and translation to Matrix4x4
        Quaternion rotMat = QuaternionFromMatrix(MathRot2Mat4x4(rot, tra));

        RotandTransRigid(rwContainer, virtContainer, rotMat, rot, tra);

        // turn on head image tracker
        if (headTracker != null)
            headTracker.SetActive(true);

        if (!fidVisible)
            rwContainer.SetActive(false);
        // calculate alignment RMS error
        double rms = GetComponent<calcAlignmentRMS>().measureRMS(rwContainer, virtContainer);
        Debug.Log("reg done");

    }

    // calculate vectors translated to center of mass
    List<Vector<double>> CalcCoMVecs(List<Vector<double>> fids, Vector<double> coM)
    {
        List<Vector<double>> vecListCoM = new List<Vector<double>>();
        foreach (Vector<double> vec in fids)
        {
            vecListCoM.Add(vec - coM);
        }
        return vecListCoM;
    }

    // calculate covariance matrix
    Matrix<double> CalcCov(List<Vector<double>> rwVecs, List<Vector<double>> virtVecs)
    {
        Matrix<double> covMatrix = Matrix<double>.Build.Dense(3, 3);
        Debug.Log("CalcCov rwvecs.count " + rwVecs.Count.ToString() + " virtvecs.count " + virtVecs.Count.ToString() + "covMatrix " + covMatrix.ToString());
        for (int i = 0; i < rwVecs.Count; i++)
        {
            covMatrix += ColVec2Mat(rwVecs[i]) * RowVec2Mat(virtVecs[i]);
        }
        return covMatrix;
    }


    // perform rigid registration from destination to source container
    void RotandTransRigid(GameObject rwObj, GameObject virtObj, Quaternion rotMat4, Matrix<double> rot, Vector<double> tra)
    {
        Vector<double> rwObjPos = Vector<double>.Build.Dense(3);
        rwObjPos[0] = rwObj.transform.position.x;
        rwObjPos[1] = rwObj.transform.position.y;
        rwObjPos[2] = rwObj.transform.position.z;
        Vector<double> virtObjPos = Vector<double>.Build.Dense(3);
       
        virtObjPos[0] = 0.0f;
        virtObjPos[1] = 0.0f;
        virtObjPos[2] = 0.0f;

        // All 
        Vector<double> transVec = rot * virtObjPos + tra;
        
        Vector<double> newObjPos = transVec;
        virtObj.transform.position = new Vector3((float)newObjPos[0], (float)newObjPos[1], (float)newObjPos[2]);
        virtObj.transform.rotation = rotMat4;
    }


    // create spherical marker
    void CreateMarker(Vector3 pos, GameObject parentObj)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // make sphere
        sphere.transform.parent = parentObj.transform; // !!! set parent before changin position, otherwise collider error! Unity 2018.1.9 BUG
        sphere.transform.localScale = Vector3.one * 0.1f; // scale sphere down
        sphere.transform.position = pos; // position sphere at cursor position
    }

    //// static functions
    ///     // convert column vector to matrix that has the vector in col 1 and 0 everywhere else
    static Matrix<double> ColVec2Mat(Vector<double> vec)
    {
        Matrix<double> colVecMatrix = Matrix<double>.Build.Dense(3, 3);
        colVecMatrix[0, 0] = vec[0];
        colVecMatrix[1, 0] = vec[1];
        colVecMatrix[2, 0] = vec[2];
        return colVecMatrix;
    }

    // convert row vector to matrix that has the vector in row 1 and 0 everywhere else
    static Matrix<double> RowVec2Mat(Vector<double> vec)
    {
        Matrix<double> rowVecMatrix = Matrix<double>.Build.Dense(3, 3);
        rowVecMatrix[0, 0] = vec[0];
        rowVecMatrix[0, 1] = vec[1];
        rowVecMatrix[0, 2] = vec[2];
        return rowVecMatrix;
    }

    static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }

    // calculate center of mass of Vector list
    static Vector<double> CalcCoM(List<Vector<double>> Fids)
    {
        Vector<double> coM = Vector<double>.Build.Dense(3);
        foreach (Vector<double> vec in Fids)
            coM += vec;
        coM = coM.Divide(Fids.Count);
        return coM;
    }

    static Matrix4x4 MathRot2Mat4x4(Matrix<double> rotmat, Vector<double> travec)
    {
        Matrix4x4 mat4 = new Matrix4x4();
        mat4.m00 = (float)rotmat[0, 0];
        mat4.m01 = (float)rotmat[0, 1];
        mat4.m02 = (float)rotmat[0, 2];
        mat4.m03 = (float)travec[0];
        mat4.m10 = (float)rotmat[1, 0];
        mat4.m11 = (float)rotmat[1, 1];
        mat4.m12 = (float)rotmat[1, 2];
        mat4.m13 = (float)travec[1];
        mat4.m20 = (float)rotmat[2, 0];
        mat4.m21 = (float)rotmat[2, 1];
        mat4.m22 = (float)rotmat[2, 2];
        mat4.m23 = (float)travec[2];
        mat4.m30 = 0;
        mat4.m31 = 0;
        mat4.m32 = 0;
        mat4.m33 = 1;
        //Debug.Log("mat4 " + mat4.ToString());
        //Debug.Log("rotmat " + rotmat.ToMatrixString());
        return mat4;

    }

}
