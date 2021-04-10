using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Matchmaking {
    public class PhotonManager : MonoBehaviourPunCallbacks {
        void Awake() {
            // シーンの自動同期：有効
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start() {
            Connect();
        }

        // Update is called once per frame
        void Update() {
            if (PhotonNetwork.InRoom) {
                if (PhotonNetwork.PlayerList.Length == 2)
                    SceneManager.LoadScene("PlayScene");
            }
        }

        public void Connect() {
            if (!PhotonNetwork.IsConnected) {
                PhotonNetwork.GameVersion = "1.0";
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void Disconnect() {
            PhotonNetwork.Disconnect();
        }

        public void JoinOrCreateRoom(string roomName) {
            RoomOptions roomOptions = new RoomOptions() {
                MaxPlayers = 2,
                IsVisible = true,
                PublishUserId = true,
                IsOpen = true,
            };

            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        public void SetProperties(string name, double rating) {
            var properties = new Hashtable();
            properties.Add("Name", name);
            properties.Add("Rating", rating);
            PhotonNetwork.SetPlayerCustomProperties(properties);
        }

        public override void OnConnectedToMaster() {
            Debug.Log("OnConnectedToMaster");
            //PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby() {
            Debug.Log("OnJoinedLobby");
        }

        public override void OnJoinedRoom() {
            Debug.Log("JoinedRoom");
#if false
            Debug.Log($"RoomName: {PhotonNetwork.CurrentRoom.Name}");
            Debug.Log($"HostName: {PhotonNetwork.MasterClient.NickName}");
            Debug.Log($"Slots: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
            Debug.Log($"UserNo: {PhotonNetwork.LocalPlayer.ActorNumber}");
            Debug.Log($"UserId: {PhotonNetwork.LocalPlayer.UserId}");
#endif
        }

        public override void OnCreatedRoom() {
            Debug.Log("CreatedRoom");
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            Debug.Log($"OnJoinRandomFailed, {message}");

            RoomOptions roomOptions = new RoomOptions() {
                MaxPlayers = 2,
                IsVisible = true,
                PublishUserId = true,
                IsOpen = true,
            };

            PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
        }
    }
}
