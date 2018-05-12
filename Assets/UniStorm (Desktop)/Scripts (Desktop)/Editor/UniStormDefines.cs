﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[InitializeOnLoad]
public class UniStormDefines {
	const string UniStormDefinesString = "USING_UNISTORM";

	static UniStormDefines (){
		InitializeUniStormDefines();
	}

	static void InitializeUniStormDefines (){
		var BTG = EditorUserBuildSettings.selectedBuildTargetGroup;
		string UniStormDef = PlayerSettings.GetScriptingDefineSymbolsForGroup(BTG);

		if (!UniStormDef.Contains(UniStormDefinesString)){
			if (string.IsNullOrEmpty(UniStormDef)){
				PlayerSettings.SetScriptingDefineSymbolsForGroup(BTG, UniStormDefinesString);
			}
			else{
				if (UniStormDef[UniStormDef.Length - 1] != ';'){
					UniStormDef += ';';
				}

				UniStormDef += UniStormDefinesString;
				PlayerSettings.SetScriptingDefineSymbolsForGroup(BTG, UniStormDef);
			}
		}
	}
}
