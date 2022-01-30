using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
	public AudioClip [] _clips;
	int _curClip;
	Sfx _sfx;
	public float _fadeDur;
    // Start is called before the first frame update
	//
	
	void Awake(){
		_sfx=FindObjectOfType<Sfx>();
	}
	public void StartPlaying(){
		StartCoroutine(PlaySong());
	}

	IEnumerator PlaySong(){
		_sfx.PlayOneShot2D(_clips[_curClip]);
		/*
		if(_clips[_curClip].name.Contains("JTUBA"))
			s.volume=_tubaVol;
			*/

		yield return new WaitForSeconds(_clips[_curClip].length);
		_curClip++;
		if(_curClip>=_clips.Length)
			_curClip=0;
		StartCoroutine(PlaySong());
	}
	
	
}
