using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameServer {
	public class DropdownScrollControl : MonoBehaviour {
		ScrollRect Sr;

		void Awake() {
			Sr = GetComponent<ScrollRect>();
		}

		void Start() {
			if (Sr == null)
				return;

			var dropdown = GetComponentInParent<Dropdown>();
			var viewport = Sr.transform.Find("Viewport").GetComponent<RectTransform>();
			var contentArea = Sr.transform.Find("Viewport/Content").GetComponent<RectTransform>();
			var contentItem = Sr.transform.Find("Viewport/Content/Item").GetComponent<RectTransform>();

			// Viewportに対するContentのスクロール位置を求める
			var areaHeight = contentArea.rect.height - viewport.rect.height;
			var cellHeight = contentItem.rect.height;
			var scrollRatio = (cellHeight * dropdown.value) / areaHeight;
			Sr.verticalNormalizedPosition = 1.0f - Mathf.Clamp(scrollRatio, 0.0f, 1.0f);
		}
	}
}