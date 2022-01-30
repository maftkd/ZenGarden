using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandAudio : MonoBehaviour
{

	AudioSource _source;
	public int _sampleRate;
	public float _frequency;
	float _duration;
	int _numCycles;
	public float _amplitude;
	public float _noiseMult;

	[Tooltip("The lerp factor for audio volume to update while drawing")]
	public float _audioSensitivity;
	float _targetVolume;
	float _actualVolume;
	float _avgCol;

	[Header("Low Pass")]
	public Vector2 _freqRange;

	[Header("Frequencies")]
	public float [] _frequencies;
	Sand _sand;
	AudioSource [] _sources;
	AudioLowPassFilter [] _filters;

	void Awake(){
		_sand=FindObjectOfType<Sand>();
		_sources = new AudioSource[_frequencies.Length];
		_filters = new AudioLowPassFilter[_frequencies.Length];
		for(int i=0; i<_frequencies.Length; i++)
		{
			_sources[i]=GenerateSound(_frequencies[i]);
			_sources[i].transform.SetParent(transform);
			_filters[i]=_sources[i].GetComponent<AudioLowPassFilter>();
		}
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
		audio.Play();
		return audio;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }
	
    // Update is called once per frame
    void Update()
    {
		_actualVolume=Mathf.Lerp(_actualVolume,_targetVolume,_audioSensitivity*Time.deltaTime);
		float z01 = _sand.GetNormalizedZ(transform.position.z);
		int curRegion=Mathf.FloorToInt(Mathf.Lerp(0,_frequencies.Length,z01));
		if(curRegion>=_frequencies.Length)
			curRegion=_frequencies.Length-1;
		for(int i=0; i<_frequencies.Length; i++){
			if(i==curRegion)
			{
				_sources[i].volume=Mathf.Lerp(_sources[i].volume,_actualVolume,_audioSensitivity*Time.deltaTime);
			}
			else
			{
				_sources[i].volume=Mathf.Lerp(_sources[i].volume,0,_audioSensitivity*Time.deltaTime);
			}
			_filters[i].cutoffFrequency=Mathf.Lerp(_freqRange.x,_freqRange.y,1-_avgCol);
		}
		//_source.volume=_actualVolume;
		//_lp.
    }

	public void SetPosition(Vector3 pos){
		transform.position=pos;
	}

	public void SetTargetVolume(float v){
		_targetVolume=v;
	}

	public void SetAverageColor(float v){
		_avgCol=v;
	}
}
