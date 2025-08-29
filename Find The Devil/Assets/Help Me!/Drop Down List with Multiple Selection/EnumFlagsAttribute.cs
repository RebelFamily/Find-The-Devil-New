using UnityEngine;
using System.Collections;

public class EnumFlagsAttribute : PropertyAttribute
 {
     public EnumFlagsAttribute() { }
 }

 /////////////How to use it////////////////
 /*e.g
	[System.Flags]
	public enum FurnitureType
	{
		None , SofaPrefab , Curtains , Table , Chairs 
	}


	[SerializeField] [EnumFlagsAttribute]
	public FurnitureType buttonsToEnable;

	use this funtion to check which options are selected in dropdownlist
	List<int> ReturnSelectedElements()
	{

		List<int> selectedElements = new List<int>();
		for (int i = 0; i < System.Enum.GetValues(typeof(References.FurnitureType)).Length; i++)
		{
		    int layer = 1 << i;
			if (((int)buttonsToEnable & layer) != 0)
		    {
		    	selectedElements.Add(i);
		    }
		}

		return selectedElements;

	}
 */

