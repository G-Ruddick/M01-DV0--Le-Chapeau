using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable {
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed = 7.5f;
    public float jumpForce = 5;
    public float turnSpeed = 2000f;

    public GameObject hatObject;
    
    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rigidBody;
    public Player photonPlayer;
    public GameObject cannon;
    public Camera camera;

    void Update() {
        // if (PhotonNetwork.IsMasterClient) {
        //     if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded) {
        //         GameManager.instance.gameEnded = true;
        //         GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
        //     }
        // }

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
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Debug.Log("x = " + x + " z = " + z);
        rigidBody.linearVelocity = new Vector3(x * moveSpeed, rigidBody.linearVelocity.y, z * moveSpeed);

        float mouseX = Input.GetAxis("Mouse X") * turnSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * 100f;

        transform.Rotate(Vector3.up * mouseX * Time.deltaTime);

        Vector3 rotation = cannon.transform.localEulerAngles;

        rotation.x -= mouseY * Time.deltaTime;
        rotation.y = 0f;
        rotation.z = 90f;

        cannon.transform.localEulerAngles = rotation;
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

        if (id == 1) {
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
