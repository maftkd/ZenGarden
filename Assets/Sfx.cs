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
		if(clip.name.Contains("JTUBA"))
			audio.volume=0.13f;
		audio.Play();
		Destroy(foobar,clip.length);
	}

	public void PlayOneShot2D(AudioClip clip,float vol){
		GameObject foobar = new GameObject("One shot audio");
		AudioSource audio = foobar.AddComponent<AudioSource>();
		audio.volume=vol;
		audio.clip=clip;
		if(clip.name.Contains("JTUBA"))
			audio.volume=0.13f;
		audio.Play();
		Destroy(foobar,clip.length);
	}

	public static AudioSource PlayOneShot3D(AudioClip clip,Vector3 pos){
		GameObject foobar = new GameObject("One shot audio");
		foobar.transform.position=pos;
		AudioSource audio = foobar.AddComponent<AudioSource>();
		audio.volume=1.0f;
		audio.clip=clip;
		audio.spatialBlend=1f;
		audio.Play();
		Destroy(foobar,clip.length);
		return audio;
	}
}
