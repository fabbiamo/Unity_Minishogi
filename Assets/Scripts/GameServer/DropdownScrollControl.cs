using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameServer
{
	public class DropdownScrollControl : MonoBehaviour
	{
		ScrollRect sr;

		void Awake()
		{
			sr = GetComponent<ScrollRect>();
		}

		void Start()
		{
			if (sr == null)
				return;

			var dropdown = GetComponentInParent<Dropdown>();
			var viewport = sr.transform.Find("Viewport").GetComponent<RectTransform>();
			var contentArea = sr.transform.Find("Viewport/Content").GetComponent<RectTransform>();
			var contentItem = sr.transform.Find("Viewport/Content/Item").GetComponent<RectTransform>();

			// Viewportに対するContentのスクロール位置を求める
			var areaHeight = contentArea.rect.height - viewport.rect.height;
			var cellHeight = contentItem.rect.height;
			var scrollRatio = (cellHeight * dropdown.value) / areaHeight;
			sr.verticalNormalizedPosition = 1.0f - Mathf.Clamp(scrollRatio, 0.0f, 1.0f);
		}
	}
}