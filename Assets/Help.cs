using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help : MonoBehaviour
{
	public RectTransform _helpScreen;
	Vector2 _hideAnchorPos;
	Vector2 _showAnchorPos;
	Vector3 _hideScale;
	Vector3 _showScale;
	public float _animDur;
	public AnimationCurve _animCurve;
	bool _active;

	void Awake(){
		_hideAnchorPos=new Vector2(Screen.width,Screen.height);
		_showAnchorPos=Vector2.zero;
		_helpScreen.anchoredPosition=_hideAnchorPos;
		_hideScale=Vector3.zero;
		_showScale=Vector3.one;
		_helpScreen.localScale=_hideScale;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ToggleActivate(){
		_active=!_active;
		Activate(_active);
	}

	public void Activate(bool a){
		StopAllCoroutines();
		if(a)
			StartCoroutine(ShowHelp());
		else
			StartCoroutine(HideHelp());
	}

	IEnumerator ShowHelp(){
		_active=true;
		_helpScreen.gameObject.SetActive(true);
		float timer=0;
		Vector2 startAnchorPos=_helpScreen.anchoredPosition;
		Vector3 startScale = _helpScreen.localScale;
		while(timer<_animDur){
			timer+=Time.deltaTime;
			float t = _animCurve.Evaluate(timer/_animDur);
			_helpScreen.anchoredPosition=Vector2.Lerp(startAnchorPos,_showAnchorPos,t);
			_helpScreen.localScale=Vector3.Lerp(startScale,_showScale,t);
			yield return null;
		}
		_helpScreen.anchoredPosition=_showAnchorPos;
		_helpScreen.localScale=_showScale;
	}

	IEnumerator HideHelp(){
		_active=false;
		float timer=0;
		Vector2 startAnchorPos=_helpScreen.anchoredPosition;
		Vector3 startScale = _helpScreen.localScale;
		while(timer<_animDur){
			timer+=Time.deltaTime;
			float t = _animCurve.Evaluate(timer/_animDur);
			_helpScreen.anchoredPosition=Vector2.Lerp(startAnchorPos,_hideAnchorPos,t);
			_helpScreen.localScale=Vector3.Lerp(startScale,_hideScale,t);
			yield return null;
		}
		_helpScreen.anchoredPosition=_hideAnchorPos;
		_helpScreen.localScale=_hideScale;
		_helpScreen.gameObject.SetActive(false);
	}

	public bool IsActive(){
		return _active;
	}
}
