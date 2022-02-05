using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rake : MonoBehaviour
{
	Camera _cam;
	float _height;
	float _radius;
	Sand _sand;
	public float _minDist;
	public float _angleLerp;
	public float _spokeRadius;
	public int _numSpokes;
	public float _rakeRadius;
	public static bool _raking;
	Vector3 _closePoint;
	Material _mat;
	public Color _emissionColor;
	public float _emissionMult;
	float _emissionFactor;
	public float _emissionDecay;

	void Awake(){
		_cam=Camera.main;
		_height=transform.position.y;
		_radius=transform.localScale.x*0.5f;//x is arbitrary, assume x=y=z
		_sand=FindObjectOfType<Sand>();
		_mat=GetComponent<Renderer>().materials[2];
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetMouseButton(0)){
			Ray r = _cam.ScreenPointToRay(Input.mousePosition);
			float t = (_height-r.origin.y)/r.direction.y;
			float x = r.origin.x+t*r.direction.x;
			float z = r.origin.z+t*r.direction.z;
			Vector3 worldSpaceHit=new Vector3(x,_height,z);
			if(!_sand.WithinBox(worldSpaceHit,_radius)){
				worldSpaceHit=_sand.ClosestPoint(worldSpaceHit,_radius);
				_closePoint=worldSpaceHit;
			}

			Vector3 diff=worldSpaceHit-transform.position;
			//check that hit is within ball radius
			if(diff.sqrMagnitude<=_radius*_radius&&diff.sqrMagnitude>_minDist*_minDist){
				float dist = diff.magnitude;
				diff.Normalize();
				Vector3 newPos=worldSpaceHit;
				transform.forward=Vector3.Lerp(transform.forward,diff.normalized,_angleLerp*Time.deltaTime*dist);
				_emissionFactor=dist*_emissionMult;
				if(_raking)
					_mat.SetColor("_EmissionColor",Color.Lerp(Color.black,_emissionColor,_emissionFactor));
				for(int i=0; i<_numSpokes; i++){
					Vector3 offset=-transform.forward;
					float xt = i/(float)(_numSpokes-1);
					float dx = Mathf.Lerp(-_rakeRadius,_rakeRadius,xt);
					offset+=transform.right*dx;
					_sand.RaiseLineFrom(_sand.transform.InverseTransformPoint(transform.position+offset),
							_sand.transform.InverseTransformPoint(newPos+offset),_spokeRadius*2f,new Vector2(transform.forward.x,transform.forward.z),true);
				}
				transform.position=newPos;
				_raking=true;
			}
			else
				_raking=false;
		}
		else
			_raking=false;
		if(!_raking){
			if(_emissionFactor>0){
				_emissionFactor-=Time.deltaTime*_emissionDecay;
				if(_emissionFactor<0)
					_emissionFactor=0;
				_mat.SetColor("_EmissionColor",Color.Lerp(Color.black,_emissionColor,_emissionFactor));
			}
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		if(_closePoint!=null)
			Gizmos.DrawWireSphere(_closePoint,_radius);
	}
}
