using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.PlayFabManager
{
    public class PhotonManager : MonoBehaviourPunCallbacks
    {
        void Awake()
        {
            // シーンの自動同期：有効
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            Connect();
        }

        // Update is called once per frame
        void Update()
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.PlayerList.Length == 2)
                    SceneManager.LoadScene("PlayScene");
            }
            else
            {
                //matchingText.text = PhotonNetwork.NetworkClientState.ToString();
            }
        }

        public void Connect()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.GameVersion = "1.0";
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void Disconnect()
        {
            PhotonNetwork.Disconnect();
        }

        public void JoinOrCreateRoom(string roomName)
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                MaxPlayers = 2,
                IsVisible = true,
                PublishUserId = true,
                IsOpen = true,
            };

            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        public void SetProperties(PlayerData.PlayerData playerData)
        {
            var properties = new Hashtable();
            properties.Add("Name", playerData.Name);
            properties.Add("Rating", playerData.Rating);
            PhotonNetwork.SetPlayerCustomProperties(properties);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");
            //PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("JoinedRoom");
            //Debug.LogFormat($"RoomName: {PhotonNetwork.CurrentRoom.Name}");
            //Debug.LogFormat($"HostName: {PhotonNetwork.MasterClient.NickName}");
            //Debug.LogFormat($"Slots: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
            //Debug.LogFormat($"UserNo: {PhotonNetwork.LocalPlayer.ActorNumber}");
            //Debug.LogFormat($"UserId: {PhotonNetwork.LocalPlayer.UserId}");
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("CreatedRoom");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogFormat("OnJoinRandomFailed, {0}", message);

            RoomOptions roomOptions = new RoomOptions()
            {
                MaxPlayers = 2,
                IsVisible = true,
                PublishUserId = true,
                IsOpen = true,
            };

            PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
        }
    }
}
