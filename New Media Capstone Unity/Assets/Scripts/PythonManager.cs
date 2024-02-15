using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Scripting.Python;
using UnityEditor;

[CustomEditor(typeof(PythonManager))]
public class PythonManager : MonoBehaviour
{
	PythonManager instance;
	static string PythonPath;


	void OnEnable()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
		PythonPath = Application.dataPath + "/Scripts/Python";
    }

	// Start is called before the first frame update
	void Start()
	{
		string scriptPath = PythonPath + "/main.py";
		PythonRunner.RunFile(scriptPath);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
