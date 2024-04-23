using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    BoxCollider2D tileCollider;  // Renamed from 'collider'
    SpriteRenderer tileRenderer; // Renamed from 'sr'
    bool activated = false;

    [SerializeField] LayerMask tileSearcherLayer; // Renamed from 'searcherLayer'

    // Start is called before the first frame update
    void Start()
    {
        tileCollider = GetComponent<BoxCollider2D>();
        tileRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (activated) return;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, tileCollider.size, 0, Vector2.zero, 0, tileSearcherLayer);
        if (hit.collider != null)
        {  // Ensuring that we're checking the hit result correctly
            GetComponent<Animator>().SetTrigger("Reveal");
            activated = true;
        }
    }
}
