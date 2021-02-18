using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Objects
    public GameObject desk;
    public GameObject chair;

    // Hands
    public GameObject leftHand;
    public GameObject rightHand;

    // Raycast
    public float maxLineLength = 100.0f;
    public float spawnDistance = 2.0f;
    public LineRenderer leftHandLine;
    public LineRenderer rightHandLine;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Spawn desk when button one is pressed
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Instantiate(desk, new Vector3(0, 10, 0), Quaternion.identity);
        }
        // Spawn chair when button two is pressed
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            Instantiate(chair, new Vector3(0, 10, 0), Quaternion.identity);
        }

        // Draw line from hands
        leftHandLine.SetPosition(0, leftHand.transform.position);
        leftHandLine.SetPosition(1, leftHand.transform.position + (leftHand.transform.forward * maxLineLength));
        rightHandLine.SetPosition(0, rightHand.transform.position);
        rightHandLine.SetPosition(1, rightHand.transform.position + (rightHand.transform.forward * maxLineLength));


    }
}
