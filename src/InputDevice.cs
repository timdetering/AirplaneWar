using System;
// using System.Threading;
using System.Collections;
using DInput=Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX;

namespace AirplaneWar
{
	enum GameActions 
	{
		Rudder,              // Separate inputs are needed in this case for
		RudderLeft,          //   Left/Right because the joystick uses an
		RudderRight,         //   axis to report both left and right, but the keyboard will use separate arrow keys.
		Pitch,             
		PitchUp, 
		PitchDown, 
		Throttle,    // Engine
		Bank,
		Shoot,
		Bomb,
		Stop,
		Brake,
		T10,
		T22,
		T33,
		T44,
		T55,
		T66,
		T77,
		T88,
		T100,
		Quit,
		FirstPersonView,
		OutsideView,
		NumOfActions       // Not an action; conveniently tracks number of
	};                     //   actions.  Keep this as the last item.

	public class DeviceState
	{
		public DInput.Device Device; //Reference to the device

		public string Name;           //Friendly name of the device      
		public bool   IsAxisAbsolute; //Relative x-axis data flag
		public int[]  InputState;     //Array of the current input values
		public int[]  PaintState;     //Array of the current paint values
		public bool[] IsMapped;       //Flags whether action was successfully mapped  
                                          
		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceState()
		{
			InputState = new int[(int)GameActions.NumOfActions];
			PaintState = new int[(int)GameActions.NumOfActions];
			IsMapped   = new bool[(int)GameActions.NumOfActions];
		}
	};
	/// <summary>
	/// Summary description for InputDevice.
	/// </summary>
	public class InputDevice
	{
 		// Constants
		private readonly Guid       AppGuid   = new Guid("B32DD425-DB33-4f9c-972F-C68269C409F6");  

		private ActionFormat     DIActionFormat = new ActionFormat();
		public ArrayList         DeviceStates   = new ArrayList();    

		public InputDevice() 
		{
            #region Step 3: Enumerate Devices.
			// ************************************************************************
			// Step 3: Enumerate Devices.
			// 
			// Enumerate through devices according to the desired action map.
			// Devices are enumerated in a prioritized order, such that devices which
			// can best be mapped to the provided action map are returned first.
			// ************************************************************************
            #endregion

			// Setup the action format for actual gameplay
			DIActionFormat.ActionMapGuid = AppGuid;
			DIActionFormat.Genre = (int)DInput.FlyingMilitary.Military;
			DIActionFormat.AxisMin = -100;
			DIActionFormat.AxisMax = 100;
			DIActionFormat.BufferSize = 16;
			CreateActionMap(DIActionFormat.Actions);
            
			try
			{
				// Enumerate devices according to the action format
				DInput.DeviceList DevList = DInput.Manager.GetDevices(DIActionFormat, EnumDevicesBySemanticsFlags.AttachedOnly);
				foreach (SemanticsInstance instance in DevList)
				{
					SetupDevice(instance.Device);
				}

			}
			catch
			{
			//	UserInterface.ShowException(ex, "EnumDevicesBySemantics");
			}

			// Start the input loop
	//		InputThread = new Thread(new ThreadStart(RunInputLoop));
	//		InputThread.Start();
		}

		public void CheckInput()
		{
			// For each device gathered during enumeration, gather input. Although when
			// using action maps the input is received according to actions, each device 
			// must still be polled individually. Although for most actions your
			// program will follow the same logic regardless of which device generated
			// the action, there are special cases which require checking from which
			// device an action originated.

			foreach (DeviceState state in DeviceStates)
			{
				BufferedDataCollection dataCollection; 
				int curState = 0;

				try 
				{
					state.Device.Acquire();
					state.Device.Poll();
					dataCollection = state.Device.GetBufferedData(); 
				}
				catch
				{
					// GetDeviceData can fail for several reasons, some of which are
					// expected during a program's execution. A device's acquisition is not
					// permanent, and your program might need to reacquire a device several
					// times. Since this sample is polling frequently, an attempt to
					// acquire a lost device will occur during the next call to CheckInput.
					continue;
				}

				if (null == dataCollection)
					continue;
				// For each buffered data item, extract the game action and perform
				// necessary game state changes. A more complex program would certainly
				// handle each action separately, but this sample simply stores raw
				// axis data for a WALK action, and button up or button down states for
				// all other game actions. 

				// Relative axis data is never reported to be zero since relative data
				// is given in relation to the last position, and only when movement 
				// occurs. Manually set relative data to zero before checking input.
				if (!state.IsAxisAbsolute)
					state.InputState[(int)GameActions.Rudder] = 0;

				foreach (BufferedData data in dataCollection)
				{
					// The value stored in Action.ApplicationData equals the value stored in 
					// the ApplicationData property of the BufferedData class. For this sample
					// we selected these action constants to be indices into an array,
					// but we could have chosen these values to represent anything
					// from class objects to delegates.

					curState = 0;

					switch ((int)data.ApplicationData)
					{
						case (int)GameActions.Rudder:
						case (int)GameActions.Bank:
						case (int)GameActions.Pitch:
						case (int)GameActions.Throttle:
						{
							// Axis data. Absolute axis data is already scaled to the
							// boundaries selected in the diACTIONFORMAT structure, but
							// relative data is reported as relative motion change 
							// since the last report. This sample scales relative data
							// and clips it to axis data boundaries.
							curState = data.Data;

							if (!state.IsAxisAbsolute)
							{
								// scale relative data
								curState *= 5;

								// clip to boundaries
								if (curState < 0)
									curState = Math.Max(curState, DIActionFormat.AxisMin);
								else
									curState = Math.Min(curState, DIActionFormat.AxisMax);
							}

							break;
						}

						default:
						{
							// 0x80 is the DirectInput mask to determine whether
							// a button is pressed
							curState = ((data.Data & 0x80) != 0) ? 1 : 0;
							break;
						}
					}

					state.InputState[ (int)data.ApplicationData ] = curState;
				}
			}
		}

		private void CreateActionMap(ActionCollection map)
		{
            #region Step 2: Define the action map.
			// ****************************************************************************
			// Step 2: Define the action map. 
			//         
			// The action map instructs DirectInput on how to map game actions to device
			// objects. By selecting a predefined game genre that closely matches our game,
			// you can largely avoid dealing directly with device details. For this sample
			// we've selected the FlyingMilitary, and this constant will need
			// to be selected into the DIACTIONFORMAT structure later to inform DirectInput
			// of our choice. Every device has a mapping from genre actions to device
			// objects, so mapping your game actions to genre actions almost guarantees
			// an appropriate device configuration for your game actions.
			//
			// If DirectInput has already been given an action map for this GUID, it
			// will have created a user map for this application 
			// (C:\Program Files\Common Files\DirectX\DirectInput\User Maps\*.ini). If a
			// map exists, DirectInput will use the action map defined in the stored user 
			// map instead of the map defined in your program. This allows the user to
			// customize controls without losing changes when the game restarts. If you 
			// wish to make changes to the default action map without changing the 
			// GUID, you will need to delete the stored user map from your hard drive
			// for the system to detect your changes and recreate a stored user map.
			// ****************************************************************************
            #endregion
            
			// Device input (joystick, etc.) that is pre-defined by dinput according
			// to genre type.
			map.Add(CreateAction(GameActions.Rudder,         FlyingMilitary.AxisRudder,      0, "Rudder"));
			map.Add(CreateAction(GameActions.Pitch,          FlyingMilitary.AxisPitch,       0, "Pitch"));
			map.Add(CreateAction(GameActions.Bank,           FlyingMilitary.AxisBank,        0, "Bank"));
			map.Add(CreateAction(GameActions.Throttle,       FlyingMilitary.AxisThrottle,    0, "Throttle"));
			map.Add(CreateAction(GameActions.Shoot,          FlyingMilitary.ButtonFire,      0, "Shoot"));
			map.Add(CreateAction(GameActions.Bomb,           FlyingMilitary.ButtonFireSecondary,    0, "Bomb"));
            
			// Keyboard input mappings
			map.Add(CreateAction(GameActions.RudderLeft,      Keyboard.Left,                  0, "Rudder left"));
			map.Add(CreateAction(GameActions.RudderRight,     Keyboard.Right,                 0, "Rudder right"));
			map.Add(CreateAction(GameActions.PitchUp,         Keyboard.Up,                    0, "Pitch up"));
			map.Add(CreateAction(GameActions.PitchDown,       Keyboard.Down,                  0, "Pitch down"));
			map.Add(CreateAction(GameActions.Shoot,           Keyboard.Space,                 0, "Shoot"));
            map.Add(CreateAction(GameActions.Bomb,            Keyboard.B,                     0, "Bomb"));
			map.Add(CreateAction(GameActions.Brake,           Keyboard.G,                     0, "Brake"));
			map.Add(CreateAction(GameActions.FirstPersonView, Keyboard.F1,                    0, "View1"));
			map.Add(CreateAction(GameActions.OutsideView,     Keyboard.F2,                    0, "View2"));


			// The AppFixed constant can be used to instruct directInput that the
			// current mapping can not be changed by the user.
			map.Add(CreateAction(GameActions.Quit,          Keyboard.Q,  ActionAttributeFlags.AppFixed, "Quit"));
			map.Add(CreateAction(GameActions.Stop,          Keyboard.D0, ActionAttributeFlags.AppFixed, "Stop"));
			map.Add(CreateAction(GameActions.T10,           Keyboard.D1, ActionAttributeFlags.AppFixed, "T1"));
			map.Add(CreateAction(GameActions.T22,           Keyboard.D2, ActionAttributeFlags.AppFixed, "T2"));
			map.Add(CreateAction(GameActions.T33,           Keyboard.D3, ActionAttributeFlags.AppFixed, "T3"));
			map.Add(CreateAction(GameActions.T44,           Keyboard.D4, ActionAttributeFlags.AppFixed, "T4"));
			map.Add(CreateAction(GameActions.T55,           Keyboard.D5, ActionAttributeFlags.AppFixed, "T5"));
			map.Add(CreateAction(GameActions.T66,           Keyboard.D6, ActionAttributeFlags.AppFixed, "T6"));
			map.Add(CreateAction(GameActions.T77,           Keyboard.D7, ActionAttributeFlags.AppFixed, "T7"));
			map.Add(CreateAction(GameActions.T88,           Keyboard.D8, ActionAttributeFlags.AppFixed, "T8"));
			map.Add(CreateAction(GameActions.T100,          Keyboard.D9, ActionAttributeFlags.AppFixed, "T9"));
          
			// Mouse input mappings
			map.Add(CreateAction(GameActions.Bank,           Mouse.XAxis,                       0, "Bank"));
			map.Add(CreateAction(GameActions.Pitch,          Mouse.YAxis,                       0, "Pitch"));
			map.Add(CreateAction(GameActions.Shoot,          Mouse.Button0,                     0, "Pitch up"));
			map.Add(CreateAction(GameActions.Bomb,           Mouse.Button1,                     0, "Pitch down"));
		}

		/// <summary>
		/// Helper method to fill the action map during initialization
		/// </summary>
		private Action CreateAction(GameActions AppData, object Semantic, object Flags, string ActionName)
		{
			Action action = new Action();

			action.ApplicationData = (int)AppData;
			action.Semantic   = (int)Semantic;
			action.Flags      = (ActionAttributeFlags)Flags;
			action.ActionName = ActionName;

			return action;
		}

		/// <summary>
		/// Handles device setup
		/// </summary>
		/// <param name="device">DirectInput Device</param>
		private void SetupDevice(Device device)
		{
			// Create a temporary DeviceState object and store device information.
			DeviceState state = new DeviceState();
			device = device;   

            #region Step 4: Build the action map against the device
			// ********************************************************************
			// Step 4: Build the action map against the device, inspect the
			//         results, and set the action map.
			//
			// It's a good idea to inspect the results after building the action
			// map against the current device. The contents of the action map
			// structure indicate how and to what object the action was mapped. 
			// This sample simply verifies the action was mapped to an object on
			// the current device, and stores the result. Note that not all actions
			// will necessarily be mapped to an object on all devices. For instance,
			// this sample did not request that QUIT be mapped to any device other
			// than the keyboard.
			// ********************************************************************
            #endregion
			try 
			{
				// Build the action map against the device
				device.BuildActionMap(DIActionFormat, ActionMapControl.Default);
			}
			catch
			{
				return;
			}

			// Inspect the results
			foreach (Action action in DIActionFormat.Actions)
			{
				if ((int)action.How != (int)ActionMechanism.Error &&
					(int)action.How != (int)ActionMechanism.Unmapped)
				{
					state.IsMapped[(int)action.ApplicationData] = true;
				}
			}

			// Set the action map
			try
			{
				device.SetActionMap(DIActionFormat, ApplyActionMap.ForceSave);
			}
			catch
			{
				return;
			}
            
			state.Device = device;

			// Store the device's friendly name for display on the chart
			state.Name = device.DeviceInformation.InstanceName;

			// Store axis absolute/relative flag
			state.IsAxisAbsolute = device.Properties.AxisModeAbsolute;
 
			DeviceStates.Add(state);
			return;
		}
	}
}
