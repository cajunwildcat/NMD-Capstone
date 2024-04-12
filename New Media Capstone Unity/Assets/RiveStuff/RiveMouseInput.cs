using Rive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(RiveTexture))]
internal class RiveMouseInput : MonoBehaviour {
    private RiveTexture riveTexture;
    private Artboard artboard => riveTexture.artboard;
    private List<StateMachine> stateMachines => riveTexture.StateMachines;
    public event RiveEventDelegate OnRiveEvent;
    public delegate void RiveEventDelegate(ReportedEvent reportedEvent);
    private Camera camera;
    Vector2 m_lastMousePosition;

    Fit fit = Fit.contain;
    Alignment alignment = Alignment.Center;

    private void Start() {
        riveTexture = GetComponent<RiveTexture>();
        camera = Camera.main;
    }

    private void Update() {
        /*Vector3 mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
        //Debug.Log($"Mouse Position: {mousePos}");
        UpdateStateMachineMousePosition(mousePos);*/

        List<Vector3> trackerPositions = KinectCoordinates.Instance.GetAllNormalizedTrackerPositions();
        foreach (Vector3 pos in trackerPositions) {
            //Debug.Log(pos);
            UpdateStateMachineMousePosition(pos);
            foreach (StateMachine sm in stateMachines) {
                sm?.Advance(Time.deltaTime/trackerPositions.Count);
            }
        }
    }

    private void UpdateStateMachineMousePosition(Vector3 normalizedPosition) {
        Vector2 mouseRiveScreenPos = new Vector2(
            normalizedPosition.x * camera.pixelWidth,
            (1 - normalizedPosition.y) * camera.pixelHeight
        );
        if (m_lastMousePosition != mouseRiveScreenPos) {
            Vector2 local = artboard.LocalCoordinate(
                mouseRiveScreenPos,
                new Rect(0, 0, camera.pixelWidth, camera.pixelHeight),
                fit,
                alignment
            );
            foreach (StateMachine sm in stateMachines) {
                sm?.PointerMove(local);
            }
            m_lastMousePosition = mouseRiveScreenPos;
        }
    }
}