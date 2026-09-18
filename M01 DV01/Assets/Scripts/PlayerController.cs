using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable {
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed = 3f;
    public float jumpForce = 5;
    public float turnSpeed = 2000f;
    public int playerHealth = 5;
    readonly public int maxHealth = 5;

    public GameObject hatObject;
    
    private Quaternion networkBodyRotation;
    private Quaternion networkCannonRotation;
    
    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rigidBody;
    public Player photonPlayer;
    public GameObject cannon;
    public GameObject camera;
    public GameObject bullet;
    public Transform muzzlePoint;

    private void Awake() {
        if (photonView.IsMine) {
            camera.SetActive(true);
        }
    }

    void Update() {
        if (PhotonNetwork.IsMasterClient) {
            int numberOfAlive = 0;
            for (int i = 0; i < GameManager.instance.players.Length; i++) {
                if (GameManager.instance.players[i] == null) {
                    numberOfAlive = 0;
                    break;
                }

                if (GameManager.instance.players[i].playerHealth > 0) {
                    numberOfAlive++;
                }
            }

            if (playerHealth > 0 && !GameManager.instance.gameEnded && numberOfAlive == 1) {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        if (!photonView.IsMine) {
            transform.rotation = networkBodyRotation;
            cannon.transform.localRotation = networkCannonRotation;
        }

        if (photonView.IsMine) {
            Move();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            TryJump();
        }
    }

    void Move() {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        Vector3 movement = x * transform.right + z * transform.forward;

        // Debug.Log("x = " + x + " z = " + z);
        rigidBody.linearVelocity = new Vector3(movement.x, rigidBody.linearVelocity.y, movement.z);

        float mouseX = Input.GetAxis("Mouse X") * turnSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * turnSpeed;

        Vector3 angle = rigidBody.rotation.eulerAngles;
        angle.x = 0f;
        angle.y += mouseX * Time.deltaTime;
        angle.z = 0f;
        rigidBody.rotation = Quaternion.Euler(angle);

        Vector3 rotation = cannon.transform.localEulerAngles;
        rotation.x -= mouseY * Time.deltaTime;
        rotation.x = Mathf.Clamp(rotation.x, 10f, 110f);
        rotation.y = 0f;
        rotation.z = 90f;
        cannon.transform.localEulerAngles = rotation;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            GameObject newBullet = PhotonNetwork.Instantiate(bullet.name, muzzlePoint.position, muzzlePoint.rotation);
        }
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

        // if (id == 1) {
        //     GameManager.instance.GiveHat(id, true);
        // }
    }

    public void SetHat (bool hasHat) {
        hatObject.SetActive(hasHat);
    }

    void OnCollisionEnter (Collision collision) {
        if (!photonView.IsMine) {
            return;
        }
    }

    [PunRPC] 
    public void TakeDamage(int damage) {
        if (!photonView.IsMine) {
            return;
        }
        
        playerHealth -= damage;
        photonView.RPC("SyncHealth", RpcTarget.Others, playerHealth);
    }

    [PunRPC]
    public void SyncHealth(int health) {
        if (!photonView.IsMine) {
            playerHealth = health;
        }
    }

    public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(playerHealth);
            stream.SendNext(rigidBody.rotation);
            stream.SendNext(cannon.transform.localRotation);
        }

        else /* if (stream.IsReading) */ {
            playerHealth = (int)stream.ReceiveNext();
            networkBodyRotation = (Quaternion)stream.ReceiveNext();
            networkCannonRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
