using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WanderingRoad;

public class GameState : ScriptableObject
{
	public Camera MainCamera;

	public string Seed { 
		get => _seed; 
		set {
			OnSeedChanged(this);
			_seed = value;
		} 
	}

	public string TerrainRootPath;
	public string TerrainManifestPath;

	private string _seed;

	public event Action<GameState> OnSeedChanged;

	[MenuItem("Assets/Create/GameState")]
	public static void CreateAsset()
	{
		Util.CreateAsset<GameState>();
	}

	public void Save(string path = null)
	{
		if(path == null)
		{
			throw new NotImplementedException();
		}

		var json = JsonUtility.ToJson(new SerialisedRep {
			DateTime = DateTime.Now,
			Seed = Seed
		});

		File.WriteAllText(path, json);

		Debug.Log($"File saved to {{{path}}}");

		throw new NotImplementedException();
	}

	public void UpdateFromJson(string json)
	{
		var data = JsonUtility.FromJson<SerialisedRep>(json);

		this.Seed = data.Seed;

	}

	public void UpdateFromLevelInfo(LevelInfo info)
	{
		throw new NotImplementedException();
	}

	private struct SerialisedRep
	{
		public DateTime DateTime;
		public string Seed;
	}
}
