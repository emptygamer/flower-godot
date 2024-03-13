using Godot;
using System;
using Flower;
using UniCoroutine;
using System.Collections.Generic;
using System.Collections;

public partial class UsageCase : Node
{
	FlowerSystem flowerSys;
    private string myName;
    private int progress = 0;
    private bool pickedUpTheKey = false;
    private bool isGameEnd = false;
    private bool isLocked = false;

	public override void _Ready()
	{
		if(!ResourceLoader.Exists("res://Resources/bgm.mp3")){
			GD.PushWarning("The audio file : 'res://Resources/bgm.mp3' is necessary for the demonstration. Please add to the Resources folder.");
		}

        flowerSys = FlowerManager.instance.GetOrCreateFlowerSystem("FlowerSample");
        flowerSys.SetupDialog();

        // Setup Variables.
        myName = "Rempty (｢･ω･)｢";
        flowerSys.SetVariable("MyName", myName);

        // Define your customized commands.
        flowerSys.RegisterCommand("UsageCase", CustomizedFunction);
        // Define your customized effects.
        flowerSys.RegisterEffect("customizedRotation", EffectCustomizedRotation);
	}

	public override void _Process(double delta)
	{
		// ----- Integration DEMO -----
        // Your own logic control.
        if(flowerSys.isCompleted && !isGameEnd && !isLocked){
            switch(progress){
                case 0:
                    flowerSys.ReadTextFromResource("res://Resources/start.txt");
                    break;
                case 1:
                    flowerSys.ReadTextFromResource("res://Resources/demo_start.txt");
                    break;
                case 2:
                    flowerSys.SetupButtonGroup();
                    if(!pickedUpTheKey){
                        flowerSys.SetupButton("Pickup the key.",()=>{
                            pickedUpTheKey = true;
                            flowerSys.Resume();
                            flowerSys.RemoveButtonGroup();
                            flowerSys.ReadTextFromResource("res://Resources/demo_key.txt");
                            progress = 2;
                            isLocked=false;
                        });
                    }
                    flowerSys.SetupButton("Open the door",()=>{
                        if(pickedUpTheKey){
                            flowerSys.Resume();
                            flowerSys.RemoveButtonGroup();
                            flowerSys.ReadTextFromResource("res://Resources/demo_door.txt");
                            isLocked=false;
                        }else{
                            flowerSys.Resume();
                            flowerSys.RemoveButtonGroup();
                            flowerSys.ReadTextFromResource("res://Resources/demo_locked_door.txt");
                            progress = 2;
                            isLocked=false;
                        }
                    });
                    isLocked=true;
                    break;
                case 3:
                    isGameEnd=true;
                    break;
            }
            progress ++;
        }

        if (!isGameEnd)
        {
            if(Input.IsKeyPressed(Key.Space)){
                // Continue the messages, stoping by [w] or [lr] keywords.
                flowerSys.Next();
            }
            if(Input.IsKeyPressed(Key.R)){
                // Resume the system that stopped by [stop] or Stop().
                flowerSys.Resume();
            }
        }
	}

	private void CustomizedFunction(List<string> _params)
    {
        var resultValue = int.Parse(_params[0]) + int.Parse(_params[1]);
        GD.Print($"Hi! This is called by CustomizedFunction with the result of parameters : {resultValue}");
    }

	IEnumerator CustomizedRotationTask(string key, Node obj, float endTime){
		if(obj is Node2D){
			var startRotationDegree = (obj as Node2D).RotationDegrees;
			var endRotationDegree = (obj as Node2D).RotationDegrees+180;
			yield return flowerSys.EffectTimerTask(key, endTime, (percent)=>{
            	// Update function.
				var currentDegree = Mathf.Lerp(startRotationDegree, endRotationDegree, percent);
				(obj as Node2D).RotationDegrees = currentDegree;
			});
        }else if(obj is Node3D){
			var startRotationDegree = (obj as Node3D).RotationDegrees;
			var endRotationDegree = (obj as Node3D).RotationDegrees + new Vector3(0,0,180);
			yield return flowerSys.EffectTimerTask(key, endTime, (percent)=>{
            	// Update function.
				var x = Mathf.Lerp(startRotationDegree.X, endRotationDegree.X, percent);
				var y = Mathf.Lerp(startRotationDegree.Y, endRotationDegree.Y, percent);
				var z = Mathf.Lerp(startRotationDegree.Z, endRotationDegree.Z, percent);
				(obj as Node3D).RotationDegrees = new Vector3(x, y, z);
			});
		}
		yield return null;
    }

    private void EffectCustomizedRotation(string key, List<string> _params){
        try{
            // Parse parameters.
            float endTime;
            try{
                endTime = float.Parse(_params[0])/1000;
            }catch(Exception e){
                throw new Exception($"Invalid effect parameters.\n{e}");
            }
            // Extract targets.
            Node sceneObj = flowerSys.GetSceneObject(key);
            // Apply tasks.
			CoroutineSystem.instance.StartCoroutine(CustomizedRotationTask($"CustomizedRotation-{key}", sceneObj, endTime));
        }catch(Exception){
            GD.PushError($"Effect - EffectCustomizedRotation @ [{key}] failed.");
        }
    }
}
