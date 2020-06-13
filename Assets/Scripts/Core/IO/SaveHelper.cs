using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WanderingRoad.IO {
    public class SaveHelper<T> where T : IStreamable
    {
        public void SaveAsset(T asset, string path)
        {
            var serialisableType = asset.ToSerialisable();

            var info = new System.IO.FileInfo(path);

            if (!info.Exists)
                Directory.CreateDirectory(info.Directory.FullName);

            var json = JsonUtility.ToJson(serialisableType);

            File.WriteAllText(path, json);            
        }

        public T LoadAsset<U>(string path) where U:ISerialisable
        {
            var serialised = JsonUtility.FromJson<U>(path);
            return (T)serialised.RestoreAsset();
        }


    }
}