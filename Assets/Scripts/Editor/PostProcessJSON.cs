using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PostProcessJSON : AssetPostprocessor {

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            string folder = Path.GetDirectoryName(str);
            string fileName = Path.GetFileNameWithoutExtension(str);
            string extension = Path.GetExtension(str);

            if (extension != ".JSON")
                continue;

            string sneakyExtension = Path.GetExtension(fileName);

            if (sneakyExtension != ".meshTile")
                continue;

            Debug.Log("Parsing: " + fileName + extension);

            fileName = Path.GetFileNameWithoutExtension(fileName);

            var meshTile = new MeshMasher.MeshTiling.MeshTile();

            if (!meshTile.PopulateFromString(((TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset))).text))
                Debug.LogError("Malformed JSON on parse, nerd");            

            AssetDatabase.CreateAsset(meshTile, folder +'/'+ fileName + "MeshTile.asset");

        }

    }
}
