using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sfx : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlayOneShot2D(AudioClip clip){
		GameObject foobar = new GameObject("One shot audio");
		AudioSource audio = foobar.AddComponent<AudioSource>();
		audio.volume=0.3f;
		audio.clip=clip;
		audio.Play();
		Destroy(foobar,clip.length);
	}
}
