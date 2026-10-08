using UnityEngine;
// Blender-authored model, using the documented historical portrait for anatomical structure.
public static class DesignFace {
 public static void Build(Transform head,Material skin){
  var asset=Resources.Load<GameObject>("Professor/Head");
  if(!asset)throw new System.InvalidOperationException("Professor head asset missing. Run Study / Import Professor Artwork.");
  Object.Instantiate(asset,head,false);
 }
}
