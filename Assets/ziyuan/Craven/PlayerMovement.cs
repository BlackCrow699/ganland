using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator anim;

    private Rigidbody rb;
    private Transform cameraTransform;
    private Vector3 movement;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        Camera mainCamera = Camera.main;
        cameraTransform = mainCamera != null ? mainCamera.transform : null;
    }

    void Update(){ 
 
        float moveX = -Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            cameraTransform = mainCamera != null ? mainCamera.transform : null;
        }
        if (cameraTransform == null) return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = -cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        movement = (camForward * moveZ + camRight * moveX).normalized;

        if (movement.magnitude > 0)
        {
            if (anim == null) return;
            if (moveX != 0)
            {
                anim.Play("WalkSide");
                transform.localScale = new Vector3(-Mathf.Sign(moveX), 1, 1);
            }
            else if (moveZ > 0)
                anim.Play("WalkUp");
            else if (moveZ < 0)
                anim.Play("WalkDown");
        }
        else
        {
            anim.Play("Idle");
        }
    }

    void FixedUpdate()
    {
        if (rb != null) rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
