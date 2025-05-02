using UnityEngine;
using UnityEngine.SceneManagement;

public class HomePage : MonoBehaviour
{
    public GameObject homeCanvas;
    public GameObject gamePageCanvas;
    public void OnOnlineClick()
    {
        homeCanvas.SetActive(false);
        gamePageCanvas.SetActive(true);

        NetworkManager.Instance.ConnectToMatchmaking();
        GameManager.Instance.IsOnlineMode = true;
        GameManager.Instance.Start();
    }

    public void OnOfflineClick()
    {
        homeCanvas.SetActive(false);
        gamePageCanvas.SetActive(true);

        GameManager.Instance.IsOnlineMode = false;
        GameManager.Instance.Start();
    }
}
