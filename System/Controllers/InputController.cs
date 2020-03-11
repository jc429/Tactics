using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputID{
	DEBUG				= (1<<0),
	DpadLeft		= (1<<1),
	DpadRight		= (1<<2),
	DpadUp			= (1<<3),
	DpadDown		= (1<<4),
	FaceN 			= (1<<5),
	FaceE 			= (1<<6),
	FaceS 			= (1<<7),
	FaceW 			= (1<<8),


	Pause				= (1<<11),
	Reset				= (1<<12)
}

public enum ButtonState{
	Active,
	Pressed,
	Released
}

public enum CursorInputType{
	DPad,
	Mouse,
	Touch
}

public static class InputController {

	static int inputQueueLength = 10;

	static ushort currentInputs = 0;
	static ushort previousInputs = 0;
	
	static List<ushort> inputQueue = new List<ushort>();

	public static void Initialize(){
		if(inputQueue == null){
			inputQueue = new List<ushort>();
		}
		ClearInputs();
	}

	public static void ClearInputs(){
		inputQueue.Clear();
	}

	public static void CaptureInputs(){
		ushort inputs = 0;

		inputs |= (Input.GetKey(KeyCode.W)) ? (ushort)InputID.DpadUp : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.S)) ? (ushort)InputID.DpadDown : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.A)) ? (ushort)InputID.DpadLeft : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.D)) ? (ushort)InputID.DpadRight : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.I)||Input.GetKey(KeyCode.UpArrow)) ? (ushort)InputID.FaceN : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.K)||Input.GetKey(KeyCode.DownArrow)) ? (ushort)InputID.FaceS : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.J)||Input.GetKey(KeyCode.LeftArrow)) ? (ushort)InputID.FaceW : (ushort)0;
		inputs |= (Input.GetKey(KeyCode.L)||Input.GetKey(KeyCode.RightArrow)) ? (ushort)InputID.FaceE : (ushort)0;
		inputs |= (Input.GetButtonDown("Pause")) ? (ushort)InputID.Pause : (ushort)0;
		inputs |= (Input.GetButtonDown("Reset")) ? (ushort)InputID.Reset : (ushort)0;

		previousInputs = currentInputs;
		currentInputs = inputs; 
		inputQueue.Add(inputs);
		if(inputQueue.Count > inputQueueLength){
			inputQueue.RemoveAt(0);
		}
	}

	public static bool CheckQueuedInput(InputID input){
		return CheckQueuedInput((ushort)input);
	}
	public static bool CheckQueuedInput(ushort input){
		if(inputQueue.Count < 1){
			return false;
		}
		for(int i = inputQueue.Count - 1; i >= 0; i--){
			if((inputQueue[i] & input) != 0){
				return true;
			}
		}
		return false;
	}

	public static bool CheckQueuedInputPressed(InputID input){
		return CheckQueuedInputPressed((ushort)input);
	}
	public static bool CheckQueuedInputPressed(ushort input){
		if(inputQueue.Count < 2){
			return false;
		}
		for(int i = inputQueue.Count - 2; i >= 0; i--){
			if((inputQueue[i] & input) == 0 && (inputQueue[i+1] & input) != 0 ){
				return true;
			}
		}
		return false;
	}

	public static bool CheckQueuedInputReleased(InputID input){
		return CheckQueuedInputReleased((ushort)input);
	}
	public static bool CheckQueuedInputReleased(ushort input){
		if(inputQueue.Count < 2){
			return false;
		}
		for(int i = inputQueue.Count - 2; i >= 0; i--){
			if((inputQueue[i] & input) != 0 && (inputQueue[i+1] & input) == 0 ){
				return true;
			}
		}
		return false;
	}

	public static bool CheckCurrentInput(InputID input){
		return CheckCurrentInput((ushort)input);
	}
	public static bool CheckCurrentInput(ushort input){
		return ((currentInputs & input) != 0);
	}

	public static bool CheckCurrentInputPressed(InputID input){
		return CheckCurrentInputPressed((ushort)input);
	}
	public static bool CheckCurrentInputPressed(ushort input){
		return ((currentInputs & input) != 0 && (previousInputs & input) == 0);
	}

	public static bool CheckCurrentInputReleased(InputID input){
		return CheckCurrentInputReleased((ushort)input);
	}
	public static bool CheckCurrentInputReleased(ushort input){
		return ((currentInputs & input) == 0 && (previousInputs & input) != 0);
	}

	public static float GetAxisHorizontal(){
		return Input.GetAxisRaw("Horizontal");
	}

	public static float GetAxisVertical(){
		return Input.GetAxisRaw("Vertical");
	}

	public static bool UpDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadUp);
		}else{
			return CheckCurrentInput(InputID.DpadUp);
		}
	}

	public static bool DownDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadDown);
		}else{
			return CheckCurrentInput(InputID.DpadDown);
		}
	}

	public static bool LeftDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadLeft);
		}else{
			return CheckCurrentInput(InputID.DpadLeft);
		}
	}

	public static bool RightDPadPressed(bool queued = false){
		if(queued){
			return CheckQueuedInput(InputID.DpadRight);
		}else{
			return CheckCurrentInput(InputID.DpadRight);
		}
	}

	public static Vector2 GetDirectionHeld(){
		Vector3 v = Vector2.zero;
		v.x += RightDPadPressed() ?  1 : 0;
		v.x += LeftDPadPressed()  ? -1 : 0;
		v.y += UpDPadPressed()    ?  1 : 0;
		v.y += DownDPadPressed()  ? -1 : 0;
		return v;
	}

	/**********************************************************************************/

	public static bool FaceButtonNorth(ButtonState state, bool queued = true){
		switch(state){
			case ButtonState.Pressed:
				return queued ? CheckQueuedInputPressed(InputID.FaceN) : CheckCurrentInputPressed(InputID.FaceN);
			case ButtonState.Released:
				return queued ? CheckQueuedInputReleased(InputID.FaceN) : CheckCurrentInputReleased(InputID.FaceN);
			case ButtonState.Active:
			default:
				return queued ? CheckQueuedInput(InputID.FaceN) : CheckCurrentInput(InputID.FaceN);
		}
	}

	public static bool FaceButtonSouth(ButtonState state, bool queued = true){
		switch(state){
			case ButtonState.Pressed:
				return queued ? CheckQueuedInputPressed(InputID.FaceS) : CheckCurrentInputPressed(InputID.FaceS);
			case ButtonState.Released:
				return queued ? CheckQueuedInputReleased(InputID.FaceS) : CheckCurrentInputReleased(InputID.FaceS);
			case ButtonState.Active:
			default:
				return queued ? CheckQueuedInput(InputID.FaceS) : CheckCurrentInput(InputID.FaceS);
		}
	}

	public static bool FaceButtonWest(ButtonState state, bool queued = true){
		switch(state){
			case ButtonState.Pressed:
				return queued ? CheckQueuedInputPressed(InputID.FaceW) : CheckCurrentInputPressed(InputID.FaceW);
			case ButtonState.Released:
				return queued ? CheckQueuedInputReleased(InputID.FaceW) : CheckCurrentInputReleased(InputID.FaceW);
			case ButtonState.Active:
			default:
				return queued ? CheckQueuedInput(InputID.FaceW) : CheckCurrentInput(InputID.FaceW);
		}
	}

	public static bool FaceButtonEast(ButtonState state, bool queued = true){
		switch(state){
			case ButtonState.Pressed:
				return queued ? CheckQueuedInputPressed(InputID.FaceE) : CheckCurrentInputPressed(InputID.FaceE);
			case ButtonState.Released:
				return queued ? CheckQueuedInputReleased(InputID.FaceE) : CheckCurrentInputReleased(InputID.FaceE);
			case ButtonState.Active:
			default:
				return queued ? CheckQueuedInput(InputID.FaceE) : CheckCurrentInput(InputID.FaceE);
		}
	}

	/**********************************************************************************/


	public static bool PauseButtonPressed(bool held = false){

		return (Input.GetButtonDown("Pause"));

		
	}

	public static bool ResetButtonPressed(bool held = false){
		
		return (Input.GetButtonDown("Reset"));

		
	}

}
