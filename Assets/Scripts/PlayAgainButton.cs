using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgainButton : MonoBehaviour
{
    public void OnPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
