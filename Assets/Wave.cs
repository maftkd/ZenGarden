using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
{
	Material _mat;
	Vector4 _ringVec;
	float _radius;
	public float _growRate;
	public float _decayRate;
	public float _minPower;
	Collider[] _colliders;
	Rock _originRock;
	public float _power;

	public delegate void WaveEventHandler(Wave other);
	public event WaveEventHandler _onDestroy;

	public void Init(Vector3 worldPos, Rock r, Wave source){
		_mat=GetComponent<Renderer>().material;
		Sand sand = FindObjectOfType<Sand>();
		Vector2 coords = sand.WorldToNormalizedCoords(worldPos);
		_ringVec=_mat.GetVector("_RingParams");
		_ringVec.x=1-coords.x;
		_ringVec.y=1-coords.y;
		_radius=0;
		_mat.SetFloat("_Radius",_radius);
		_colliders = new Collider[5];
		_originRock=r;
		if(source!=null){
			_power=source._power*_decayRate;
			if(_power<_minPower)
			{
				if(_onDestroy!=null)
					_onDestroy.Invoke(this);
				Destroy(gameObject);
			}
		}
		else
			_power=1f;

		_ringVec.w=_power;
		_mat.SetVector("_RingParams",_ringVec);
	}

    // Update is called once per frame
    void Update()
    {
		_radius+=Time.deltaTime*_growRate;
		_mat.SetFloat("_Radius",_radius);
		if(_radius>1){
			if(_onDestroy!=null)
				_onDestroy.Invoke(this);
			Destroy(gameObject);
		}
		else{
			int cols = Physics.OverlapSphereNonAlloc(transform.position,_radius*10,_colliders);
			for(int i=0;i<cols;i++){
				if(_colliders[i].transform!=transform)
				{
					Rock r = _colliders[i].GetComponent<Rock>();
					if(r!=null&&!r._freshRock){
						r.GetExcitedBy(null,this);
					}
				}
			}
		}
    }
}
