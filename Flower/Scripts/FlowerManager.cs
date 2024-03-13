using Godot;
using System.Collections.Generic;

namespace Flower{
	public partial class FlowerManager
	{
		private static FlowerManager _instance;
		public static FlowerManager instance{
			get {
				if(_instance == null){
					_instance = new FlowerManager();
				}
				return _instance;
			}
		}
		private Dictionary<string, FlowerSystem> flowerSystemMap = new Dictionary<string, FlowerSystem>();
		public FlowerSystem GetOrCreateFlowerSystem(string key="DefaultFlower"){
			if(flowerSystemMap.ContainsKey(key) && Node.IsInstanceValid(flowerSystemMap[key])){
				return flowerSystemMap[key];
			}else{
				var fs = new FlowerSystem();
				fs.Name = $"FlowerSystem-${key}";
				flowerSystemMap[key] = fs;
				var mainScene = (SceneTree)Engine.GetMainLoop();
				mainScene.CurrentScene.CallDeferred("add_child",fs);
				return fs;
			}
		}
	}
}
