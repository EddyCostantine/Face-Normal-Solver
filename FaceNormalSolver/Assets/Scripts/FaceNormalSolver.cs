using UnityEngine;

/// <summary>
/// FaceNormalSolver Class contains method for detecting and correcting mesh flipped normals.
/// </summary>
/// <author>Eddy Costantine, 2020,</author>
/// <license>MIT</license>
public static class FaceNormalSolver
{
    private static MeshFilter _meshFilter;
    private static Mesh _mesh;
    private static int[] _meshTriangles;
    private static Vector3[] _meshVertices;
    private static Vector3 _currentTriangleNormal;
    private static int[] _triangleStatus;
    private static float _precision = 0.00001f;

    /// <value>Get and Sets the precision distance from mesh triangle to avoid raycast self collision.</value>
    /// In other words, raycasting a ray initiated from center of triangle hits that triangle, 
    /// Thus the need to move a small distance along the direction of N and -N (N is the triangle face normal).
    public static float Precision 
    { 
        get { return _precision; } 
        set { if (value > 0) { _precision = value; }} 
    }

    /// <summary>
    /// Detects and corrects flipped normals of a Manifold Mesh.
    /// </summary>
    /// <param name="meshFilter">MeshFilter component of gameobject.</param>
    /// <param name="iterations">Number of times to reiterate over mesh.</param>
    /// <remarks>
    /// Requires Read/Write to be enabled in Model Import Settings.
    /// Requires Mesh Collider attached to Gameobject with Mesh Component.
    /// Avoid Convert Units option in Model Import Settings.
    /// </remarks>
    /// <example><code>FaceNormalSolver.RecalculateNormals(GetComponent(typeof(MeshFilter)) as MeshFilter);</code></example>
    public static void RecalculateNormals(MeshFilter meshFilter, int iterations = 1)
    {
        Debug.Log(typeof(string).Assembly.ImageRuntimeVersion);
        // MeshFilter filter = GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (meshFilter == null) return;
        InitializeMeshVariables(meshFilter);

        for (int i = 0; i < iterations; i++)
        {
            // Iterate in each submesh if multiple ones exist.
            for (int m = 0; m < _mesh.subMeshCount; m++)
            {
                _meshTriangles = _mesh.GetTriangles(m);

                for (int j = 0; j < _triangleStatus.Length; j++)
                {
                    Debug.Log(j);
                    if (_triangleStatus[j] == 0)
                    {
                        _currentTriangleNormal = CalculateFaceNormal(j);

                        var centroids = CalculateCentroids(j);

                        Physics.Raycast(centroids.centroidFront, _currentTriangleNormal, out RaycastHit frontSideHitInfo, Mathf.Infinity);//Ray along direction of N
                        Physics.Raycast(centroids.centroidBack, -1 * _currentTriangleNormal, out RaycastHit backSideHitInfo, Mathf.Infinity);// Ray along direction of -N

                        // CASE ONE: Front side Ray hits nothing and back side hits another triangle
                        if (frontSideHitInfo.distance == 0 && backSideHitInfo.distance != 0)
                        {
                            // Mark the triangle as good to keep: Don't flip
                            _triangleStatus[j] = 1;

                            // Front side of triangle is facing outside, ==> Back side should face Back side of another triangle
                            CheckTriangle(_currentTriangleNormal * -1, CalculateFaceNormal(backSideHitInfo.triangleIndex), backSideHitInfo.triangleIndex);
                        }
                        // CASE TWO: Back side Ray hits nothing and frontside hits anther triangle
                        else if (backSideHitInfo.distance == 0 && frontSideHitInfo.distance != 0)
                        {
                            // Flip triangle, then mark as good to keep
                            FlipTriangle(_meshTriangles, j * 3);
                            _triangleStatus[j] = 1;

                            // Now that the Front side of triangle is facing outside, ==> Back side should face Back side of another triangle
                            CheckTriangle(_currentTriangleNormal, CalculateFaceNormal(frontSideHitInfo.triangleIndex), frontSideHitInfo.triangleIndex);
                        }
                        // CASE THREE: If both sides hit triangles, check if the triangle hit by the positive side Ray was tested and marked as good
                        else if (_triangleStatus[frontSideHitInfo.triangleIndex] == 1)
                        {
                            //Front side of triangle is facing outside, ==> Back side should face Back side of another triangle
                            CheckTriangle(_currentTriangleNormal * -1, CalculateFaceNormal(frontSideHitInfo.triangleIndex), j);
                        }
                        // CASE FOUR: If the previously checked triangle status was false, check if the triangle hit by the negative side Ray was tested and marked as good
                        else if (_triangleStatus[backSideHitInfo.triangleIndex] == 1)
                        {
                            //Front side of triangle is facing outside, ==> Back side should face Back side of another triangle
                            CheckTriangle(_currentTriangleNormal * -1, CalculateFaceNormal(backSideHitInfo.triangleIndex), j);
                        }
                    }
                }
                _mesh.SetTriangles(_meshTriangles, m);
                _mesh.RecalculateNormals();
            }
        }
    }

    /// <summary>
    /// Checks whether to flip triangle or directly mark status as good.  
    /// </summary>
    private static void CheckTriangle(Vector3 currentTriangleNormal, Vector3 secondTriangleNormal, int triangleIndex)
    {
        if (Vector3.Dot(currentTriangleNormal, secondTriangleNormal) > 0)
        {
            _triangleStatus[triangleIndex] = 1;
        }
        else
        {
            FlipTriangle(_meshTriangles, triangleIndex * 3);
            _triangleStatus[triangleIndex] = 1;
        }
    }

    private static void InitializeMeshVariables(MeshFilter filter)
    {
        Physics.queriesHitBackfaces = true;
        _meshFilter = filter;
        _mesh = filter.mesh;
        _meshTriangles = _mesh.triangles;
        _meshVertices = _mesh.vertices;
        _triangleStatus = new int[_meshTriangles.Length / 3];
    }

    private static (Vector3 centroidFront, Vector3 centroidBack) CalculateCentroids(int index)
    {
        Vector3 centroid = ((_meshVertices[_meshTriangles[(index * 3)]] + _meshVertices[_meshTriangles[(index * 3) + 1]] + _meshVertices[_meshTriangles[(index * 3) + 2]]) / 3);
        //Transform centroid position from local space to world space
        centroid = _meshFilter.transform.TransformPoint(centroid);
        //Slightly shift the centroid position along the direction of the normals(N & -N) to avoid self collision
        Vector3 centroidFront = centroid + _precision * _currentTriangleNormal;
        Vector3 centroidBack = centroid - _precision * _currentTriangleNormal;
        return (centroidFront, centroidBack);
    }

    /// <summary>
    /// Flips mesh triangle face by flipping two vertices
    /// </summary>
    private static void FlipTriangle(int[] trianglesArray, int vertexIndex)
    {
        int temp = trianglesArray[vertexIndex + 0];
        trianglesArray[vertexIndex + 0] = trianglesArray[vertexIndex + 1];
        trianglesArray[vertexIndex + 1] = temp;
    }
    /// <summary>
    /// Calculates the face normal from each three consecutive vertices of triangle in meshTriangles array
    /// </summary>
    private static Vector3 CalculateFaceNormal(int triangleIndex)
    {
        Vector3 firstEdge = _meshVertices[_meshTriangles[triangleIndex * 3 + 1]] - _meshVertices[_meshTriangles[triangleIndex * 3 + 0]];//sec - ist
        Vector3 secondEdge = _meshVertices[_meshTriangles[triangleIndex * 3 + 2]] - _meshVertices[_meshTriangles[triangleIndex * 3 + 0]];
        Vector3 cross = Vector3.Cross(firstEdge, secondEdge);
        cross = _meshFilter.transform.TransformDirection(cross); // Transforms direction from local space to world space.
        return cross.normalized;
    }

}