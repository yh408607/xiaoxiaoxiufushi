using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameUlit
{
    public static class LevelScoreSave
    {
        public static void SaveBestStar(string levelName, int stars)
        {
            string key = $"Level_{levelName}_BestStar";

            int oldStars = PlayerPrefs.GetInt(key, 0);

            if (stars > oldStars)
            {
                PlayerPrefs.SetInt(key, stars);
                PlayerPrefs.Save();
            }
        }

        public static int GetBestStar(string levelName)
        {
            string key = $"Level_{levelName}_BestStar";
            return PlayerPrefs.GetInt(key, 0);
        }
    }

}
