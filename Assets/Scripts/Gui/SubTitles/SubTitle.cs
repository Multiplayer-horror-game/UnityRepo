using System.Collections.Generic;
using UnityEngine;

namespace Gui.SubTitles
{
    [CreateAssetMenu(fileName = "SubTitle", menuName = "ScriptableObjects/SubTitle", order = 1)]
    public class SubTitle : ScriptableObject
    {
        public AudioClip audioClip;
        
        public List<SubTitleFragments> subTitles = new();
    }
}
