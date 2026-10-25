using UnityEngine;
public class TreasureChest : MonoBehaviour
{
    public Transform lid;
    public GameObject prize;
    public AudioClip openSound;
    public float openAngle = 110;
    public float lidSpeed = 180;
    private Quaternion closedRotation;
    private playerController looter;
    private bool open;
    private bool won;
    void Start()
    {
        closedRotation = lid.localRotation;
        prize.SetActive(false);
    }
    public void Toggle(playerController player)
    {
        if (won)
        {
            return;
        }

        open = !open;
        if (open)
        {
            looter = player;
            prize.SetActive(true);
            AudioSource.PlayClipAtPoint(openSound, Camera.main.transform.position);
        }
    }
    void Update()
    {
        Quaternion target = open ? closedRotation * Quaternion.Euler(-openAngle, 0, 0) : closedRotation;
        lid.localRotation = Quaternion.RotateTowards(lid.localRotation, target, lidSpeed * Time.deltaTime);
        if (looter != null && !open && !won && lid.localRotation == target)
        {
            won = true;
            prize.SetActive(false);
            looter.Win();
        }
    }
}
