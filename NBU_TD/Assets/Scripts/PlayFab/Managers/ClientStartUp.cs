using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab.Networking;
using System.Collections.Generic;
using UnityEngine;

public class ClientStartUp : MonoBehaviour
{
    [SerializeField]
    private Configuration configuration;

    void Start()
    {
        if (configuration.buildType == BuildType.REMOTE_CLIENT)
        {
            if (configuration.buildId == "")
            {
                throw new System.Exception("A remote client build must have a buildId. Add it to the Configuration. Get this from your Multiplayer Game Manager in the PlayFab web console.");
                return;
            }
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CreateAccount = true,
                CustomId = SystemInfo.deviceUniqueIdentifier
            };

            PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnLoginError);
        }
        // asd
    }

    void OnPlayFabLoginSuccess(LoginResult loginResult)
    {
        Debug.Log("Login Success!");

        RequestMultyplayerServer();
    }


    private void RequestMultyplayerServer()
    {
        Debug.Log("[clientStartUp].RequestMultyplayerServer");

        RequestMultiplayerServerRequest serverRequest = new RequestMultiplayerServerRequest();
        serverRequest.BuildId = configuration.buildId;
        // session GUID string
        //Debug.Log(System.Guid.NewGuid().ToString());
        serverRequest.SessionId = "36e101b5-7f22-459d-915b-6e71adec6191"; //System.Guid.NewGuid().ToString(); 
        serverRequest.PreferredRegions = new List<string>() { AzureRegion.NorthEurope.ToString() };


        PlayFabMultiplayerAPI.RequestMultiplayerServer(serverRequest, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);

    }

    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        if (response == null)
        {
            return;
        }


        Debug.Log("**** THESE ARE THE DETAILS **** -- IP:" + response.IPV4Address + " Port:" + (ushort)response.Ports[0].Num);

        UnityNetworkServer.Instance.networkAddress = response.IPV4Address;
        UnityNetworkServer.Instance.GetComponent<kcp2k.KcpTransport>().Port = (ushort)response.Ports[0].Num;
        UnityNetworkServer.Instance.StartClient();
    }

    private void OnRequestMultiplayerServerError(PlayFabError error)
    {
        Debug.Log("Error!!!" + error);
    }

    void OnLoginError(PlayFabError playFabError)
    {
        Debug.Log("Login failed!");
    }
}
