// Toony Colors Pro+Mobile 2
// (c) 2014-2018 Jean Moreno

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Represents a Toony Colors Pro 2 configuration to generate the corresponding shader

public class TCP2_Config
{
	//--------------------------------------------------------------------------------------------------

	public string Filename = "TCP2 Custom";
	public string ShaderName = "Toony Colors Pro 2/User/My TCP2 Shader";
	public string configType = "Normal";
	public string templateFile = "TCP2_ShaderTemplate_Default";
	public int shaderTarget = 30;
	public List<string> Features = new List<string>();
	public List<string> Flags = new List<string>();
	public Dictionary<string, string> Keywords = new Dictionary<string, string>();
	public bool isModifiedExternally;

	//--------------------------------------------------------------------------------------------------

	private enum ParseBlock
	{
		None,
		Features,
		Flags
	}

	static public TCP2_Config CreateFromFile(TextAsset asset)
	{
		return CreateFromFile(asset.text);
	}
	static public TCP2_Config CreateFromFile(string text)
	{
		string[] lines = text.Split(new string[]{"\n","\r\n"}, System.StringSplitOptions.RemoveEmptyEntries);
		TCP2_Config config = new TCP2_Config();

		//Flags
		ParseBlock currentBlock = ParseBlock.None;
		for(int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			
			if(line.StartsWith("//")) continue;
			
			string[] data = line.Split(new string[]{"\t"}, System.StringSplitOptions.RemoveEmptyEntries);
			if(line.StartsWith("#"))
			{
				currentBlock = ParseBlock.None;
				
				switch(data[0])
				{
					case "#filename":	config.Filename = data[1]; break;
					case "#shadername":	config.ShaderName = data[1]; break;
					case "#features":	currentBlock = ParseBlock.Features; break;
					case "#flags":		currentBlock = ParseBlock.Flags; break;
					
					default: Debug.LogWarning("[TCP2 Shader Config] Unrecognized tag: " + data[0] + "\nline " + (i+1)); break;
				}
			}
			else
			{
				if(data.Length > 1)
				{
					bool enabled = false;
					bool.TryParse(data[1], out enabled);
					
					if(enabled)
					{
						if(currentBlock == ParseBlock.Features)
							config.Features.Add(data[0]);
						else if(currentBlock == ParseBlock.Flags)
							config.Flags.Add(data[0]);
						else
							Debug.LogWarning("[TCP2 Shader Config] Unrecognized line while parsing : " + line + "\nline " + (i+1));
					}
				}
			}
		}
		
		return config;
	}

	static public TCP2_Config CreateFromShader(Shader shader)
	{
		ShaderImporter shaderImporter = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;
		string[] features;
		string[] flags;
		string[] customData;
		Dictionary<string, string> keywords;
		TCP2_ShaderGeneratorUtils.ParseUserData(shaderImporter, out features, out flags, out keywords, out customData);
		if(features != null && features.Length > 0 && features[0] == "USER")
		{
			var config = new TCP2_Config();
			config.ShaderName = shader.name;
			config.Filename = System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(shader)).Replace(".shader","");
			config.Features = new List<string>(features);
			config.Flags = (flags != null) ? new List<string>(flags) : new List<string>();
			config.Keywords = (keywords != null) ? new Dictionary<string, string>(keywords) : new Dictionary<string, string>();
			//config.AutoNames();

			//mCurrentShader = shader;
			//mConfigChoice = mUserShadersLabels.IndexOf(shader.name);
			//mDirtyConfig = false;
			//AutoNames();
			//mCurrentHash = mCurrentConfig.ToHash();

			config.isModifiedExternally = false;
			if(customData != null && customData.Length > 0)
			{
				foreach(string data in customData)
				{
					//Hash
					if(data.Length > 0 && data[0] == 'h')
					{
						string dataHash = data;
						string fileHash = TCP2_ShaderGeneratorUtils.GetShaderContentHash(shaderImporter);

						if(!string.IsNullOrEmpty(fileHash) && dataHash != fileHash)
						{
							config.isModifiedExternally = true;
						}
					}
					//Timestamp
					else
					{
						ulong timestamp;
						if(ulong.TryParse(data, out timestamp))
						{
							if(shaderImporter.assetTimeStamp != timestamp)
							{
								config.isModifiedExternally = true;
							}
						}
					}

					//Shader Model target
					if(data.StartsWith("SM:"))
					{
						config.shaderTarget = int.Parse(data.Substring(3));
					}

					//Configuration Type
					if(data.StartsWith("CT:"))
					{
						config.configType = data.Substring(3);
					}

					//Configuration File
					if(data.StartsWith("CF:"))
					{
						config.templateFile = data.Substring(3);
					}
				}
			}

			return config;
		}
		else
		{
			return null;
		}
	}

	public TCP2_Config Copy()
	{
		TCP2_Config config = new TCP2_Config();

		config.Filename = this.Filename;
		config.ShaderName = this.ShaderName;

		foreach (string feature in this.Features)
			config.Features.Add(feature);

		foreach (string flag in this.Flags)
			config.Flags.Add(flag);

		foreach (KeyValuePair<string, string> kvp in this.Keywords)
			config.Keywords.Add(kvp.Key, kvp.Value);

		config.shaderTarget = this.shaderTarget;
		config.configType = this.configType;
		config.templateFile = this.templateFile;

		return config;
	}

	public string GetShaderTargetCustomData()
	{
		return string.Format("SM:{0}", this.shaderTarget);
	}

	public string GetConfigTypeCustomData()
	{
		if (configType != "Normal")
		{
			return string.Format("CT:{0}", this.configType);
		}

		return null;
	}

	public string GetConfigFileCustomData()
	{
		return string.Format("CF:{0}", this.templateFile);
	}

	public int ToHash()
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.Append(this.Filename);
		sb.Append(this.ShaderName);
		List<string> orderedFeatures = new List<string>(this.Features);
		orderedFeatures.Sort();
		List<string> orderedFlags = new List<string>(this.Flags);
		orderedFlags.Sort();
		List<string> sortedKeywordsKeys = new List<string>(this.Keywords.Keys);
		sortedKeywordsKeys.Sort();
		List<string> sortedKeywordsValues = new List<string>(this.Keywords.Values);
		sortedKeywordsValues.Sort();

		foreach(string f in orderedFeatures)
			sb.Append(f);
		foreach(string f in orderedFlags)
			sb.Append(f);
		foreach(string f in sortedKeywordsKeys)
			sb.Append(f);
		foreach(string f in sortedKeywordsValues)
			sb.Append(f);

		sb.Append(shaderTarget.ToString());

		return sb.ToString().GetHashCode();
	}

	//Convert Config to ShaderImporter UserData
	public string ToUserData( string[] customData )
	{
		string userData = "";
		if (!this.Features.Contains("USER"))
			userData = "USER,";

		foreach (string feature in this.Features)
			if (feature.Contains("USER"))
				userData += string.Format("{0},", feature);
			else
				userData += string.Format("F{0},", feature);
		foreach (string flag in this.Flags)
			userData += string.Format("f{0},", flag);
		foreach (KeyValuePair<string, string> kvp in this.Keywords)
			userData += string.Format("K{0}:{1},", kvp.Key, kvp.Value);
		foreach (string custom in customData)
			userData += string.Format("c{0},", custom);
		userData = userData.TrimEnd(',');

		return userData;
	}

	public void AutoNames()
	{
		string rawName = this.ShaderName.Replace("Toony Colors Pro 2/", "");
		this.Filename = rawName;
	}

	//--------------------------------------------------------------------------------------------------
	// FEATURES

	public bool HasFeature(string feature)
	{
		return TCP2_ShaderGeneratorUtils.HasEntry(this.Features, feature);
	}

	public bool HasFeaturesAny(params string[] features)
	{
		return TCP2_ShaderGeneratorUtils.HasAnyEntries(this.Features, features);
	}

	public bool HasFeaturesAll( params string[] features )
	{
		return TCP2_ShaderGeneratorUtils.HasAllEntries(this.Features, features);
	}

	public void ToggleFeature( string feature, bool enable )
	{
		if(string.IsNullOrEmpty(feature))
			return;

		TCP2_ShaderGeneratorUtils.ToggleEntry(this.Features, feature, enable);
	}

	//--------------------------------------------------------------------------------------------------
	// FLAGS

	public bool HasFlag( string flag )
	{
		return TCP2_ShaderGeneratorUtils.HasEntry(this.Flags, flag);
	}

	public bool HasFlagsAny( params string[] flags )
	{
		return TCP2_ShaderGeneratorUtils.HasAnyEntries(this.Flags, flags);
	}

	public bool HasFlagsAll( params string[] flags )
	{
		return TCP2_ShaderGeneratorUtils.HasAllEntries(this.Flags, flags);
	}

	public void ToggleFlag( string flag, bool enable )
	{
		TCP2_ShaderGeneratorUtils.ToggleEntry(this.Flags, flag, enable);
	}

	//--------------------------------------------------------------------------------------------------
	// KEYWORDS

	public string GetKeyword( string key )
	{
		if (key == null)
			return null;

		if (!this.Keywords.ContainsKey(key))
			return null;

		return this.Keywords[key];
	}

	public void SetKeyword( string key, string value )
	{
		if (string.IsNullOrEmpty(value))
		{
			if (this.Keywords.ContainsKey(key))
				this.Keywords.Remove(key);
		}
		else
		{
			if (this.Keywords.ContainsKey(key))
				this.Keywords[key] = value;
			else
				this.Keywords.Add(key, value);
		}
	}

	public void RemoveKeyword( string key )
	{
		if (this.Keywords.ContainsKey(key))
			this.Keywords.Remove(key);
	}
}
