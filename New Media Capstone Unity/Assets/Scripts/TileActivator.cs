
using UnityEngine;

public class TileActivator : MonoBehaviour {
    CircleCollider2D collider;

    private void Start() {
        collider = GetComponent<CircleCollider2D>();
    }

    private void Update() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, transform.localScale.x / 2, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits) {
            GameObject gm = hit.collider.gameObject;
            switch (gm.tag) {
                case "SVG Tile":
                    gm.GetComponent<SVGTile>().Activate();
                    break;
                case "CircleTile":
                    gm.GetComponent<CircleTile>().Activate();
                    break;
                case "CubeTile":
                    gm.GetComponent<ColumnTile>().Activate();
                    break;
            }
        }
    }
}