using UnityEngine;
using System.Collections.Generic;

public class BillboardController : MonoBehaviour {

    public List<Transform> BillboardTransformList = new List<Transform>();

    public Camera Camera;

    Quaternion _cameraRotation;
    Quaternion _targetRotation;

    // Use this for initialization
    void Awake()
    {

    }

    // Use this for initialization
    void Start () {
	
	}

    public void AddSprite(Transform transform)
    {
        BillboardTransformList.Add(transform);
        transform.rotation = _targetRotation;
    }

	
	// Update is called once per frame
	void LateUpdate () {

        if(_cameraRotation != Camera.transform.rotation)
        {
            _cameraRotation = Camera.transform.rotation;

            var targetLookAt = Camera.transform.forward;
            _targetRotation = Quaternion.LookRotation(new Vector3(targetLookAt.x,0,targetLookAt.z));
            



            for (int i = 0; i < BillboardTransformList.Count; i++)
            {
                BillboardTransformList[i].rotation = _targetRotation;
            }
        }

	
	}
}
