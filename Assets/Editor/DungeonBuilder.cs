using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class DungeonBuilder
{
    const string GamePath = "Assets/Scenes/minigame.unity";
    const string DungeonPath = "Assets/Scenes/Dungeon.unity";
    const string Decrepit = "Assets/Decrepit Dungeon LITE/Prefabs/";
    const string LowPoly = "Assets/BrokenVector/LowPolyDungeon/Prefabs/";
    const string Sounds = "Assets/HintsStarsLite/";
    const string Coin = "Assets/ithappy/Inferno_World_Free/Prefabs/Props/Coin_001";
    const int TunnelWidth = 3;
    const int TunnelLength = 6;
    const int RoomSize = 5;
    static readonly Vector3 ExitPosition = new Vector3(42, -16, 9.5f);
    static readonly Vector3 Bottom = new Vector3(0.5f, 0, 0.5f);
    static readonly Vector3 Top = new Vector3(0.5f, 1, 0.5f);
    static readonly Vector3 Center = new Vector3(0.5f, 0.5f, 0.5f);

    static Vector2 tile;
    static float wallHeight;
    static float wallThickness;

    [MenuItem("Tools/Build Dungeon")]
    static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene game = EditorSceneManager.OpenScene(GamePath);
        AddExit(game);
        EditorSceneManager.SaveScene(game);

        Scene dungeon = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(dungeon);
        Measure();
        Transform level = new GameObject("Dungeon").transform;
        BuildTunnel(level);
        BuildRoom(level);
        ConvertToUrp(level.gameObject);
        CopyRig(game);
        AddArrivalSound();
        SetLighting();
        EditorSceneManager.SaveScene(dungeon, DungeonPath);

        if (!EditorBuildSettings.scenes.Any(s => s.path == DungeonPath))
        {
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(DungeonPath, true)).ToArray();
        }
        EditorSceneManager.OpenScene(DungeonPath);
    }

    static void AddExit(Scene game)
    {
        foreach (GameObject root in game.GetRootGameObjects().Where(r => r.name == "Dungeon Exit"))
        {
            Object.DestroyImmediate(root);
        }

        GameObject exit = new GameObject("Dungeon Exit");
        exit.transform.position = ExitPosition;
        Bounds door = BoundsOf(Spawn(Decrepit + "Doorways/Doorway_A", exit.transform, ExitPosition, 90, new Vector3(0.5f, 0, 1)));

        GameObject darkness = GameObject.CreatePrimitive(PrimitiveType.Quad);
        darkness.name = "Darkness";
        Object.DestroyImmediate(darkness.GetComponent<Collider>());
        darkness.transform.SetParent(exit.transform);
        darkness.transform.position = new Vector3(door.center.x, door.center.y, door.max.z - 0.05f);
        darkness.transform.localScale = new Vector3(door.size.x, door.size.y, 1);
        darkness.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Decrepit Dungeon LITE/Materials/M_Black.mat");

        BoxCollider trigger = exit.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, 2, -1.5f);
        trigger.size = new Vector3(3, 4, 2);
        exit.AddComponent<SceneExit>();
        ConvertToUrp(exit);
        exit.SetActive(false);

        playerController player = Object.FindAnyObjectByType<playerController>();
        player.exit = exit;
        player.pickupSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Magic Score 9.wav");
        player.lifeLostSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Marimba 3 Notes Descend.wav");
        player.gameOverSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Solo Shimmer 2.wav");
    }

    static void AddArrivalSound()
    {
        AudioSource source = new GameObject("Arrival Sound").AddComponent<AudioSource>();
        source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Unlocked Secret.wav");
        source.playOnAwake = true;
        source.spatialBlend = 0;
    }

    static void Measure()
    {
        GameObject floor = Spawn(Decrepit + "Floors/Floor_A", null, Vector3.zero, 0, Bottom);
        GameObject wall = Spawn(Decrepit + "Walls/Wall_A", null, Vector3.zero, 0, Bottom);
        tile = new Vector2(BoundsOf(floor).size.x, BoundsOf(floor).size.z);
        wallHeight = BoundsOf(wall).size.y;
        wallThickness = BoundsOf(wall).size.x;
        Object.DestroyImmediate(floor);
        Object.DestroyImmediate(wall);
    }

    static void BuildTunnel(Transform level)
    {
        Transform tunnel = Hall("Tunnel", level, TunnelWidth, TunnelLength, 0);
        float half = TunnelWidth * tile.x / 2;
        EndWall(tunnel, TunnelWidth, -wallThickness / 2, false);

        for (int j = 1; j < TunnelLength; j += 2)
        {
            Pillar(tunnel, -half + 1.2f, j * tile.y);
            Pillar(tunnel, half - 1.2f, j * tile.y);
        }
        for (int j = 0; j < TunnelLength; j += 2)
        {
            Torch(tunnel, new Vector3(-half, 3, (j + 0.5f) * tile.y), Vector3.right);
            Torch(tunnel, new Vector3(half, 3, (j + 0.5f) * tile.y), Vector3.left);
        }

        Prop(Decrepit + "Props/Barrel", tunnel, -half + 1, 2.2f * tile.y, 0);
        Prop(Decrepit + "Props/Barrel", tunnel, -half + 1, 2.5f * tile.y, 40);
        Prop(Decrepit + "Props/Crate_A", tunnel, half - 1.5f, 1.6f * tile.y, 15);
        Prop(Decrepit + "Props/Crate_B", tunnel, half - 1.5f, 2.1f * tile.y, -10);
        Prop(Decrepit + "Props/Bucket", tunnel, -half + 2.5f, 0.4f * tile.y, 0);
        Prop(Decrepit + "Props/Table_A", tunnel, half - 2, 4.5f * tile.y, 90);
        Prop(Decrepit + "Props/Stool_A", tunnel, half - 3.5f, 4.4f * tile.y, 30);
        Prop(Decrepit + "Props/Floor_Cage", tunnel, -half + 3, 4.5f * tile.y, 0);
    }

    static void BuildRoom(Transform level)
    {
        float z0 = TunnelLength * tile.y;
        float z1 = z0 + RoomSize * tile.y;
        float half = RoomSize * tile.x / 2;
        Transform room = Hall("Room", level, RoomSize, RoomSize, z0);
        EndWall(room, RoomSize, z0, true);
        EndWall(room, RoomSize, z1 + wallThickness / 2, false);

        for (float z = z0 + 1; z < z1 - 5;)
        {
            GameObject carpet = Spawn(LowPoly + "Furniture/Carpet_Red", room, new Vector3(0, 0.01f, z), 90, new Vector3(0.5f, 0, 0), 1.5f);
            DisableColliders(carpet);
            z += BoundsOf(carpet).size.z;
        }
        for (int k = 1; k < RoomSize; k += 2)
        {
            Pillar(room, -1.2f * tile.x, z0 + k * tile.y);
            Pillar(room, 1.2f * tile.x, z0 + k * tile.y);
        }
        for (int j = 0; j < RoomSize; j += 2)
        {
            Torch(room, new Vector3(-half, 3, z0 + (j + 0.5f) * tile.y), Vector3.right);
            Torch(room, new Vector3(half, 3, z0 + (j + 0.5f) * tile.y), Vector3.left);
        }
        Torch(room, new Vector3(-3, 4, z1), Vector3.back);
        Torch(room, new Vector3(3, 4, z1), Vector3.back);

        GameObject throne = Prop(LowPoly + "Furniture/Throne", room, 0, z1 - 2.5f, 180, 2.5f);
        AddTreasureChest(room, BoundsOf(throne).min.z - 3);
        Prop(LowPoly + "Furniture/Barrel_Big", room, -half + 2, z1 - 2, 0);
        Prop(LowPoly + "Furniture/Barrel_Big", room, half - 2, z1 - 2, 0);
        Prop(LowPoly + "Furniture/Barrel_Big", room, -half + 2, z0 + 2, 0);
        Prop(LowPoly + "Furniture/WeaponRack_Big", room, half - 0.6f, z0 + 1.5f * tile.y, -90);
        Prop(LowPoly + "Furniture/WeaponRack_Big", room, half - 0.6f, z0 + 3 * tile.y, -90);
        Prop(Decrepit + "Props/Floor_Cage", room, half - 4, z0 + 2.25f * tile.y, 0);

        float tableX = -half + 4;
        float tableZ = z0 + 2.5f * tile.y;
        GameObject table = Prop(LowPoly + "Furniture/Table_Big", room, tableX, tableZ, 90);
        Prop(LowPoly + "Furniture/Bench", room, tableX - 1.8f, tableZ, 90);
        Prop(LowPoly + "Furniture/Bench", room, tableX + 1.8f, tableZ, 90);
        Spawn(LowPoly + "Lamps/Candlestick_Triple", room, new Vector3(tableX, BoundsOf(table).max.y, tableZ), 0, Bottom);

        Spawn(LowPoly + "Furniture/Banner", room, new Vector3(-1.5f * tile.x, 7, z1 - 0.2f), 180, Center, 1.5f);
        Spawn(LowPoly + "Furniture/Banner", room, new Vector3(1.5f * tile.x, 7, z1 - 0.2f), 180, Center, 1.5f);

        Light[] lights = Spawn(LowPoly + "Lamps/Chandelier", room, new Vector3(0, 2 * wallHeight, z1 - 3.5f), 0, Top, 2).GetComponentsInChildren<Light>();
        foreach (Light light in lights.Skip(1))
        {
            light.enabled = false;
        }
        lights[0].range = 18;
        lights[0].intensity = 3;
        lights[0].shadows = LightShadows.None;
    }

    static void AddTreasureChest(Transform parent, float z)
    {
        const float scale = 4;
        GameObject chest = Prop(LowPoly + "Furniture/Chest", parent, 0, z, 180, scale);
        chest.name = "Treasure Chest";
        foreach (Transform t in chest.GetComponentsInChildren<Transform>())
        {
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
        }
        Bounds body = BoundsOf(chest.transform.Find("Chest").gameObject);

        BoxCollider solid = chest.AddComponent<BoxCollider>();
        solid.center = chest.transform.InverseTransformPoint(body.center);
        solid.size = body.size / scale;
        BoxCollider reach = chest.AddComponent<BoxCollider>();
        reach.isTrigger = true;
        reach.center = solid.center;
        reach.size = solid.size * 2;

        Transform prize = new GameObject("Prize").transform;
        prize.SetParent(chest.transform, false);
        GameObject sample = Spawn(Coin, null, Vector3.zero, 0, Center);
        Vector3 size = BoundsOf(sample).size;
        float coinScale = body.size.x / 8 / Mathf.Max(size.x, size.y, size.z);
        Object.DestroyImmediate(sample);
        Random.InitState(1);
        for (int i = 0; i < 24; i++)
        {
            Vector3 position = new Vector3(
                Mathf.Lerp(body.min.x, body.max.x, Random.Range(0.15f, 0.85f)),
                body.max.y - Random.Range(0, 0.25f) * body.size.y,
                Mathf.Lerp(body.min.z, body.max.z, Random.Range(0.2f, 0.8f)));
            DisableColliders(Spawn(Coin, prize, position, Random.Range(0, 360f), Bottom, coinScale / scale));
        }

        TreasureChest treasure = chest.AddComponent<TreasureChest>();
        treasure.lid = chest.transform.Find("Chest_Lid");
        treasure.prize = prize.gameObject;
        treasure.openSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Coin Bag Reward.wav");
    }

    static Transform Hall(string name, Transform parent, int cols, int rows, float z0)
    {
        Transform hall = new GameObject(name).transform;
        hall.SetParent(parent);
        float half = cols * tile.x / 2;
        float length = rows * tile.y;

        for (int j = 0; j < rows; j++)
        {
            float z = z0 + (j + 0.5f) * tile.y;
            for (int i = 0; i < cols; i++)
            {
                float x = (i - (cols - 1) / 2f) * tile.x;
                DisableColliders(Spawn(Decrepit + "Floors/Floor_A", hall, new Vector3(x, 0, z), 0, Top));
                Spawn(Decrepit + "Walls/Ceiling", hall, new Vector3(x, 2 * wallHeight, z), 0, Bottom);
            }
            for (int row = 0; row < 2; row++)
            {
                Spawn(Decrepit + "Walls/Wall_A", hall, new Vector3(-half - wallThickness / 2, row * wallHeight, z), 0, Bottom);
                Spawn(Decrepit + "Walls/Wall_A", hall, new Vector3(half + wallThickness / 2, row * wallHeight, z), 0, Bottom);
            }
        }

        BoxCollider floor = hall.gameObject.AddComponent<BoxCollider>();
        floor.center = new Vector3(0, -0.5f, z0 + length / 2);
        floor.size = new Vector3(2 * half, 1, length);
        return hall;
    }

    static void EndWall(Transform parent, int cols, float z, bool arch)
    {
        for (int i = 0; i < cols; i++)
        {
            float x = (i - (cols - 1) / 2f) * tile.x;
            bool door = arch && Mathf.Approximately(x, 0);
            Spawn(Decrepit + (door ? "Arches/Arch_A" : "Walls/Wall_A"), parent, new Vector3(x, 0, z), 90, Bottom);
            Spawn(Decrepit + "Walls/Wall_A", parent, new Vector3(x, wallHeight, z), 90, Bottom);
        }
    }

    static void Pillar(Transform parent, float x, float z)
    {
        Spawn(Decrepit + "Pillars/Pillar_A", parent, new Vector3(x, 0, z), 0, Bottom);
        Spawn(Decrepit + "Pillars/Pillar_A", parent, new Vector3(x, wallHeight, z), 0, Bottom);
    }

    static void Torch(Transform parent, Vector3 wall, Vector3 inward)
    {
        GameObject torch = Spawn(LowPoly + "Lamps/Torch_Wall", parent, wall + inward * 0.15f, Quaternion.LookRotation(inward).eulerAngles.y, Center);
        foreach (Light light in torch.GetComponentsInChildren<Light>())
        {
            light.range = 10;
            light.intensity = 2.5f;
            light.shadows = LightShadows.None;
        }
    }

    static GameObject Prop(string prefab, Transform parent, float x, float z, float yaw, float scale = 1)
    {
        return Spawn(prefab, parent, new Vector3(x, 0, z), yaw, Bottom, scale);
    }

    static GameObject Spawn(string prefab, Transform parent, Vector3 position, float yaw, Vector3 anchor, float scale = 1)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab + ".prefab"), parent);
        go.transform.rotation = Quaternion.Euler(0, yaw, 0) * go.transform.rotation;
        go.transform.localScale *= scale;
        Bounds b = BoundsOf(go);
        go.transform.position += position - (b.min + Vector3.Scale(b.size, anchor));
        return go;
    }

    static Bounds BoundsOf(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    static void DisableColliders(GameObject go)
    {
        foreach (Collider c in go.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
    }

    static void CopyRig(Scene game)
    {
        GameObject rig = new GameObject("Rig");
        SceneManager.MoveGameObjectToScene(rig, game);
        foreach (GameObject root in game.GetRootGameObjects().Where(r => r.name is "Player" or "Main Camera" or "Canvas" or "EventSystem"))
        {
            root.transform.SetParent(rig.transform, true);
        }

        GameObject copy = Object.Instantiate(rig);
        Transform player = copy.transform.Find("Player");
        Transform cam = copy.transform.Find("Main Camera");
        Vector3 spawn = new Vector3(0, 0.5f, 1.2f * tile.y);
        cam.position += spawn - player.position;
        player.position = spawn;
        player.GetComponent<playerController>().exit = null;

        Camera camera = cam.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        copy.transform.DetachChildren();
        Object.DestroyImmediate(copy);
    }

    static void SetLighting()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.1f, 0.14f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.fogColor = new Color(0.03f, 0.02f, 0.04f);
    }

    static void ConvertToUrp(GameObject root)
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        foreach (Material m in root.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Distinct())
        {
            if (m == null || m.shader.name != "Standard" && !(m.shader.name.StartsWith("Legacy Shaders/") && !m.shader.name.Contains("Particles")))
            {
                continue;
            }

            Texture texture = m.mainTexture;
            Color color = m.HasColor("_Color") ? m.color : Color.white;
            float smoothness = m.HasFloat("_Glossiness") ? m.GetFloat("_Glossiness") : 0.2f;
            m.shader = lit;
            m.SetTexture("_BaseMap", texture);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            if (m.GetTexture("_BumpMap") != null)
            {
                m.EnableKeyword("_NORMALMAP");
            }
            if (m.GetTexture("_MetallicGlossMap") != null)
            {
                m.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            if (m.GetTexture("_EmissionMap") != null || m.GetColor("_EmissionColor").maxColorComponent > 0)
            {
                m.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(m);
        }
        AssetDatabase.SaveAssets();
    }
}
