using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace Assets.Scripts.Matching
{
    public class PhotonManager : MonoBehaviourPunCallbacks
    {
        //[SerializeField] private TextMeshProUGUI matchingText = default;

        string gameVersion = "1.0";
        int maxPlayers = 2;

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
                //matchingText.text = "matching...";

                if (PhotonNetwork.PlayerList.Length == maxPlayers)
                {
                    Player[] players = PhotonNetwork.PlayerList;
                    foreach (var player in players)
                    {
                        Debug.Log(player.NickName + " ID = " + player.UserId);
                    }

                    SceneManager.LoadScene("PlayScene");
                }
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
                PhotonNetwork.GameVersion = gameVersion;
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
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                PublishUserId = true,
                IsOpen = true,
            };

            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
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

            Debug.LogFormat("RoomName: {0}", PhotonNetwork.CurrentRoom.Name);
            Debug.LogFormat("HostName: {0}", PhotonNetwork.MasterClient.NickName);
            Debug.LogFormat("Slots: {0}/{1}", PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);

            Debug.LogFormat("UserNo: {0}", PhotonNetwork.LocalPlayer.ActorNumber);
            Debug.LogFormat("UserId: {0}", PhotonNetwork.LocalPlayer.UserId);
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
