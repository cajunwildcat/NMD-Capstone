using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    BoxCollider2D collider;
    SpriteRenderer sr;
    bool activated = false;

    [SerializeField] LayerMask searcherLayer;

    bool found = false;
    // Start is called before the first frame update
    void Start() {
        collider = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (activated) return;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, collider.size, 0, Vector2.zero, 0, searcherLayer);
        if (hit) {
            GetComponent<Animator>().SetTrigger("Reveal");
            activated = true;
        }
    }
}