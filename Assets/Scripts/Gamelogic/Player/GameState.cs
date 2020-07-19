using System;
using System.Collections;
using System.Collections.Generic;
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
}
