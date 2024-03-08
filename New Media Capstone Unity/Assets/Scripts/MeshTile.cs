
using UnityEngine;

public class MeshTile : MonoBehaviour {
    CircleCollider2D collider;

    private void Start() {
        collider = GetComponent<CircleCollider2D>();
    }

    private void Update() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, transform.localScale.x / 2, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.CompareTag("SVG Tile")) {
                GameObject gm = hit.collider.gameObject;
                gm.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
}