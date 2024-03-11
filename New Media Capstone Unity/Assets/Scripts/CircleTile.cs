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
    float circleAnimLength = 2.2f;
    bool animating = false;

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
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, collider.radius * transform.localScale.x, Vector2.zero, 0, searcherLayer);
        if (!activated && hit) {
            Activate();
        }
        if (activated && !hit && prevHit) {
            StartCoroutine(TimeOut(3f));
        }
        prevHit = hit;
    }

    IEnumerator TimeOut(float t) {
        new WaitUntil(() => !animating);
        yield return new WaitForSeconds(t);
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, collider.radius * transform.localScale.x, Vector2.zero, 0, searcherLayer);
        if (hit) yield break;
        anim.SetBool("Reveal", false);
        sm.enabled = activated = false;
        yield return new WaitForSeconds(1);
        anim.ResetTrigger("FadeOut");
    }

    IEnumerator WaitFor(Action action, float time) {
        yield return new WaitForSeconds(time);
        action();
    }

    public void Activate() {
        StopAllCoroutines();
        anim.SetBool("Reveal", true);
        sm.sprite = sr.sprite;
        sm.enabled = activated = animating = true;
        StartCoroutine(WaitFor(() => {
            animating = false;
        }, circleAnimLength));
    }
}