using System;
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
	CamLeft			= (1<<9),
	CamRight		= (1<<10),
	CamUp				= (1<<11),
	CamDown			= (1<<12),
	TriggerL		= (1<<13),
	TriggerR		= (1<<14),


	Pause				= (1<<30),
	Reset				= (1<<31)
}

public enum InputAxis{
	CamHorizontal		= 0,
	CamVertical			= 1,
	CamZoom					= 2
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
	const int inputAxisCount = 3;

	// this frame and last frame's inputs
	static UInt32 currentInputs = 0;
	static UInt32 previousInputs = 0;
	
	// input history lasting X frames
	static int inputQueueLength = 10;
	static List<UInt32> inputQueue = new List<UInt32>();

	static float[] inputAxisArray = new float[inputAxisCount];

	public static void Initialize(){
		if(inputQueue == null){
			inputQueue = new List<UInt32>();
		}
		ClearInputs();
	}

	public static void ClearInputs(){
		inputQueue.Clear();
	}

	public static void CaptureInputs(){
		UInt32 inputs = 0;
		//TODO: figure out a way to not hardcode these
		inputs |= (Input.GetKey(KeyCode.UpArrow)) ? (UInt32)InputID.DpadUp : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.DownArrow)) ? (UInt32)InputID.DpadDown : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.LeftArrow)) ? (UInt32)InputID.DpadLeft : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.RightArrow)) ? (UInt32)InputID.DpadRight : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.I)) ? (UInt32)InputID.FaceN : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.K)) ? (UInt32)InputID.FaceS : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.J)) ? (UInt32)InputID.FaceW : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.L)) ? (UInt32)InputID.FaceE : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.W)) ? (UInt32)InputID.CamUp : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.S)) ? (UInt32)InputID.CamDown : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.A)) ? (UInt32)InputID.CamLeft : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.D)) ? (UInt32)InputID.CamRight : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.Q)) ? (UInt32)InputID.TriggerL : (UInt32)0;
		inputs |= (Input.GetKey(KeyCode.E)) ? (UInt32)InputID.TriggerR : (UInt32)0;
//		inputs |= (Input.GetButtonDown("Pause")) ? (UInt32)InputID.Pause : (UInt32)0;
//		inputs |= (Input.GetButtonDown("Reset")) ? (UInt32)InputID.Reset : (UInt32)0;

		previousInputs = currentInputs;
		currentInputs = inputs; 
		inputQueue.Add(inputs);
		if(inputQueue.Count > inputQueueLength){
			inputQueue.RemoveAt(0);
		}

		inputAxisArray[(int)InputAxis.CamHorizontal] = Input.GetAxis("Camera Horizontal");
		inputAxisArray[(int)InputAxis.CamVertical] = Input.GetAxis("Camera Vertical");
		inputAxisArray[(int)InputAxis.CamZoom] = Input.GetAxis("Camera Zoom");
	}

	public static bool CheckQueuedInput(InputID input){
		return CheckQueuedInput((UInt32)input);
	}
	public static bool CheckQueuedInput(UInt32 input){
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
		return CheckQueuedInputPressed((UInt32)input);
	}
	public static bool CheckQueuedInputPressed(UInt32 input){
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
		return CheckQueuedInputReleased((UInt32)input);
	}
	public static bool CheckQueuedInputReleased(UInt32 input){
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
		return CheckCurrentInput((UInt32)input);
	}
	public static bool CheckCurrentInput(UInt32 input){
		return ((currentInputs & input) != 0);
	}

	public static bool CheckCurrentInputPressed(InputID input){
		return CheckCurrentInputPressed((UInt32)input);
	}
	public static bool CheckCurrentInputPressed(UInt32 input){
		return ((currentInputs & input) != 0 && (previousInputs & input) == 0);
	}

	public static bool CheckCurrentInputReleased(InputID input){
		return CheckCurrentInputReleased((UInt32)input);
	}
	public static bool CheckCurrentInputReleased(UInt32 input){
		return ((currentInputs & input) == 0 && (previousInputs & input) != 0);
	}

	public static float GetAxisHorizontal(){
		return Input.GetAxisRaw("Horizontal");
	}

	public static float GetAxisVertical(){
		return Input.GetAxisRaw("Vertical");
	}

	public static float GetAxis(InputAxis axis){
		return inputAxisArray[(int)axis];
	}

	//returns a value based on two inputs
	public static int GetCustomAxis(InputID inputA, InputID inputB){
		bool a = CheckCurrentInput(inputA);
		bool b = CheckCurrentInput(inputB);
		if(a && !b){
			return -1;
		}
		if(b && !a){
			return 1;
		}
		return 0;
	}


	/**********************************************************************************/

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
