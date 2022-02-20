using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RockSpawner : MonoBehaviour
{

	public float _rockSpacing;
	public Transform _rockPrefab;
	public int _numRocks;
	public Vector2 _sizeRange;
	Renderer _renderer;
	public float _lerpRate;
	public float _maxProgPerSec;
	public float _minProgPerSec;
	ParticleSystem _appearParts;
	Material _rockMat;
	public float _showTime;
	public AudioClip _appearClip;
	public float _appearVol;
	public float _appearDelay;
	Sfx _sfx;
	public float _chargeRate;
	Transform _curRock;

	void Awake(){
		GetComponent<MeshRenderer>().enabled=false;
		_appearParts=transform.GetChild(1).GetComponent<ParticleSystem>();
		_sfx=FindObjectOfType<Sfx>();

		//instance new rock
		CreateNewRock();
	}

	public void SetDrawVel(float v){
	}

	void Update(){
	}

	IEnumerator ShowRock(){
		yield return new WaitForSeconds(_appearDelay);
		float timer=0;
		float glowThresh=_rockMat.GetFloat("_GlowThresh");
		_rockMat.SetFloat("_GlowThresh",0);
		float emission=2f;
		_rockMat.SetFloat("_Emission", emission);
		float timeFrac=0;
		//_appearParts.Play();
		_sfx.PlayOneShot2D(_appearClip,_appearVol);
		float fracPow=0;
		while(timer<_showTime){
			timer+=Time.deltaTime;
			timeFrac=timer/_showTime;
			fracPow=Mathf.Pow(timeFrac,2);
			_rockMat.SetFloat("_Dissolve",1-timeFrac);
			_rockMat.SetFloat("_GlowThresh",Mathf.Lerp(0,glowThresh,fracPow));
			_rockMat.SetFloat("_Emission",Mathf.Lerp(emission,0,fracPow));
			yield return null;
		}
		_rockMat.SetFloat("_Dissolve",0);
		_rockMat.SetFloat("_Emission",0);
		_renderer.shadowCastingMode=ShadowCastingMode.On;
	}

	public void Reset(){
		CreateNewRock();
	}

	public void Charge(){
	}

	void CreateNewRock(){
		_curRock=Instantiate(_rockPrefab,transform);
		_renderer=_curRock.GetComponent<Renderer>();
		Vector3 eulers = Random.insideUnitSphere*360f;
		_curRock.eulerAngles=eulers;
		_curRock.localScale=Vector3.one*Random.Range(_sizeRange.x,_sizeRange.y);
		_curRock.localPosition=Vector3.zero;
		_rockMat=_curRock.GetComponent<MeshRenderer>().material;
		_rockMat.SetFloat("_Dissolve",1);
		StartCoroutine(ShowRock());
	}
}
