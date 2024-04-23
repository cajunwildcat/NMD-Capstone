using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenObject : MonoBehaviour {
    BoxCollider2D collider;
    [SerializeField] Material unlitSprite;
    SpriteRenderer progressSprite;

    [SerializeField] LayerMask searcherLayer;

    float counter;
    float findTime = 3;
    bool found = false;
    // Start is called before the first frame update
    void Start() {
        collider = transform.GetChild(1).GetComponent<BoxCollider2D>();
        progressSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (found) return;
        RaycastHit2D hit = Physics2D.BoxCast(collider.transform.position, collider.size, 0, Vector2.zero, 0, searcherLayer);
        if (hit) {
            counter += Time.deltaTime;
            if (counter >= findTime) {
                progressSprite.material = unlitSprite;
                found = true;
            }
        }
        else {
            counter = 0;
        }
    }
}