using UnityEngine;
using System.Collections;

public class BillboardBehaviour : MonoBehaviour {

    BillboardController _dad;

    void Awake()
    {
        _dad = FindObjectOfType<BillboardController>();
        _dad.AddSprite(transform);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    void OnEnable()
    {
        _dad.BillboardTransformList.Add(transform);
    }

    void OnDisable()
    {
        _dad.BillboardTransformList.Remove(transform);
    }

    void OnDestroy()
    {
        _dad.BillboardTransformList.Remove(transform);
    }
}
