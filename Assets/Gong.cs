using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gong : MonoBehaviour
{
    [SerializeField]
    Sand sand;

    int selectedPattern;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("Selected Pattern: " + selectedPattern);
            selectedPattern++;
            if(selectedPattern >= sand._patterns.Length)
            {
                selectedPattern = 0;
            }
            sand.GenerateMesh();
            sand.LoadPattern(selectedPattern);
        }
    }
}
