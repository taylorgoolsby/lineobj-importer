using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class LineobjPostprocessor : AssetPostprocessor {
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string str in importedAssets) {
            if (str.EndsWith(".lineobj")) {
                using (StreamReader sr = File.OpenText(str)) {
                    string s = string.Empty;
                    while ((s = sr.ReadLine()) != null) {
                        Debug.Log(s);
                    }
                }
            }
        }
    }
}
