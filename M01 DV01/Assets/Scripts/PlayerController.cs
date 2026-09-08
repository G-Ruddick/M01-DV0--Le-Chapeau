using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable {
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed = 7.5f;
    public float jumpForce = 5;

    public GameObject hatObject;
    
    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rigidBody;
    public Player photonPlayer;

    void Update() {
        if (PhotonNetwork.IsMasterClient) {
            if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded) {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        if(photonView.IsMine) {
            Move();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            TryJump();
        }

        if (hatObject.activeInHierarchy) {
            curHatTime += Time.deltaTime;
        }
    }

    void Move() {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rigidBody.linearVelocity = new Vector3(x, rigidBody.linearVelocity.y, z);
    }

    void TryJump() {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f)) {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public void Initialize(Player player) {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine) {
            rigidBody.isKinematic = true;
        }

        if(id == 1) {
            GameManager.instance.GiveHat(id, true);
        }
    }

    public void SetHat (bool hasHat) {
        hatObject.SetActive(hasHat);
    }

    void OnCollisionEnter (Collision collision) {
        if (!photonView.IsMine) {
            return;
        }

        if (collision.gameObject.CompareTag("Player")) {
            if (GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat) {
                if (GameManager.instance.CanGetHat()) {
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(curHatTime);
        }
        else if (stream.IsReading) {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}
