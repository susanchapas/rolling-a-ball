using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class NpcDialogue : MonoBehaviour, IInteractable
{
    public string speaker = "Skeleton Guard";
    [TextArea] public string[] intro;
    [TextArea] public string[] puns;
    [TextArea] public string[] thanks;
    public GameObject panel;
    public AudioClip vanishSound;
    private TextMeshProUGUI text;
    private string[] lines;
    private int line;
    private int pun;
    private bool asking;
    private playerController priest;
    void Start()
    {
        text = panel.GetComponentInChildren<TextMeshProUGUI>(true);
    }
    public void ShowPrompt(bool visible)
    {
        lines = intro;
        line = -1;
        asking = false;
        panel.SetActive(visible);
        Say("...", "Press E to talk");
    }
    public void Interact(playerController player)
    {
        if (asking || !isActiveAndEnabled)
        {
            return;
        }

        priest = player;
        line++;
        if (line == lines.Length - 1 && lines != thanks)
        {
            asking = true;
            Say(lines[line], "Press Y for yes, N for no");
        }
        else if (line < lines.Length)
        {
            Say(lines[line], "Press E to continue");
        }
        else
        {
            Vanish();
        }
    }
    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (!asking || keyboard == null)
        {
            return;
        }

        if (keyboard.yKey.wasPressedThisFrame)
        {
            priest.GivePotions();
            Play(thanks);
        }
        else if (keyboard.nKey.wasPressedThisFrame)
        {
            Play(new[] { puns[pun++ % puns.Length] + "\nNow... do we have a deal?" });
        }
    }
    void Play(string[] sequence)
    {
        asking = false;
        lines = sequence;
        line = -1;
        Interact(priest);
    }
    void Say(string body, string hint)
    {
        text.text = $"<b>{speaker}</b>\n{body}\n\n<size=60%>{hint}</size>";
    }
    void Vanish()
    {
        panel.SetActive(false);
        if (vanishSound != null)
        {
            AudioSource.PlayClipAtPoint(vanishSound, Camera.main.transform.position);
        }
        gameObject.SetActive(false);
    }
}
