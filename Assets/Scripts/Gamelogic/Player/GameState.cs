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

	public int Seed { 
		get => _seed; 
		set {
			_seed = value;
			OnSeedChanged(this);			
		} 
	}

	public string TerrainRootPath;
	public string TerrainManifestPath;

	private int _seed;

	public event Action<GameState> OnSeedChanged;

	public event Action<GameState> OnTerrainLoaded;

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

		var info = new System.IO.FileInfo(path);

		if (!info.Exists)
			Directory.CreateDirectory(info.Directory.FullName);

		var json = JsonUtility.ToJson(new SerialisedRep {
			DateTime = DateTime.Now,
			Seed = Seed
		});

		File.WriteAllText(path, json);

		Debug.Log($"File saved to {{{path}}}");

		//throw new NotImplementedException();
	}

	public void TerrainLoaded()
    {
		OnTerrainLoaded(this);
    }

	public void UpdateFromJson(string json)
	{
		var data = JsonUtility.FromJson<SerialisedRep>(json);

		this.Seed = data.Seed;

	}

	public void UpdateFromLevelInfo(LevelInfo info)
	{
		this.Seed = info.World;
		//throw new NotImplementedException();
	}

	private struct SerialisedRep
	{
		public DateTime DateTime;
		public int Seed;
	}
}
