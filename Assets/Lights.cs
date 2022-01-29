using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    [SerializeField]
    Light sun;
    [SerializeField]
    List<Light> lights;
    [SerializeField]
    float daylight;
    [SerializeField]
    float nightlight;
    [SerializeField]
    float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sun.transform.Rotate(Vector3.right, speed);

        //sun.intensity -= speed;
        //if (sun.intensity < daylight)
        //{
        //    foreach (Light light in lights)
        //    {
        //        light.intensity += speed;
        //        light.GetComponent<Flicker>().enabled = true;
        //    }
        //}
        //if(sun.intensity <= nightlight)
        //{
        //    foreach (Light light in lights)
        //    {
        //        light.intensity -= speed;
        //        if(light.intensity <= 0)
        //        {
        //            light.GetComponent<Flicker>().enabled = false;
        //        }
        //    }
        //}
    }
}
