using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneExit : MonoBehaviour
{
    public string sceneName = "Dungeon";
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<playerController>() != null)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
