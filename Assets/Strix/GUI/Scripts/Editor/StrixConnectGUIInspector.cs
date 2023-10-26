using System;
using SoftGear.Strix.Unity.Runtime;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StrixConnectGUI))]
public class StrixConnectGUIInspector : Editor {
    private SerializedProperty hostProperty;
    private SerializedProperty portProperty;

    private void OnEnable() {
        hostProperty = serializedObject.FindProperty("host");
        portProperty = serializedObject.FindProperty("port");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        var error = default(string);
        var parseResult = default(UrlParseResult);
        var connectionArgs = default(ConnectionArgs?);

        try {
            parseResult = UrlParseResult.Parse(hostProperty.stringValue);
            connectionArgs = ConnectionArgs.FromUrlParseResult(parseResult, portProperty.intValue);
        } catch (Exception e) {
            error = e.Message;
        }

        var currentProperty = serializedObject.GetIterator();
        currentProperty.Next(enterChildren: true);

        while (currentProperty.NextVisible(enterChildren: false)) {
            if (error == null && connectionArgs != null && !string.IsNullOrEmpty(parseResult.Scheme) && currentProperty.name == "port") {
                GUI.enabled = false;
                EditorGUILayout.TextField("Port", $"{connectionArgs.Value.Port} (automatically determined from host)");
                GUI.enabled = true;
            } else if (currentProperty.name == "host") {
                if (error != null) {
                    var colorBackup = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    EditorGUILayout.PropertyField(currentProperty);
                    GUI.backgroundColor = colorBackup;
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                } else {
                    EditorGUILayout.PropertyField(currentProperty);

                    if (connectionArgs != null) {
                        var protocol = connectionArgs.Value.Protocol;
#if UNITY_WEBGL
                        if (protocol != "WEB_SOCKET" && protocol != "WEB_SOCKET_SECURE") {
                            EditorGUILayout.HelpBox("WebGL platform only supports WebSocket connections. Please add \"ws://\" or \"wss://\" at the beginning of your host string.", MessageType.Warning);
                        }
#else
                        if (protocol == "WEB_SOCKET" || protocol == "WEB_SOCKET_SECURE") {
                            EditorGUILayout.HelpBox("It is not recommended to use WebSockets on platforms other than WebGL for performance reasons. Since WebSocket frames has higher network traffic and CPU overhead, please consider using TCP or UDP instead, or switch your platform to WebGL in the build settings.", MessageType.Warning);
                        }
#endif
                    }
                }
            } else {
                EditorGUILayout.PropertyField(currentProperty);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}