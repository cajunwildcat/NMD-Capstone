using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnTile : MonoBehaviour {
    Vector3 target;
    float startZ;
    // Start is called before the first frame update
    void Start() {
        target = transform.position;
        startZ = target.z;
    }

    // Update is called once per frame
    void Update() {
        if (target != transform.position) {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime);
        }
    }

    public void Activate() {
        StopCoroutine("GoBackDown");
        float finalZ = Random.Range(startZ + 2f, startZ - 3f);
        target.z = finalZ;
        StartCoroutine(GoBackDown());
    }

    IEnumerator GoBackDown() {
        yield return new WaitForSeconds(3f);
        target.z = startZ;
    }
}
