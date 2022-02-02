using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Ship : EditorWindow
{
	[MenuItem("Build/Ship Web")]
	public static void SendIt(){
		Debug.Log("shipping");
		//determine build path
		string buildPath=Application.dataPath;
		buildPath = Directory.GetParent(buildPath).FullName.Replace('\\', '/') + "/buildPath/HTML";
		if(!Directory.Exists(buildPath)){
			Directory.CreateDirectory(buildPath);
		}
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath,BuildTarget.WebGL, BuildOptions.None);
		string bat = "cd Butler\nbutler push "+buildPath+" rithmgaming/soul-sand:HTML5";

        string batPath = Directory.GetParent(Application.dataPath).FullName.Replace('\\','/')+"/Butler/ship.bat";
        File.WriteAllText(batPath, bat);

        System.Diagnostics.Process.Start("cmd.exe", "/k " + batPath);
	}

	[MenuItem("Build/Ship Windows")]
	public static void SendItW(){
		Debug.Log("shipping");
		//determine build path
		string buildPath=Application.dataPath;
		buildPath = Directory.GetParent(buildPath).FullName.Replace('\\', '/') + "/buildPath/Windows";
		if(!Directory.Exists(buildPath)){
			Directory.CreateDirectory(buildPath);
		}
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath,BuildTarget.StandaloneWindows, BuildOptions.None);
		string bat = "cd Butler\nbutler push "+buildPath+" rithmgaming/soul-sand:windows";

        string batPath = Directory.GetParent(Application.dataPath).FullName.Replace('\\','/')+"/Butler/ship.bat";
        File.WriteAllText(batPath, bat);

        System.Diagnostics.Process.Start("cmd.exe", "/k " + batPath);
	}

}
