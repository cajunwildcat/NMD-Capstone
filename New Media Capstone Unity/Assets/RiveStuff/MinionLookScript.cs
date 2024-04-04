using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rive;

[RequireComponent(typeof(RiveTexture))]
public class MinionLookScript : MonoBehaviour
{
    public float xLookDamp = 0.5f;
    public float yLookDamp = 0.5f;
    public GameObject peopleFollower;

    private StateMachine _riveStateMachine;
    private SMINumber _lookHorizontalInput;
    private SMINumber _lookVerticalInput;

    void Start()
    {
        var riveTexture = GetComponent<RiveTexture>();
        _riveStateMachine = riveTexture.stateMachine;
        _lookHorizontalInput = _riveStateMachine.GetNumber("track_x");
        _lookVerticalInput = _riveStateMachine.GetNumber("track_y");
        float randomTime = Random.Range(0.0f, 4.0f);
        _riveStateMachine.Advance(0.0f);
        _riveStateMachine.Advance(randomTime);
    }

    void Update()
    {
        Vector3 pos = KinectCoordinates.Instance.GetNearestFollower(Vector3.zero);
        Vector3 directionToCamera = pos - transform.position;
        directionToCamera.Normalize();
        float xProduct = Vector3.Dot(transform.right, directionToCamera);
        float yProduct = Vector3.Dot(transform.up, directionToCamera);
        _lookHorizontalInput.Value = xProduct * xLookDamp * 100;
        _lookVerticalInput.Value = yProduct * yLookDamp * 100;

    }
}
