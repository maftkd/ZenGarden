using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class Rock : MonoBehaviour
{

	public float _projectHeight;
	Camera _cam;
	Sand _sand;
	float _buffer;
	int _state;
	public float _placeDur;
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
	Vector3 _startPos;
	int _level;
	public static bool _holding;
	RockSpawner _spawner;
	Renderer _renderer;
	public bool _freshRock;

	void Awake(){
		_buffer=transform.localScale.x*0.5f;
		_cam=Camera.main;
		_sand=FindObjectOfType<Sand>();
		_renderer=GetComponent<Renderer>();
		_light=transform.GetComponentInChildren<Light>();
		_spawner=FindObjectOfType<RockSpawner>();
		_freshRock=true;
	}

    // Start is called before the first frame update
    void Start()
    {
		_startPos=transform.position;
    }

	Ray r;
    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://idle on side
				break;
			case 1://dragging
				r = _cam.ScreenPointToRay(Input.mousePosition);
				float t = (_projectHeight-r.origin.y)/r.direction.y;
				float x = r.origin.x+t*r.direction.x;
				float z = r.origin.z+t*r.direction.z;
				Vector3 worldSpaceHit=new Vector3(x,_projectHeight,z);
				transform.position=worldSpaceHit;
				break;
			case 2://thumping
				break;
			case 3://on board
				r = _cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				_charging=false;
				if(Physics.Raycast(r.origin,r.direction,out hit, 50f, 1)){
					if(hit.transform==transform)
					{
						if(Input.GetMouseButton(0))
							_charging=true;
					}
				}
				if(_charging){
					_spawner.Charge();
					if(!_light.enabled){
						_light.enabled=true;
						_parts.Play();
						if(_source!=null)
							StartCoroutine(FadeOutSource(_source));
						_source = Sfx.PlayOneShot3D(_sustain[_sourceIndex],transform.position);
						_source.volume=_sustainVol;
					}
					if(_emission<_maxEmission){
						_emission+=Time.deltaTime*_chargeRate;
						if(_emission>_maxEmission)
							_emission=_maxEmission;
						_light.intensity=_emission;
						_mat.SetFloat("_Emission",_emission);
					}
				}
				else{
					if(_parts.isPlaying)
						_parts.Stop();
					if(_emission>0){
						_emission-=Time.deltaTime*_chargeRate;
						if(_emission<0)
							_emission=0;
						_light.intensity=_emission;
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
		_state=2;

		
		//set colors
		float z01 = _sand.GetNormalizedZ(transform.position.z);
		_level = Mathf.FloorToInt(z01*SandAudio._instance._frequencyZones.Length);
		float hue = _level/(float)SandAudio._instance._frequencyZones.Length;
		Color c = Color.HSVToRGB(hue,1,1);
		_mat=_renderer.material;
		_mat.SetColor("_EmissionColor",c);
		_light.color=c;
		_parts=transform.GetComponentInChildren<ParticleSystem>();
		ParticleSystem.MainModule main = _parts.main;
		Color dustColor=Color.HSVToRGB(hue,0.33f,0.42f);
		main.startColor=c;

		//animate position;
		Vector3 startPos=transform.position;
		Vector3 endPos=startPos;
		endPos.y=0;
		float timer=0;
		while(timer<_placeDur){
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(startPos,endPos,timer/_placeDur);
			yield return null;
		}
		//play plop
		AudioSource s = Sfx.PlayOneShot3D(_plops[_level],transform.position);
		s.volume=_thumpVol;

		//sand ripple
		_sand.Ripple(transform.position);

		//reset spawner
		if(_freshRock)
		{
			_spawner.Reset();
			_freshRock=false;
		}
		_startPos=transform.position;

		//play dust
		Transform dust = Instantiate(_dustParts,transform.position,Quaternion.identity);

		//set light pos
		_light.transform.position=transform.position+Vector3.up*_lightOffset;

		_sourceIndex = _level;
		_charging=false;
		_state=3;
	}

	void OnMouseDown(){
		/*
		if(_sand.IsRippling())
			return;
			*/
		_state=1;
		_holding=true;
		_renderer.shadowCastingMode=ShadowCastingMode.On;
		//StopCharging(true);
	}

	void OnMouseUp(){
		if(_state!=1)
			return;
		_holding=false;
		//Debug.Log("stop charging");
		StopCharging();
		if(_sand.WithinBox(transform.position,_buffer)){
			StartCoroutine(Place());
		}
		else if(_freshRock){
			Reset();
		}
		else{
			Destroy(gameObject);
		}
	}

	public void Reset(){
		transform.position=_startPos;
		_state=0;
		StopCharging(true);
	}

	/*
	void OnMouseEnter(){
		if(Input.GetMouseButton(0))
		{
			_charging=true;
		}
		else
		{
			//Debug.Log("stop charging");
			StopCharging();
		}
	}
	*/

	/*
	void OnMouseExit(){
		StopCharging();
	}
	*/

	void StopCharging(bool fast=false){
		_charging=false;
		if(_parts!=null)
			_parts.Stop();
		if(fast){
			_emission=0;
			if(_source!=null)
				_source.volume=0;
			if(_light!=null)
			{
				_light.intensity=0;
			}
			if(_mat!=null)
			{
				_mat.SetFloat("_Emission",0);
			}
		}
	}

	IEnumerator FadeOutSource(AudioSource s){
		while(s!=null&&s.volume>0)
		{
			s.volume=Mathf.Lerp(s.volume,0,Time.deltaTime*_sustainDecay);
			yield return null;
		}
	}

	public bool OnBoard(){
		return _state==3;
	}
}
