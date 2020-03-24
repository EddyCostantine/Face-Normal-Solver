using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //FaceNormalSolver.Precision = 0.00001f;
        FaceNormalSolver.RecalculateNormals(GetComponent(typeof(MeshFilter)) as MeshFilter);
        //FaceNormalSolver.RecalculateNormals(GetComponent(typeof(MeshFilter)) as MeshFilter,2);
    }
}
