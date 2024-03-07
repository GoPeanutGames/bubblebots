using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
	[RequireComponent(typeof(VerticalLayoutGroup))]
	public class ChangeVerticalLayoutGroupOnPlatform: MonoBehaviour
	{
		public float cellSpacingDesktop;
		public float cellSpacingMobile;
		private VerticalLayoutGroup _verticalLayoutGroup;
    
		void Start()
		{
			_verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
			if (Application.isMobilePlatform){
				_verticalLayoutGroup.spacing = cellSpacingMobile;
			}
			else{
				_verticalLayoutGroup.spacing = cellSpacingDesktop;
			}
		}
	}
}