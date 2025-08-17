using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public Transform orientation;

    public float groundDrag;

    float horizontalInput;
    float verticalInput;

    public Transform playerStartPos;

    Vector3 moveDirection;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        PlayerInput();
    }

    void FixedUpdate()
    {
        PlayerMove();    
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //When the map resets, move the player
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space detected for Player");
            for (int i = 0; i < this.transform.childCount; i++)
            {
                this.transform.GetChild(i).transform.position = playerStartPos.position;
            }   
        }
    }

    private void PlayerMove()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        rb.linearDamping = groundDrag;
    }
}
