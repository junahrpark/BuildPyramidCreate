using UnityEngine;
using System.Collections;

public class ActivateDestructible : MonoBehaviour {
    
    private Rigidbody[] myRG;
	// Use this for initialization
	void Start () {

        myRG = GetComponentsInChildren<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0))
        { //Bottone Sinistro
            for (int x = 0; x < myRG.Length; x++)
            {
                myRG[x].isKinematic = false;
            }
        }

    }
}
