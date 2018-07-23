using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForwardSlowly : MonoBehaviour {

    public float Speed;
    public bool MoveForward = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (MoveForward)
        {
            transform.Translate(Vector3.forward*Speed);
        }
		
	}
}
