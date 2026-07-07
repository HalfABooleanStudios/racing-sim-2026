// CreateHDRPMaterialsFromBakedTextures.cs
//
// Builds one HDRP/Lit Material per part from the PBR textures baked out of
// Blender (BaseColor / Metallic / Roughness / Normal PNGs, one folder per
// material, matching the naming produced by bake_for_unity_resume.py).
//
// HDRP's Lit shader doesn't take separate Metallic + Roughness maps - it
// wants a single packed "Mask Map":
//   R = Metallic, G = Ambient Occlusion, B = Detail Mask, A = Smoothness
// Since we don't have a baked AO map, G is filled with white (no occlusion).
// Smoothness = 1 - Roughness, packed into the alpha channel.
//
// HOW TO USE
// 1. Put this file anywhere under an "Editor" folder in your project,
//    e.g. Assets/Editor/CreateHDRPMaterialsFromBakedTextures.cs
// 2. Check the CONFIGURATION constants below match your project layout.
// 3. In Unity: Tools > RB7 Import > Create HDRP Materials From Baked Textures
// 4. (Optional) Select the imported model's root object in the Hierarchy
//    first - if AUTO_ASSIGN_TO_SELECTION is true, the script will walk its
//    Renderers and swap in the new materials wherever a slot's existing
//    material name matches a part name.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateHDRPMaterialsFromBakedTextures
{
    // ======================= CONFIGURATION =======================
    private const string TEXTURES_ROOT = "Assets/Frontend/Models (3D)/Textures";
    private const string MATERIALS_ROOT = "Assets/Frontend/Models (3D)/Materials";
    private const string GENERATED_MASKS_ROOT = "Assets/Frontend/Models (3D)/Textures/_Generated";

    // If false, materials that already exist at MATERIALS_ROOT are left
    // alone (safe to re-run after adding more baked parts).
    private const bool FORCE_RECREATE = false;

    // If true, after creating materials the script will look at whatever
    // GameObject(s) you have selected in the Hierarchy and swap matching
    // material slots over to the newly created materials.
    private const bool AUTO_ASSIGN_TO_SELECTION = true;
    // ===============================================================

    [MenuItem("Tools/RB7 Import/Create HDRP Materials From Baked Textures")]
    public static void CreateMaterials()
    {
        if (!AssetDatabase.IsValidFolder(TEXTURES_ROOT))
        {
            Debug.LogError($"Textures folder not found: {TEXTURES_ROOT}. " +
                            "Update TEXTURES_ROOT at the top of the script.");
            return;
        }

        EnsureFolder(MATERIALS_ROOT);
        EnsureFolder(GENERATED_MASKS_ROOT);

        string texturesSystemPath = ToSystemPath(TEXTURES_ROOT);
        string[] partFolders = Directory.GetDirectories(texturesSystemPath);

        int created = 0, skipped = 0, failed = 0;

        foreach (string folder in partFolders)
        {
            string partName = Path.GetFileName(folder);
            if (partName == "_Generated") continue;

            try
            {
                bool didCreate = ProcessPart(partName);
                if (didCreate) created++;
                else skipped++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create material for '{partName}': {e}");
                failed++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Materials done. Created: {created}, skipped: {skipped}, failed: {failed}. " +
                  $"({partFolders.Length} part folders found total)");

        if (AUTO_ASSIGN_TO_SELECTION)
            AssignMaterialsToSelection();
    }

    [MenuItem("Tools/RB7 Import/Assign Materials To Selected Hierarchy")]
    public static void AssignMaterialsToSelectionMenu() => AssignMaterialsToSelection();

    private static bool ProcessPart(string partName)
    {
        string matPath = $"{MATERIALS_ROOT}/{partName}.mat";
        if (!FORCE_RECREATE && AssetDatabase.LoadAssetAtPath<Material>(matPath) != null)
        {
            return false; // already exists
        }

        string baseColorPath = $"{TEXTURES_ROOT}/{partName}/{partName}_BaseColor.png";
        string metallicPath = $"{TEXTURES_ROOT}/{partName}/{partName}_Metallic.png";
        string roughnessPath = $"{TEXTURES_ROOT}/{partName}/{partName}_Roughness.png";
        string normalPath = $"{TEXTURES_ROOT}/{partName}/{partName}_Normal.png";

        if (!File.Exists(ToSystemPath(baseColorPath)) ||
            !File.Exists(ToSystemPath(metallicPath)) ||
            !File.Exists(ToSystemPath(roughnessPath)) ||
            !File.Exists(ToSystemPath(normalPath)))
        {
            Debug.LogWarning($"'{partName}': one or more expected texture files are missing, skipping. " +
                              "(Re-run the Blender bake script - it resumes automatically.)");
            return false;
        }

        ConfigureImport(baseColorPath, TextureImporterType.Default, sRGB: true, readable: false);
        ConfigureImport(normalPath, TextureImporterType.NormalMap, sRGB: false, readable: false);
        ConfigureImport(metallicPath, TextureImporterType.Default, sRGB: false, readable: true);
        ConfigureImport(roughnessPath, TextureImporterType.Default, sRGB: false, readable: true);

        Texture2D baseColorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(baseColorPath);
        Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        Texture2D metallicTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
        Texture2D roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);

        // --- Pack Metallic (R) + Smoothness (A, from inverted Roughness) ---
        Texture2D maskMap = BuildMaskMap(metallicTex, roughnessTex);
        string maskMapAssetPath = $"{GENERATED_MASKS_ROOT}/{partName}_MaskMap.png";
        File.WriteAllBytes(ToSystemPath(maskMapAssetPath), maskMap.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(maskMap);
        AssetDatabase.ImportAsset(maskMapAssetPath, ImportAssetOptions.ForceUpdate);
        ConfigureImport(maskMapAssetPath, TextureImporterType.Default, sRGB: false, readable: false);
        Texture2D maskMapAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(maskMapAssetPath);

        // --- Build the material ---
        Shader hdrpLit = Shader.Find("HDRP/Lit");
        if (hdrpLit == null)
        {
            Debug.LogError("Shader 'HDRP/Lit' not found - is the HDRP package installed in this project?");
            return false;
        }

        Material mat = new Material(hdrpLit) { name = partName };
        mat.SetTexture("_BaseColorMap", baseColorTex);
        mat.SetColor("_BaseColor", Color.white);

        mat.SetTexture("_NormalMap", normalTex);
        mat.SetFloat("_NormalScale", 1f);

        mat.SetTexture("_MaskMap", maskMapAsset);
        mat.SetFloat("_Metallic", 1f);
        mat.SetFloat("_Smoothness", 1f);

        ValidateHDRPMaterial(mat);

        AssetDatabase.CreateAsset(mat, matPath);
        return true;
    }

    /// <summary>
    /// Packs Metallic (R channel of the metallic bake) and Smoothness
    /// (1 - R channel of the roughness bake) into a single RGBA mask map.
    /// AO (G) is filled with white, Detail Mask (B) with 0.
    /// </summary>
    private static Texture2D BuildMaskMap(Texture2D metallic, Texture2D roughness)
    {
        bool sameSize = metallic.width == roughness.width && metallic.height == roughness.height;
        if (!sameSize)
        {
            Debug.LogWarning($"Metallic ({metallic.width}x{metallic.height}) and Roughness " +
                              $"({roughness.width}x{roughness.height}) sizes differ for '{metallic.name}' - " +
                              "resampling roughness to match metallic.");
        }

        Color32[] metallicPixels = metallic.GetPixels32();
        Color32[] roughnessPixels = sameSize ? roughness.GetPixels32() : null;
        Color32[] outPixels = new Color32[metallicPixels.Length];

        for (int i = 0; i < outPixels.Length; i++)
        {
            byte m = metallicPixels[i].r;
            byte rgh;
            if (sameSize)
            {
                rgh = roughnessPixels[i].r;
            }
            else
            {
                int x = i % metallic.width;
                int y = i / metallic.width;
                float u = (x + 0.5f) / metallic.width;
                float v = (y + 0.5f) / metallic.height;
                rgh = (byte)Mathf.RoundToInt(roughness.GetPixelBilinear(u, v).r * 255f);
            }
            byte smoothness = (byte)(255 - rgh);
            outPixels[i] = new Color32(m, 255, 0, smoothness);
        }

        var maskMap = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, false, true);
        maskMap.SetPixels32(outPixels);
        maskMap.Apply();
        return maskMap;
    }

    /// <summary>
    /// Sets up the keywords/passes HDRP/Lit needs for the properties we
    /// just assigned. Uses reflection to call HDRP's own validation API
    /// (UnityEditor.Rendering.HighDefinition.HDMaterial.ValidateMaterial)
    /// so this script still compiles if the exact API differs slightly
    /// across HDRP package versions; falls back to manual keywords.
    /// </summary>
    private static void ValidateHDRPMaterial(Material mat)
    {
        try
        {
            var asm = System.Reflection.Assembly.Load("Unity.RenderPipelines.HighDefinition.Editor");
            var type = asm?.GetType("UnityEditor.Rendering.HighDefinition.HDMaterial");
            var method = type?.GetMethod("ValidateMaterial", new[] { typeof(Material) });
            if (method != null)
            {
                method.Invoke(null, new object[] { mat });
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"HDRP material validation API not found ({e.Message}), " +
                              "falling back to manual keyword setup.");
        }

        mat.EnableKeyword("_NORMALMAP");
        mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
        mat.EnableKeyword("_MASKMAP");
        mat.SetFloat("_NormalMapSpace", 0f); // 0 = tangent space
    }

    private static void AssignMaterialsToSelection()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.Log("No GameObject selected in the Hierarchy - skipping auto-assignment. " +
                      "Select the model's root object and run " +
                      "Tools > RB7 Import > Assign Materials To Selected Hierarchy to do it manually.");
            return;
        }

        int assigned = 0;
        foreach (var root in Selection.gameObjects)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] sharedMats = renderer.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < sharedMats.Length; i++)
                {
                    string currentName = sharedMats[i] != null ? sharedMats[i].name : null;
                    if (string.IsNullOrEmpty(currentName)) continue;

                    string sanitized = SanitizeName(currentName);
                    string matPath = $"{MATERIALS_ROOT}/{sanitized}.mat";
                    Material newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

                    if (newMat != null && sharedMats[i] != newMat)
                    {
                        sharedMats[i] = newMat;
                        changed = true;
                        assigned++;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = sharedMats;
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        Debug.Log($"Auto-assigned {assigned} material slot(s) on the selected hierarchy.");
    }

    // Mirrors Blender's bpy.path.clean_name() enough to match folder/material names.
    private static string SanitizeName(string name)
    {
        char[] invalid = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        foreach (char c in invalid) name = name.Replace(c, '_');
        return name;
    }

    private static void ConfigureImport(string assetPath, TextureImporterType type, bool sRGB, bool readable)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (importer == null) return;

        bool dirty = false;
        if (importer.textureType != type) { importer.textureType = type; dirty = true; }
        if (importer.sRGBTexture != sRGB) { importer.sRGBTexture = sRGB; dirty = true; }
        if (importer.isReadable != readable) { importer.isReadable = readable; dirty = true; }
        if (dirty) importer.SaveAndReimport();
    }

    private static void EnsureFolder(string assetFolderPath)
    {
        if (AssetDatabase.IsValidFolder(assetFolderPath)) return;
        string parent = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
        string leaf = Path.GetFileName(assetFolderPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    private static string ToSystemPath(string assetPath)
    {
        // Assets/... paths are relative to the project root (one level above Assets).
        string projectRoot = Directory.GetCurrentDirectory();
        return Path.Combine(projectRoot, assetPath);
    }
}
