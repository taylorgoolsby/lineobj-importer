# lineobj-importer
### Usage
1. Export model as `.obj`.

![Blender export settings](/examples/readme/blender-export-settings.jpg?raw=true "Blender export settings")

Groups are not supported, so make sure the `.obj` doesn't have groups. Blender will automatically flatten groups with the settings shown above.

2. Rename the file extension from `.obj` to `.lineobj`

3. When you open Unity, the model should automatically be imported.

### Known to work with:
* Blender: 2.78
* Unity: 2017.1

### Supported OBJ features
* Vertices
* Edges
* Faces
* Vertex Normals
* Object Names

### Unsupported OBJ features
* Vertex Colors
* Texture coordinates
* Free-form geometry (like NURBS)
* Negative indices
* Materials
* Smooth shading
* Groups