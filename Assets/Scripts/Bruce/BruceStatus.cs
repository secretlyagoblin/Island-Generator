using UnityEngine;
using System.Collections;

public class BruceStatus : MonoBehaviour {

    int _thirstIntLevel = 0;
    float _thirstFloatLevel = 0f;
    float _thirstRate = 0.1f;

    public bool NearWaterSource = false;


	// Use this for initialization
	void Start () {

        //Debug.Log("Thirst Level at " + _thirstIntLevel);

    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Fire1") && NearWaterSource) { _thirstFloatLevel = _thirstIntLevel = 0; Debug.Log("Thirst Level reset to " + _thirstIntLevel); }

        _thirstFloatLevel += (_thirstRate * Time.deltaTime);


        if (_thirstIntLevel + 1 < _thirstFloatLevel)
        {
            _thirstIntLevel++;
            //Debug.Log("Thirst Level increased to " + _thirstIntLevel);
        }

        
        	
	}
}
