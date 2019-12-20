#region using

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Input;

#endregion

namespace OpenTKFramework.Framework {
    internal class InputManager {
        private static InputManager      instance;
        private        ControllerState[] curJoyDown;
        private        bool[]            curKeysDown;
        private        bool[]            curMouseDown;

        private GameWindow          game;
#pragma warning disable 414
        // ReSharper disable once InconsistentNaming
        private bool isInitialized;
#pragma warning restore 414
        private float[]             joyDeadZone;
        private ControllerMapping[] joyMapping;
        private PointF              mouseDeltaPos = new PointF( 0, 0 );
        private Point               mousePosition = new Point( 0, 0 );
        private int                 numJoysticks;
        private ControllerState[]   prevJoyDown;
        private bool[]              prevKeysDown;
        private bool[]              prevMouseDown;

        private InputManager() { }

        public static InputManager Instance {
            get {
                if ( instance == null ) instance = new InputManager();
                return instance;
            }
        }

        public int MouseX => this.mousePosition.X;

        public int MouseY => this.mousePosition.Y;

        public Point MousePosition => this.mousePosition;

        public float MouseDeltaX => this.mouseDeltaPos.X;

        public float MouseDeltaY => this.mouseDeltaPos.Y;

        public PointF MouseDelta => this.mouseDeltaPos;

        [Obsolete]
        public Dictionary<int, string> Gamepads {
            get {
                var result = new Dictionary<int, string>();

                for ( var i = 0; i < this.numJoysticks; i++ ) {
                    var caps = Joystick.GetCapabilities( i );
                    if ( caps.IsConnected ) result.Add( i, this.game.Joysticks[i].DeviceType + ": " + this.game.Joysticks[i].Description );
                }

                return result;
            }
        }

        public int NumGamepads {
            get {
                var count = 0;

                for ( var i = 0; i < this.numJoysticks; i++ ) {
                    var caps = Joystick.GetCapabilities( i );

                    if ( caps.IsConnected )
                        count += 1;
                    /*Console.WriteLine("Joystick: " + i);
                        Console.WriteLine("\tDescription: " + game.Joysticks[i].Description);
                        Console.WriteLine("\tType: " + game.Joysticks[i].DeviceType);
                        Console.WriteLine("\tString: " + game.Joysticks[i].ToString());*/
                }

                return count;
            }
        }

        [Obsolete]
        public void Initialize(GameWindow window) {
            this.game = window;

            var numKeys         = (int) Key.LastKey;
            var numMouseButtons = (int) MouseButton.LastButton;

            this.prevKeysDown = new bool[numKeys];
            this.curKeysDown  = new bool[numKeys];
            for ( var i = 0; i < numKeys; ++i ) this.prevKeysDown[i] = this.curKeysDown[i] = false;

            this.prevMouseDown = new bool[numMouseButtons];
            this.curMouseDown  = new bool[numMouseButtons];
            for ( var i = 0; i < numMouseButtons; ++i ) this.prevMouseDown[i] = this.curMouseDown[i] = false;

            this.numJoysticks = this.game.Joysticks.Count;
            this.joyMapping   = new ControllerMapping[this.numJoysticks];
            this.prevJoyDown  = new ControllerState[this.numJoysticks];
            this.curJoyDown   = new ControllerState[this.numJoysticks];
            this.joyDeadZone  = new float[this.numJoysticks];
            for ( var i = 0; i < this.numJoysticks; ++i ) {
                this.joyMapping[i]  = new ControllerMapping();
                this.prevJoyDown[i] = new ControllerState();
                this.curJoyDown[i]  = new ControllerState();
                this.joyDeadZone[i] = 0.25f;
            }

            this.isInitialized = true;
        }

        public void Shutdown() {
            this.prevKeysDown  = null;
            this.curKeysDown   = null;
            this.prevMouseDown = null;
            this.curMouseDown  = null;
            this.joyMapping    = null;
            this.prevJoyDown   = null;
            this.curJoyDown    = null;
            this.joyDeadZone   = null;
            this.isInitialized = false;
        }

        [Obsolete]
        public void Update() {
            for ( var i = 0; i < this.curKeysDown.Length; ++i ) {
                this.prevKeysDown[i] = this.curKeysDown[i];
                this.curKeysDown[i]  = this.game.Keyboard[(Key) i];
            }

            for ( var i = 0; i < this.curMouseDown.Length; ++i ) {
                this.prevMouseDown[i] = this.curMouseDown[i];
                this.curMouseDown[i]  = this.game.Mouse[(MouseButton) i];
            }

            this.mousePosition.X = this.game.Mouse.X;
            this.mousePosition.Y = this.game.Mouse.Y;
            this.mouseDeltaPos.X = this.game.Mouse.XDelta / (float) this.game.ClientSize.Width;
            this.mouseDeltaPos.Y = this.game.Mouse.YDelta / (float) this.game.ClientSize.Height;

            for ( var i = 0; i < this.numJoysticks; ++i )
                if ( IsConnected( i ) ) {
                    var state = Joystick.GetState( i );

                    for ( var j = 0; j < this.curJoyDown[i].Buttons.Length; ++j ) {
                        this.prevJoyDown[i].Buttons[j] = this.curJoyDown[i].Buttons[j];
                        this.curJoyDown[i].Buttons[j]  = this.joyMapping[i].HasButtons[j] ? state.GetButton( this.joyMapping[i].Buttons[j] ) == ButtonState.Pressed : false;
                    }

                    this.prevJoyDown[i].LeftAxis.X  = this.curJoyDown[i].LeftAxis.X;
                    this.prevJoyDown[i].LeftAxis.Y  = this.curJoyDown[i].LeftAxis.Y;
                    this.prevJoyDown[i].RightAxis.X = this.curJoyDown[i].RightAxis.X;
                    this.prevJoyDown[i].RightAxis.Y = this.curJoyDown[i].RightAxis.Y;

                    this.curJoyDown[i].LeftAxis.X  = this.joyMapping[i].HasLeftAxisX ? state.GetAxis( this.joyMapping[i].LeftAxisX ) : 0.0f;
                    this.curJoyDown[i].LeftAxis.Y  = this.joyMapping[i].HasLeftAxisY ? state.GetAxis( this.joyMapping[i].LeftAxisY ) : 0.0f;
                    this.curJoyDown[i].RightAxis.X = this.joyMapping[i].HasRightAxisX ? state.GetAxis( this.joyMapping[i].RightAxisX ) : 0.0f;
                    this.curJoyDown[i].RightAxis.Y = this.joyMapping[i].HasRightAxisY ? state.GetAxis( this.joyMapping[i].RightAxisY ) : 0.0f;

                    if ( this.curJoyDown[i].LeftAxis.X > 0.0f && this.curJoyDown[i].LeftAxis.X < this.joyDeadZone[i] )
                        this.curJoyDown[i].LeftAxis.X                                                                                                      = 0.0f;
                    else if ( this.curJoyDown[i].LeftAxis.X < 0.0f && this.curJoyDown[i].LeftAxis.X > -this.joyDeadZone[i] ) this.curJoyDown[i].LeftAxis.X = 0.0f;

                    if ( this.curJoyDown[i].LeftAxis.Y > 0.0f && this.curJoyDown[i].LeftAxis.Y < this.joyDeadZone[i] )
                        this.curJoyDown[i].LeftAxis.Y                                                                                                      = 0.0f;
                    else if ( this.curJoyDown[i].LeftAxis.Y < 0.0f && this.curJoyDown[i].LeftAxis.Y > -this.joyDeadZone[i] ) this.curJoyDown[i].LeftAxis.Y = 0.0f;

                    if ( this.curJoyDown[i].RightAxis.X > 0.0f && this.curJoyDown[i].RightAxis.X < this.joyDeadZone[i] )
                        this.curJoyDown[i].RightAxis.X                                                                                                        = 0.0f;
                    else if ( this.curJoyDown[i].RightAxis.X < 0.0f && this.curJoyDown[i].RightAxis.X > -this.joyDeadZone[i] ) this.curJoyDown[i].RightAxis.X = 0.0f;

                    if ( this.curJoyDown[i].RightAxis.Y > 0.0f && this.curJoyDown[i].RightAxis.Y < this.joyDeadZone[i] )
                        this.curJoyDown[i].RightAxis.Y                                                                                                        = 0.0f;
                    else if ( this.curJoyDown[i].RightAxis.Y < 0.0f && this.curJoyDown[i].RightAxis.Y > -this.joyDeadZone[i] ) this.curJoyDown[i].RightAxis.Y = 0.0f;
                }
        }

        public bool KeyDown(Key key) => this.curKeysDown[(int) key];

        public bool KeyUp(Key key) => !this.curKeysDown[(int) key];

        public bool KeyPressed(Key key) => !this.prevKeysDown[(int) key] && this.curKeysDown[(int) key];

        public bool KeyReleased(Key key) => this.prevKeysDown[(int) key] && !this.curKeysDown[(int) key];

        public Key[] GetAllKeysDown() {
            var collection = new List<Key>();

            for ( var i = 0; i < this.curKeysDown.Length; ++i )
                if ( this.curKeysDown[i] )
                    collection.Add( (Key) i );

            return collection.ToArray();
        }

        public bool MouseDown(MouseButton button) => this.curMouseDown[(int) button];

        public bool MouseUp(MouseButton button) => !this.curMouseDown[(int) button];

        public bool MousePressed(MouseButton button) => !this.prevMouseDown[(int) button] && this.curMouseDown[(int) button];

        public bool MouseReleased(MouseButton button) => this.prevMouseDown[(int) button] && !this.curMouseDown[(int) button];

        public void SetMousePosition(Point newPos) { Mouse.SetPosition( newPos.X, newPos.Y ); }

        public void CenterMouse() {
            double x = this.game.X + this.game.Width  / 2;
            double y = this.game.Y + this.game.Height / 2;
            Mouse.SetPosition( x, y );
        }

        public bool IsConnected(int padNum) {
            var caps = Joystick.GetCapabilities( padNum );
            return caps.IsConnected;
        }

        public float GetDeadzone(int controllerNum) => this.joyDeadZone[controllerNum];

        public void SetDeadzone(int controller, float value) { this.joyDeadZone[controller] = value; }

        public ControllerMapping GetMapping(int joyNum) => this.joyMapping[joyNum];

        public bool GetButton(int joystick, ref JoystickButton button, params JoystickButton[] excludeButtons) {
            for ( var i = 0; i < this.numJoysticks; ++i )
                if ( IsConnected( i ) ) {
                    var state = Joystick.GetState( i );
                    foreach ( JoystickButton enumVal in Enum.GetValues( typeof(JoystickButton) ) )
                        if ( state.GetButton( enumVal ) == ButtonState.Pressed ) {
                            var cont = false;
                            foreach ( var exclude in excludeButtons )
                                if ( exclude == enumVal ) {
                                    cont = true;
                                    break;
                                }

                            if ( cont ) continue;

                            button = enumVal;
                            return true;
                        }
                }

            return false;
        }

        public bool GetAxis(int joystick, ref JoystickAxis axis, params JoystickAxis[] excludeAxis) {
            for ( var i = 0; i < this.numJoysticks; ++i )
                if ( IsConnected( i ) ) {
                    var state = Joystick.GetState( i );
                    foreach ( JoystickAxis enumVal in Enum.GetValues( typeof(JoystickAxis) ) ) {
                        var greaterThan0 = Math.Abs( state.GetAxis( enumVal ) )       > GetDeadzone( joystick );
                        var lessThan1    = 1.0 - Math.Abs( state.GetAxis( enumVal ) ) > GetDeadzone( joystick );

                        if ( greaterThan0 && lessThan1 ) {
                            var cont = false;
                            foreach ( var exclude in excludeAxis )
                                if ( exclude == enumVal ) {
                                    cont = true;
                                    break;
                                }

                            if ( cont ) continue;

                            axis = enumVal;
                            return true;
                        }
                    }
                }

            return false;
        }

        public bool HasAButton(int padNum) => this.joyMapping[padNum].HasA;

        public bool HasBButton(int padNum) => this.joyMapping[padNum].HasB;

        public bool HasXButton(int padNum) => this.joyMapping[padNum].HasX;

        public bool HasYButton(int padNum) => this.joyMapping[padNum].HasY;

        public bool HasSelectButton(int padNum) => this.joyMapping[padNum].HasSelect;

        public bool HasStartButton(int padNum) => this.joyMapping[padNum].HasStart;

        public bool HasHomeButton(int padNum) => this.joyMapping[padNum].HasHome;

        public bool HasDPad(int padNum) => this.joyMapping[padNum].HasUp && this.joyMapping[padNum].HasDown && this.joyMapping[padNum].HasLeft && this.joyMapping[padNum].HasRight;

        public bool HasL1(int padNum) => this.joyMapping[padNum].HasL1;

        public bool HasL2(int padNum) => this.joyMapping[padNum].HasL2;

        public bool HasR1(int padNum) => this.joyMapping[padNum].HasR1;

        public bool HasR2(int padNum) => this.joyMapping[padNum].HasR2;

        public bool HasLeftStick(int padNum) => this.joyMapping[padNum].HasLeftAxisX && this.joyMapping[padNum].HasLeftAxisY;

        public bool HasRightStick(int padNum) => this.joyMapping[padNum].HasRightAxisX && this.joyMapping[padNum].HasRightAxisY;

        public bool ADown(int padNum) => this.curJoyDown[padNum].A;

        public bool AUp(int padNum) => !this.curJoyDown[padNum].A;

        public bool APressed(int padNum) => !this.prevJoyDown[padNum].A && this.curJoyDown[padNum].A;

        public bool AReleased(int padNum) => this.prevJoyDown[padNum].A && !this.curJoyDown[padNum].A;

        public bool BDown(int padNum) => this.curJoyDown[padNum].B;

        public bool BUp(int padNum) => !this.curJoyDown[padNum].B;

        public bool BPressed(int padNum) => !this.prevJoyDown[padNum].B && this.curJoyDown[padNum].B;

        public bool BReleased(int padNum) => this.prevJoyDown[padNum].B && !this.curJoyDown[padNum].B;

        public bool XDown(int padNum) => this.curJoyDown[padNum].X;

        public bool XUp(int padNum) => !this.curJoyDown[padNum].X;

        public bool XPressed(int padNum) => !this.prevJoyDown[padNum].X && this.curJoyDown[padNum].X;

        public bool XReleased(int padNum) => this.prevJoyDown[padNum].X && !this.curJoyDown[padNum].X;

        public bool YDown(int padNum) => this.curJoyDown[padNum].Y;

        public bool YUp(int padNum) => !this.curJoyDown[padNum].Y;

        public bool YPressed(int padNum) => !this.prevJoyDown[padNum].Y && this.curJoyDown[padNum].Y;

        public bool YReleased(int padNum) => this.prevJoyDown[padNum].Y && !this.curJoyDown[padNum].Y;

        public bool SelectDown(int padNum) => this.curJoyDown[padNum].Select;

        public bool SelectUp(int padNum) => !this.curJoyDown[padNum].Select;

        public bool SelectPressed(int padNum) => !this.prevJoyDown[padNum].Select && this.curJoyDown[padNum].Select;

        public bool SelectReleased(int padNum) => this.prevJoyDown[padNum].Select && !this.curJoyDown[padNum].Select;

        public bool StartDown(int padNum) => this.curJoyDown[padNum].Start;

        public bool StartUp(int padNum) => !this.curJoyDown[padNum].Start;

        public bool StartPressed(int padNum) => !this.prevJoyDown[padNum].Start && this.curJoyDown[padNum].Start;

        public bool StartReleased(int padNum) => this.prevJoyDown[padNum].Start && !this.curJoyDown[padNum].Start;

        public bool L1Down(int padNum) => this.curJoyDown[padNum].L1;

        public bool L1Up(int padNum) => !this.curJoyDown[padNum].L1;

        public bool L1Pressed(int padNum) => !this.prevJoyDown[padNum].L1 && this.curJoyDown[padNum].L1;

        public bool L1Released(int padNum) => this.prevJoyDown[padNum].L1 && !this.curJoyDown[padNum].L1;

        public bool L2Down(int padNum) => this.curJoyDown[padNum].L2;

        public bool L2Up(int padNum) => !this.curJoyDown[padNum].L2;

        public bool L2Pressed(int padNum) => !this.prevJoyDown[padNum].L2 && this.curJoyDown[padNum].L2;

        public bool L2Released(int padNum) => this.prevJoyDown[padNum].L2 && !this.curJoyDown[padNum].L2;

        public bool R1Down(int padNum) => this.curJoyDown[padNum].R1;

        public bool R1Up(int padNum) => !this.curJoyDown[padNum].R1;

        public bool R1Pressed(int padNum) => !this.prevJoyDown[padNum].R1 && this.curJoyDown[padNum].R1;

        public bool R1Released(int padNum) => this.prevJoyDown[padNum].R1 && !this.curJoyDown[padNum].R1;

        public bool UpDown(int padNum) => this.curJoyDown[padNum].Up;

        public bool UpUp(int padNum) => !this.curJoyDown[padNum].Up;

        public bool UpPressed(int padNum) => !this.prevJoyDown[padNum].Up && this.curJoyDown[padNum].Up;

        public bool UpReleased(int padNum) => this.prevJoyDown[padNum].Up && !this.curJoyDown[padNum].Up;

        public bool DownDown(int padNum) => this.curJoyDown[padNum].Down;

        public bool DownUp(int padNum) => !this.curJoyDown[padNum].Down;

        public bool DownPressed(int padNum) => !this.prevJoyDown[padNum].Down && this.curJoyDown[padNum].Down;

        public bool DownReleased(int padNum) => this.prevJoyDown[padNum].Down && !this.curJoyDown[padNum].Down;

        public bool LeftDown(int padNum) => this.curJoyDown[padNum].Left;

        public bool LeftUp(int padNum) => !this.curJoyDown[padNum].Left;

        public bool LeftPressed(int padNum) => !this.prevJoyDown[padNum].Left && this.curJoyDown[padNum].Left;

        public bool LeftReleased(int padNum) => this.prevJoyDown[padNum].Left && !this.curJoyDown[padNum].Left;

        public bool RightDown(int padNum) => this.curJoyDown[padNum].Right;

        public bool RightUp(int padNum) => !this.curJoyDown[padNum].Right;

        public bool RightPressed(int padNum) => !this.prevJoyDown[padNum].Right && this.curJoyDown[padNum].Right;

        public bool RightReleased(int padNum) => this.prevJoyDown[padNum].Right && !this.curJoyDown[padNum].Right;

        public float LeftStickX(int padNum) => this.curJoyDown[padNum].LeftAxis.X;

        public float LeftStickY(int padNum) => this.curJoyDown[padNum].LeftAxis.Y;

        public PointF LeftStick(int padNum) => this.curJoyDown[padNum].LeftAxis;

        public float RightStickX(int padNum) => this.curJoyDown[padNum].RightAxis.X;

        public float RightStickY(int padNum) => this.curJoyDown[padNum].RightAxis.Y;

        public PointF RightStick(int padNum) => this.curJoyDown[padNum].RightAxis;

        public class ControllerMapping {
            public JoystickAxis[] Axis = { JoystickAxis.Axis0, JoystickAxis.Axis1, JoystickAxis.Axis2, JoystickAxis.Axis3 };

            public JoystickButton[] Buttons = { JoystickButton.Button1, JoystickButton.Button2, JoystickButton.Button3, JoystickButton.Button4, JoystickButton.Button5, JoystickButton.Button6, JoystickButton.Button7, JoystickButton.Button8, JoystickButton.Button9, JoystickButton.Button10, JoystickButton.Button11, JoystickButton.Button12, JoystickButton.Button13, JoystickButton.Button14 };

            public bool[] HasAxis    = new bool[4];
            public bool[] HasButtons = new bool[15];

            public JoystickButton A {
                get => this.Buttons[0];
                set {
                    this.Buttons[0]    = value;
                    this.HasButtons[0] = true;
                }
            }

            public JoystickButton B {
                get => this.Buttons[1];
                set {
                    this.Buttons[1]    = value;
                    this.HasButtons[1] = true;
                }
            }

            public JoystickButton X {
                get => this.Buttons[2];
                set {
                    this.Buttons[2]    = value;
                    this.HasButtons[2] = true;
                }
            }

            public JoystickButton Y {
                get => this.Buttons[3];
                set {
                    this.Buttons[3]    = value;
                    this.HasButtons[3] = true;
                }
            }

            public JoystickButton Start {
                get => this.Buttons[4];
                set {
                    this.Buttons[4]    = value;
                    this.HasButtons[4] = true;
                }
            }

            public JoystickButton Select {
                get => this.Buttons[5];
                set {
                    this.Buttons[5]    = value;
                    this.HasButtons[5] = true;
                }
            }

            public JoystickButton Up {
                get => this.Buttons[6];
                set {
                    this.Buttons[6]    = value;
                    this.HasButtons[6] = true;
                }
            }

            public JoystickButton Down {
                get => this.Buttons[7];
                set {
                    this.Buttons[7]    = value;
                    this.HasButtons[7] = true;
                }
            }

            public JoystickButton Left {
                get => this.Buttons[8];
                set {
                    this.Buttons[8]    = value;
                    this.HasButtons[8] = true;
                }
            }

            public JoystickButton Right {
                get => this.Buttons[9];
                set {
                    this.Buttons[9]    = value;
                    this.HasButtons[9] = true;
                }
            }

            public JoystickButton Home {
                get => this.Buttons[10];
                set {
                    this.Buttons[10]    = value;
                    this.HasButtons[10] = true;
                }
            }

            public JoystickButton L1 {
                get => this.Buttons[11];
                set {
                    this.Buttons[11]    = value;
                    this.HasButtons[11] = true;
                }
            }

            public JoystickButton L2 {
                get => this.Buttons[12];
                set {
                    this.Buttons[12]    = value;
                    this.HasButtons[12] = true;
                }
            }

            public JoystickButton R1 {
                get => this.Buttons[13];
                set {
                    this.Buttons[13]    = value;
                    this.HasButtons[13] = true;
                }
            }

            public JoystickButton R2 {
                get => this.Buttons[14];
                set {
                    this.Buttons[14]    = value;
                    this.HasButtons[14] = true;
                }
            }

            public JoystickAxis LeftAxisX {
                get => this.Axis[0];
                set {
                    this.Axis[0]    = value;
                    this.HasAxis[0] = true;
                }
            }

            public JoystickAxis LeftAxisY {
                get => this.Axis[1];
                set {
                    this.Axis[1]    = value;
                    this.HasAxis[1] = true;
                }
            }

            public JoystickAxis RightAxisX {
                get => this.Axis[2];
                set {
                    this.Axis[2]    = value;
                    this.HasAxis[2] = true;
                }
            }

            public JoystickAxis RightAxisY {
                get => this.Axis[3];
                set {
                    this.Axis[3]    = value;
                    this.HasAxis[3] = true;
                }
            }

            public bool HasA      { get => this.HasButtons[0];  set => this.HasButtons[0] = value; }
            public bool HasB      { get => this.HasButtons[1];  set => this.HasButtons[1] = value; }
            public bool HasX      { get => this.HasButtons[2];  set => this.HasButtons[2] = value; }
            public bool HasY      { get => this.HasButtons[3];  set => this.HasButtons[3] = value; }
            public bool HasStart  { get => this.HasButtons[4];  set => this.HasButtons[4] = value; }
            public bool HasSelect { get => this.HasButtons[5];  set => this.HasButtons[5] = value; }
            public bool HasUp     { get => this.HasButtons[6];  set => this.HasButtons[6] = value; }
            public bool HasDown   { get => this.HasButtons[7];  set => this.HasButtons[7] = value; }
            public bool HasLeft   { get => this.HasButtons[8];  set => this.HasButtons[8] = value; }
            public bool HasRight  { get => this.HasButtons[9];  set => this.HasButtons[9] = value; }
            public bool HasHome   { get => this.HasButtons[10]; set => this.HasButtons[10] = value; }
            public bool HasL1     { get => this.HasButtons[11]; set => this.HasButtons[11] = value; }
            public bool HasL2     { get => this.HasButtons[12]; set => this.HasButtons[12] = value; }
            public bool HasR1     { get => this.HasButtons[13]; set => this.HasButtons[13] = value; }
            public bool HasR2     { get => this.HasButtons[14]; set => this.HasButtons[14] = value; }

            public bool HasLeftAxisX  { get => this.HasAxis[0]; set => this.HasAxis[0] = value; }
            public bool HasLeftAxisY  { get => this.HasAxis[1]; set => this.HasAxis[1] = value; }
            public bool HasRightAxisX { get => this.HasAxis[2]; set => this.HasAxis[2] = value; }
            public bool HasRightAxisY { get => this.HasAxis[3]; set => this.HasAxis[3] = value; }
        }

        public class ControllerState {
            public bool[] Buttons = new bool[15];

            public PointF LeftAxis  = new PointF( 0, 0 );
            public PointF RightAxis = new PointF( 0, 0 );

            public bool A      { get => this.Buttons[0];  set => this.Buttons[0] = value; }
            public bool B      { get => this.Buttons[1];  set => this.Buttons[1] = value; }
            public bool X      { get => this.Buttons[2];  set => this.Buttons[2] = value; }
            public bool Y      { get => this.Buttons[3];  set => this.Buttons[3] = value; }
            public bool Start  { get => this.Buttons[4];  set => this.Buttons[4] = value; }
            public bool Select { get => this.Buttons[5];  set => this.Buttons[5] = value; }
            public bool Up     { get => this.Buttons[6];  set => this.Buttons[6] = value; }
            public bool Down   { get => this.Buttons[7];  set => this.Buttons[7] = value; }
            public bool Left   { get => this.Buttons[8];  set => this.Buttons[8] = value; }
            public bool Right  { get => this.Buttons[9];  set => this.Buttons[9] = value; }
            public bool Home   { get => this.Buttons[10]; set => this.Buttons[10] = value; }
            public bool L1     { get => this.Buttons[11]; set => this.Buttons[11] = value; }
            public bool L2     { get => this.Buttons[12]; set => this.Buttons[12] = value; }
            public bool R1     { get => this.Buttons[13]; set => this.Buttons[13] = value; }
            public bool R2     { get => this.Buttons[14]; set => this.Buttons[14] = value; }
        }
    }
}