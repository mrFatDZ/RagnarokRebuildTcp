﻿using System.IO;
using Assets.Scripts;
using Assets.Scripts.Editor;
using Assets.Scripts.MapEditor.Editor;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class RagnarokCopyFromRealClient : EditorWindow
    {
        [MenuItem("Ragnarok/Copy data from client data folder", priority = 1)]
        public static void CopyClientData()
        {
            var dataDir = RagnarokDirectory.GetRagnarokDataDirectorySafe;

            if (dataDir == null)
            {
                var prompt = @"Before you continue, you will need to specify a directory containing the contents of an extracted data.grf. "
                             + "For this import process to work correctly, the files will need to have been extracted with the right locale and working korean file names.";

                if (!EditorUtility.DisplayDialog("Copy from RO Client", prompt, "Continue", "Cancel"))
                    return;

                RagnarokDirectory.SetDataDirectory();

                dataDir = RagnarokDirectory.GetRagnarokDataDirectorySafe;
                if (dataDir == null)
                    return;
            }

            bool TestPath(string fileName)
            {
                if (!File.Exists(Path.Combine(dataDir, fileName)))
                {
                    Debug.LogError($"Could not verify client data directory \"{dataDir}\" is valid. File checked: {fileName} ");
                    return false;
                }

                return true;
            }

            if (!TestPath("prontera.gat") || !TestPath(@"texture\워터\water000.jpg"))
                return;

            var prompt2 = @"This import process will copy files from your data folder into this project. " 
                          + "Because this includes converting all maps and objects, expect this process to take more than an hour."
                          + "\n\nWhen complete, the lighting window will load where you can bake the lighting for all the scenes (accessible via 'Ragnarok->Lighting Manager'). "
                          + "You will also need to manually copy over your BGM into the music folder if you want music."
                          + "\n\nLastly, before you run you will need to use 'Ragnarok->Update Addressables' to make sure everything can load.";

            if (!EditorUtility.DisplayDialog("Copy from RO Client", prompt2, "Continue", "Cancel"))
                return;

            CopyFolder(Path.Combine(dataDir, "wav/"), "Assets/Sounds/", true);
            CopyFolder(Path.Combine(dataDir, "sprite/몬스터"), "Assets/Sprites/Monsters/");
            CopyFolder(Path.Combine(dataDir, "sprite/npc"), "Assets/Sprites/Npcs/");
            CopyFolder(Path.Combine(dataDir, "sprite/이팩트"), "Assets/Sprites/Effects/");
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/머리통/남"), "Assets/Sprites/Characters/HeadMale/");
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/머리통/여"), "Assets/Sprites/Characters/HeadFemale/");
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/몸통/남"), "Assets/Sprites/Characters/BodyMale/");
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/몸통/여"), "Assets/Sprites/Characters/BodyFemale/");
            
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/몸통/남"), "Assets/Sprites/Characters/BodyMale/");
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/몸통/여"), "Assets/Sprites/Characters/BodyFemale/");
            CopyFolder(Path.Combine(dataDir, "texture/유저인터페이스/illust"), "Assets/Sprites/Cutins/");
            
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/성직자"), "Assets/Sprites/Weapons/Acolyte/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/궁수"), "Assets/Sprites/Weapons/Archer/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/마법사"), "Assets/Sprites/Weapons/Mage/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/상인"), "Assets/Sprites/Weapons/Merchant/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/초보자"), "Assets/Sprites/Weapons/Novice/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/검사"), "Assets/Sprites/Weapons/Swordsman/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/도둑"), "Assets/Sprites/Weapons/Thief/", false, true);
            CopyFolder(Path.Combine(dataDir, "sprite/인간족/슈퍼노비스"), "Assets/Sprites/Weapons/SuperNovice/", false, true);

            CopySingleFile(Path.Combine(dataDir, "sprite/cursors.act"), "Assets/Sprites/");
            CopySingleFile(Path.Combine(dataDir, "sprite/cursors.spr"), "Assets/Sprites/");
            CopySingleFile(Path.Combine(dataDir, "sprite/이팩트/emotion.act"), "Assets/Sprites/");
            CopySingleFile(Path.Combine(dataDir, "sprite/이팩트/emotion.spr"), "Assets/Sprites/");
            CopySingleFile(Path.Combine(dataDir, "sprite/이팩트/숫자.act"), "Assets/Sprites/damagenumbers.act");
            CopySingleFile(Path.Combine(dataDir, "sprite/이팩트/숫자.spr"), "Assets/Sprites/damagenumbers.spr");
            
            AssetDatabase.Refresh();
            
            EffectStrImporter.Import(); //effects
            EffectStrImporter.ImportEffectTextures();
            RagnarokMapImporterWindow.ImportWater();
            RagnarokMapImporterWindow.ImportAllFiles();
            ItemIconImporter.ImportItems();
            
            RoLightingManagerWindow.CreateOrOpen();
        }

        private static void CopySingleFile(string src, string dest)
        {
            var fName = Path.GetFileName(dest);
            if (string.IsNullOrWhiteSpace(fName))
                dest = Path.Combine(dest, Path.GetFileName(src));

            var dir = Path.GetDirectoryName(dest);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Debug.Log($"${src}  :::  {dest}");

            if(!File.Exists(dest))
                File.Copy(src, dest, false);
        }

        private static bool CopyFolder(string src, string dest, bool recursive = false, bool maleFemaleSplit = false)
        {
            var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var hasFiles = false;

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            foreach (var path in Directory.GetFiles(src, "*", opt))
            {
                var rel = Path.GetRelativePath(src, path);
                var destPath = Path.Combine(dest, rel);

                hasFiles = true;

                if (maleFemaleSplit)
                {
                    if (rel.Contains("_남_"))
                        destPath = Path.Combine(dest, "Male/", rel);
                    if (rel.Contains("_여_"))
                        destPath = Path.Combine(dest, "Female/", rel);
                }

                if (File.Exists(destPath.Replace(".bmp", ".png")))
                    continue;

                var outDir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                var ext = Path.GetExtension(path);
                var fName = Path.GetFileName(path);

                if (ext == ".bmp")
                {
                    var tex = TextureImportHelper.LoadTexture(path);
                    TextureImportHelper.SaveAndUpdateTexture(tex, destPath.Replace(".bmp", ".png"), ti =>
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        ti.spriteImportMode = SpriteImportMode.Single;
                    });
                    //TextureImportHelper.GetOrImportTextureToProject(fName, path, destPath);
                }
                else
                    File.Copy(path, destPath, false);
            }

            AssetDatabase.Refresh();

            return hasFiles;
        }
    }
}