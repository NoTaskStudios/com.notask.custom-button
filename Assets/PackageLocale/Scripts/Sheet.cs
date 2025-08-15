using System;
using UnityEngine;

namespace SimpleLocalization.Runtime
{
	[Serializable]
	public class Sheet
	{
		public string Name;
		public long Id;
        public TextAsset TextAsset;
    }
}