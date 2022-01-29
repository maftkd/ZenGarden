//Singing bowl
//
//This is probably temp, but just a fun way to reset the board while testing
//
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingingBowl : MonoBehaviour
{
	Sand _sand;
    [SerializeField]
    GameObject mallet;
    [SerializeField]
    Vector3 offset;
    [SerializeField]
    float radius;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    float boost;

    Vector3 previousPosition;

    float volSmoothed;
    [SerializeField]
    float volLerp;

    void Awake()
    {
        _sand = FindObjectOfType<Sand>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(mallet != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 8;
            mallet.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
            mallet.transform.position = PlaceOnCircle(mallet.transform.position);

            float volCur = (previousPosition - mallet.transform.position).magnitude * boost;
            volSmoothed = Mathf.Lerp(volSmoothed,volCur,Time.deltaTime*volLerp);
            audioSource.volume = volSmoothed;
            previousPosition = mallet.transform.position;
        }
    }

    void OnMouseDown()
    {
        _sand.Init();
        audioSource.Play();
    }

    private Vector3 PlaceOnCircle(Vector3 pos)
    {
        float angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
        pos.x = radius * Mathf.Cos(angle * Mathf.Deg2Rad) + transform.position.x;
        pos.y = transform.position.y;
        pos.z = radius * Mathf.Sin(angle * Mathf.Deg2Rad) + transform.position.z;

        return pos;
    }
}
