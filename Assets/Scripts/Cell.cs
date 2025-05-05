using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cell : MonoBehaviour
{
    public TMP_Text cellText;
    public int row, col;
    private Button button;

    public void Setup(int r, int c)
    {
        row = r;
        col = c;
        button = GetComponent<Button>();
        // button.onClick.AddListener(OnClick);
        cellText.text = "";
        button.interactable = true;
    }

    public void SetText(string text)
    {
        // cellText.text = text;
        // button.interactable = false;
        cellText.text = text;

        if (text == "X")
        {
            cellText.color = Color.red;
            GetComponent<Image>().color = new Color(1f, 0.9f, 0.9f); 
        }
        else if (text == "O")
        {
            cellText.color = Color.blue;
            GetComponent<Image>().color = new Color(0.9f, 0.9f, 1f); 
        }

        button.interactable = false; 
    }

    public void OnClick()
    {
        Debug.Log($"Cell clicked at ({row}, {col})");
        GameManager.Instance.MakeMove(row, col, this);
    }
}
