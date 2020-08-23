using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Matching
{
    public class RoomListView : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private RoomListEntry roomListEntryPrefab = default; // RoomListEntryプレハブの参照

        private Dictionary<string, RoomListEntry> activeEntries = new Dictionary<string, RoomListEntry>();
        private Stack<RoomListEntry> inactiveEntries = new Stack<RoomListEntry>();

        private void Awake()
        {
        }

        // ルームリストが更新された時に呼ばれるコールバック
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var info in roomList)
            {
                RoomListEntry entry;
                if (activeEntries.TryGetValue(info.Name, out entry))
                {
                    if (!info.RemovedFromList)
                    {
                        // リスト要素を更新する
                        entry.Activate(info);
                    }
                    else
                    {
                        // リスト要素を削除する
                        activeEntries.Remove(info.Name);
                        entry.Deactivate();
                        inactiveEntries.Push(entry);
                    }
                }
                else if (!info.RemovedFromList)
                {
                    // リスト要素を追加する
                    entry = (inactiveEntries.Count > 0)
                        ? inactiveEntries.Pop().SetAsLastSibling()
                        : Instantiate(roomListEntryPrefab, this.transform);
                    entry.Activate(info);
                    activeEntries.Add(info.Name, entry);
                }
            }
        }
    }
}