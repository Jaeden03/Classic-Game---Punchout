#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

/// <summary>
/// <para>Allows a single enum to store all phases of a button press.</para>
/// <para>- bools for just being pressed or released this frame are .down and .up.</para>
/// <para>- bools for status are isHeld and isFree.</para>
/// <para>- Button .isHeld the same frame as .down and .isFree the same frame as .up.</para>
/// <para>- Evaluating as bool returns value of .isHeld</para>
/// </summary>
[System.Serializable]
public class ButtonState {
    internal const int showDownUpEditorFrames = 1;
    public enum eInputButtonState { free, held, up, down };
    public eInputButtonState state;
    internal int showDown = 0, showUp = 0;


    /// <summary>
    /// Is this button currently NOT held (i.e., free)?
    /// </summary>
    public bool isFree {
        get { return ( state == eInputButtonState.free || state == eInputButtonState.up ); }
    }
    /// <summary>
    /// Is this button currently held.
    /// </summary>
    public bool isHeld {
        get { return ( state == eInputButtonState.held || state == eInputButtonState.down ); }
    }
    /// <summary>
    /// Was this button released this frame (more exactly since the last Progress() was called)?
    /// </summary>
    public bool up {
        get { return ( state == eInputButtonState.up ); }
    }
    /// <summary>
    /// Was this button pressed this frame (more exactly since the last Progress() was called)?
    /// </summary>
    public bool down {
        get { return ( state == eInputButtonState.down ); }
    }

    public override string ToString() {
        return stat;
    }

    public string stat {
        get {
            if ( state == eInputButtonState.free ) return "____";
            if ( state == eInputButtonState.held ) return "HELD";
            if ( state == eInputButtonState.down ) return "DOWN";
            if ( state == eInputButtonState.up ) return "_UP_";
            return "_REL";
        }
    }

    public char Char {
        get {
            if ( state == eInputButtonState.down || ( showDown > 0 ) ) return 'v';
            if ( state == eInputButtonState.up || ( showUp > 0 ) ) return '^';
            if ( state == eInputButtonState.free ) return '¯';
            if ( state == eInputButtonState.held ) return '_';
            return ' ';
        }
    }
    /// <summary>
    /// Converts eIBS.pressed to eIBS.down
    /// Converts eIBS.released to eIBS.up
    /// </summary>
    public void Progress() {
        if ( state == eInputButtonState.down ) {
            state = eInputButtonState.held;
        }
        if ( state == eInputButtonState.up ) {
            state = eInputButtonState.free;
        }
    }

    /// <summary>
    /// This assigns the value of the ButtonState based on a bool.
    /// If the state is up or released, and buttonVal==true, state will become pressed
    /// If the state is down or pressed, and buttonVal==false, state will become released
    /// This does NOT automatically Progress() the ButtonState!!!
    /// </summary>
    /// <param name="buttonVal"></param>
    public void Set( bool buttonVal ) {
        if ( buttonVal && isFree ) {
            state = eInputButtonState.down;
            showDown = showDownUpEditorFrames;
        }
        if ( !buttonVal && isHeld ) {
            state = eInputButtonState.up;
            showUp = showDownUpEditorFrames;
        }
    }

    public static implicit operator bool (ButtonState bs) {
        return bs.isHeld;
    }

    public static implicit operator ButtonState(bool b) {
        ButtonState bs = new ButtonState();
        bs.Set(b);
        return bs;
    }

    public static implicit operator string( ButtonState bs ) {
        return bs.stat;
    }
}



#if UNITY_EDITOR
[CustomPropertyDrawer( typeof( ButtonState ) )]
public class ButtonState_Drawer : PropertyDrawer {
    SerializedProperty m_stat;

    // Draw the property inside the given rect
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        // Init the SerializedProperty fields
        //if ( m_show == null ) m_show = property.FindPropertyRelative( "show" );
        //if ( m_recNum == null ) m_recNum = property.FindPropertyRelative( "recNum" );
        //if ( m_playerName == null ) m_playerName = property.FindPropertyRelative( "playerName" );
        //if ( m_dateTime == null ) m_dateTime = property.FindPropertyRelative( "dateTime" );

        ButtonState bs = fieldInfo.GetValue( property.serializedObject.targetObject ) as ButtonState;
        

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty( position, label, property );

        // Draw label
        //position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), GUIContent.none );// label );
        

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        EditorGUI.LabelField(position, property.displayName );
        EditorGUI.indentLevel = 4;
        EditorGUI.LabelField( position, $"[{bs}]" );
        EditorGUI.indentLevel = 8;
        if ( bs.showDown-- > 0 ) {
            EditorGUI.LabelField( position, $"Down" );
            //EditorUtility.SetDirty( property.serializedObject.targetObject ); // Repaint
        } else if (bs.showUp-- > 0 ) {
            EditorGUI.LabelField( position, $"Up" );
            //EditorUtility.SetDirty( property.serializedObject.targetObject ); // Repaint
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
        if ( Application.isPlaying ) {
            EditorUtility.SetDirty( property.serializedObject.targetObject ); // Repaint
        }
    }
}
#endif