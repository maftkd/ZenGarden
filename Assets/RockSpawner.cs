using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{

	public float _rockSpacing;
	public Transform _rockPrefab;
	public int _numRocks;
	public Vector2 _sizeRange;
	float _progress;
	float _prevProg;
	float _targetProgress;
	public Material _progressMat;
	public Material _progressMatSmall;
	public float _lerpRate;
	public float _maxProgPerSec;
	public float _minProgPerSec;
	ParticleSystem _appearParts;
	Material _rockMat;
	public float _showTime;
	public AudioClip _appearClip;
	Sfx _sfx;
	public float _chargeRate;

	void Awake(){
		GetComponent<MeshRenderer>().enabled=false;
		_progressMat.SetFloat("_FillAmount",0);
		_progressMat.SetFloat("_Intensity",0);
		_progressMatSmall.SetFloat("_FillAmount",0);
		_progressMatSmall.SetFloat("_Intensity",0);
		_appearParts=transform.GetChild(1).GetComponent<ParticleSystem>();
		_sfx=FindObjectOfType<Sfx>();

		//instance new rock
		CreateNewRock();
	}

	public void SetDrawVel(float v){
		if(_progress>=1f)
			return;
		float clamped = Mathf.Clamp01(v);
		float increment=clamped*Time.deltaTime*_maxProgPerSec;
		_targetProgress+=increment;
	}

	void Update(){
		if(_progress<1f){
			_progress=Mathf.Lerp(_progress,_targetProgress,_lerpRate*Time.deltaTime);
			_progressMat.SetFloat("_FillAmount",_progress);
			float diff=_progress-_prevProg;
			if(_progress>=1){
				_progress=1;
				_progressMat.SetFloat("_Intensity",1f);
				_progressMat.SetFloat("_FillAmount",1f);
				_progressMatSmall.SetFloat("_Intensity",1f);
				_progressMatSmall.SetFloat("_FillAmount",1f);

				StartCoroutine(ShowRock());
			}
		}

		_prevProg=_progress;
	}

	IEnumerator ShowRock(){
		float timer=0;
		float glowThresh=_rockMat.GetFloat("_GlowThresh");
		_rockMat.SetFloat("_GlowThresh",0);
		float emission=2f;
		_rockMat.SetFloat("_Emission", emission);
		float timeFrac=0;
		_appearParts.Play();
		_sfx.PlayOneShot2D(_appearClip);
		while(timer<_showTime){
			timer+=Time.deltaTime;
			timeFrac=timer/_showTime;
			_rockMat.SetFloat("_Dissolve",1-timeFrac);
			_rockMat.SetFloat("_GlowThresh",Mathf.Lerp(0,glowThresh,timeFrac));
			_rockMat.SetFloat("_Emission",Mathf.Lerp(emission,0,Mathf.Sqrt(timeFrac)));
			yield return null;
		}
		_rockMat.SetFloat("_Dissolve",0);
		_rockMat.SetFloat("_Emission",0);
	}

	public void Reset(){
		_progress=0;
		_targetProgress=0;
		CreateNewRock();
	}

	public void Charge(){
		if(_progress>=1f)
			return;
		_targetProgress+=_chargeRate*Time.deltaTime;
	}

	void CreateNewRock(){
		Transform rock = Instantiate(_rockPrefab,transform);
		Vector3 eulers = Random.insideUnitSphere*360f;
		rock.eulerAngles=eulers;
		rock.localScale=Vector3.one*Random.Range(_sizeRange.x,_sizeRange.y);
		rock.localPosition=Vector3.zero;
		_rockMat=rock.GetComponent<MeshRenderer>().material;
		_rockMat.SetFloat("_Dissolve",1);
		_progressMat.SetFloat("_Intensity",0f);
		_progressMatSmall.SetFloat("_Intensity",0f);
		_progressMat.SetFloat("_FillAmount",0f);
		_progressMatSmall.SetFloat("_FillAmount",0f);
	}
}
