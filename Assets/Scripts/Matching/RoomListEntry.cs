using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Matching
{
    public class RoomListEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameLabel = default;
        [SerializeField] private TextMeshProUGUI playerCounter = default;

        private RectTransform rectTransform;
        private Button button;
        private string roomName;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            button = GetComponent<Button>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // リスト要素がクリックされたら，対応したルーム名のルームに参加する
            button.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomName));
        }

        public void Activate(RoomInfo info)
        {
            roomName = info.Name;

            nameLabel.text = roomName;
            //nameLabel.text = (string)info.CustomProperties["DisplayName"];
            playerCounter.SetText("{0}/{1}", info.PlayerCount, info.MaxPlayers);

            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public RoomListEntry SetAsLastSibling()
        {
            rectTransform.SetAsLastSibling();
            return this;
        }
    }
}
