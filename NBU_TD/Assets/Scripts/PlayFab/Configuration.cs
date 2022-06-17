using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configuration : MonoBehaviour
{
	public BuildType buildType;
	public string buildId = "";
	public string ipAddress = "";
	public ushort port = 0;
	public bool playFabDebugging = false;
}

public enum BuildType
{
	LOCAL,
	REMOTE_CLIENT,
	REMOTE_SERVER
}