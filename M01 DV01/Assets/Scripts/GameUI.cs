using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour {
    public PlayerUIContainer[] playerContainers;
    public TextMeshProUGUI winText;

    public static GameUI instance;

    void Awake() {
        instance = this;
    }

    void Start() {
        InitializePlayerUI();
    }

    void Update() {
        UpdatePlayerUI();
    }

    void InitializePlayerUI() {
        for (int i = 0; i < playerContainers.Length; ++i) {
            PlayerUIContainer container = playerContainers[i];

            if (i < PhotonNetwork.PlayerList.Length) {
                container.obj.SetActive(true);
                container.nameText.text = PhotonNetwork.PlayerList[i].NickName;
            }

            else {
                container.obj.SetActive(false);
            }
        }
    }

    void UpdatePlayerUI() {
        foreach (PlayerController player in GameManager.instance.players) {
            if (player == null) {
                continue;
            }

            int playerIndex = player.id - 1;
            if (playerIndex < 0 || playerIndex >= playerContainers.Length) {
                continue;
            }

            playerContainers[playerIndex].hatTimeSlider.value = player.playerHealth;
        }  
    }

    public void SetWinText(string winnerName) {
        winText.gameObject.SetActive(true);
        winText.text = winnerName + " wins";
    }
}

[System.Serializable]
public class PlayerUIContainer {
    public GameObject obj;
    public TextMeshProUGUI nameText;
    public Slider hatTimeSlider;
}