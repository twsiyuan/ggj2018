﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour {
	public void SwitchLevel(string levelName){
		SceneManager.LoadScene (levelName, LoadSceneMode.Single);
	}
}