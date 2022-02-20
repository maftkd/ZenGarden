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
    Transform _mallet;
    [SerializeField]
    float radius;
    Sand _sand;
	float _bowlHeight;
	Camera _cam;
	public float _malletHeightOffset;
	public float _noiseMult;
    AudioSource _source;
	public float _frequency;
	public int _sampleRate;
	public float _amplitude;
	float _duration;
	int _numCycles;
	public float _spinDur;
	public AnimationCurve _envelope;
	public float _spinSpeed;
	public Material _glowMat;
	public float _rockFallSpeed;

    float volSmoothed;
    float amplitudeSmoothed;
	
	int _state;

    void Awake()
    {
        _sand = FindObjectOfType<Sand>();
		_bowlHeight=transform.position.y;
		_cam = Camera.main;
		_source = GenerateSound(_frequency);
		_source.transform.SetParent(transform);
		_mallet=transform.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://idle
				break;
			case 1:
				break;
			case 2:
				Vector3 mousePos = Input.mousePosition;
				Ray r = _cam.ScreenPointToRay(Input.mousePosition);
				float t = _bowlHeight+_malletHeightOffset-r.origin.y/r.direction.y;
				float x = r.origin.x+t*r.direction.x;
				float z = r.origin.z+t*r.direction.z;
				Vector3 worldSpaceHit=new Vector3(x,_bowlHeight+_malletHeightOffset,z);
				_mallet.position = worldSpaceHit;
				_mallet.localPosition = PlaceOnCircle(_mallet.localPosition);
				break;
			default:
				break;
		}
    }

    void OnMouseDown()
    {
		if(_state!=0)
			return;
		SpinNext();
		//StartCoroutine(Spin());
    }

    private Vector3 PlaceOnCircle(Vector3 pos)
    {
        float angle = Mathf.Atan2(pos.z, pos.x);
        pos.x = radius * Mathf.Cos(angle);
        pos.z = radius * Mathf.Sin(angle);
        return pos;
    }

	public AudioSource GenerateSound(float frequency){
		float cycleDur=1f/frequency;
		_numCycles=Mathf.FloorToInt(frequency);
		_duration=cycleDur*_numCycles;
		int numSamples=Mathf.FloorToInt(_sampleRate*_duration);
		float [] samples = new  float[numSamples];
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)(numSamples-1);
			float t = t01*_duration;
			float v = Mathf.Sin(t*frequency*Mathf.PI*2)*_amplitude;
			v+=(Random.value*2-1)*_noiseMult;
			samples[i]=v;
		}

		AudioClip clip = AudioClip.Create("noise",numSamples,1,_sampleRate,false);
		clip.SetData(samples,0);
		GameObject source = new GameObject("Source-"+frequency.ToString("0.0"));
		AudioSource audio = source.AddComponent<AudioSource>();
		AudioLowPassFilter lp = source.AddComponent<AudioLowPassFilter>();
		audio.clip=clip;
		audio.spatialBlend=1f;
		audio.volume=0;
		audio.loop=true;
		return audio;
	}

	IEnumerator Spin(bool rotate=false, bool last=false){
		_state=1;
		float timer=0;
		_source.Play();
		_source.volume=0;
		float theta=0;
		if(rotate)
			_sand.CacheNextPattern();
		else if(last)
			_sand.CacheLastPattern();
		else
			_sand.CacheCurPattern();
		Rock[] rocks = FindObjectsOfType<Rock>();
		while(timer<_spinDur){
			timer+=Time.deltaTime;
			theta+=_spinSpeed*Time.deltaTime;
			Vector3 pos=_mallet.localPosition;
			pos.x=Mathf.Cos(theta)*radius;
			pos.z=Mathf.Sin(theta)*radius;
			_mallet.localPosition=pos;
			float tEnv=_envelope.Evaluate(timer/_spinDur);
			_source.volume=tEnv;
			_glowMat.SetFloat("_Emission",tEnv);
			float level=timer/_spinDur;
			level=level*level*level;
			_sand.Level(level);
			_sand.BlendToCachedPattern(level);
			_sand.UpdateMeshData();
			/*
			foreach(Rock r in rocks){
				if(r.OnBoard())
					r.transform.position+=Vector3.down*Time.deltaTime*_rockFallSpeed;
			}
			*/
			yield return null;
		}
		/*
		for(int i=rocks.Length-1; i>=0;i--)
			if(!rocks[i]._freshRock)
				Destroy(rocks[i].gameObject);
				*/
		_glowMat.SetFloat("_Emission",0);
		_source.volume=0;
		_source.Stop();
		_state=0;
	}

	public void SpinNext(){
		if(_state!=0)
			return;
		StartCoroutine(Spin(true));
	}

	public void SpinLast(){
		if(_state!=0)
			return;
		StartCoroutine(Spin(false,true));
	}
}
