using System;

namespace PlayableDoomguy {
    public class STPercentage : MonoBehaviour {
        public Image D1;
        public Image D2;
        public Image D3;
        private int currentPercentage = 100;

        public void UpdatePercentage(float current, float max) {
            currentPercentage = Mathf.Clamp(Mathf.RoundToInt(current / max * 100f), 0, 200);
        }

        public void UpdatePercentage(int percentage) {
            currentPercentage = percentage;
        }

        public void FixedUpdate() {
            char[] parts = currentPercentage.ToString().ToCharArray();

            bool showFirst = parts.Length >= 3;
            bool showSecond = parts.Length >= 2;

            int index = 0;

            if (showFirst) {
                D1.enabled = true;
                D1.sprite = STBarTextManager.map[parts[index]];
                index++;
            }
            else {
                D1.enabled = false;
            }

            if (showSecond) {
                D1.enabled = true;
                D2.sprite = STBarTextManager.map[parts[index]];
                index++;
            }
            else {
                D2.enabled = false;
            }

            D3.sprite = STBarTextManager.map[parts[index]];
        }
    }

    public class STBarTextManager {
        public static Dictionary<char, Sprite> map = new();

        public static void Init() {
            for (int i = 0; i < 10; i++) {
                map.Add(i.ToString().ToCharArray()[0], Plugin.bundle.LoadAsset<Sprite>("STTNUM" + i + ".png"));
            }

            map.Add('%', Plugin.bundle.LoadAsset<Sprite>("STTPRCNT.png"));
        }
    }
}