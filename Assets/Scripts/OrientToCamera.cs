using UnityEngine;
using System.Collections;

public class OrientToCamera : MonoBehaviour {

    public GameObject MainCamera = null;

    // Use this for initialization
    void Start()
    {

        if (MainCamera == null)
        {
            GameObject.Find("MainCamera");
        }

    }

    // Update is called once per frame
    void Update()
    {

        var newForward = MainCamera.transform.forward;
        newForward.y = 0;
        transform.forward = newForward;

    }
}
