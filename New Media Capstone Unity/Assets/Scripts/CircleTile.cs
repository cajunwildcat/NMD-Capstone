using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTile : MonoBehaviour {
    CircleCollider2D collider;
    SpriteRenderer sr;
    SpriteMask sm;
    Animator anim;
    bool activated = false;
    public float Radius;
    [SerializeField] LayerMask searcherLayer;
    bool prevHit = false;

    // Start is called before the first frame update
    void Start() {
        collider = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        sm = GetComponent<SpriteMask>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        if (sm.sprite != sr.sprite) {
            sm.sprite = sr.sprite;
        }
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, collider.radius, Vector2.zero, 0, searcherLayer);
        if (!activated && hit) {
            anim.SetTrigger("Reveal");
            sm.enabled = activated = true;
        }
        if (activated && !hit && prevHit) {
            StartCoroutine(TimeOut(3f));
        }
        prevHit = hit;
    }

    IEnumerator TimeOut(float t) {
        yield return new WaitForSeconds(t);
        anim.SetTrigger("FadeOut");
        anim.ResetTrigger("Reveal");
        yield return new WaitForSeconds(1);
        anim.ResetTrigger("FadeOut");
        sm.enabled = activated = false;
        sm.material.color = Color.white;
    }
}