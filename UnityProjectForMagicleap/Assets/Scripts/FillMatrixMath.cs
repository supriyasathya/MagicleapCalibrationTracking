using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class FillMatrixMath : MonoBehaviour {

    // put all children Transforms in a Math.Net Vector list
    public List<Vector<double>> TransVecList()
    {
        List<Vector<double>> _vecList = new List<Vector<double>>();
        foreach (Transform _fidsTrans in this.GetComponentInChildren<Transform>())
        {
            Vector<double> _tmpVec = Vector<double>.Build.Dense(3);
            _tmpVec[0] = _fidsTrans.position.x;
            _tmpVec[1] = _fidsTrans.position.y;
            _tmpVec[2] = _fidsTrans.position.z;
            _vecList.Add(_tmpVec);
        }
        return _vecList;
    }

    // unused code (switched to List)
    // put all children Transforms in a Math.Net Matrix (not used anymore)
    public Matrix<double> Trans2Matrix()
    {
        // create a dense zero-vector of length 10
        Matrix<double> _fidMat = Matrix<double>.Build.Dense(3, 4);
        int i = 0;
        foreach (Transform _fidsTrans in this.GetComponentInChildren<Transform>())
        {
            _fidMat[0, i] = _fidsTrans.transform.position.x;
            _fidMat[2, i] = _fidsTrans.transform.position.y;
            _fidMat[3, i] = _fidsTrans.transform.position.z;
            i += 1;
        }
        return _fidMat;
    }

}
