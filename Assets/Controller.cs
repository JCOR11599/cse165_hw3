using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Objects
    public GameObject desk;
    public GameObject chair;

    // Player
    public GameObject player;

    // Hands (just right hand for now)
    public GameObject leftHand;
    public GameObject rightHand;
    public LineRenderer rightHandLine;
    private Vector3 prevLeftHandPosition;
    private Vector3 prevRightHandPosition;
    private Vector3 prevPlayerPosition;
    private Vector3 midPoint;
    private bool held = false;

    // Raycast
    public float maxLineLength = 100.0f;
    public float spawnDistance = 5.0f;
    public Material selectedMaterial;
    private GameObject selectedObject;
    private GameObject lastSelected;
    private bool isMoving = false;
    private bool isRaycasting = false;

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
            selectedObject = Instantiate(chair, rightHand.transform.position + (rightHand.transform.forward * spawnDistance), Quaternion.identity);
            held = true;

        }
        else if (OVRInput.GetUp(OVRInput.Button.One))
        {
            selectedObject = null;
            held = false;
        }

        // Spawn chair when button two is pressed
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            selectedObject = Instantiate(desk, rightHand.transform.position + (rightHand.transform.forward * spawnDistance), Quaternion.identity);
            held = true;
        }
        else if (OVRInput.GetUp(OVRInput.Button.Two))
        {
            selectedObject = null;
            held = false;
        }

        // Switch between Ray-Casting and Virtual Hand
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            isRaycasting = !isRaycasting;
            rightHandLine.enabled = isRaycasting;
        }

        // Currently raycasting
        if (isRaycasting)
        {
            // Calculate endpoint
            Vector3 leftEndpoint = leftHand.transform.position + (leftHand.transform.forward * maxLineLength);
            Vector3 rightEndpoint = rightHand.transform.position + (rightHand.transform.forward * maxLineLength);

            // Raycasts
            RaycastHit hit;
            Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
            if (Physics.Raycast(ray, out hit))
            {
                // track last selectedObject
                lastSelected = selectedObject;

                // point to object being hit
                if (!held)
                {
                    selectedObject = hit.transform.gameObject;
                }

                // prevent selecting walls and floor
                if (selectedObject.CompareTag("Selectable"))
                {
                    // change endpoint to hit location
                    rightEndpoint = hit.point;

                    // Highlight object
                    selectedObject.GetComponent<Outline>().enabled = true;

                    // Move object according to controller
                    if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.9 || held)
                    {
                        if (!isMoving)
                        {
                            selectedObject.transform.SetParent(rightHand.transform);
                            selectedObject.GetComponent<Rigidbody>().isKinematic = true;
                            isMoving = true;
                        }
                    }
                    // Release object
                    else
                    {
                        if (selectedObject)
                        {
                            selectedObject.transform.SetParent(null);
                            selectedObject.GetComponent<Rigidbody>().isKinematic = false;
                            isMoving = false;
                        }
                    }
                }
                else
                {
                    selectedObject = null;
                }
            }
            else
            {
                selectedObject = null;
            }

            // Unhighlight object
            if (lastSelected && (lastSelected != selectedObject || selectedObject == null))
            {
                lastSelected.GetComponent<Outline>().enabled = false;

                lastSelected = null;
            }

            // Draw line from hand
            rightHandLine.SetPosition(0, rightHand.transform.position);
            rightHandLine.SetPosition(1, rightEndpoint);
        }
        else
        {
            // Rotating 
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) && OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
            {
                prevLeftHandPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                prevRightHandPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                midPoint = prevLeftHandPosition + (prevRightHandPosition - prevLeftHandPosition) / 2;
            }
            // Right Hand Air Grab
            else if (!OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) && OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
            {
                prevPlayerPosition = player.transform.position;
                prevRightHandPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            }
            // Left Hand Air Grab
            else if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) && !OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
            {
                prevPlayerPosition = player.transform.position;
                prevLeftHandPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            }

            // Rotating 
            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            {
                float rAngle = Vector3.Angle(prevRightHandPosition - midPoint, OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) - midPoint) * Time.deltaTime;
                float lAngle = Vector3.Angle(prevLeftHandPosition - midPoint, OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) - midPoint) * Time.deltaTime;
                player.transform.RotateAround(midPoint, Vector3.up, rAngle - lAngle);
            }
            // Grabbing the Air Technique
            else if (!OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            {
                player.transform.position = prevPlayerPosition + (prevRightHandPosition - OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
            }
            // Grabbing the Air Technique
            else if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && !OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            {
                player.transform.position = prevPlayerPosition + (prevLeftHandPosition - OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
            }
        }
    }

    void LateUpdate()
    {
        /*
        if (!isRaycasting)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            {
                player.transform.position = prevPlayerPosition + 2 * (prevRightHandPosition - OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
            }
        }
        */
    }
}
