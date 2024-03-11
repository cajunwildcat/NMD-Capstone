using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFiller : MonoBehaviour {
    Sprite background;
    public GameObject TilePrefab;

    public int TileRows = 5;
    public int TileCols = 8;

    // Start is called before the first frame update
    void Start() {
        background = GetComponent<SpriteRenderer>().sprite;
        Bounds bounds = background.bounds;
        float totalWidth = bounds.size.x;
        float totalHeight = bounds.size.y;
        //Debug.Log(bounds.min);
        float tileWidth = totalWidth / TileCols;
        float tileHeight = totalHeight / TileRows;

        float startX = bounds.min.x + (tileWidth / 2);
        float startY = bounds.max.y - (tileHeight / 2);
        for (int y = 0; y < TileRows; y++) {
            for (int x = 0; x < TileCols; x++) {
                GameObject newTile = Instantiate(TilePrefab, new Vector3(startX + (x * tileWidth), startY - (y * tileHeight), 0), Quaternion.identity);
                newTile.transform.SetParent(transform,true);
                newTile.transform.localScale = new Vector3(tileWidth,tileHeight);
                newTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
        }
    }
}