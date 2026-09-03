using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks {
    [Header ("Screens")]
    public GameObject _mainScreen;
    public GameObject _lobbyScreen;

    [Header ("Main Screen")]
    public Button _createRoomButton;
    public Button _joinRoomButton;
    
    [Header ("Lobby Screen")]
    public TextMeshProUGUI _playerListText;
    public Button _startGameButton;

    void Start() {
        _createRoomButton.interactable = false;
        _joinRoomButton.interactable = false;
    }

    public override void OnConnectedToMaster() {
        _createRoomButton.interactable = true;
        _joinRoomButton.interactable = true;
    }

    void SetScreen(GameObject screen) { 
        _mainScreen.SetActive(false);
        _lobbyScreen.SetActive(false);

        screen.SetActive(true);
    }

    public void OnCreatedRoomButton(TMP_InputField roomNameInput) {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    public void onJoinRoomButton(TMP_InputField roomNameInput) {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
    }

    public void OnPlayerNameUpdate(TMP_InputField playerNameInput) {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnJoinedRoom() {
        SetScreen(_lobbyScreen);
    } 

    [PunRPC]
    public void UpdateLobbyUI() {
        _playerListText.text = "";

        foreach(Player player in PhotonNetwork.PlayerList) {
            _playerListText.text += player.NickName + "\n";
        }

        _startGameButton.interactable = PhotonNetwork.IsMasterClient;

        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        UpdateLobbyUI();
    }

    public void OnLeaveLobbyButton() {
        PhotonNetwork.LeaveRoom();
        SetScreen(_mainScreen);
    }

    public void OnStartGameButton() {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
}
