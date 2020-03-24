![](https://github.com/EddyCostantine/Face-Normal-Solver/blob/master/Documentation/Images/Face%20Normal%20Solver%20Demo%20One.jpg)
# Face Normal Solver
[FaceNormalSolver.cs](FaceNormalSolver/Assets/Scripts/FaceNormalSolver.cs) is a script that uses Unity's Raycast System to detect and correct flipped normals of a Manifold Mesh.
### Key Features
* Runtime
* Automatic detection of flipped faces
* Supports convex and concave manifold meshes
## Requirements
In order to ensure that the script performs properly on your mesh
* Enable Read/Write option in model's Import Settings
* Preferably disable Convert Units option in model's Import Settings
* Attach Mesh Collider component to the gameObject having Mesh component
## Example
The Unity project in this Repository demonstrates the usage of [FaceNormalSolver.cs](FaceNormalSolver/Assets/Scripts/FaceNormalSolver.cs)
### Usage
To apply face normal correction to a mesh simply call the RecalculateNormals function and pass your MeshFilter as an argument as follows:</br>
`FaceNormalSolver.RecalculateNormals(GetComponent(typeof(MeshFilter)) as MeshFilter);`</br></br>
In case of a complicated geometry you can increase the number iterations used to solve the mesh as follows:</br>
`FaceNormalSolver.RecalculateNormals(GetComponent(typeof(MeshFilter)) as MeshFilter, 2 );`

![](https://github.com/EddyCostantine/Face-Normal-Solver/blob/master/Documentation/Images/Face%20Normal%20Solver%20Demo%20Two.jpg)

## Future Improvements
* Supporting a higher percentage of overall flipped faces
* Supporting plane meshes
* Optimizing reiteration

Note that the Hard Polygon Shading has to do with the Vertex Normals of the mesh.
## How to contribute
Feel free to experiment with the project, report and fix bugs, improve structure and functionality, or even request new featues.
To do so: Fork the repository, create a branch in your fork, add your changes and submit a Pull Request.
