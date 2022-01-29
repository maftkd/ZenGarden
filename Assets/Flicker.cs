using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicker : MonoBehaviour
{
    [SerializeField]
    Light light;
    [SerializeField]
    float range;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = light.intensity + Random.Range(-range, range);
    }
}
