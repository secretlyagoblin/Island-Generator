using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject Player;
    public Camera PlayerCamera;
    public GameState State;
    public float PlayerTriggerDelay = 4f;

    void Awake()
    {
        State.OnTerrainLoaded += TriggerPlayer;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TriggerPlayer(GameState state)
    {
        Invoke(nameof(InvokePlayer), PlayerTriggerDelay);
    }

    private void InvokePlayer()
    {
        State.MainCamera.gameObject.SetActive(false);
        State.MainCamera = PlayerCamera;

        var ray = new Ray(Player.transform.position, Vector3.down);
        Physics.Raycast(ray, out var info, 200f);

        Debug.Log($"Moving down to {info.collider.gameObject.name}");

        Player.SetActive(true);

        Player.transform.position = info.point + (Vector3.up * 1.3f);


    }
}
