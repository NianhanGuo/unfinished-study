using UnityEditor;
using System.IO;
[InitializeOnLoad] public static class StudyAutoSetup {
 static StudyAutoSetup(){EditorApplication.delayCall+=Run;}
 static void Run(){if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=Run;return;}if(!File.Exists("Assets/Study/Studio.unity"))StudioBuilder.Create();}
}
