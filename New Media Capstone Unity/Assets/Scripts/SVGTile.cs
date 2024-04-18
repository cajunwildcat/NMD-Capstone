using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVGTile : MonoBehaviour
{
    private bool activated = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate() {
        if (activated) return;
        activated = true;
        StartCoroutine(Grow());
    }

    IEnumerator Grow() {
        float counter = 0;
        while (counter < 1) {
            counter += Time.deltaTime;
            transform.GetChild(0).transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, counter);
            yield return null;
        }
    }
}
