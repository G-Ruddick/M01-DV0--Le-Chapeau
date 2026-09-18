using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPun {
    [SerializeField] private float bulletForce = 50.0f;
    [SerializeField] private int damage = 1;

    [SerializeField] private Rigidbody rigidBody;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player")) {
            PlayerController target = other.gameObject.GetComponent<PlayerController>();

            target.photonView.RPC("TakeDamage", target.photonView.Owner, damage);
        }
        
        PhotonNetwork.Destroy(gameObject);
    }

    private void Start() {
        rigidBody.AddForce(transform.right * bulletForce, ForceMode.VelocityChange);
    }
}
