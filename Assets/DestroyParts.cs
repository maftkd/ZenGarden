using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParts : MonoBehaviour
{
	ParticleSystem _parts;
    // Start is called before the first frame update
    void Start()
    {
		_parts=GetComponent<ParticleSystem>();
		Destroy(gameObject,_parts.main.startLifetime.constantMax+_parts.main.duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
