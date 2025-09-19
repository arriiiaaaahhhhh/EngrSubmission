using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //reference the transform
    Transform t;

    public static bool isSwimming;
    public static bool inWater;

    //if inwater and not swimming, then activate float on top of water code. 
    //if not in water, activate walk code
    //if swimming, activate swimming code

    public LayerMask waterMask;

    Rigidbody rb;

    [Header("Player Rotation")]
    public float sensitivity = 1;

    //clamp variables
    public float rotationMin;
    public float rotationMax;

    //mouse input
    float rotationX;
    float rotationY;

    [Header("Player Movement")]
    public float speed = 1;
    float moveX;
    float moveY;
    float moveZ;

    // Inventory
    InventorySystem iSystem;


    // Setup
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        t = transform;
        Cursor.lockState = CursorLockMode.Locked;

        inWater = false;
        rb.useGravity = true;

        iSystem = InventorySystem.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!UIBaseClass.menuOpen)
        {
            LookAround();
        }

        // For debugging
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void FixedUpdate()
    {
        if (!UIBaseClass.menuOpen)
        {
            SwimmingOrFloating();
            Move();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SwitchMovement();
    }

    private void OnTriggerExit(Collider other)
    {
        SwitchMovement();
    }

    void SwitchMovement()
    {
        //toggle isSwimming
        inWater = !inWater;

        //change the rigidbody accordingly
        rb.useGravity = !rb.useGravity;
    }

    void SwimmingOrFloating()
    {
        bool swimCheck = false;

        if (inWater)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(t.position.x, t.position.y + 0.5f, t.position.z), Vector3.down, out hit, Mathf.Infinity, waterMask))
            {
                if (hit.distance < 0.65)
                {
                    swimCheck = true;
                }
            }
            else
            {
                swimCheck = true;
            }
        }
        isSwimming = swimCheck;
        Debug.Log(isSwimming);
    }

    void LookAround()
    {
        //get the mouse input
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += Input.GetAxis("Mouse Y") * sensitivity;

        //clamp the values of x and y
        rotationY = Mathf.Clamp(rotationY, rotationMin, rotationMax);

        //setting the rotation value every update
        t.localRotation = Quaternion.Euler(-rotationY, rotationX, 0);
    }

    void Move()
    {
        //get the movement input
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Forward");
        moveY = Input.GetAxis("Vertical");

        //check if the player is standing still
        if (inWater) //If in water, velocity = 0
        {
            rb.velocity = new Vector2(0, 0);
        }
        else
        {
            if (moveX == 0 && moveZ == 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        //check if player is on land
        if (!inWater)
        {
            //move the character (land ver)
            t.Translate(new Quaternion(0, t.rotation.y, 0, t.rotation.w) * new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed, Space.World);
        }
        else
        {
            //check here if the player is swimming above water
            if (!isSwimming)
            {
                //if the character is floating on the water, clamp the moveY value, so they cant rise further than that.
                moveY = Mathf.Min(moveY, 0);

                //convert the local direction vector into a worldspace vector. 
                Vector3 clampedDirection = transform.TransformDirection(new Vector3(moveX, moveY, moveZ));
                //clamp the values
                clampedDirection = new Vector3(clampedDirection.x, Mathf.Min(clampedDirection.y, 0), clampedDirection.z);

                t.Translate(clampedDirection * Time.deltaTime * speed, Space.World);

            }
            else
            {
                //move the character (swimming ver)
                t.Translate(new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed);
                t.Translate(new Vector3(0, moveY, 0) * Time.deltaTime * speed, Space.World);
            }
        }
    }
}