using UnityEngine;
using UnityEditor;

public class MaskFieldExample : EditorWindow
{
    static int flags = 0;
    static string[] options = new string[] {"CanJump", "CanShoot", "CanSwim"};

    [MenuItem("Examples/Mask Field usage")]
    static void Init()
    {
        MaskFieldExample window = (MaskFieldExample)GetWindow(typeof(MaskFieldExample));
        window.Show();
    }

    void OnGUI()
    {
        flags = EditorGUILayout.MaskField("Player Flags", flags, options);
		Check();
    }




	int mCategory; // This is my maskfield's result

	void Check()
	{

		for (int i = 0; i < options.Length; i++)
		{
		    int layer = 1 << i;
			if ((flags & layer) != 0)
		    {
		    	Debug.Log(options[i].ToString());
		        //This group is selected
		    }
		}

	}


}