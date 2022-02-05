using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArpBall : MonoBehaviour
{
	Camera _cam;
	float _height;
	float _radius;
	public float _rollMult;
	Vector3 _vel;
	public float _speedMult;
	public float _decel;
	Sand _sand;
	Vector3 _prevPos;
	public float _minSpeed;
	public static bool _rolling;
	Material _mat;
	Vector4 _emission;
	public float _minDotToEmit;
	float _emissionFactor;
	public float _emissionDecay;

	void Awake(){
		_cam=Camera.main;
		_height=transform.position.y;
		_radius=transform.localScale.x*0.5f;//x is arbitrary, assume x=y=z
		_vel=Vector3.zero;
		_sand=FindObjectOfType<Sand>();
		_mat=GetComponent<Renderer>().material;
		_emission=Vector4.zero;
	}

    // Start is called before the first frame update
    void Start()
    {
		_sand.RaiseCircleFalloff(_sand.transform.InverseTransformPoint(transform.position),
				_radius*2f);
    }

    // Update is called once per frame
    void Update()
    {
		bool contact=false;
		if(Input.GetMouseButton(0)){
			Ray r = _cam.ScreenPointToRay(Input.mousePosition);
			float t = (_height-r.origin.y)/r.direction.y;
			float x = r.origin.x+t*r.direction.x;
			float z = r.origin.z+t*r.direction.z;
			Vector3 worldSpaceHit=new Vector3(x,_height,z);
			Vector3 diff=transform.position-worldSpaceHit;
			//check that hit is within ball radius
			if(diff.sqrMagnitude<=_radius*_radius){
				//Vector3 pDiff = diff.normalized;
				diff.Normalize();
				//diff = new Vector3(_sand._drawDir.x,0,_sand._drawDir.y);
				//diff.Normalize();
				//diff=Vector3.Lerp(pDiff,diff,0.5f);
				_vel=diff*_speedMult;
				Vector3 newPos=worldSpaceHit+diff*_radius;
				contact=true;
				_sand.RaiseLineFrom(_sand.transform.InverseTransformPoint(transform.position),
						_sand.transform.InverseTransformPoint(newPos),
						_radius*2f,new Vector2(_vel.x,_vel.z),false);
				RollTo(newPos,diff);
				_rolling=true;
			}
			else
				_rolling=false;
		}
		_prevPos=Input.mousePosition;
		if(!contact&&_vel.sqrMagnitude>_minSpeed*_minSpeed){
			_vel=Vector3.Lerp(_vel,Vector3.zero,_decel*Time.deltaTime);
			Vector3 newPos=transform.position+_vel*Time.deltaTime;
			RollTo(newPos,_vel.normalized);
			_sand.RaiseHalfCircleFalloff(_sand.transform.InverseTransformPoint(newPos),
					_radius*2f,new Vector2(_vel.x,_vel.z),false,true);
			_rolling=true;
		}
		else
			_rolling=false;

		if(!_rolling){
			if(_emissionFactor>0){
				_emissionFactor-=Time.deltaTime*_emissionDecay;
				if(_emissionFactor<0)
					_emissionFactor=0;
				_emission=Vector4.Lerp(Vector4.zero,_emission,_emissionFactor);
				_mat.SetVector("_Emission",_emission);
			}
		}
    }

	public void RollTo(Vector3 newPos,Vector3 diff){

		if(!_sand.WithinBox(newPos,_radius))
			return;
		float dist=(newPos-transform.position).magnitude;
		float circumfrence=Mathf.PI;
		float radians=dist/circumfrence;
		float degrees = radians*Mathf.Rad2Deg;
		Vector3 dir=Vector3.Cross(Vector3.up,diff);
		transform.Rotate(dir*degrees*_rollMult,Space.World);
		transform.position=newPos;
		Emit();
	}

	void Emit(){
		float yEmit=Mathf.Abs(Vector3.Dot(transform.up,Vector3.up));
		float xEmit=Mathf.Abs(Vector3.Dot(transform.right,Vector3.up));
		float zEmit=Mathf.Abs(Vector3.Dot(transform.forward,Vector3.up));
		_emission.x=Mathf.InverseLerp(_minDotToEmit,1,xEmit);
		_emission.y=Mathf.InverseLerp(_minDotToEmit,1,yEmit);
		_emission.z=Mathf.InverseLerp(_minDotToEmit,1,zEmit);
		_mat.SetVector("_Emission",_emission);
		_emissionFactor=1f;
	}
}
