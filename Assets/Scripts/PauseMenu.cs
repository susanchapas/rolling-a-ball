using UnityEngine;
public class PauseMenu : MonoBehaviour
{
    void OnEnable()
    {
        Time.timeScale = 0;
    }
    void OnDisable()
    {
        Time.timeScale = 1;
    }
}
