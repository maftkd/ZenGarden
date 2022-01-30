using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{

	public CanvasGroup _overlay;
	public float _fadeInDur;
	public SandAudio _sa;
	AudioSource _sandSound;
	public float _sandAudioFreq;
	public float _sandAudioVol;
	public AudioSource _atmosphere;
	public float _atmosphereVol;
	public Color _sandDark;
	public Color _sandLight;
	public float _sandFallTime;
	public ParticleSystem _sandParts;
	public float _sandFadeOutDur;
	public float _flipDur;
	public AnimationCurve _flipCurve;
	public AudioSource _music;
	public float _musicVol;
	public CanvasGroup _buttCover;
	public Material _space;
	public float _spaceFadeDur;
	public GameObject _sandbox;
	public bool _skipIntro;
	public GameObject _bowl;

	void Awake(){
		if(!_skipIntro){
			_sandSound=_sa.GenerateSound(_sandAudioFreq);
			_sandSound.transform.SetParent(transform);
		}
	}

    // Start is called before the first frame update
    void Start()
    {
		if(_skipIntro){
			MeshRenderer [] mr = transform.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer m in mr)
				m.enabled=false;
			_overlay.alpha=0f;
			_buttCover.alpha=0f;
			_sandbox.SetActive(true);
			_sandParts.Stop();
			_space.SetFloat("_Fade",0);
			_bowl.SetActive(true);
			return;
		}

		_space.SetFloat("_Fade",1);
		_sandbox.SetActive(false);
		_bowl.SetActive(false);
		MeshRenderer [] mr2 = transform.GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer m in mr2)
			m.enabled=true;
		_buttCover.alpha=1f;
		StartCoroutine(IntroR());
		StartCoroutine(SandFall());
    }

	IEnumerator IntroR(){
		float timer=0;
		while(timer<_fadeInDur){
			timer+=Time.deltaTime;
			_overlay.alpha=1-(timer/_fadeInDur);
			_sandSound.volume=timer/_fadeInDur*_sandAudioVol;
			_atmosphere.volume=timer/_fadeInDur*_atmosphereVol;
			yield return null;
		}
	}

	IEnumerator SandFall(){
		MeshRenderer bot=transform.GetChild(0).GetComponent<MeshRenderer>();
		MeshRenderer top=transform.GetChild(1).GetComponent<MeshRenderer>();
		Material botMat=bot.material;
		Material topMat=top.material;
		botMat.SetColor("_Color",_sandLight);
		topMat.SetColor("_Color",_sandDark);
		botMat.SetFloat("_FillAmount",0);
		botMat.SetFloat("_Flip",0);
		topMat.SetFloat("_FillAmount",0);
		topMat.SetFloat("_Flip",1);

		var col = _sandParts.colorOverLifetime;
		Gradient grad = new Gradient();
		grad.SetKeys(new GradientColorKey[] { new GradientColorKey(_sandDark, 0.0f), new GradientColorKey(_sandLight, 0.5f) }, 
				new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } );
		col.color=grad;

		float timer=0;
		while(timer<_sandFallTime){
			timer+=Time.deltaTime;
			float t= timer/_sandFallTime;
			botMat.SetFloat("_FillAmount",t);
			topMat.SetFloat("_FillAmount",t);
			yield return null;
		}
		_sandParts.Stop();
		StartCoroutine(FadeOutSandSound());

		Quaternion curRot=transform.rotation;
		transform.Rotate(-Vector3.forward*180f);
		Quaternion endRot=transform.rotation;
		timer=0;
		while(timer<_flipDur){
			timer+=Time.deltaTime;
			float t = _flipCurve.Evaluate(timer/_flipDur);
			transform.rotation=Quaternion.Slerp(curRot,endRot,t);
			botMat.SetColor("_Color",Color.Lerp(_sandLight,_sandDark,t));
			topMat.SetColor("_Color",Color.Lerp(_sandDark,_sandLight,t));
			yield return null;
		}

		timer=0;
		_sandParts.transform.Rotate(Vector3.up*180);
		_sandParts.Play();
		botMat.SetFloat("_Flip",1);
		botMat.SetFloat("_FillAmount",0);
		topMat.SetFloat("_Flip",0);
		topMat.SetFloat("_FillAmount",0);
		while(timer<_fadeInDur){
			timer+=Time.deltaTime;
			_overlay.alpha=(timer/_fadeInDur);
			_music.volume=timer/_fadeInDur*_musicVol;
			botMat.SetFloat("_FillAmount",timer/_sandFallTime);
			topMat.SetFloat("_FillAmount",timer/_sandFallTime);
			yield return null;
		}

		_sandParts.Stop();
		_overlay.alpha=1f;
		bot.enabled=false;
		top.enabled=false;
		GetComponent<MeshRenderer>().enabled=false;
		_buttCover.alpha=0f;
		_sandbox.SetActive(true);
		_bowl.SetActive(true);

		timer=0;
		while(timer<_fadeInDur){
			timer+=Time.deltaTime;
			_overlay.alpha=1-timer/_fadeInDur;
			yield return null;
		}
		_overlay.alpha=0f;


		timer=0;
		while(timer<_spaceFadeDur){
			timer+=Time.deltaTime;
			_space.SetFloat("_Fade",1-timer/_spaceFadeDur);
			yield return null;
		}
		_space.SetFloat("_Fade",0);
	}

	IEnumerator FadeOutSandSound(){
		float timer=0;
		while(timer<_sandFadeOutDur)
		{
			timer+=Time.deltaTime;
			_sandSound.volume=(1-timer/_sandFadeOutDur)*_sandAudioVol;
			yield return null;
		}
	}
}
