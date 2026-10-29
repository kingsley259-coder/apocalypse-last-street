using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
public class MixamoImportRules : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if(!assetPath.Contains("/Mixamo/"))return;
        var m=(ModelImporter)assetImporter;
        m.animationType=ModelImporterAnimationType.Human;
        m.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        m.importAnimation=true;
        m.importCameras=false;
        m.importLights=false;
        m.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
    }
    void OnPreprocessAnimation()
    {
        if(!assetPath.Contains("/Mixamo/"))return;
        var m=(ModelImporter)assetImporter;
        var clips=m.defaultClipAnimations;
        foreach(var c in clips)
        {
            string n=Path.GetFileNameWithoutExtension(assetPath).ToLower();
            c.loopTime=!new[]
            {
                "death","dying","firing","reaction","reloading","toss","jump"
            }
            .Any(n.Contains);
            c.lockRootRotation=true;
            c.lockRootHeightY=true;
            c.lockRootPositionXZ=false;
            c.keepOriginalPositionY=true;
            c.keepOriginalPositionXZ=false;
            c.keepOriginalOrientation=true;
        }
        m.clipAnimations=clips;
    }
}
