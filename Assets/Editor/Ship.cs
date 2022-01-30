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

		//before shipping lets change out the css stuff that unity gives by default
		/*
		string indexPath = buildPath+"/index.html";
		string [] lines = File.ReadAllLines(indexPath);
		string html = "";
		bool firstScriptFound=false;
		bool deleteStuff=false;
		foreach(string s in lines)
		{
			Debug.Log(s);
			if(s.Contains("margin"))
			{
				html+="<div id=\"unityContainer\" style=\"width: 100%; height: 100%; position:fixed;left:50%;top:50%; transform:translate(-50%,-50%);\"></div>";
			}
			else if(s.Contains("script"))
			{
				//also this version of unity tries to throw in this extra script that seems to cause things to break
				//something about filling the screen if mobile device
				if(!firstScriptFound)
				{
					firstScriptFound=true;
					html+=s;
				}
				else{
					//don't add
					deleteStuff=!deleteStuff;
					Debug.Log("Deleting line: "+s);
				}
			}
			else if(deleteStuff)
			{
				Debug.Log("Deleting line: "+s);
			}
			else
				html+=s;
		}

		File.WriteAllText(indexPath,html);
		Debug.Log(html);
		*/

        System.Diagnostics.Process.Start("cmd.exe", "/k " + batPath);
	}

}
