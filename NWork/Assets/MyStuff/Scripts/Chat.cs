using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Chat : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI AllChatText;
    [SerializeField] TMP_InputField UserInput;

    public override void OnNetworkSpawn()
    {
        
    }
    [Rpc(SendTo.Server)]
    public void SubmittMessageRpc(FixedString128Bytes message)
    {
        UpdateMessageRpc(message);
    }
    [Rpc(SendTo.Everyone)]
    public void UpdateMessageRpc(FixedString128Bytes message)
    {
        AllChatText.text = message.ToString();
    }
    private void Update()
    {
        
    }
    public void SendMessage()
    {
        SubmittMessageRpc(UserInput.text);
    }
}
