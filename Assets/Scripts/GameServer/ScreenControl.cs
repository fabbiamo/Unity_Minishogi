using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameServer
{
	public class ScreenControl : MonoBehaviour
    {
		[SerializeField]
		Button startNewGameButton = default;

		[SerializeField]
		Button undoButton = default;

		[SerializeField]
		Button resignButton = default;

		[SerializeField]
		Dropdown dropdown = default;

		[SerializeField]
		Button startingPositionButton = default;

		[SerializeField]
		Button previousPositionButton = default;

		[SerializeField]
		Button nextPositionButton = default;

		[SerializeField]
		Button lastPositionButton = default;

        public int DropdownValue { get { return dropdown.value; } }

		private void Awake()
		{
			startingPositionButton.onClick.AddListener(() => OnClickPositionButton(0));
			previousPositionButton.onClick.AddListener(() => OnClickPositionButton(1));
			nextPositionButton.onClick.AddListener(() => OnClickPositionButton(2));
			lastPositionButton.onClick.AddListener(() => OnClickPositionButton(3));
		}

		public void Clear()
		{
			dropdown.options.Clear();
			dropdown.options.Add(new Dropdown.OptionData { text = "     初期局面" });
			dropdown.RefreshShownValue();
		}

		public void EntryItem(string kifMove)
		{
			dropdown.options.Add(new Dropdown.OptionData { text = kifMove });
			dropdown.SetValueWithoutNotify(dropdown.options.Count - 1); // callbackを呼ばない
			dropdown.RefreshShownValue();
		}

		public void RemoveItem(int index, int count)
		{
			dropdown.options.RemoveRange(index + 1, count); // 「初期局面」の分だけindexをずらす
			dropdown.SetValueWithoutNotify(dropdown.options.Count - 1); // callbackを呼ばない
			dropdown.RefreshShownValue();
		}

		public void Interactable(bool startButtonEnable, bool isGameEnded)
		{
			startNewGameButton.interactable = startButtonEnable;
			undoButton.interactable = dropdown.value > 0 && !isGameEnded;
			resignButton.interactable = !isGameEnded;
			dropdown.template.GetComponentInChildren<Toggle>().interactable = isGameEnded;

			startingPositionButton.interactable = previousPositionButton.interactable = dropdown.value > 0 && isGameEnded;
			nextPositionButton.interactable = lastPositionButton.interactable = dropdown.value < dropdown.options.Count - 1 && isGameEnded;
		}

		/// <summary>
		/// dropdown.valueの値を変える
		/// GUIManager.OnClickDropdownItem()が呼ばれる
		/// </summary>
		/// <param name="type"></param>
		public void OnClickPositionButton(int type)
		{
			if (type == 0)
				dropdown.value = 0;

			else if (type == 1)
				--dropdown.value;

			else if (type == 2)
				++dropdown.value;

			else
				dropdown.value = dropdown.options.Count - 1;
		}
	}
}
