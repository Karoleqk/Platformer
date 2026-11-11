using UnityEngine;
using UnityEngine.SceneManagement;

public class Castle : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        SceneManager.LoadScene("Level1_1");
    }
}
