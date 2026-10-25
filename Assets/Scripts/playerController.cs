using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
public class playerController : MonoBehaviour
{
    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private GameObject northDoor;
    private int lives;
    private Vector3 checkpoint;
    private Transform currentPlatform;
    private readonly HashSet<Transform> platforms = new HashSet<Transform>();
    private float dashEndTime;
    private float nextDashTime;
    private TreasureChest chest;
    public float speed = 5;
    public float jumpForce = 5;
    public float dashMultiplier = 1.5f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 2;
    public float turnSpeed = 720;
    public float groundCheckDistance = 0.6f;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject exit;
    public AudioClip pickupSound;
    public AudioClip lifeLostSound;
    public AudioClip gameOverSound;
    public GameObject[] hearts;
    public Slider dashSlider;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0.0f;
        count = 0;
        lives = hearts.Length;
        checkpoint = rb.position;
        foreach (EnemyMovement enemy in FindObjectsByType<EnemyMovement>())
        {
            platforms.Add(enemy.platform);
        }
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
    void Update()
    {
        dashSlider.value = 1 - (nextDashTime - Time.time) / (dashDuration + dashCooldown);
    }
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        float currentSpeed = Time.time < dashEndTime ? speed * dashMultiplier : speed;

        rb.linearVelocity = movement * currentSpeed + Vector3.up * rb.linearVelocity.y;
        if (movement != Vector3.zero)
        {
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(movement), turnSpeed * Time.fixedDeltaTime));
        }
        UpdateCheckpoint();
    }
    void UpdateCheckpoint()
    {
        if (!Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        foreach (Transform platform in platforms)
        {
            if (platform != currentPlatform && hit.transform.IsChildOf(platform))
            {
                currentPlatform = platform;
                checkpoint = rb.position;
                return;
            }
        }
    }
    void OnSprint()
    {
        if (Time.time >= nextDashTime)
        {
            dashEndTime = Time.time + dashDuration;
            nextDashTime = dashEndTime + dashCooldown;
        }
    }
    void OnJump()
    {
        if (Time.timeScale > 0 && Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            PlaySound(pickupSound);
            count++;
            SetCountText();
        }
        else if(other.gameObject.CompareTag("KillFloor"))
        {
            Lose();
        }
        else if(other.TryGetComponent(out TreasureChest found))
        {
            chest = found;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TreasureChest>() != null)
        {
            chest = null;
        }
    }
    void OnInteract()
    {
        if (chest != null)
        {
            chest.Toggle(this);
        }
    }
    public void Win()
    {
        winTextObject.SetActive(true);
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
            if (exit != null)
            {
                exit.SetActive(true);
            }
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
        if (lives <= 0)
        {
            return;
        }

        lives--;
        hearts[lives].SetActive(false);
        if (lives > 0)
        {
            PlaySound(lifeLostSound);
            Respawn();
            return;
        }

        PlaySound(gameOverSound);
        Destroy(gameObject);
        winTextObject.SetActive(true);
        winTextObject.GetComponentInChildren<TextMeshProUGUI>().text = "You Lose!";
    }
    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }
    void Respawn()
    {
        rb.position = checkpoint;
        rb.linearVelocity = Vector3.zero;
        foreach (EnemyMovement enemy in FindObjectsByType<EnemyMovement>())
        {
            enemy.ResetPosition();
        }
    }
}
