using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameServer {
	public class ScreenControl : MonoBehaviour {
		[SerializeField]
		Button StartNewGameButton = default;

		[SerializeField]
		Button UndoButton = default;

		[SerializeField]
		Button ResignButton = default;

		[SerializeField]
		Dropdown Dropdown = default;

		[SerializeField]
		Button StartingPositionButton = default;

		[SerializeField]
		Button PreviousPositionButton = default;

		[SerializeField]
		Button NextPositionButton = default;

		[SerializeField]
		Button LastPositionButton = default;

		public int DropdownValue { get { return Dropdown.value; } }

		void Awake() {
			StartingPositionButton.onClick.AddListener(() => OnClickPositionButton(0));
			PreviousPositionButton.onClick.AddListener(() => OnClickPositionButton(1));
			NextPositionButton.onClick.AddListener(() => OnClickPositionButton(2));
			LastPositionButton.onClick.AddListener(() => OnClickPositionButton(3));
		}

        public void Clear() {
			Dropdown.options.Clear();
			Dropdown.options.Add(new Dropdown.OptionData { text = "     初期局面" });
			Dropdown.RefreshShownValue();
		}

		public void EntryItem(string kifMove) {
			Dropdown.options.Add(new Dropdown.OptionData { text = kifMove });
			Dropdown.SetValueWithoutNotify(Dropdown.options.Count - 1); // callbackを呼ばない
			Dropdown.RefreshShownValue();
		}

		public void RemoveItem(int index, int count) {
			Dropdown.options.RemoveRange(index + 1, count); // 「初期局面」の分だけindexをずらす
			Dropdown.SetValueWithoutNotify(Dropdown.options.Count - 1); // callbackを呼ばない
			Dropdown.RefreshShownValue();
		}

		public void Interactable(bool startButtonEnable, bool isGameEnded) {
			StartNewGameButton.interactable = startButtonEnable;
			UndoButton.interactable = Dropdown.value > 0 && !isGameEnded;
			ResignButton.interactable = !isGameEnded;
			Dropdown.template.GetComponentInChildren<Toggle>().interactable = isGameEnded;

			StartingPositionButton.interactable = PreviousPositionButton.interactable = Dropdown.value > 0 && isGameEnded;
			NextPositionButton.interactable = LastPositionButton.interactable = Dropdown.value < Dropdown.options.Count - 1 && isGameEnded;
		}

		/// <summary>
		/// dropdown.valueの値を変える
		/// GUIManager.OnClickDropdownItem()が呼ばれる
		/// </summary>
		/// <param name="type"></param>
		public void OnClickPositionButton(int type) {
			if (type == 0)
				Dropdown.value = 0;

			else if (type == 1)
				--Dropdown.value;

			else if (type == 2)
				++Dropdown.value;

			else
				Dropdown.value = Dropdown.options.Count - 1;
		}
	}
}
