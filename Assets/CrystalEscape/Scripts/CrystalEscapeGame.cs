using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrystalEscape
{
    public enum GameState
    {
        Ready,
        Playing,
        Won,
        Lost
    }

    public static class PrimitiveUtility
    {
        // Direct typed references keep the concrete collider classes from being
        // stripped by IL2CPP when CreatePrimitive is used in WebGL builds.
        public static void RemoveGeneratedCollider(GameObject primitive)
        {
            CapsuleCollider capsule = primitive.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                UnityEngine.Object.Destroy(capsule);
                return;
            }

            SphereCollider sphere = primitive.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                UnityEngine.Object.Destroy(sphere);
                return;
            }

            BoxCollider box = primitive.GetComponent<BoxCollider>();
            if (box != null)
            {
                UnityEngine.Object.Destroy(box);
                return;
            }

            MeshCollider mesh = primitive.GetComponent<MeshCollider>();
            if (mesh != null)
            {
                UnityEngine.Object.Destroy(mesh);
            }
        }
    }

    public static class CrystalEscapeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (UnityEngine.Object.FindAnyObjectByType<CrystalEscapeRuntime>() != null)
            {
                return;
            }

            new GameObject("Crystal Escape Runtime").AddComponent<CrystalEscapeRuntime>();
        }
    }

    public sealed class CrystalEscapeRuntime : MonoBehaviour
    {
        private readonly Color midnight = new Color(0.027f, 0.055f, 0.15f);
        private readonly Color cyan = new Color(0.145f, 0.91f, 1f);
        private readonly Color magenta = new Color(1f, 0.145f, 0.36f);
        private readonly Color violet = new Color(0.32f, 0.14f, 0.46f);

        private Material stoneMaterial;
        private Material stoneAccentMaterial;
        private Material darkStoneMaterial;
        private Material crystalMaterial;
        private Material hazardMaterial;
        private Material portalLockedMaterial;
        private Material portalReadyMaterial;
        private Material playerMaterial;
        private Material visorMaterial;
        private Material voidMaterial;
        private Material moonMaterial;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.shadowDistance = 42f;
            QualitySettings.antiAliasing = 2;

            BuildMaterials();
            ConfigureWorld();
            BuildAtmosphere();

            AudioManager audioManager = new GameObject("Audio Manager").AddComponent<AudioManager>();
            GameManager gameManager = new GameObject("Game Manager").AddComponent<GameManager>();
            gameManager.Initialize(audioManager, 5, 3);

            PlayerController player = BuildPlayer(gameManager);
            gameManager.SetPlayer(player);

            BuildArena(gameManager);
            GameUI ui = BuildUI(gameManager);
            gameManager.SetUI(ui);
        }

        private void BuildMaterials()
        {
            stoneMaterial = CreateMaterial("Midnight Stone", new Color(0.055f, 0.075f, 0.17f), 0.18f, 0.3f);
            stoneAccentMaterial = CreateMaterial("Violet Stone", new Color(0.12f, 0.085f, 0.26f), 0.22f, 0.42f);
            darkStoneMaterial = CreateMaterial("Distant Ruins", new Color(0.025f, 0.035f, 0.085f), 0.05f, 0.16f);
            crystalMaterial = CreateMaterial("Crystal Cyan", cyan, 0.08f, 0.92f, cyan * 3f);
            hazardMaterial = CreateMaterial("Hazard Red", new Color(0.42f, 0.018f, 0.055f), 0.42f, 0.64f, magenta * 2.5f);
            portalLockedMaterial = CreateMaterial("Portal Locked", new Color(0.12f, 0.045f, 0.23f), 0.1f, 0.72f, violet * 1.35f);
            portalReadyMaterial = CreateMaterial("Portal Ready", new Color(0.035f, 0.36f, 0.52f), 0.08f, 0.9f, cyan * 3.5f);
            playerMaterial = CreateMaterial("Explorer White", new Color(0.88f, 0.93f, 1f), 0.12f, 0.6f, new Color(0.025f, 0.06f, 0.11f));
            visorMaterial = CreateMaterial("Explorer Visor", new Color(0.015f, 0.11f, 0.2f), 0.25f, 0.9f, cyan * 0.65f);
            voidMaterial = CreateMaterial("Violet Void", new Color(0.11f, 0.018f, 0.18f), 0f, 0.15f, new Color(0.24f, 0.025f, 0.34f));
            moonMaterial = CreateMaterial("Moon", new Color(0.66f, 0.73f, 1f), 0f, 0.3f, new Color(0.72f, 0.78f, 1f) * 1.6f);
        }

        private Material CreateMaterial(string name, Color color, float metallic, float smoothness, Color? emission = null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", emission.Value);
                if (material.HasProperty("_EmissiveColor")) material.SetColor("_EmissiveColor", emission.Value);
            }
            return material;
        }

        private void ConfigureWorld()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.035f, 0.045f, 0.11f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.075f, 0.055f, 0.18f);
            RenderSettings.fogDensity = 0.018f;

            Camera existingCamera = Camera.main;
            if (existingCamera != null) Destroy(existingCamera.gameObject);
            Light existingLight = UnityEngine.Object.FindAnyObjectByType<Light>();
            if (existingLight != null) Destroy(existingLight.gameObject);

            GameObject moonLightObject = new GameObject("Cold Moonlight");
            Light moonLight = moonLightObject.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.56f, 0.67f, 1f);
            moonLight.intensity = 0.82f;
            moonLight.shadows = LightShadows.Soft;
            moonLight.shadowStrength = 0.72f;
            moonLightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private void BuildAtmosphere()
        {
            GameObject voidFloor = CreateBlock("Glowing Violet Void", new Vector3(0f, -6.4f, 4f), new Vector3(74f, 1f, 86f), voidMaterial);
            PrimitiveUtility.RemoveGeneratedCollider(voidFloor);

            GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moon.name = "Distant Moon";
            moon.transform.position = new Vector3(-18f, 18f, 34f);
            moon.transform.localScale = Vector3.one * 6.5f;
            moon.GetComponent<Renderer>().material = moonMaterial;
            PrimitiveUtility.RemoveGeneratedCollider(moon);

            for (int i = 0; i < 42; i++)
            {
                float angle = i * 2.39996f;
                float radius = 20f + (i % 8) * 3.1f;
                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.name = "Star " + (i + 1);
                star.transform.position = new Vector3(
                    Mathf.Sin(angle) * radius,
                    10f + (i % 9) * 1.35f,
                    -5f + Mathf.Cos(angle) * radius + (i % 4) * 4f);
                float size = 0.055f + (i % 3) * 0.035f;
                star.transform.localScale = Vector3.one * size;
                star.GetComponent<Renderer>().material = crystalMaterial;
                PrimitiveUtility.RemoveGeneratedCollider(star);
            }

            for (int i = 0; i < 18; i++)
            {
                bool left = i % 2 == 0;
                float z = -14f + i * 2.35f;
                float x = (left ? -1f : 1f) * (11.5f + (i % 5) * 2.1f);
                float height = 5f + (i % 6) * 1.55f;
                GameObject monolith = CreateBlock(
                    "Distant Ruin " + (i + 1),
                    new Vector3(x, -1.2f + height * 0.5f, z),
                    new Vector3(2.2f + (i % 3), height, 2.4f + (i % 2)),
                    darkStoneMaterial);
                monolith.transform.rotation = Quaternion.Euler(0f, (i % 5 - 2) * 7f, (left ? 1f : -1f) * (i % 3));
                PrimitiveUtility.RemoveGeneratedCollider(monolith);
            }
        }

        private PlayerController BuildPlayer(GameManager gameManager)
        {
            Vector3 spawn = new Vector3(0f, 0.2f, -14.3f);
            GameObject root = new GameObject("Player");
            root.tag = "Player";
            root.transform.position = spawn;

            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.44f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.42f;
            controller.slopeLimit = 52f;

            GameObject visualRoot = new GameObject("Explorer Visual Rig");
            visualRoot.transform.SetParent(root.transform, false);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Explorer Body";
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.82f, 0.92f, 0.72f);
            body.GetComponent<Renderer>().material = playerMaterial;
            PrimitiveUtility.RemoveGeneratedCollider(body);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Explorer Head";
            head.transform.SetParent(visualRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.72f, 0.02f);
            head.transform.localScale = new Vector3(0.88f, 0.78f, 0.82f);
            head.GetComponent<Renderer>().material = playerMaterial;
            PrimitiveUtility.RemoveGeneratedCollider(head);

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visor.name = "Explorer Visor";
            visor.transform.SetParent(visualRoot.transform, false);
            visor.transform.localPosition = new Vector3(0f, 1.76f, 0.37f);
            visor.transform.localScale = new Vector3(0.58f, 0.25f, 0.13f);
            visor.GetComponent<Renderer>().material = visorMaterial;
            PrimitiveUtility.RemoveGeneratedCollider(visor);

            Transform leftArm = CreateLimb("Left Arm", visualRoot.transform, new Vector3(-0.56f, 1.05f, 0f), playerMaterial).transform;
            Transform rightArm = CreateLimb("Right Arm", visualRoot.transform, new Vector3(0.56f, 1.05f, 0f), playerMaterial).transform;
            Transform leftLeg = CreateLimb("Left Leg", visualRoot.transform, new Vector3(-0.25f, 0.28f, 0f), playerMaterial).transform;
            Transform rightLeg = CreateLimb("Right Leg", visualRoot.transform, new Vector3(0.25f, 0.28f, 0f), playerMaterial).transform;

            GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "Soft Explorer Shadow";
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = new Vector3(0f, -0.18f, 0f);
            shadow.transform.localScale = new Vector3(0.62f, 0.018f, 0.44f);
            shadow.GetComponent<Renderer>().material = darkStoneMaterial;
            PrimitiveUtility.RemoveGeneratedCollider(shadow);

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 2.85f, -5.65f);
            cameraObject.transform.localRotation = Quaternion.Euler(9f, 0f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = midnight;
            camera.fieldOfView = 59f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 90f;
            cameraObject.AddComponent<AudioListener>();

            PlayerController player = root.AddComponent<PlayerController>();
            player.Initialize(
                controller,
                cameraObject.transform,
                gameManager,
                spawn,
                visualRoot.transform,
                body.transform,
                leftArm,
                rightArm,
                leftLeg,
                rightLeg);
            return player;
        }

        private GameObject CreateLimb(string name, Transform parent, Vector3 localPosition, Material material)
        {
            GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            limb.name = name;
            limb.transform.SetParent(parent, false);
            limb.transform.localPosition = localPosition;
            limb.transform.localScale = new Vector3(0.22f, 0.38f, 0.22f);
            limb.GetComponent<Renderer>().material = material;
            PrimitiveUtility.RemoveGeneratedCollider(limb);
            return limb;
        }

        private void BuildArena(GameManager gameManager)
        {
            CreateStonePlatform("Start Plaza", new Vector3(0f, 0f, -13f), 5, 4, 2f);
            CreateStonePlatform("First Bridge", new Vector3(0f, 0.15f, -6f), 3, 3, 2f);
            CreateStonePlatform("Mid Plaza", new Vector3(0f, 0.5f, 0f), 5, 3, 2f);
            CreateStonePlatform("Upper Bridge", new Vector3(0f, 0.75f, 5f), 3, 2, 2f);
            CreateStonePlatform("Upper Plaza", new Vector3(0f, 1f, 9.5f), 5, 3, 2f);

            for (int i = 0; i < 4; i++)
            {
                float top = 1.25f + i * 0.29f;
                CreateBlock("Portal Stair " + (i + 1), new Vector3(0f, top - 0.16f, 12.7f + i * 0.68f), new Vector3(5.8f, 0.32f, 0.78f), stoneAccentMaterial);
            }
            CreateStonePlatform("Portal Dais", new Vector3(0f, 2.25f, 16.4f), 5, 3, 2f);

            CreateBrokenSideIsland(new Vector3(-7.2f, 0.1f, -5f), 2, 2);
            CreateBrokenSideIsland(new Vector3(7.4f, 0.65f, 2.4f), 2, 2);
            CreateBrokenSideIsland(new Vector3(-7.7f, 1.15f, 10.2f), 2, 2);

            Vector3[] crystalPositions =
            {
                new Vector3(0f, 1.35f, -10.8f),
                new Vector3(-1.05f, 1.55f, -5.5f),
                new Vector3(1.45f, 1.9f, 0.3f),
                new Vector3(-0.95f, 2.15f, 5.7f),
                new Vector3(0f, 2.42f, 10.5f)
            };
            for (int i = 0; i < crystalPositions.Length; i++)
            {
                BuildCrystal(i + 1, crystalPositions[i], gameManager);
            }

            BuildHazard("Spike Trap 1", new Vector3(1.2f, 0.15f, -7.25f), new Vector3(1.5f, 1f, 1.45f), gameManager);
            BuildHazard("Spike Trap 2", new Vector3(-1.45f, 0.5f, 1.75f), new Vector3(1.55f, 1f, 1.55f), gameManager);
            BuildHazard("Spike Trap 3", new Vector3(1.45f, 1f, 8.6f), new Vector3(1.7f, 1f, 1.65f), gameManager);
            BuildPortal(new Vector3(0f, 2.25f, 17.25f), gameManager);

            AddRouteTrim(new Vector3(-2.7f, 0.42f, -6f), new Vector3(0.12f, 0.18f, 5.6f));
            AddRouteTrim(new Vector3(2.7f, 0.42f, -6f), new Vector3(0.12f, 0.18f, 5.6f));
            AddRouteTrim(new Vector3(-2.75f, 1.24f, 9.5f), new Vector3(0.12f, 0.18f, 5.8f));
            AddRouteTrim(new Vector3(2.75f, 1.24f, 9.5f), new Vector3(0.12f, 0.18f, 5.8f));
        }

        private void CreateStonePlatform(string name, Vector3 center, int widthTiles, int depthTiles, float tileSize)
        {
            for (int x = 0; x < widthTiles; x++)
            {
                for (int z = 0; z < depthTiles; z++)
                {
                    float px = center.x + (x - (widthTiles - 1) * 0.5f) * tileSize;
                    float pz = center.z + (z - (depthTiles - 1) * 0.5f) * tileSize;
                    float seam = 0.08f;
                    Material material = ((x + z) % 3 == 0) ? stoneAccentMaterial : stoneMaterial;
                    GameObject tile = CreateBlock(
                        name + " Tile " + x + "-" + z,
                        new Vector3(px, center.y - 0.3f, pz),
                        new Vector3(tileSize - seam, 0.6f, tileSize - seam),
                        material);
                    tile.transform.rotation = Quaternion.Euler(0f, ((x * 7 + z * 11) % 5 - 2) * 0.35f, 0f);
                }
            }
        }

        private void CreateBrokenSideIsland(Vector3 center, int widthTiles, int depthTiles)
        {
            CreateStonePlatform("Broken Side Island", center, widthTiles, depthTiles, 2f);
            GameObject pillar = CreateBlock("Island Ruin Pillar", center + new Vector3(0.65f, 2.2f, 0.55f), new Vector3(0.8f, 4.2f, 0.8f), stoneAccentMaterial);
            PrimitiveUtility.RemoveGeneratedCollider(pillar);
        }

        private void AddRouteTrim(Vector3 position, Vector3 scale)
        {
            GameObject trim = CreateBlock("Cyan Route Trim", position, scale, crystalMaterial);
            PrimitiveUtility.RemoveGeneratedCollider(trim);
        }

        private GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().material = material;
            return block;
        }

        private GameObject TryCreateTripoVisual(string resourcePath, Transform parent, string displayName, Vector3 desiredSize, Vector3 desiredCenter)
        {
            GameObject source = Resources.Load<GameObject>(resourcePath);
            if (source == null) return null;

            GameObject instance = Instantiate(source, parent, false);
            instance.name = displayName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (Collider importedCollider in instance.GetComponentsInChildren<Collider>(true)) Destroy(importedCollider);
            foreach (Camera importedCamera in instance.GetComponentsInChildren<Camera>(true)) Destroy(importedCamera);
            foreach (Light importedLight in instance.GetComponentsInChildren<Light>(true)) Destroy(importedLight);

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return instance;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float scaleX = desiredSize.x / Mathf.Max(bounds.size.x, 0.001f);
            float scaleY = desiredSize.y / Mathf.Max(bounds.size.y, 0.001f);
            float scaleZ = desiredSize.z / Mathf.Max(bounds.size.z, 0.001f);
            instance.transform.localScale = Vector3.one * Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 currentCenter = parent.InverseTransformPoint(bounds.center);
            instance.transform.localPosition += desiredCenter - currentCenter;
            return instance;
        }

        private void BuildCrystal(int number, Vector3 position, GameManager gameManager)
        {
            GameObject root = new GameObject("Tripo Crystal Socket " + number);
            root.transform.position = position;
            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.radius = 0.8f;
            trigger.isTrigger = true;

            GameObject visual = TryCreateTripoVisual(
                "Tripo/Crystal/Crystal",
                root.transform,
                "Tripo Crystal - Functional Reward Asset",
                new Vector3(1.2f, 1.8f, 1.2f),
                Vector3.zero);
            if (visual == null)
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Crystal Fallback";
                visual.transform.SetParent(root.transform, false);
                visual.transform.localScale = new Vector3(0.65f, 1.15f, 0.65f);
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                visual.GetComponent<Renderer>().material = crystalMaterial;
                PrimitiveUtility.RemoveGeneratedCollider(visual);
            }

            GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            halo.name = "Crystal Ground Halo";
            halo.transform.SetParent(root.transform, false);
            halo.transform.localPosition = new Vector3(0f, -1.18f, 0f);
            halo.transform.localScale = new Vector3(0.66f, 0.018f, 0.66f);
            halo.GetComponent<Renderer>().material = crystalMaterial;
            PrimitiveUtility.RemoveGeneratedCollider(halo);

            for (int i = 0; i < 3; i++)
            {
                GameObject mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mote.name = "Crystal Mote " + (i + 1);
                mote.transform.SetParent(root.transform, false);
                mote.transform.localScale = Vector3.one * (0.07f + i * 0.015f);
                mote.GetComponent<Renderer>().material = crystalMaterial;
                PrimitiveUtility.RemoveGeneratedCollider(mote);
                OrbitingMote orbit = mote.AddComponent<OrbitingMote>();
                orbit.Initialize(number * 0.7f + i * 2.1f, 0.58f + i * 0.12f, 0.8f + i * 0.32f);
            }

            Light glow = root.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = cyan;
            glow.range = 5.5f;
            glow.intensity = 1.75f;
            glow.shadows = LightShadows.None;

            Collectible collectible = root.AddComponent<Collectible>();
            collectible.Initialize(gameManager, visual.transform, number * 0.4f, crystalMaterial);
        }

        private void BuildHazard(string name, Vector3 position, Vector3 scale, GameManager gameManager)
        {
            GameObject root = new GameObject(name + " - Tripo Spike Socket");
            root.transform.position = position;
            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.36f, 0f);
            trigger.size = new Vector3(scale.x, 0.9f, scale.z);

            GameObject underglow = CreateBlock(name + " Red Underglow", position + new Vector3(0f, 0.07f, 0f), new Vector3(scale.x * 1.12f, 0.12f, scale.z * 1.12f), hazardMaterial);
            underglow.transform.SetParent(root.transform, true);
            PrimitiveUtility.RemoveGeneratedCollider(underglow);

            GameObject tripoVisual = TryCreateTripoVisual(
                "Tripo/SpikeTrap/SpikeTrap",
                root.transform,
                "Tripo Spike Trap - Functional Damage Asset",
                new Vector3(scale.x * 0.95f, 1f, scale.z * 0.95f),
                new Vector3(0f, 0.46f, 0f));
            if (tripoVisual == null)
            {
                for (int i = 0; i < 9; i++)
                {
                    GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    spike.name = "Spike Fallback";
                    spike.transform.SetParent(root.transform, false);
                    spike.transform.localPosition = new Vector3((i % 3 - 1) * scale.x * 0.28f, 0.45f, (i / 3 - 1) * scale.z * 0.28f);
                    spike.transform.localScale = new Vector3(0.12f, 0.45f, 0.12f);
                    spike.GetComponent<Renderer>().material = hazardMaterial;
                    PrimitiveUtility.RemoveGeneratedCollider(spike);
                }
            }

            Hazard hazard = root.AddComponent<Hazard>();
            hazard.Initialize(gameManager, 1, underglow.transform);
        }

        private void BuildPortal(Vector3 position, GameManager gameManager)
        {
            GameObject root = new GameObject("Exit Portal - Tripo Portal Socket");
            root.transform.position = position;
            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2.8f, 0f);
            trigger.size = new Vector3(4.5f, 5.6f, 1.8f);

            GameObject visualRoot = new GameObject("Portal Visuals");
            visualRoot.transform.SetParent(root.transform, false);
            GameObject tripoVisual = TryCreateTripoVisual(
                "Tripo/ExitPortal/ExitPortal",
                visualRoot.transform,
                "Tripo Exit Portal - Functional End Asset",
                new Vector3(5.7f, 5.8f, 1.7f),
                new Vector3(0f, 2.9f, 0f));
            if (tripoVisual == null)
            {
                for (int i = 0; i < 16; i++)
                {
                    float angle = i * Mathf.PI * 2f / 16f;
                    GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segment.name = "Stone Portal Segment";
                    segment.transform.SetParent(visualRoot.transform, false);
                    segment.transform.localPosition = new Vector3(Mathf.Cos(angle) * 2.6f, 2.9f + Mathf.Sin(angle) * 2.6f, 0f);
                    segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);
                    segment.transform.localScale = new Vector3(0.78f, 0.34f, 0.8f);
                    segment.GetComponent<Renderer>().material = stoneAccentMaterial;
                    PrimitiveUtility.RemoveGeneratedCollider(segment);
                }
            }

            GameObject energyRing = new GameObject("Open Portal Energy Ring");
            energyRing.transform.SetParent(visualRoot.transform, false);
            energyRing.transform.localPosition = new Vector3(0f, 2.9f, 0.12f);
            Renderer[] ringRenderers = new Renderer[20];
            for (int i = 0; i < ringRenderers.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / ringRenderers.Length;
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = "Energy Rune " + (i + 1);
                segment.transform.SetParent(energyRing.transform, false);
                segment.transform.localPosition = new Vector3(Mathf.Cos(angle) * 2.15f, Mathf.Sin(angle) * 2.15f, 0f);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);
                segment.transform.localScale = new Vector3(0.62f, 0.1f, 0.14f);
                ringRenderers[i] = segment.GetComponent<Renderer>();
                ringRenderers[i].material = portalLockedMaterial;
                PrimitiveUtility.RemoveGeneratedCollider(segment);
            }

            for (int i = 0; i < 8; i++)
            {
                GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fragment.name = "Floating Portal Fragment " + (i + 1);
                fragment.transform.SetParent(root.transform, false);
                fragment.transform.localScale = Vector3.one * (0.18f + (i % 3) * 0.08f);
                fragment.GetComponent<Renderer>().material = stoneAccentMaterial;
                PrimitiveUtility.RemoveGeneratedCollider(fragment);
                OrbitingFragment orbit = fragment.AddComponent<OrbitingFragment>();
                orbit.Initialize(i * Mathf.PI * 0.25f, 3.3f + (i % 2) * 0.45f, 2.9f + (i % 3 - 1) * 0.65f);
            }

            GameObject portalLightObject = new GameObject("Portal Cyan Light");
            portalLightObject.transform.SetParent(root.transform, false);
            portalLightObject.transform.localPosition = new Vector3(0f, 2.9f, -0.2f);
            Light portalLight = portalLightObject.AddComponent<Light>();
            portalLight.type = LightType.Point;
            portalLight.color = violet;
            portalLight.range = 10f;
            portalLight.intensity = 1.15f;
            portalLight.shadows = LightShadows.None;

            ExitPortal portal = root.AddComponent<ExitPortal>();
            portal.Initialize(gameManager, energyRing.transform, ringRenderers, portalLight, portalLockedMaterial, portalReadyMaterial, violet, cyan);
        }

        private GameUI BuildUI(GameManager gameManager)
        {
            GameObject canvasObject = new GameObject("Game UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("Event System");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            GameUI ui = canvasObject.AddComponent<GameUI>();
            ui.Build(canvasObject.transform, gameManager, cyan, magenta);
            return ui;
        }
    }

    public sealed class GameManager : MonoBehaviour
    {
        public GameState State { get; private set; } = GameState.Ready;
        public int Crystals { get; private set; }
        public int RequiredCrystals { get; private set; }
        public int Health { get; private set; }
        public int MaximumHealth { get; private set; }

        private AudioManager audioManager;
        private GameUI ui;
        private PlayerController player;
        private float startTime;
        private float nextTimerRefresh;

        public void Initialize(AudioManager audio, int requiredCrystals, int maximumHealth)
        {
            audioManager = audio;
            RequiredCrystals = requiredCrystals;
            MaximumHealth = maximumHealth;
            Health = maximumHealth;
        }

        public void SetPlayer(PlayerController value) => player = value;

        public void SetUI(GameUI value)
        {
            ui = value;
            ui.UpdateHud(Crystals, RequiredCrystals, Health, MaximumHealth);
            ui.ShowReady();
        }

        private void Update()
        {
            if (State == GameState.Playing && ui != null && Time.unscaledTime >= nextTimerRefresh)
            {
                ui.UpdateTimer(Time.time - startTime);
                nextTimerRefresh = Time.unscaledTime + 0.2f;
            }
        }

        public void StartGame()
        {
            if (State != GameState.Ready) return;
            State = GameState.Playing;
            startTime = Time.time;
            nextTimerRefresh = 0f;
            player.SetControlEnabled(true);
            ui.ShowPlaying();
            ui.ShowMessage("FOLLOW THE CYAN CRYSTALS", 2.1f);
            audioManager.PlayMusic();
        }

        public void CollectCrystal()
        {
            if (State != GameState.Playing) return;
            Crystals = Mathf.Min(Crystals + 1, RequiredCrystals);
            audioManager.PlayPickup();
            ui.UpdateHud(Crystals, RequiredCrystals, Health, MaximumHealth);
            ui.PulseCrystal();
            ui.ShowMessage(Crystals >= RequiredCrystals ? "PORTAL READY — RUN!" : "+1 CRYSTAL", 1.35f);
        }

        public void TakeDamage(int amount)
        {
            if (State != GameState.Playing) return;
            Health = Mathf.Max(0, Health - amount);
            audioManager.PlayHurt();
            ui.UpdateHud(Crystals, RequiredCrystals, Health, MaximumHealth);
            ui.FlashDamage();
            if (Health <= 0) Finish(false);
        }

        public void TryExit()
        {
            if (State != GameState.Playing) return;
            if (Crystals < RequiredCrystals)
            {
                int remaining = RequiredCrystals - Crystals;
                ui.ShowMessage("PORTAL LOCKED — " + remaining + " CRYSTAL" + (remaining == 1 ? "" : "S") + " LEFT", 1.7f);
                return;
            }
            Finish(true);
        }

        private void Finish(bool won)
        {
            State = won ? GameState.Won : GameState.Lost;
            player.SetControlEnabled(false);
            float elapsed = Time.time - startTime;
            ui.ShowEnd(won, Crystals, RequiredCrystals, elapsed);
            audioManager.StopMusic();
            if (won) audioManager.PlayWin(); else audioManager.PlayLose();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartGame()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex >= 0) SceneManager.LoadScene(activeScene.buildIndex);
            else SceneManager.LoadScene(activeScene.name);
        }
    }

    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController controller;
        private Transform cameraTransform;
        private Transform visualRoot;
        private Transform body;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private GameManager gameManager;
        private Vector3 spawnPosition;
        private Vector3 cameraBasePosition;
        private Vector3 horizontalVelocity;
        private Vector3 knockbackVelocity;
        private float verticalVelocity;
        private float pitch = 9f;
        private float nextDamageTime;
        private float damageShake;
        private float landingKick;
        private bool wasGrounded;
        private bool controlEnabled;

        private const float MoveSpeed = 6.6f;
        private const float JumpHeight = 1.55f;
        private const float Gravity = -25f;
        private const float MouseSensitivity = 2.05f;

        public void Initialize(
            CharacterController characterController,
            Transform playerCamera,
            GameManager manager,
            Vector3 respawnPosition,
            Transform explorerVisual,
            Transform bodyTransform,
            Transform leftArmTransform,
            Transform rightArmTransform,
            Transform leftLegTransform,
            Transform rightLegTransform)
        {
            controller = characterController;
            cameraTransform = playerCamera;
            cameraBasePosition = playerCamera.localPosition;
            gameManager = manager;
            spawnPosition = respawnPosition;
            visualRoot = explorerVisual;
            body = bodyTransform;
            leftArm = leftArmTransform;
            rightArm = rightArmTransform;
            leftLeg = leftLegTransform;
            rightLeg = rightLegTransform;
            SetControlEnabled(false);
        }

        public void SetControlEnabled(bool value)
        {
            controlEnabled = value;
            enabled = true;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }

        private void Update()
        {
            if (!controlEnabled)
            {
                AnimateExplorer(0f);
                return;
            }

            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
            pitch = Mathf.Clamp(pitch - mouseY, -20f, 48f);

            Vector3 input = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
            input = Vector3.ClampMagnitude(input, 1f);
            Vector3 desiredVelocity = input * MoveSpeed;
            float acceleration = input.sqrMagnitude > 0.01f ? 26f : 34f;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVelocity, acceleration * Time.deltaTime);

            bool groundedBeforeMove = controller.isGrounded;
            if (groundedBeforeMove && verticalVelocity < 0f) verticalVelocity = -2f;
            if (groundedBeforeMove && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                landingKick = -0.08f;
            }
            verticalVelocity += Gravity * Time.deltaTime;

            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, 12f * Time.deltaTime);
            Vector3 movement = horizontalVelocity + knockbackVelocity;
            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);

            if (controller.isGrounded && !wasGrounded) landingKick = 0.14f;
            wasGrounded = controller.isGrounded;
            landingKick = Mathf.MoveTowards(landingKick, 0f, Time.deltaTime * 0.7f);
            damageShake = Mathf.MoveTowards(damageShake, 0f, Time.deltaTime * 1.8f);

            float speed01 = Mathf.Clamp01(horizontalVelocity.magnitude / MoveSpeed);
            float bob = controller.isGrounded ? Mathf.Sin(Time.time * 10.5f) * 0.045f * speed01 : 0f;
            Vector3 shake = UnityEngine.Random.insideUnitSphere * (damageShake * 0.13f);
            cameraTransform.localPosition = cameraBasePosition + new Vector3(Mathf.Sin(Time.time * 5.25f) * 0.025f * speed01, bob - landingKick, 0f) + shake;
            cameraTransform.localRotation = Quaternion.Euler(pitch + landingKick * 13f + shake.x * 2f, 0f, -mouseX * 0.28f);
            AnimateExplorer(speed01);

            if (transform.position.y < -4.5f)
            {
                TeleportToSpawn();
                TryTakeDamage(1, transform.position - transform.forward);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void AnimateExplorer(float speed01)
        {
            float swing = Mathf.Sin(Time.time * 10f) * 34f * speed01;
            leftArm.localRotation = Quaternion.Euler(swing, 0f, -10f);
            rightArm.localRotation = Quaternion.Euler(-swing, 0f, 10f);
            leftLeg.localRotation = Quaternion.Euler(-swing * 0.62f, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(swing * 0.62f, 0f, 0f);
            visualRoot.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.035f * speed01, 0f);
            body.localScale = new Vector3(0.82f + speed01 * 0.025f, 0.92f - speed01 * 0.018f, 0.72f);
        }

        private void TeleportToSpawn()
        {
            controller.enabled = false;
            transform.position = spawnPosition;
            transform.rotation = Quaternion.identity;
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
            knockbackVelocity = Vector3.zero;
            verticalVelocity = 0f;
            pitch = 9f;
        }

        public void TryTakeDamage(int amount, Vector3 sourcePosition)
        {
            if (Time.time < nextDamageTime || gameManager.State != GameState.Playing) return;
            nextDamageTime = Time.time + 1f;
            Vector3 away = transform.position - sourcePosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = -transform.forward;
            knockbackVelocity = away.normalized * 6.5f;
            verticalVelocity = Mathf.Max(verticalVelocity, 3f);
            damageShake = 0.65f;
            gameManager.TakeDamage(amount);
        }
    }

    public sealed class Collectible : MonoBehaviour
    {
        private GameManager gameManager;
        private Transform visual;
        private Material burstMaterial;
        private float phase;
        private Vector3 baseLocalPosition;
        private bool collected;

        public void Initialize(GameManager manager, Transform visualTransform, float animationPhase, Material effectMaterial)
        {
            gameManager = manager;
            visual = visualTransform;
            phase = animationPhase;
            burstMaterial = effectMaterial;
            baseLocalPosition = visual.localPosition;
        }

        private void Update()
        {
            if (visual == null) return;
            visual.Rotate(0f, 72f * Time.deltaTime, 0f, Space.World);
            visual.localPosition = baseLocalPosition + Vector3.up * (Mathf.Sin(Time.time * 2.5f + phase) * 0.16f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player") || gameManager.State != GameState.Playing) return;
            collected = true;
            PickupBurst.Spawn(transform.position, burstMaterial);
            gameManager.CollectCrystal();
            gameObject.SetActive(false);
        }
    }

    public sealed class Hazard : MonoBehaviour
    {
        private GameManager gameManager;
        private Transform underglow;
        private Vector3 baseScale;
        private int damage;

        public void Initialize(GameManager manager, int damageAmount, Transform glowTransform)
        {
            gameManager = manager;
            damage = damageAmount;
            underglow = glowTransform;
            baseScale = underglow.localScale;
        }

        private void Update()
        {
            float pulse = 1f + Mathf.Sin(Time.time * 5f + transform.position.z) * 0.07f;
            underglow.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z * pulse);
        }

        private void OnTriggerStay(Collider other)
        {
            if (gameManager.State != GameState.Playing || !other.CompareTag("Player")) return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) player.TryTakeDamage(damage, transform.position);
        }
    }

    public sealed class ExitPortal : MonoBehaviour
    {
        private GameManager gameManager;
        private Transform energyRing;
        private Renderer[] ringRenderers;
        private Light portalLight;
        private Material lockedMaterial;
        private Material readyMaterial;
        private Color lockedColor;
        private Color readyColor;
        private bool unlocked;

        public void Initialize(GameManager manager, Transform ring, Renderer[] renderers, Light glow, Material locked, Material ready, Color lockedLight, Color readyLight)
        {
            gameManager = manager;
            energyRing = ring;
            ringRenderers = renderers;
            portalLight = glow;
            lockedMaterial = locked;
            readyMaterial = ready;
            lockedColor = lockedLight;
            readyColor = readyLight;
        }

        private void Update()
        {
            bool shouldUnlock = gameManager.Crystals >= gameManager.RequiredCrystals;
            if (shouldUnlock && !unlocked)
            {
                unlocked = true;
                for (int i = 0; i < ringRenderers.Length; i++) ringRenderers[i].material = readyMaterial;
                portalLight.color = readyColor;
            }

            energyRing.Rotate(0f, 0f, (unlocked ? 28f : 10f) * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(Time.time * (unlocked ? 4.8f : 2.1f)) * (unlocked ? 0.045f : 0.018f);
            energyRing.localScale = Vector3.one * pulse;
            portalLight.intensity = unlocked ? 3.8f + Mathf.Sin(Time.time * 4.8f) * 0.45f : 1.1f;
            portalLight.color = unlocked ? readyColor : lockedColor;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) gameManager.TryExit();
        }
    }

    public sealed class OrbitingMote : MonoBehaviour
    {
        private float angle;
        private float radius;
        private float height;

        public void Initialize(float startAngle, float orbitRadius, float orbitHeight)
        {
            angle = startAngle;
            radius = orbitRadius;
            height = orbitHeight;
        }

        private void Update()
        {
            angle += Time.deltaTime * 1.8f;
            transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle * 1.7f) * 0.2f + height, Mathf.Sin(angle) * radius);
        }
    }

    public sealed class OrbitingFragment : MonoBehaviour
    {
        private float angle;
        private float radius;
        private float height;

        public void Initialize(float startAngle, float orbitRadius, float orbitHeight)
        {
            angle = startAngle;
            radius = orbitRadius;
            height = orbitHeight;
        }

        private void Update()
        {
            angle += Time.deltaTime * 0.22f;
            transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, height + Mathf.Sin(angle * 2f) * 0.35f, Mathf.Sin(angle) * 0.8f);
            transform.Rotate(new Vector3(17f, 29f, 11f) * Time.deltaTime, Space.Self);
        }
    }

    public static class PickupBurst
    {
        public static void Spawn(Vector3 position, Material material)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                GameObject shard = GameObject.CreatePrimitive(i % 3 == 0 ? PrimitiveType.Sphere : PrimitiveType.Cube);
                shard.name = "Crystal Pickup Spark";
                shard.transform.position = position;
                shard.transform.localScale = Vector3.one * (0.08f + (i % 3) * 0.035f);
                shard.GetComponent<Renderer>().material = material;
                PrimitiveUtility.RemoveGeneratedCollider(shard);
                Vector3 velocity = new Vector3(Mathf.Cos(angle), 0.65f + (i % 4) * 0.18f, Mathf.Sin(angle)) * 3.2f;
                shard.AddComponent<BurstShard>().Initialize(velocity, 0.72f);
            }
        }
    }

    public sealed class BurstShard : MonoBehaviour
    {
        private Vector3 velocity;
        private float lifetime;
        private float age;

        public void Initialize(Vector3 initialVelocity, float duration)
        {
            velocity = initialVelocity;
            lifetime = duration;
        }

        private void Update()
        {
            age += Time.deltaTime;
            velocity += Vector3.down * 4.2f * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(new Vector3(130f, 210f, 90f) * Time.deltaTime);
            float scale = Mathf.Clamp01(1f - age / lifetime);
            transform.localScale = Vector3.one * Mathf.Max(0.001f, scale * 0.12f);
            if (age >= lifetime) Destroy(gameObject);
        }
    }

    public sealed class AudioManager : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioSource effectsSource;
        private AudioClip music;
        private AudioClip pickup;
        private AudioClip hurt;
        private AudioClip win;
        private AudioClip lose;

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            effectsSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.25f;
            effectsSource.volume = 0.76f;

            music = Resources.Load<AudioClip>("Audio/BGM");
            pickup = Resources.Load<AudioClip>("Audio/Pickup");
            hurt = Resources.Load<AudioClip>("Audio/Hurt");
            win = Resources.Load<AudioClip>("Audio/Win");
            lose = Resources.Load<AudioClip>("Audio/Lose");

            if (music == null) music = CreateTone("Procedural BGM", 12f, 110f, 0.11f, true);
            if (pickup == null) pickup = CreateSweep("Procedural Pickup", 0.28f, 620f, 1200f, 0.38f);
            if (hurt == null) hurt = CreateSweep("Procedural Hurt", 0.34f, 180f, 70f, 0.48f);
            if (win == null) win = CreateSweep("Procedural Win", 1.2f, 440f, 980f, 0.35f);
            if (lose == null) lose = CreateSweep("Procedural Lose", 1f, 260f, 90f, 0.35f);
        }

        public void PlayMusic() { musicSource.clip = music; musicSource.Play(); }
        public void StopMusic() => musicSource.Stop();
        public void PlayPickup() => effectsSource.PlayOneShot(pickup);
        public void PlayHurt() => effectsSource.PlayOneShot(hurt);
        public void PlayWin() => effectsSource.PlayOneShot(win);
        public void PlayLose() => effectsSource.PlayOneShot(lose);

        private AudioClip CreateTone(string clipName, float duration, float frequency, float volume, bool layered)
        {
            const int sampleRate = 44100;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float wave = Mathf.Sin(t * frequency * Mathf.PI * 2f);
                if (layered) wave += Mathf.Sin(t * frequency * 1.5f * Mathf.PI * 2f) * 0.42f;
                float rhythm = 0.62f + Mathf.Sin(t * Mathf.PI) * 0.13f;
                data[i] = wave * volume * rhythm;
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSweep(string clipName, float duration, float startFrequency, float endFrequency, float volume)
        {
            const int sampleRate = 44100;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = i / (float)samples;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                phase += frequency / sampleRate;
                float envelope = Mathf.Sin(progress * Mathf.PI);
                data[i] = Mathf.Sin(phase * Mathf.PI * 2f) * envelope * volume;
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }

    public sealed class GameUI : MonoBehaviour
    {
        private GameObject startPanel;
        private GameObject hudPanel;
        private GameObject endPanel;
        private Text crystalText;
        private Text healthText;
        private Text timerText;
        private Text portalText;
        private Text messageText;
        private Text endTitle;
        private Text endSummary;
        private Image feedbackFlash;
        private Coroutine messageRoutine;
        private Coroutine flashRoutine;
        private Coroutine pulseRoutine;
        private Font font;

        public void Build(Transform root, GameManager gameManager, Color cyan, Color magenta)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            feedbackFlash = CreateImage(root, "Screen Feedback", new Color(1f, 0f, 0.1f, 0f), Vector2.zero, Vector2.one);
            feedbackFlash.raycastTarget = false;

            startPanel = CreatePanel(root, "Start Panel", new Color(0.008f, 0.015f, 0.055f, 0.64f));
            CreateText(startPanel.transform, "CRYSTAL ESCAPE", 82, cyan, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.84f), true);
            CreateText(startPanel.transform, "THE FLOATING RUINS ARE COLLAPSING", 27, Color.white, TextAnchor.MiddleCenter, new Vector2(0.12f, 0.56f), new Vector2(0.88f, 0.65f), false);
            CreateText(startPanel.transform, "COLLECT 5 CRYSTALS  •  SURVIVE 3 LIVES  •  REACH THE PORTAL", 23, new Color(0.75f, 0.82f, 0.98f), TextAnchor.MiddleCenter, new Vector2(0.1f, 0.43f), new Vector2(0.9f, 0.55f), false);
            CreateText(startPanel.transform, "WASD  MOVE     MOUSE  LOOK     SPACE  JUMP", 22, new Color(0.65f, 0.72f, 0.88f), TextAnchor.MiddleCenter, new Vector2(0.16f, 0.32f), new Vector2(0.84f, 0.42f), false);
            CreateButton(startPanel.transform, "BEGIN THE ESCAPE", cyan, new Vector2(0.34f, 0.16f), new Vector2(0.66f, 0.27f), gameManager.StartGame);

            hudPanel = new GameObject("HUD Panel");
            hudPanel.transform.SetParent(root, false);
            RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
            Stretch(hudRect, Vector2.zero, Vector2.one);
            CreateImage(hudPanel.transform, "Top HUD Backplate", new Color(0.008f, 0.015f, 0.055f, 0.78f), new Vector2(0f, 0.89f), new Vector2(1f, 1f));
            crystalText = CreateText(hudPanel.transform, "CRYSTALS  0 / 5", 29, cyan, TextAnchor.MiddleLeft, new Vector2(0.025f, 0.91f), new Vector2(0.35f, 0.985f), true);
            timerText = CreateText(hudPanel.transform, "00:00", 28, Color.white, TextAnchor.MiddleCenter, new Vector2(0.42f, 0.92f), new Vector2(0.58f, 0.98f), true);
            healthText = CreateText(hudPanel.transform, "LIVES  3 / 3", 29, Color.white, TextAnchor.MiddleRight, new Vector2(0.65f, 0.91f), new Vector2(0.975f, 0.985f), true);
            portalText = CreateText(hudPanel.transform, "PORTAL LOCKED  •  5 LEFT", 21, new Color(0.68f, 0.5f, 0.9f), TextAnchor.MiddleCenter, new Vector2(0.32f, 0.86f), new Vector2(0.68f, 0.92f), true);
            messageText = CreateText(hudPanel.transform, "", 40, Color.white, TextAnchor.MiddleCenter, new Vector2(0.16f, 0.7f), new Vector2(0.84f, 0.83f), true);
            CreateText(hudPanel.transform, "+", 26, new Color(cyan.r, cyan.g, cyan.b, 0.7f), TextAnchor.MiddleCenter, new Vector2(0.48f, 0.47f), new Vector2(0.52f, 0.53f), true);
            CreateText(hudPanel.transform, "WASD  MOVE   |   SPACE  JUMP   |   ESC  RELEASE MOUSE", 18, new Color(0.62f, 0.68f, 0.83f), TextAnchor.MiddleCenter, new Vector2(0.18f, 0.01f), new Vector2(0.82f, 0.065f), false);

            endPanel = CreatePanel(root, "End Panel", new Color(0.008f, 0.015f, 0.055f, 0.88f));
            endTitle = CreateText(endPanel.transform, "YOU ESCAPED!", 78, cyan, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.61f), new Vector2(0.9f, 0.82f), true);
            endSummary = CreateText(endPanel.transform, "", 31, Color.white, TextAnchor.MiddleCenter, new Vector2(0.2f, 0.4f), new Vector2(0.8f, 0.6f), false);
            CreateButton(endPanel.transform, "RUN AGAIN", cyan, new Vector2(0.35f, 0.23f), new Vector2(0.65f, 0.34f), gameManager.RestartGame);
            CreateButton(endPanel.transform, "BACK TO TITLE", new Color(0.3f, 0.28f, 0.46f), new Vector2(0.4f, 0.11f), new Vector2(0.6f, 0.19f), gameManager.RestartGame);

            startPanel.SetActive(true);
            hudPanel.SetActive(false);
            endPanel.SetActive(false);
            feedbackFlash.transform.SetAsLastSibling();
        }

        public void ShowReady()
        {
            startPanel.SetActive(true);
            hudPanel.SetActive(false);
            endPanel.SetActive(false);
        }

        public void ShowPlaying()
        {
            startPanel.SetActive(false);
            hudPanel.SetActive(true);
            endPanel.SetActive(false);
        }

        public void ShowEnd(bool won, int crystals, int required, float elapsed)
        {
            startPanel.SetActive(false);
            hudPanel.SetActive(false);
            endPanel.SetActive(true);
            endTitle.text = won ? "YOU ESCAPED!" : "LOST TO THE VOID";
            endTitle.color = won ? new Color(0.145f, 0.91f, 1f) : new Color(1f, 0.145f, 0.36f);
            TimeSpan time = TimeSpan.FromSeconds(elapsed);
            endSummary.text = "CRYSTALS  " + crystals + " / " + required + "\nESCAPE TIME  " + time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
        }

        public void UpdateHud(int crystals, int required, int health, int maximumHealth)
        {
            crystalText.text = "CRYSTALS  " + crystals + " / " + required;
            healthText.text = "LIVES  " + health + " / " + maximumHealth;
            int remaining = required - crystals;
            portalText.text = remaining > 0 ? "PORTAL LOCKED  •  " + remaining + " LEFT" : "PORTAL READY  •  RUN!";
            portalText.color = remaining > 0 ? new Color(0.68f, 0.5f, 0.9f) : new Color(0.145f, 0.91f, 1f);
        }

        public void UpdateTimer(float elapsed)
        {
            TimeSpan time = TimeSpan.FromSeconds(elapsed);
            timerText.text = time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
        }

        public void ShowMessage(string text, float duration)
        {
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(MessageRoutine(text, duration));
        }

        private IEnumerator MessageRoutine(string text, float duration)
        {
            messageText.text = text;
            messageText.transform.localScale = Vector3.one * 1.08f;
            for (float t = 0f; t < 0.16f; t += Time.deltaTime)
            {
                messageText.transform.localScale = Vector3.Lerp(Vector3.one * 1.08f, Vector3.one, t / 0.16f);
                yield return null;
            }
            messageText.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(Mathf.Max(0f, duration - 0.16f));
            messageText.text = string.Empty;
        }

        public void FlashDamage()
        {
            StartFeedbackFlash(new Color(1f, 0.02f, 0.12f), 0.34f, 0.34f);
        }

        public void PulseCrystal()
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseTextRoutine());
            StartFeedbackFlash(new Color(0.1f, 0.9f, 1f), 0.15f, 0.22f);
        }

        private void StartFeedbackFlash(Color color, float peakAlpha, float duration)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(color, peakAlpha, duration));
        }

        private IEnumerator FlashRoutine(Color color, float peakAlpha, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float alpha = Mathf.Sin((t / duration) * Mathf.PI) * peakAlpha;
                feedbackFlash.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            feedbackFlash.color = new Color(color.r, color.g, color.b, 0f);
        }

        private IEnumerator PulseTextRoutine()
        {
            for (float t = 0f; t < 0.28f; t += Time.deltaTime)
            {
                float pulse = 1f + Mathf.Sin((t / 0.28f) * Mathf.PI) * 0.22f;
                crystalText.transform.localScale = Vector3.one * pulse;
                yield return null;
            }
            crystalText.transform.localScale = Vector3.one;
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            return CreateImage(parent, name, color, Vector2.zero, Vector2.one).gameObject;
        }

        private Image CreateImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(Transform parent, string value, int size, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, bool bold)
        {
            GameObject go = new GameObject("Text - " + value);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 11;
            text.resizeTextMaxSize = size;
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0.05f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private void CreateButton(Transform parent, string label, Color color, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            Image image = CreateImage(parent, "Button - " + label, color, anchorMin, anchorMax);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colors.pressedColor = new Color(0.72f, 0.78f, 0.88f);
            button.colors = colors;
            button.onClick.AddListener(action);
            CreateText(image.transform, label, 28, new Color(0.015f, 0.025f, 0.08f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, true);
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
