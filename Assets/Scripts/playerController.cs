using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class playerController : MonoBehaviour
{
    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private GameObject northDoor;
    public float speed = 5;
    public float jumpForce = 5;
    public float turnSpeed = 720;
    public float groundCheckDistance = 0.6f;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0.0f;
        count = 0;
        northDoor = GameObject.Find("Walls/North Door");

        SetCountText();
        winTextObject.SetActive(false);
    }
    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();

        movementX = movementVector.x;
        movementY = movementVector.y;
    }
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        rb.linearVelocity = movement * speed + Vector3.up * rb.linearVelocity.y;
        if (movement != Vector3.zero)
        {
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(movement), turnSpeed * Time.fixedDeltaTime));
        }
    }
    void OnJump()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
        else if(other.gameObject.CompareTag("KillFloor"))
        {
            Lose();
        }
    }
    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 3 && northDoor != null && northDoor.activeSelf)
        {
            northDoor.SetActive(false);
            PetFollow pet = FindAnyObjectByType<PetFollow>();
            if (pet != null)
            {
                pet.BakeNavMesh();
            }
        }

        if(count >= 15)
        {
            winTextObject.SetActive(true);
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Destroy(enemy);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Lose();
        }
    }
    void Lose()
    {
        Destroy(gameObject);
        winTextObject.SetActive(true);
        winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
    }
}
