using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelProgressBarHandler : MonoBehaviour
{
    [SerializeField] private List<Image> levelIndicators;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite completedSprite;
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private int levelsPerSet = 6;

    [SerializeField] private Sprite devilLevelDefaultSprite;
    [SerializeField] private Sprite devilLevelCurrentSprite;
    [SerializeField] private Text levelcount;
    
    private void OnEnable()
    {
        //UpdateProgressBar();
    }

    public void UpdateProgressBar()
    {
        if (levelIndicators == null || levelIndicators.Count != levelsPerSet)
        {
            return;
        }

        int playerTotalLevel = GameManager.Instance.levelManager.GlobalLevelNumber;
        int levelInCurrentSetIndex = (playerTotalLevel) % levelsPerSet;
        int devilLevelIndex = levelsPerSet - 1; // The index for the 6th level (0-indexed)
        
        Debug.Log("UpdateProgressBar() = " + playerTotalLevel );
        
        levelcount.text = "Level " + (playerTotalLevel+1);
        for (int i = 0; i < levelsPerSet; i++)
        {
            if (i < levelInCurrentSetIndex)
            {
                levelIndicators[i].sprite = completedSprite;
            }
            else if (i == levelInCurrentSetIndex)
            {
                if (i == devilLevelIndex)
                {
                    levelIndicators[i].sprite = devilLevelCurrentSprite;
                }
                else
                {
                    levelIndicators[i].sprite = currentSprite;
                }
            }
            else
            {
                if (i == devilLevelIndex)
                {
                    levelIndicators[i].sprite = devilLevelDefaultSprite;
                }
                else
                {
                    levelIndicators[i].sprite = defaultSprite;
                }
            }
            levelIndicators[i].SetNativeSize();
        }
    }
}