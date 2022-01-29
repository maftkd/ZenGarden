//Singing bowl
//
//This is probably temp, but just a fun way to reset the board while testing
//
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingingBowl : MonoBehaviour
{
	Sand _sand;

	void Awake(){
		_sand=FindObjectOfType<Sand>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnMouseDown(){
		_sand.Init();
		GetComponent<AudioSource>().Play();
	}
}
