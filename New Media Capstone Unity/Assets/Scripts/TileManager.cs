using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour {
    public int rowCount = 4;
    public int columnCount = 3;
    public GameObject tilePrefab;
    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < rowCount; i++) {
            for (int j = 0; j < columnCount; j++) {
                GameObject newTile = Instantiate(tilePrefab, new Vector3(j, i, 0) + transform.position, Quaternion.identity);
                newTile.transform.parent = transform;
            }
        }
    }
}