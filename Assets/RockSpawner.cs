using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{

	public float _rockSpacing;
	public Transform _rockPrefab;
	public int _numRocks;
	public Vector2 _sizeRange;

	void Awake(){
		GetComponent<MeshRenderer>().enabled=false;
		float totalDist=(_numRocks-1)*_rockSpacing;
		float minZLocal=-totalDist*0.5f;
		float maxZLocal=-minZLocal;
		for(int i=0; i<_numRocks; i++){
			Transform rock = Instantiate(_rockPrefab,transform);
			Vector3 eulers = Random.insideUnitSphere*360f;
			rock.eulerAngles=eulers;
			rock.localScale=Vector3.one*Random.Range(_sizeRange.x,_sizeRange.y);

			rock.localPosition=Vector3.forward*Mathf.Lerp(minZLocal,maxZLocal,i/(float)(_numRocks-1));
			//Rock r = rock.GetComponent<Rock>();
			//r._spawner=this;
		}
	}
}
