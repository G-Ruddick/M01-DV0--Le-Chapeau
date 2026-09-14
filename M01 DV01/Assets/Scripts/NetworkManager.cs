using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks {
    public static NetworkManager instance;

    void Awake() {
        if (instance != null && instance != this) {
            gameObject.SetActive(false);
            Destroy(gameObject);
            Debug.Log("deleted instance");
        }
        else {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
    }

    void Start() {
        if(!PhotonNetwork.IsConnected) {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster() {
        Debug.Log("connected to mastert server");
    }

    public override void OnCreatedRoom() {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
    }

    public void CreateRoom(string roomName) {
        PhotonNetwork.CreateRoom(roomName);
    }

    public void JoinRoom(string roomName) {
        Debug.Log("Joining Room " + roomName);
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC]
    public void ChangeScene(string sceneName) {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
