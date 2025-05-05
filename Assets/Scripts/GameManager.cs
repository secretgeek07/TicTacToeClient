using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject homePageCanvas;
    public GameObject cellPrefab;
    public Transform gridPanel;
    public TMP_Text playerTurnText;
    public EndPage endPage;
    public GameObject gamePageCanvas;
    public bool IsOnlineMode = false;

    public TMP_Text statusText;
    public CanvasGroup gridGroup;
    private string[,] board = new string[3, 3];
    private string currentPlayer = "X";
    private bool gameOver = false;
    private bool isMyTurn = true; 
    private string myMark;
    private List<Cell> allCells = new List<Cell>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Start()
    {
        InitBoard();
        // UpdateTurnText();
    }

    public void InitBoard()
    {
        allCells.Clear();
        foreach (Transform child in gridPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 9; i++)
        {
            GameObject cell = Instantiate(cellPrefab, gridPanel);
            Cell cellScript = cell.GetComponent<Cell>();
            cellScript.Setup(i / 3, i % 3);
            allCells.Add(cellScript);
        }

        board = new string[3, 3];
        currentPlayer = "X";
        gameOver = false;
        // isMyTurn = true;
        if (!IsOnlineMode)
        {
            isMyTurn = true;
        }

        gamePageCanvas.SetActive(true);

        if (IsOnlineMode)
        {
            statusText.text = "Waiting for opponent to join...";
            SetBoardInteractable(false);
        }
        else
        {
            statusText.text = "";
            SetBoardInteractable(true);
            UpdateTurnText();
        }
        Debug.Log($"[Init] IsOnline: {IsOnlineMode}, MyMark: {myMark}, isMyTurn: {isMyTurn}");
    }

    public void MakeMove(int row, int col, Cell cell)
    {
         Debug.Log($"Cell 2 clicked at ({row}, {col})");
        if (gameOver || board[row, col] != null) return;
        Debug.Log($"Cell 3 clicked at ({row}, {col})");
        if (IsOnlineMode && !isMyTurn) return;
        Debug.Log($"Cell 4 clicked at ({row}, {col})");
        // board[row, col] = currentPlayer;
        // cell.SetText(currentPlayer);
        board[row, col] = myMark;
        cell.SetText(myMark);


        if (IsOnlineMode)
        {
            NetworkManager.Instance.SendMove(row, col);
            isMyTurn = false;
            SetBoardInteractable(false);
        }

        if (CheckWin())
        {
            gameOver = true;
            Invoke(nameof(ShowWinScreen), 1f);
            return;
        }

        if (CheckDraw())
        {
            gameOver = true;
            Invoke(nameof(ShowDrawScreen), 1f);
            return;
        }

        SwitchTurn();
    }

    public void ReceiveMove(int row, int col)
    {
        if (gameOver) return;

        board[row, col] = currentPlayer;
        allCells[row * 3 + col].SetText(currentPlayer);

        if (CheckWin())
        {
            gameOver = true;
            Invoke(nameof(ShowWinScreen), 1f);
            return;
        }

        if (CheckDraw())
        {
            gameOver = true;
            Invoke(nameof(ShowDrawScreen), 1f);
            return;
        }

        SwitchTurn();
        // isMyTurn = true;
    }

    private void ShowWinScreen()
    {
        gamePageCanvas.SetActive(false);
        endPage.ShowResult($"Player {currentPlayer} Wins!");
    }

    private void ShowDrawScreen()
    {
        gamePageCanvas.SetActive(false);
        endPage.ShowResult("It's a Draw!");
    }

    private void SwitchTurn()
    {
        // currentPlayer = (currentPlayer == "X") ? "O" : "X";
        // Debug.Log($"[SwitchTurn] IsOnline: {IsOnlineMode}, MyMark: {myMark}, isMyTurn: {isMyTurn}, currentPlayer: {currentPlayer}");
        // UpdateTurnText();

        currentPlayer = (currentPlayer == "X") ? "O" : "X";
        if (IsOnlineMode)
        {
            isMyTurn = (myMark == currentPlayer);
            SetBoardInteractable(isMyTurn);
        }

        Debug.Log($"[SwitchTurn] IsOnline: {IsOnlineMode}, MyMark: {myMark}, isMyTurn: {isMyTurn}, currentPlayer: {currentPlayer}");
        UpdateTurnText();
    }

    // private void UpdateTurnText()
    // {
    //     playerTurnText.text = $"Player {currentPlayer}'s Turn";
    // }
    private void UpdateTurnText()
    {
        if (IsOnlineMode)
        {
            if ((myMark == currentPlayer) && isMyTurn)
            {
                playerTurnText.text = $"Your turn, move with '{myMark}'";
            }
            else
            {
                string opponentMark = myMark == "X" ? "O" : "X";
                playerTurnText.text = $"Opponent's turn, they're playing '{opponentMark}'";
            }
        }
        else
        {
            playerTurnText.text = $"Player {currentPlayer}'s Turn";
        }
    }

    private bool CheckWin()
    {
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] != null && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2]) return true;
            if (board[0, i] != null && board[0, i] == board[1, i] && board[1, i] == board[2, i]) return true;
        }

        if (board[0, 0] != null && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2]) return true;
        if (board[0, 2] != null && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0]) return true;

        return false;
    }

    private bool CheckDraw()
    {
        foreach (var cell in board)
        {
            if (cell == null) return false;
        }
        return true;
    }

    public void ReplayGame()
    {
       endPage.gameObject.SetActive(false);
       gamePageCanvas.SetActive(false);
       homePageCanvas.SetActive(true);
    }

    public void StartOnlineGame()
    {
        currentPlayer = "X"; 
        //isMyTurn = (currentPlayer == "X");
        statusText.text = "Opponent joined. Game started!";
        SetBoardInteractable(isMyTurn);
    }
    public void ReceiveOpponentMove(int row, int col)
    {
        if (gameOver || board[row, col] != null) return;

        Debug.Log($"Applying opponent move at {row}, {col}");

        foreach (var cell in allCells)
        {
            if (cell.row == row && cell.col == col)
            {
                // board[row, col] = currentPlayer;
                // cell.SetText(currentPlayer);
                string opponentMark = myMark == "X" ? "O" : "X";
                board[row, col] = opponentMark;
                cell.SetText(opponentMark);
                break;
            }
        }

        if (CheckWin())
        {
            gameOver = true;
            Invoke(nameof(ShowWinScreen), 1f);
            return;
        }

        if (CheckDraw())
        {
            gameOver = true;
            Invoke(nameof(ShowDrawScreen), 1f);
            return;
        }

        SwitchTurn(); 
    }
    public void SetBoardInteractable(bool interactable)
    {
        if (gridPanel != null)
        {
            CanvasGroup group = gridPanel.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.interactable = interactable;
                group.blocksRaycasts = interactable;
            }
        }
    }

    public void StartOnlineGameAfterOpponentJoins()
    {
        statusText.text = $"Game Started! You are '{myMark}'";
        SetBoardInteractable(isMyTurn);
        UpdateTurnText();
    }
    public void SetPlayerMark(string mark)
    {
        myMark = mark;
        currentPlayer = "X"; // X always starts
        isMyTurn = (myMark == "X"); // I go first only if I'm X
        Debug.Log($"SetPlayerMark: My mark is {myMark}, isMyTurn: {isMyTurn}");
        Debug.Log($"[SetPlayerMarl] IsOnline: {IsOnlineMode}, MyMark: {myMark}, isMyTurn: {isMyTurn}");
    }
    
    public void HandleTimeout()
    {
        if (gameOver) return;

        gameOver = true;
        SetBoardInteractable(false);
        gamePageCanvas.SetActive(false);
        endPage.ShowResult("Game ended due to timeout.");
    }
}
