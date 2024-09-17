using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class PlayerNetwork2 : NetworkBehaviour
{
    [SerializeField] private int moveSpeed = 5, boosts = 1, boostStrenght;
    private int ToBigDiscrepency = 5;
    [SerializeField]private Vector3 lastServerPosition, spawnPosition, newPosition;

    
    [SerializeField] private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>
        (new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<bool> IsOutsideTrack = new NetworkVariable<bool>
        (false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkVariable<int> networkBoosts = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private NetworkVariable<int> networkSpeed = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    [SerializeField] private GameObject currentWaypoint;
    [SerializeField] private int amounOfLaps, lapsDone;
    [SerializeField] NetworkManagerUI networkManager;


    private void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsLocalPlayer)
        {
            string otherName = other.name;
            HandleWaypointsServerRpc(otherName);
            Debug.Log(otherName);
        }
    }
    [ServerRpc]
    private void HandleWaypointsServerRpc(string waypointName)
    {
        if (waypointName == currentWaypoint.name) return;
        currentWaypoint.name = waypointName;
        lapsDone++;
        if (lapsDone == amounOfLaps * 2)
        {
            networkmanagerClientRpc();
        }

    }
    [ClientRpc]
    void networkmanagerClientRpc()
    {
        networkManager.FinishTextOn();
    }
    

    void FixedUpdate()
    {
        predictMovement();
        HandleInput();
    }
    void HandleInput()
    {
        if (!IsLocalPlayer) return;
        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        if (Input.GetKey(KeyCode.Space)) SpeedBoost();


        newPosition = transform.position + moveDir * moveSpeed * Time.deltaTime;
        if (IsOwner && !IsHost)
        {
            RequestPositionServerRpc(newPosition);
        }
        else if (IsHost)
        {
            IsPlayerOnTrack();
            transform.position = newPosition;
        }

    }
    private void SpeedBoost()
    {
        if (IsHost && boosts > 0)
        {
            boosts--;
            moveSpeed += boostStrenght;
        }
        else if (!IsHost && IsLocalPlayer)
        {
            RequestBoostServerRpc();
        }
    }
    [ServerRpc]
    private void RequestBoostServerRpc()
    {
        if (networkBoosts.Value > 0)
        {
            networkSpeed.Value += boostStrenght;
            networkBoosts.Value--;
        }
    }
    void predictMovement()
    {
        if (IsHost) { return; }
        if (!IsLocalPlayer) { return; }

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
       
        
        if (moveDir != Vector3.zero)
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
        
    }
    [ServerRpc]
    void RequestPositionServerRpc(Vector3 position)
    {
        IsPlayerOnTrack();

        if (Vector3.Distance(position, lastServerPosition) < ToBigDiscrepency)
        {
            networkedPosition.Value = position;
            lastServerPosition = position;
        }
        else
        {
            networkedPosition.Value = lastServerPosition;
        }
    }    
    private void CheckPositionMaybeReconsile()
    {
        if (IsOwner && !IsHost)
        {
            
            if (Vector3.Distance(transform.position, networkedPosition.Value) > ToBigDiscrepency)
            {
                transform.position = networkedPosition.Value;
            }
        }
    }
    private void IsPlayerOnTrack()
    {
        if (IsHost)
        {
            Vector3 position = transform.position;
            if (position.z > 1.8f && position.z < 22.42f && position.x > -12.8f && position.x < 12.8f)
            {
                Debug.Log("outside track");
                networkBoosts.Value++;
                networkSpeed.Value--;
                if (networkSpeed.Value == 0) networkSpeed.Value = 3;
            }
        }
    }

    private void LateUpdate()
    {
        CheckPositionMaybeReconsile();
        if (networkBoosts.Value < boosts)
        {
            boosts--;
            moveSpeed += boostStrenght;
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsClient) StartCoroutine("AssignUI");
        if (IsLocalPlayer)
        {
            spawnPosition = transform.position;
            lastServerPosition = spawnPosition;
            
            if (IsHost) StartCoroutine("AssignUI");


            if (IsOwner)
            {
                spawnPosition = transform.position;
                lastServerPosition = spawnPosition;
                networkedPosition.Value = spawnPosition;
            }
            else
            {
                spawnPosition = transform.position;
                lastServerPosition = spawnPosition;
            }
        }
        boostStrenght = 10;
    }
    public IEnumerator AssignUI()
    {
        yield return new WaitForSeconds(0.2f);
        networkManager = Object.FindObjectOfType<NetworkManagerUI>();
    }
}
