using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Xnput : MonoBehaviour {
    static public bool DEBUG = false;
    static public bool DEBUG_EVERY_FRAME = false;
    static Xnput _S;

    public InfoProperty info = new InfoProperty( $"Xnput Instructions",
        "To check any of the buttons, us Xnput just like the old Unity Input class." +
        " The following are available:"                                              +
        "\n\tXnput.GetButton()"                                                      +
        "\n\tXnput.GetButtonDown()"                                                  +
        "\n\tXnput.GetButtonUp()"                                                    +
        "\n\tXnput.GetAxisRaw()",
        true, false );

    public ButtonState up, down, left, right, b, a, start, select;
    public string buttons;
    public Vector2 moveRaw;
    //public float h, v;
    public enum eAxis { horizontal, vertical };
    public float hRaw { get { return moveRaw.x; } }
    public float vRaw { get { return moveRaw.y; } }

    public enum eButton { up, down, left, right, b, a, start, select };
    public Dictionary<eButton, ButtonState> buttonDict;

    // Start is called before the first frame update
    void Awake() {
        if (_S != null) {
            Destroy( gameObject );
            return;
        }
        _S = this;
        buttonDict = new Dictionary<eButton, ButtonState>();
        buttonDict.Add( eButton.up, up );
        buttonDict.Add( eButton.down, down );
        buttonDict.Add( eButton.left, left );
        buttonDict.Add( eButton.right, right );
        buttonDict.Add( eButton.a, a );
        buttonDict.Add( eButton.b, b );
        buttonDict.Add( eButton.start, start );
        buttonDict.Add( eButton.select, select );
    }

    // LateUpdate is called once per frame after all Updates have completed
    void LateUpdate() {
        buttons = $"U:{up.Char} D:{down.Char} L:{left.Char} R:{right.Char} B:{b.Char} A:{a.Char} Se:{select.Char} St:{start.Char}";
        if ( DEBUG_EVERY_FRAME ) Debug.Log( buttons );
        // Progress all ButtonStates
        foreach ( ButtonState bs in buttonDict.Values ) {
            bs.Progress();
        }
        //up.Progress();
        //down.Progress();
        //left.Progress();
        //right.Progress();
        //a.Progress();
        //b.Progress();
        //start.Progress();
        //select.Progress();
        // TODO: Manage easing on move, h, and v values
    }

    static public bool GetButton(eButton eB) {
        return _S.buttonDict[eB];
    }
    static public bool GetButtonDown(eButton eB) {
        return _S.buttonDict[eB].down;
    }
    static public bool GetButtonUp( eButton eB ) {
        return _S.buttonDict[eB].up;
    }

    static public float GetAxisRaw( eAxis axis ) {
        if ( axis == eAxis.horizontal ) return _S.hRaw;
        if ( axis == eAxis.vertical ) return _S.vRaw;
        // Debug.LogError( $"Xnput does not have an axis named \"{axis}\"." );
        return 0;
    }


    #region PlayerInput Functions
    private void OnMove( InputValue value ) {
        moveRaw = value.Get<Vector2>();
        if ( DEBUG ) Debug.Log( $"moveRaw: {moveRaw}" );
    }

    private void OnUp( InputValue value ) {
        up.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"up: {up}" );
        //float f = value.Get<float>();
        //if ( f > 0.5f ) up = true;
    }

    private void OnDown( InputValue value ) {
        down.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"down: {down}" );
    }

    private void OnLeft( InputValue value ) {
        left.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"left: {left}" );
    }

    private void OnRight( InputValue value ) {
        right.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"right: {right}" );
    }

    private void OnA( InputValue value ) {
        a.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"a: {a}" );
    }

    private void OnB( InputValue value ) {
        b.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"b: {b}" );
    }

    private void OnStart( InputValue value ) {
        start.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"start: {start}" );
    }

    private void OnSelect( InputValue value ) {
        select.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"select: {select}" );
    }

    #endregion
}

//public struct RockyCharacterInputs {
//    public float MoveAxisForward;
//    public float MoveAxisRight;
//    public Quaternion CameraRotation;
//    public ButtonState Jump, Crouch, Dive, NoClip;
//    public int RespawnAt; // The default value for this is 0 for NO TELEPORT

//    public void ProgressButtonStates() {
//        Jump.Progress();
//        Crouch.Progress();
//        Dive.Progress();
//        NoClip.Progress();
//    }

//    public override string ToString() {
//        System.Text.StringBuilder sb = new System.Text.StringBuilder();
//        sb.Append( $"Move: [{MoveAxisForward:0.00}, {MoveAxisRight:0.00}]" );
//        sb.Append( $"  Jump: {Jump.ToString()}" );
//        sb.Append( $"  Crch: {Crouch.ToString()}" );
//        sb.Append( $"  NoCl: {NoClip.ToString()}" );
//        sb.Append( $"  Spwn: {RespawnAt}" );
//        // sb.Append( $"  Dive: {Dive.ToString()}" );
//        sb.Append( $"  Rot: {CameraRotation.eulerAngles}" );
//        return sb.ToString();
//    }
//}