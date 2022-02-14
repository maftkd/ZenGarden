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
	public float _lerpRate;
	public float _maxProgPerSec;
	public float _minProgPerSec;
	Transform _lead;
	ParticleSystem _leadParts;
	ParticleSystem _appearParts;
	public LineRenderer _line;
	Transform _lineT;
	Vector3[] _linePositions;
	float[] _lineVals;
	float[] _distances;
	Material _rockMat;
	public float _showTime;
	public AudioClip _appearClip;
	Sfx _sfx;
	public float _chargeRate;

	void Awake(){
		GetComponent<MeshRenderer>().enabled=false;
		_progressMat.SetFloat("_Progress",0);
		_lead=transform.GetChild(0);
		_leadParts=_lead.GetComponent<ParticleSystem>();
		_appearParts=transform.GetChild(1).GetComponent<ParticleSystem>();
		_sfx=FindObjectOfType<Sfx>();
		_lineT=_line.transform;
		_linePositions = new Vector3[_line.positionCount];
		_line.GetPositions(_linePositions);
		_lineVals = new float[_linePositions.Length];
		_distances = new float[_linePositions.Length-1];
		float totalDist=0;
		for(int i=1; i<_linePositions.Length; i++){
			_distances[i-1] = (_linePositions[i]-_linePositions[i-1]).magnitude;
			totalDist+=_distances[i-1];
		}
		_lineVals[0]=0;
		for(int i=1; i<_linePositions.Length; i++){
			_lineVals[i]=_lineVals[i-1]+_distances[i-1]/totalDist;
		}
		foreach(float f in _lineVals)
			Debug.Log(f);
		CalcLeadPos();

		//instance new rock
		CreateNewRock();
	}

	public void SetDrawVel(float v){
		if(_progress>=1f)
			return;
		float clamped = Mathf.Clamp01(v);
		float increment=clamped*Time.deltaTime*_maxProgPerSec;
		_targetProgress+=increment;
		/*
		if(clamped>0&&!_leadParts.isPlaying)
			_leadParts.Play();
			*/
	}

	void Update(){
		if(_progress<1f){
			_progress=Mathf.Lerp(_progress,_targetProgress,_lerpRate*Time.deltaTime);
			_progressMat.SetFloat("_Progress",_progress);
			CalcLeadPos();
			float diff=_progress-_prevProg;
			/*
			if(diff<_minProgPerSec*Time.deltaTime&&_leadParts.isPlaying)
				_leadParts.Stop();
				*/
			if(_progress>=1){
				_progress=1;
				//_leadParts.Stop();
				_progressMat.SetFloat("_Intensity",1f);

				Debug.Log("Full!");
				StartCoroutine(ShowRock());
			}
		}

		_prevProg=_progress;
	}

	void CalcLeadPos(){
		for(int i=1; i<_lineVals.Length; i++){
			if(_progress>=_lineVals[i-1]&&_progress<_lineVals[i]){
				float t01 = Mathf.InverseLerp(_lineVals[i-1],_lineVals[i],_progress);
				_lead.position=_lineT.TransformPoint(Vector3.Lerp(_linePositions[i-1],_linePositions[i],t01));
				return;
			}
		}
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
		_progressMat.SetFloat("_Intensity",0f);
		CalcLeadPos();
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
	}
}
