//using System.Collections;
using System.Diagnostics;
//using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Scripting.Python;
//using UnityEditor;

public class PythonMainTestRun : MonoBehaviour
{
    public string scriptPath;

    private void Awake()
    {
        scriptPath = Application.dataPath + "/Scripts/Python/main.py";
    }


    void OnMouseDown()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("python") //System.Diagnostics class for startinfo object
        {
            Arguments = $"\"{scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo }) //System.Diagnostics class for process object
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            UnityEngine.Debug.Log(output); //fix
            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError(error);
            }
        }
    }

}
