using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T_CharacterControllerRigidbody : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log(name + " rigidbody: " + rb);
    }
}
