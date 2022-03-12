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
	public Vector2 _thumpVolRange;
	public float _sustainVol;
	Vector3 _startPos;
	int _level;
	public static bool _holding;
	RockSpawner _spawner;
	Renderer _renderer;
	public bool _freshRock;

	//animating ring
	[Header("Animated ring")]
	public float _ringWidth;
	public float _minRadius;
	public float _maxRadius;
	Material _sandMat;
	Vector4 _ringVec;
	float _ringTimer;
	public float _ringFreq;
	public float _fallGrav;

	//excitement
	List<Wave> _excitedWaves;
	Rock _excitedRock;

	public Transform _wave;

	void Awake(){
		_buffer=transform.localScale.x*0.5f;
		_cam=Camera.main;
		_sand=FindObjectOfType<Sand>();
		_renderer=GetComponent<Renderer>();
		_light=transform.GetComponentInChildren<Light>();
		_spawner=FindObjectOfType<RockSpawner>();
		_sandMat=_sand.GetComponent<MeshRenderer>().material;
		_excitedWaves = new List<Wave>();
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
				//set sand vector
				Vector3 local = _sand.transform.InverseTransformPoint(worldSpaceHit);
				_ringTimer+=Time.deltaTime;
				float rad = Mathf.Lerp(_minRadius,_maxRadius,(Mathf.Sin(_ringTimer*Mathf.PI*2f*_ringFreq-Mathf.PI*0.3f)+1f)*0.5f);
				_ringVec = new Vector4(local.x,local.z,rad,_ringWidth);
				_sandMat.SetVector("_DropVec",_ringVec);
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
		float rad = _ringVec.z;
		_ringVec = Vector4.zero;
		_sandMat.SetVector("_DropVec",_ringVec);

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
		float rad01 = Mathf.InverseLerp(_minRadius,_maxRadius,rad);
		s.volume=Mathf.Lerp(_thumpVolRange.x,_thumpVolRange.y,rad01);

		//sand ripple
		_sand.Ripple(transform.position,rad,this);

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
		_state=1;
		_holding=true;
		//set sand vec
		_ringVec = new Vector4(-1f,0f,0f,0.02f);
		_ringTimer=0f;
		_sandMat.SetVector("_DropVec",_ringVec);
	}

	void OnMouseUp(){
		if(_state!=1)
			return;
		_holding=false;
		StopCharging();
		if(_sand.WithinBox(transform.position,_buffer)){
			StartCoroutine(Place());
		}
		else{
			StartCoroutine(FallToTheAbyss());
		}
		/*
		else if(_freshRock){
			Reset();
		}
		else{
			StartCoroutine(FallToTheAbyss());
		}
		*/
	}

	public void Reset(){
		transform.position=_startPos;
		_state=0;
		StopCharging(true);
	}

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

	IEnumerator FallToTheAbyss(){
		float vel=0;
		if(_freshRock)
		{
			_spawner.Reset();
			_freshRock=false;
		}
		_state=4;
		_ringVec = Vector4.zero;
		_sandMat.SetVector("_DropVec",_ringVec);
		enabled=false;
		while(transform.position.y>-30f){
			vel+=_fallGrav*Time.deltaTime;
			transform.position+=Vector3.down*vel*Time.deltaTime;
			yield return null;
		}
		Destroy(gameObject);
	}

	public void GetExcitedBy(Rock other,Wave otherWave){
		if(other!=null)
		{
			if(other==_excitedRock)
				return;
			_excitedRock=other;
		}
		else if(otherWave!=null){
			if(_excitedWaves.Contains(otherWave))
			{
				return;
			}
		}

		//currently ignoring other, but could use it
		StartCoroutine(EmitAirborneWave(other,otherWave));
	}

	IEnumerator EmitAirborneWave(Rock other,Wave otherWave){
		if(otherWave!=null)
		{
			_excitedWaves.Add(otherWave);
			otherWave._onDestroy+=CullOldWaves;
		}
		Transform waveT = Instantiate(_wave);
		Wave wave = waveT.GetComponent<Wave>();
		_excitedWaves.Add(wave);
		wave.Init(transform.position,this,otherWave);
		yield return null;
		if(other!=null)
		{
			float dur = _sand.GetRemainingRippleTime();
			yield return new WaitForSeconds(dur);
			_excitedRock=null;
		}
	}

	public bool IsPlaced(){
		return _state==3;
	}

	public void CullOldWaves(Wave wave){
		_excitedWaves.Remove(wave);
	}
}
