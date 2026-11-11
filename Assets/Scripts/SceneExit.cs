using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneExit : MonoBehaviour
{
    public string sceneName = "Dungeon";
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out playerController player))
        {
            player.CarryPotions();
            SceneManager.LoadScene(sceneName);
        }
    }
}
