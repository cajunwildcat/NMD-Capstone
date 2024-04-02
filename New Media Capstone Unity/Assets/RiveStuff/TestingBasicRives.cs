using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rive;

public class TestingBasicRives : MonoBehaviour
{
    // Start is called before the first frame update
    public Rive.Asset asset; // pass in .riv asset in the inspector
    private Rive.File m_file;
private void Start()
    {
        if (asset != null)
        {
            m_file = Rive.File.Load(asset);
        }
    }

}
