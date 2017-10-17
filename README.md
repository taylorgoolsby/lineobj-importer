# lineobj-importer
### Problem
Whenever Unity imports a mesh, it will only recognize primitives which are connected to a face. Any floating vertices or edges will not be imported. For example, this `.obj` has no faces, so when it is imported into Unity, no `Mesh` object will be created for it:
```
o Plane
v -1.000000 0.000000 1.000000
v 1.000000 0.000000 1.000000
v -1.000000 0.000000 -1.000000
v 1.000000 0.000000 -1.000000
l 3 1
l 1 2
l 2 4
l 4 3
```
`lineobj-importer` is a custom parser to fix this problem. The parser detects when `.lineobj` files are imported via `OnPostprocessAllAssets`.

### Results
Before (in Blender):

![Before](/examples/readme/before.gif?raw=true "Before")

After (in Unity):

![Final](/examples/readme/final.gif?raw=true "Final")

![Prefab](/examples/readme/prefab.jpg?raw=true "Prefab")

### Usage
1. Place [LineobjPostprocessor.cs](examples/basic-usage/Assets/LineobjPostprocessor.cs) anywhere in your Unity project.
2. Export model as `.obj` from Blender with these settings:

    ![Blender export settings](/examples/readme/blender-export-settings.jpg?raw=true "Blender export settings")

    Make sure to uncheck `Write Materials` and check `Triangulate Faces`.

3. Rename the file extension from `.obj` to `.lineobj`

4. When you open Unity, the model should automatically be imported. All Blender objects should have been flattened and merged into a single `Mesh`.

### Known to work with versions:
* Blender: 2.78
* Unity: 2017.1

### Supported OBJ features
* Vertices
* Edges
* Faces

### Unsupported OBJ features
* Vertex Normals
* Vertex Colors
* Texture coordinates
* Free-form geometry (like NURBS)
* Negative indices
* Materials
* Smooth shading
* Groups
* Object Names