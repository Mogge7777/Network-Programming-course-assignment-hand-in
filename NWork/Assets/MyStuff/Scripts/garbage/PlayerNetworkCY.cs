using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

public class PlayerNetworkCY : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI LivesText1;
    [SerializeField] private TextMeshProUGUI LivesText2;

    [SerializeField] private int moveSpeed = 5, boosts = 3;
    private int ToBigDiscrepency = 1;
    private Vector3 lastServerPosition, spawnPosition, newPosition;

    private bool hasAsignedUIText = false;

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>
        (new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> IsOutsideTrack = new NetworkVariable<bool>
        (false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    /*
    private NetworkVariable<int> Lives = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private NetworkVariable<int> Lives2 = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    */
    private NetworkVariable<int> networkBoosts = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    /*  
      private NetworkVariable<int> UIP1 = new NetworkVariable<int>(
          3, NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server);
      private NetworkVariable<int> UIP2 = new NetworkVariable<int>(
         3, NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server);
    */

    private void Update()
    {
       
    }

    void FixedUpdate()
    {
        predictMovement();
        HandleInput(); 
    }
    void HandleInput()
    {
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
            moveSpeed += 3;
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
            networkBoosts.Value--;
        }
    }
    void predictMovement()
    {
        if (IsHost) { return; }

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        if(moveDir != Vector3.zero)
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
            Debug.Log("Possible Cheating detected");
        }
    }
    private void CheckPositionMaybeReconsile()
    {
        float ToBigDiscrepency = 1;

        if (IsOwner && !IsHost)
        {
            if (Vector3.Distance(transform.position, networkedPosition.Value) > ToBigDiscrepency)
            {
                transform.position = networkedPosition.Value;
                Debug.Log("Reconciling position with server: " + OwnerClientId);
            }
        }
    }
    private void IsPlayerOnTrack()
    {
        if (IsHost)
        {
            Vector3 hostPosition = transform.position;
            if (hostPosition.z > 1.8f && hostPosition.z < 22.42f && hostPosition.x > -12.8f && hostPosition.x < 12.8f)
            {
                /*
                Lives.Value--;
                Lives2.Value--;
                Debug.Log("Lives.value: " + Lives.Value + " ClientID: " + OwnerClientId);
                */
            }
        }
    }

    private void LateUpdate()
    {
        CheckPositionMaybeReconsile();
        if (networkBoosts.Value < boosts)
        {
            boosts--;
            moveSpeed += 3;
        }
        if (LivesText1 != null)
        {
            /*
            LivesText1.text = Lives.Value.ToString();
            LivesText2.text = Lives2.Value.ToString();
            */
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            StartCoroutine(AssignUIText());
            /*
            if (IsHost)
            {
                if (!hasAsignedUIText)
                {
                    LivesText1 = GameObject.Find("Lives1").GetComponent<TextMeshProUGUI>();
                    LivesText2 = GameObject.Find("Lives2").GetComponent<TextMeshProUGUI>();
                    Debug.Log("Host: Assigning Lives1 to Player " + OwnerClientId);
                    hasAsignedUIText = true;
                }
            }
            else if (IsClient && !IsHost)
            {
                LivesText1 = GameObject.Find("Lives2").GetComponent<TextMeshProUGUI>();
                LivesText2 = GameObject.Find("Lives1").GetComponent<TextMeshProUGUI>();
                Debug.Log("Client: Assigning Lives2 to Player " + OwnerClientId);
            }
            */
            if (IsOwner)
            {
              spawnPosition = transform.position;
              lastServerPosition = spawnPosition;
            }
            else 
            {
                spawnPosition = transform.position;
                lastServerPosition = spawnPosition;
            }
            if (IsOwner) 
            {
               networkedPosition.Value = spawnPosition;
            }
        }
    }
    private IEnumerator AssignUIText()
    {
        yield return new WaitForSeconds(0.5f);

        if (IsLocalPlayer)
        {
            if (IsHost)
            {
                if (!hasAsignedUIText)
                {
                    LivesText1 = GameObject.Find("Lives1").GetComponent<TextMeshProUGUI>();
                    LivesText2 = GameObject.Find("Lives2").GetComponent<TextMeshProUGUI>();
                    Debug.Log("Host: Assigning Lives1 and Lives2 to Player " + OwnerClientId);
                    hasAsignedUIText = true;
                }
            }
            else if (IsClient && !IsHost)
            {
                LivesText1 = GameObject.Find("Lives2").GetComponent<TextMeshProUGUI>();
                LivesText2 = GameObject.Find("Lives2").GetComponent<TextMeshProUGUI>();
                Debug.Log("Client: Assigning Lives2 to Player " + OwnerClientId);
            }
        }
    }
}
