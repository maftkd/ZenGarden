using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Rock : MonoBehaviour
{

	public float _projectHeight;
	Camera _cam;
	public Vector2 _sizeRange;
	Sand _sand;
	float _buffer;
	int _state;
	public float _placeDur;
	public RockSpawner _spawner;
	public AudioClip _hitAudio;
	public Transform _dustParts;
	bool _charging;
	Material _mat;
	float _emission;
	public float _maxEmission;
	public float _chargeRate;
	Light _light;
	public float _lightOffset;
	ParticleSystem _parts;
	public AudioClip [] _plops;
	public AudioClip [] _sustain;
	AudioSource _source;
	public float _sustainDecay;
	int _sourceIndex;
	public float _thumpVol;
	public float _sustainVol;

	void Awake(){
		Vector3 eulers = Random.insideUnitSphere*360f;
		transform.eulerAngles=eulers;
		transform.localScale=Vector3.one*Random.Range(_sizeRange.x,_sizeRange.y);
		_buffer=transform.localScale.x*0.5f;
		_cam=Camera.main;
		_sand=FindObjectOfType<Sand>();

		Ray r = _cam.ScreenPointToRay(Input.mousePosition);
		float t = (_projectHeight-r.origin.y)/r.direction.y;
		float x = r.origin.x+t*r.direction.x;
		float z = r.origin.z+t*r.direction.z;
		Vector3 worldSpaceHit=new Vector3(x,_projectHeight,z);
		transform.position=worldSpaceHit;

		float hue = Random.value;
		Color c = Color.HSVToRGB(hue,1,1);
		_mat=GetComponent<Renderer>().material;
		_mat.SetColor("_EmissionColor",c);
		_light=transform.GetComponentInChildren<Light>();
		_light.color=c;

		_parts=transform.GetComponentInChildren<ParticleSystem>();
		ParticleSystem.MainModule main = _parts.main;
		main.startColor=c;

		_sourceIndex = Random.Range(0,_sustain.Length);
		//_mixer
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				Ray r = _cam.ScreenPointToRay(Input.mousePosition);
				float t = (_projectHeight-r.origin.y)/r.direction.y;
				float x = r.origin.x+t*r.direction.x;
				float z = r.origin.z+t*r.direction.z;
				Vector3 worldSpaceHit=new Vector3(x,_projectHeight,z);
				transform.position=worldSpaceHit;

				if(Input.GetMouseButtonDown(0)){
					/*
					t=-r.origin.y/r.direction.y;
					x=r.origin.x+t*r.direction.x;
					z=r.origin.z+t*r.direction.z;
					Vector3 pos = new Vector3(x,0,z);
					*/
					if(_sand.WithinBox(transform.position,_buffer)){
						StartCoroutine(Place());
					}
				}
				break;
			case 1:
				if(_charging){
					if(!_light.enabled){
						_light.enabled=true;
						_parts.Play();
						if(_source!=null)
							StartCoroutine(FadeOutSource(_source));
						_source = Sfx.PlayOneShot3D(_sustain[_sourceIndex],transform.position);
						_source.volume=_sustainVol;
						//_source.outputAudioMixerGroup = _mixer.FindMatchingGroups(_mixerName)[0];
					}
					if(_emission<_maxEmission){
						_emission+=Time.deltaTime*_chargeRate;
						if(_emission>_maxEmission)
							_emission=_maxEmission;
						_light.intensity=_emission/_maxEmission;
						_mat.SetFloat("_Emission",_emission);
					}
				}
				else{
					if(_emission>0){
						_emission-=Time.deltaTime*_chargeRate;
						if(_emission<0)
							_emission=0;
						_light.intensity=_emission/_maxEmission;
						_mat.SetFloat("_Emission",_emission);
					}
					else{
						if(_light.enabled)
						{
							_light.enabled=false;
						}
					}
					if(_source!=null&&_source.volume>0){
						_source.volume=Mathf.Lerp(_source.volume,0,Time.deltaTime*_sustainDecay);
					}
				}
				break;
			default:
				break;
		}
    }

	IEnumerator Place(){
		_state=1;
		_spawner.Reset();
		Vector3 startPos=transform.position;
		Vector3 endPos=startPos;
		endPos.y=0;
		float timer=0;
		while(timer<_placeDur){
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(startPos,endPos,timer/_placeDur);
			yield return null;
		}
		AudioSource s = Sfx.PlayOneShot3D(_plops[Random.Range(0,_plops.Length)],transform.position);
		s.volume=_thumpVol;
		//s.outputAudioMixerGroup = _mixer.FindMatchingGroups(_mixerName)[0];
		Instantiate(_dustParts,transform.position,Quaternion.identity);
		_light.transform.position=transform.position+Vector3.up*_lightOffset;
	}

	void OnMouseOver(){
		if(Input.GetMouseButton(0))
			_charging=true;
		else
			StopCharging();
	}
	void OnMouseExit(){
		StopCharging();
	}

	void StopCharging(){
		_charging=false;
		_parts.Stop();
	}

	IEnumerator FadeOutSource(AudioSource s){
		while(s!=null&&s.volume>0)
		{
			s.volume=Mathf.Lerp(s.volume,0,Time.deltaTime*_sustainDecay);
			yield return null;
		}
	}
}
