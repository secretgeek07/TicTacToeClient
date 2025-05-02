using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndPage : MonoBehaviour
{
    public TMP_Text resultText;

    public void ShowResult(string result)
    {
        resultText.text = result;
        gameObject.SetActive(true);
    }

    public void OnReplayClick()
    {
        
        GameManager.Instance.ReplayGame();
    }
}
