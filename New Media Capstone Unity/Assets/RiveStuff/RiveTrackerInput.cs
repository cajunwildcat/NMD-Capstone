using Rive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(RiveTexture))]
internal class RiveTrackerInput : MonoBehaviour {
    public event RiveEventDelegate OnRiveEvent;
    public delegate void RiveEventDelegate(ReportedEvent reportedEvent);

    private RiveTexture riveTexture;
    private Artboard Artboard => riveTexture.artboard;
    private List<StateMachine> StateMachines => riveTexture.StateMachines;
    Vector2 m_lastMousePosition;

    private MeshRenderer mr;
    private Bounds bounds;

    Fit fit = Fit.contain;
    Alignment alignment = Alignment.Center;

    private void Start() {
        riveTexture = GetComponent<RiveTexture>();
        mr = GetComponent<MeshRenderer>();
        bounds = mr.bounds;
    }

    private void Update() {
        List<Vector3> trackerPositions = HandsKinect.Instance.GetAllTrackerWorldPositions();
        bool updated = false;
        foreach (Vector3 pos in trackerPositions) {
            Vector2 local = GetArtboardLocalPosition(pos);
            if (m_lastMousePosition == local) continue;

            updated = true;
            foreach (StateMachine sm in StateMachines) {
                sm?.PointerMove(local);
                sm?.Advance(Time.deltaTime / trackerPositions.Count);
            }
            m_lastMousePosition = local;
        }
        if (!updated) {
            foreach (StateMachine sm in StateMachines) {
                sm?.Advance(Time.deltaTime);
            }
        }
    }

    private Vector2 GetArtboardLocalPosition(Vector3 worldPos) {
        //take the world position and normalize in relation to the rive texture's mesh bounds
        //we have to flip it because our 16:9 textures get rendered rotated 90 degrees
        Vector2 normalizedPosition = new Vector3(
                (worldPos.y - bounds.min.y) / (bounds.max.y - bounds.min.y),
                (worldPos.x - bounds.min.x) / (bounds.max.x - bounds.min.x)
            );
        //because our coordinates are already normalized we pass in (1,1) as the "screen" size
        Vector2 local = Artboard.LocalCoordinate(
            normalizedPosition,
            new Rect(Vector2.zero, new(1,1)),
            fit,
            alignment
        );
        return local;
    }
}