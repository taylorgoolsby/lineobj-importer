using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;

[ScriptedImporter(1, "lineobj")]
public class LineobjImporter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
        int lastSlash = ctx.assetPath.LastIndexOf('/');
        int lastDot = ctx.assetPath.LastIndexOf('.');
        string assetName = ctx.assetPath.Substring(lastSlash + 1, lastDot - lastSlash - 1);
        Debug.Log("path: " + assetName);

        GameObject mainAsset = new GameObject();
        ctx.SetMainAsset("MainAsset", mainAsset);

        Dictionary<string, object> obj = ParseObj(ctx.assetPath);
        
        List<Mesh> meshes = new List<Mesh>();
        foreach (KeyValuePair<string, object> entry in obj) {
            Mesh mesh = ConstructMesh((Dictionary<string, object>) entry.Value);
            mesh.name = entry.Key;
            meshes.Add(mesh);
        }

        Mesh finalMesh = CombineMeshes(meshes.ToArray());
        finalMesh.name = assetName;

        MeshFilter meshFilter = mainAsset.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = finalMesh;
        mainAsset.AddComponent<MeshRenderer>();
        ctx.AddSubAsset("Mesh", finalMesh);
    }

    private Mesh CombineMeshes(Mesh[] meshes) {
        List<Vector3> combinedVertices = new List<Vector3>();
        List<int> combinedTriangles = new List<int>();
        List<int> combinedLines = new List<int>();
        for (int i = 0; i < meshes.Length; i++) {
            Mesh target = meshes[i];
            int indexOffset = combinedVertices.Count;

            Vector3[] vertices = target.vertices;
            foreach (Vector3 v in vertices) {
                combinedVertices.Add(v);
            }

            for (int j = 0; j < target.subMeshCount; j++) {
                if (j == 0) {
                    int[] triangles = target.GetIndices(0);
                    foreach (int index in triangles) {
                        combinedTriangles.Add(index + indexOffset);
                    }
                } else if (j == 1) {
                    int[] lines = target.GetIndices(1);
                    foreach (int index in lines) {
                        combinedLines.Add(index + indexOffset);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = combinedVertices.ToArray();
        mesh.SetIndices(combinedTriangles.ToArray(), MeshTopology.Triangles, 0);
        mesh.SetIndices(combinedLines.ToArray(), MeshTopology.Lines, 1);
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh ConstructMesh(Dictionary<string, object> data) {
        Mesh result = new Mesh();
        if (!data.ContainsKey("v")) {
            return result;
        }

        List<string[]> v = (List<string[]>) data["v"];
        Vector3[] vertices = new Vector3[v.Count];
        for (int i = 0; i < v.Count; i++) {
            string[] raw = v[i];
            float x = float.Parse(raw[0]);
            float y = float.Parse(raw[1]);
            float z = float.Parse(raw[2]);
            vertices[i] = new Vector3(x, y, z);
        }

        result.vertices = vertices;

        // subMesh 0 is like a regular mesh which uses MeshTopology.Triangles
        if (data.ContainsKey("f")) {
            List<string[]> f = (List<string[]>) data["f"];
            int[] indices = new int[f.Count * 3];
            for (int i = 0; i < f.Count; i++) {
                string[] raw = f[i];
                string s1 = raw[0];
                string s2 = raw[1];
                string s3 = raw[2];
                if (s1.Contains("//")) {
                    s1 = s1.Remove(s1.IndexOf("//"));
                }
                if (s2.Contains("//")) {
                    s2 = s2.Remove(s2.IndexOf("//"));
                }
                if (s3.Contains("//")) {
                    s3 = s3.Remove(s3.IndexOf("//"));
                }
                int v1 = int.Parse(s1) - 1;
                int v2 = int.Parse(s2) - 1;
                int v3 = int.Parse(s3) - 1;
                indices[i * 3] = v1;
                indices[i * 3 + 1] = v2;
                indices[i * 3 + 2] = v3;
            }

            result.SetIndices(indices, MeshTopology.Triangles, 0, false);
        }

        // subMesh 1 is the line mesh which uses MeshTopology.Lines
        if (data.ContainsKey("l")) {
            List<string[]> l = (List<string[]>) data["l"];
            int[] indices = new int[l.Count * 2];
            for (int i = 0; i > l.Count; i++) {
                string[] raw = l[i];
                int v1 = int.Parse(raw[0]) - 1;
                int v2 = int.Parse(raw[1]) - 1;
                indices[i * 2] = v1;
                indices[i * 2 + 1] = v2;
            }
            result.SetIndices(indices, MeshTopology.Lines, 0, false);
        }

        result.RecalculateBounds();
        return result;
    }

    /*
    Converts obj text file into json-like structure:
        {
            Cube.001: {v: [], vn: [], f: [], l: []},
            Plane.001: {v: [], vn: [], f: [], l: []},
        }
     */
    private Dictionary<string, object> ParseObj(string filepath) {
        Dictionary<string, object> result = new Dictionary<string, object>();
        using (StreamReader sr = File.OpenText(filepath)) {
            string s = string.Empty;
            string currentObjectName = null;
            string[] line;
            while ((s = sr.ReadLine()) != null) {
                if (s.StartsWith("o ")) {
                    currentObjectName = s.Split(' ')[1];
                    result.Add(currentObjectName, new Dictionary<string, object>());
                    continue;
                }

                if (currentObjectName != null) {
                    if (s.StartsWith("v ")) {
                        line = s.Split(' ');
                        string[] lineData = { line[1], line[2], line[3] };
                        Dictionary<string, object> currentObject = (Dictionary<string, object>) result[currentObjectName];
                        if (!currentObject.ContainsKey("v")) {
                            currentObject.Add("v", new List<string[]>());
                        }
                        List<string[]> v = (List<string[]>) currentObject["v"];
                        v.Add(lineData);
                    } else if (s.StartsWith("vn ")) {
                        line = s.Split(' ');
                        string[] lineData = { line[1], line[2], line[3] };
                        Dictionary<string, object> currentObject = (Dictionary<string, object>) result[currentObjectName];
                        if (!currentObject.ContainsKey("vn")) {
                            currentObject.Add("vn", new List<string[]>());
                        }
                        List<string[]> vn = (List<string[]>) currentObject["vn"];
                        vn.Add(lineData);
                    } else if (s.StartsWith("f ")) {
                        line = s.Split(' ');
                        if (line.Length > 4) {
                            Debug.LogError("Your model must be exported with triangulated faces.");
                            continue;
                        }
                        string[] lineData = { line[1], line[2], line[3] };
                        Dictionary<string, object> currentObject = (Dictionary<string, object>) result[currentObjectName];
                        if (!currentObject.ContainsKey("f")) {
                            currentObject.Add("f", new List<string[]>());
                        }
                        List<string[]> f = (List<string[]>) currentObject["f"];
                        f.Add(lineData);
                    } else if (s.StartsWith("l ")) {
                        line = s.Split(' ');
                        string[] lineData = { line[1], line[2] };
                        Dictionary<string, object> currentObject = (Dictionary<string, object>) result[currentObjectName];
                        if (!currentObject.ContainsKey("l")) {
                            currentObject.Add("l", new List<string[]>());
                        }
                        List<string[]> l = (List<string[]>) currentObject["l"];
                        l.Add(lineData);
                    }
                }
            }
        }
        return result;
    }

    // for debugging
    private void LogObj(Dictionary<string, object> obj) {
        string ind = "  ";
        string result = "";
        result += "{\n";
        foreach (KeyValuePair<string, object> entry in obj) {
            result += ind + entry.Key + ": {\n";
            Dictionary<string, object> geo = (Dictionary<string, object>) entry.Value;
            foreach (KeyValuePair<string, object> component in geo) {
                List<string[]> elements = (List<string[]>) component.Value;
                result += ind + ind + component.Key + ": [\n";
                foreach (string[] sarr in elements) {
                    string atoms = "[";
                    foreach (string s in sarr) {
                        atoms += s + ", ";
                    }
                    atoms += "]";
                    result += ind + ind + ind + atoms + "\n";
                }
                result += ind + ind + "]\n";
            }
            result += ind + "}\n";
        }
        result += "}";
        Debug.Log(result);
    }
}
 