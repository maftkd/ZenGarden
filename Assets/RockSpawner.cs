using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{

	public Transform _rockPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SpawnRock(){
		gameObject.SetActive(false);
		Transform rock = Instantiate(_rockPrefab);
		Rock r = rock.GetComponent<Rock>();
		r._spawner=this;
	}

	public void Reset(){
		gameObject.SetActive(true);

	}
}
