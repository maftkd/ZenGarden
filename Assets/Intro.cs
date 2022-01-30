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

	void Awake(){
		_sandSound=_sa.GenerateSound(_sandAudioFreq);
		_sandSound.transform.SetParent(transform);
	}

    // Start is called before the first frame update
    void Start()
    {
		_space.SetFloat("_Fade",1);
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
			yield return null;
		}

		timer=0;
		while(timer<_fadeInDur){
			timer+=Time.deltaTime;
			_overlay.alpha=(timer/_fadeInDur);
			_music.volume=timer/_fadeInDur*_musicVol;
			yield return null;
		}

		bot.enabled=false;
		top.enabled=false;
		GetComponent<MeshRenderer>().enabled=false;
		_overlay.alpha=0f;
		_buttCover.alpha=0f;

		_sandbox.SetActive(true);

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
