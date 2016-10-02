using UnityEngine;
using System.Collections.Generic;

public class OrientChildren : MonoBehaviour {

    public GameObject MainCamera;

    Transform[] _children;


	// Use this for initialization
	void Start () {

        _children = GetChildren();

        foreach(var child in _children)
        {
            var orient = child.gameObject.AddComponent<OrientToCamera>();
            orient.MainCamera = MainCamera;
        }


	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    Transform[] GetChildren()
    {
        var childCount = transform.childCount;
        var outputList = new List<Transform>();

        for(var x = 0; x< childCount; x++)
        {
            outputList.Add(transform.GetChild(x));
        }

        return outputList.ToArray();
    }
}
