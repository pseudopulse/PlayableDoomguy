using System;

namespace PlayableDoomguy {
    [CreateAssetMenu(menuName = "ScriptableObject/AudioCollection", fileName = "AudioCollection", order = 1)]
    public class AudioCollection : ScriptableObject {
        public List<Audio> Audio;

        public AudioClip FetchClipByName(string name) {
            return Audio.FirstOrDefault(x => x.Name == name)?.Clip ?? Audio[0].Clip;
        }
    }
    
    [Serializable]
    public class Audio {
        public string Name;
        public AudioClip Clip;
    }
}