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

    public static class CrystalEscapeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (UnityEngine.Object.FindObjectOfType<CrystalEscapeRuntime>() != null)
            {
                return;
            }

            new GameObject("Crystal Escape Runtime").AddComponent<CrystalEscapeRuntime>();
        }
    }

    public sealed class CrystalEscapeRuntime : MonoBehaviour
    {
        private readonly Color midnight = new Color(0.025f, 0.035f, 0.09f);
        private readonly Color cyan = new Color(0.1f, 0.9f, 1f);
        private readonly Color magenta = new Color(1f, 0.12f, 0.5f);
        private readonly Color violet = new Color(0.35f, 0.12f, 0.72f);

        private Material groundMaterial;
        private Material wallMaterial;
        private Material crystalMaterial;
        private Material hazardMaterial;
        private Material portalMaterial;
        private Material playerMaterial;

        private void Awake()
        {
            Application.targetFrameRate = 120;
            BuildMaterials();
            ConfigureWorld();

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
            groundMaterial = CreateMaterial("Ground", new Color(0.05f, 0.07f, 0.16f), 0.2f, 0.35f);
            wallMaterial = CreateMaterial("Walls", new Color(0.14f, 0.08f, 0.28f), 0.25f, 0.5f);
            crystalMaterial = CreateMaterial("Crystal Placeholder", cyan, 0.1f, 0.9f, cyan * 2.1f);
            hazardMaterial = CreateMaterial("Hazard Placeholder", new Color(0.55f, 0.04f, 0.12f), 0.45f, 0.65f, magenta * 1.2f);
            portalMaterial = CreateMaterial("Portal Placeholder", new Color(0.08f, 0.25f, 0.65f), 0.2f, 0.85f, cyan * 1.7f);
            playerMaterial = CreateMaterial("Player", new Color(0.92f, 0.95f, 1f), 0.2f, 0.65f, new Color(0.05f, 0.15f, 0.2f));
        }

        private Material CreateMaterial(string name, Color color, float metallic, float smoothness, Color? emission = null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            return material;
        }

        private void ConfigureWorld()
        {
            RenderSettings.ambientLight = new Color(0.14f, 0.16f, 0.28f);
            Camera existingCamera = Camera.main;
            if (existingCamera != null) Destroy(existingCamera.gameObject);
            Light existingLight = UnityEngine.Object.FindObjectOfType<Light>();
            if (existingLight != null) Destroy(existingLight.gameObject);

            GameObject sunObject = new GameObject("Moon Light");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.55f, 0.65f, 1f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        }

        private PlayerController BuildPlayer(GameManager gameManager)
        {
            GameObject root = new GameObject("Player");
            root.tag = "Player";
            root.transform.position = new Vector3(0f, 1.1f, -9.5f);

            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.GetComponent<Renderer>().material = playerMaterial;
            Destroy(visual.GetComponent<Collider>());

            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eye.name = "Facing Marker";
            eye.transform.SetParent(visual.transform, false);
            eye.transform.localPosition = new Vector3(0f, 0.25f, 0.47f);
            eye.transform.localScale = new Vector3(0.45f, 0.18f, 0.08f);
            eye.GetComponent<Renderer>().material = crystalMaterial;
            Destroy(eye.GetComponent<Collider>());

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 2.35f, -4.8f);
            cameraObject.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = midnight;
            camera.fieldOfView = 66f;
            camera.nearClipPlane = 0.1f;
            cameraObject.AddComponent<AudioListener>();

            PlayerController player = root.AddComponent<PlayerController>();
            player.Initialize(controller, cameraObject.transform, gameManager);
            return player;
        }

        private void BuildArena(GameManager gameManager)
        {
            CreateBlock("Arena Floor", new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 24f), groundMaterial);
            CreateBlock("North Wall", new Vector3(0f, 1.25f, 12f), new Vector3(25f, 3.5f, 1f), wallMaterial);
            CreateBlock("South Wall", new Vector3(0f, 1.25f, -12f), new Vector3(25f, 3.5f, 1f), wallMaterial);
            CreateBlock("East Wall", new Vector3(12f, 1.25f, 0f), new Vector3(1f, 3.5f, 25f), wallMaterial);
            CreateBlock("West Wall", new Vector3(-12f, 1.25f, 0f), new Vector3(1f, 3.5f, 25f), wallMaterial);

            CreateBlock("Center Divider A", new Vector3(-4f, 0.65f, 0.5f), new Vector3(6.5f, 1.3f, 1f), wallMaterial);
            CreateBlock("Center Divider B", new Vector3(4.8f, 0.65f, -2.2f), new Vector3(1f, 1.3f, 7f), wallMaterial);
            CreateBlock("Side Platform", new Vector3(-7.7f, 0.25f, 6.8f), new Vector3(5f, 0.5f, 4f), wallMaterial);

            Vector3[] crystalPositions =
            {
                new Vector3(-8f, 1.1f, -6.8f),
                new Vector3(7.8f, 1.1f, -7.2f),
                new Vector3(-7.5f, 1.85f, 6.8f),
                new Vector3(7.4f, 1.1f, 5.6f),
                new Vector3(0f, 1.1f, 3.2f)
            };
            for (int i = 0; i < crystalPositions.Length; i++)
            {
                BuildCrystal(i + 1, crystalPositions[i], gameManager);
            }

            BuildHazard("Spike Trap 1", new Vector3(-3.5f, 0f, -5.5f), new Vector3(3.4f, 1.1f, 2.1f), gameManager);
            BuildHazard("Spike Trap 2", new Vector3(7.2f, 0f, 0.5f), new Vector3(2.2f, 1.1f, 3.8f), gameManager);
            BuildHazard("Spike Trap 3", new Vector3(-3.2f, 0f, 7.6f), new Vector3(4.5f, 1.1f, 1.8f), gameManager);
            BuildPortal(new Vector3(0f, 0f, 9.3f), gameManager);

            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 10.5f, 0.18f, Mathf.Sin(angle) * 10.5f);
                GameObject lightPillar = CreateBlock("Glow Marker", position, new Vector3(0.16f, 0.36f, 0.16f), crystalMaterial);
                Destroy(lightPillar.GetComponent<Collider>());
            }
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
            if (source == null)
            {
                return null;
            }

            GameObject instance = Instantiate(source, parent, false);
            instance.name = displayName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (Collider importedCollider in instance.GetComponentsInChildren<Collider>(true))
            {
                Destroy(importedCollider);
            }
            foreach (Camera importedCamera in instance.GetComponentsInChildren<Camera>(true))
            {
                Destroy(importedCamera);
            }
            foreach (Light importedLight in instance.GetComponentsInChildren<Light>(true))
            {
                Destroy(importedLight);
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return instance;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float scaleX = desiredSize.x / Mathf.Max(bounds.size.x, 0.001f);
            float scaleY = desiredSize.y / Mathf.Max(bounds.size.y, 0.001f);
            float scaleZ = desiredSize.z / Mathf.Max(bounds.size.z, 0.001f);
            float uniformScale = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));
            instance.transform.localScale = Vector3.one * uniformScale;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 currentCenter = parent.InverseTransformPoint(bounds.center);
            instance.transform.localPosition += desiredCenter - currentCenter;
            return instance;
        }

        private void BuildCrystal(int number, Vector3 position, GameManager gameManager)
        {
            GameObject root = new GameObject("Tripo Crystal Socket " + number);
            root.transform.position = position;
            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.radius = 0.75f;
            trigger.isTrigger = true;

            GameObject visual = TryCreateTripoVisual(
                "Tripo/Crystal/Crystal",
                root.transform,
                "Tripo Crystal - Functional Reward Asset",
                new Vector3(1.15f, 1.7f, 1.15f),
                Vector3.zero);
            if (visual == null)
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "PLACEHOLDER - Add Resources/Tripo/Crystal/Crystal.fbx";
                visual.transform.SetParent(root.transform, false);
                visual.transform.localScale = new Vector3(0.65f, 1.15f, 0.65f);
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                visual.GetComponent<Renderer>().material = crystalMaterial;
                Destroy(visual.GetComponent<Collider>());
            }

            Collectible collectible = root.AddComponent<Collectible>();
            collectible.Initialize(gameManager, visual.transform, number * 0.4f);
        }

        private void BuildHazard(string name, Vector3 position, Vector3 scale, GameManager gameManager)
        {
            GameObject root = new GameObject(name + " - Tripo Spike Socket");
            root.transform.position = position;
            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.35f, 0f);
            trigger.size = new Vector3(scale.x, 0.8f, scale.z);

            GameObject tripoVisual = TryCreateTripoVisual(
                "Tripo/SpikeTrap/SpikeTrap",
                root.transform,
                "Tripo Spike Trap - Functional Damage Asset",
                new Vector3(scale.x * 0.95f, 1f, scale.z * 0.95f),
                new Vector3(0f, 0.45f, 0f));
            if (tripoVisual == null)
            {
                GameObject baseVisual = CreateBlock("PLACEHOLDER - Add Resources/Tripo/SpikeTrap/SpikeTrap.fbx", new Vector3(position.x, 0.12f, position.z), new Vector3(scale.x, 0.24f, scale.z), hazardMaterial);
                baseVisual.transform.SetParent(root.transform, true);
                Destroy(baseVisual.GetComponent<Collider>());

                int spikeCount = Mathf.Max(3, Mathf.RoundToInt(scale.x * scale.z));
                for (int i = 0; i < spikeCount; i++)
                {
                    GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    spike.name = "Spike Visual";
                    spike.transform.SetParent(root.transform, false);
                    float x = ((i % Mathf.Max(1, Mathf.RoundToInt(scale.x))) / Mathf.Max(1f, scale.x - 1f) - 0.5f) * (scale.x - 0.5f);
                    float z = ((i / Mathf.Max(1, Mathf.RoundToInt(scale.x))) / Mathf.Max(1f, scale.z - 1f) - 0.5f) * (scale.z - 0.5f);
                    spike.transform.localPosition = new Vector3(x, 0.45f, z);
                    spike.transform.localScale = new Vector3(0.16f, 0.45f, 0.16f);
                    spike.GetComponent<Renderer>().material = hazardMaterial;
                    Destroy(spike.GetComponent<Collider>());
                }
            }

            Hazard hazard = root.AddComponent<Hazard>();
            hazard.Initialize(gameManager, 1);
        }

        private void BuildPortal(Vector3 position, GameManager gameManager)
        {
            GameObject root = new GameObject("Exit Portal - Tripo Portal Socket");
            root.transform.position = position;
            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.7f, 0f);
            trigger.size = new Vector3(3.4f, 3.6f, 1.2f);

            GameObject visualRoot = new GameObject("Portal Visuals");
            visualRoot.transform.SetParent(root.transform, false);
            GameObject tripoVisual = TryCreateTripoVisual(
                "Tripo/ExitPortal/ExitPortal",
                visualRoot.transform,
                "Tripo Exit Portal - Functional End Asset",
                new Vector3(3.4f, 3.5f, 1.1f),
                new Vector3(0f, 1.75f, 0f));
            if (tripoVisual == null)
            {
                visualRoot.name = "PLACEHOLDER - Add Resources/Tripo/ExitPortal/ExitPortal.fbx";
                for (int i = 0; i < 14; i++)
                {
                    float angle = i * Mathf.PI * 2f / 14f;
                    GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segment.name = "Portal Ring Segment";
                    segment.transform.SetParent(visualRoot.transform, false);
                    segment.transform.localPosition = new Vector3(Mathf.Cos(angle) * 1.7f, 1.75f + Mathf.Sin(angle) * 1.7f, 0f);
                    segment.transform.localRotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
                    segment.transform.localScale = new Vector3(0.55f, 0.22f, 0.5f);
                    segment.GetComponent<Renderer>().material = portalMaterial;
                    Destroy(segment.GetComponent<Collider>());
                }
            }

            GameObject energy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            energy.name = "Portal Energy";
            energy.transform.SetParent(visualRoot.transform, false);
            energy.transform.localPosition = new Vector3(0f, 1.75f, 0.15f);
            energy.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            energy.transform.localScale = new Vector3(1.35f, 0.05f, 1.35f);
            energy.GetComponent<Renderer>().material = crystalMaterial;
            Destroy(energy.GetComponent<Collider>());

            ExitPortal portal = root.AddComponent<ExitPortal>();
            portal.Initialize(gameManager, energy.transform);
        }

        private GameUI BuildUI(GameManager gameManager)
        {
            GameObject canvasObject = new GameObject("Game UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
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

        public void StartGame()
        {
            State = GameState.Playing;
            startTime = Time.time;
            player.SetControlEnabled(true);
            ui.ShowPlaying();
            audioManager.PlayMusic();
        }

        public void CollectCrystal()
        {
            if (State != GameState.Playing) return;
            Crystals = Mathf.Min(Crystals + 1, RequiredCrystals);
            audioManager.PlayPickup();
            ui.UpdateHud(Crystals, RequiredCrystals, Health, MaximumHealth);
            ui.ShowMessage(Crystals >= RequiredCrystals ? "PORTAL UNLOCKED!" : "+1 CRYSTAL", 1.2f);
        }

        public void TakeDamage(int amount)
        {
            if (State != GameState.Playing) return;
            Health = Mathf.Max(0, Health - amount);
            audioManager.PlayHurt();
            ui.UpdateHud(Crystals, RequiredCrystals, Health, MaximumHealth);
            ui.FlashDamage();
            if (Health <= 0)
            {
                Finish(false);
            }
        }

        public void TryExit()
        {
            if (State != GameState.Playing) return;
            if (Crystals < RequiredCrystals)
            {
                int remaining = RequiredCrystals - Crystals;
                ui.ShowMessage("COLLECT " + remaining + " MORE CRYSTAL" + (remaining == 1 ? "" : "S"), 1.5f);
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
            if (activeScene.buildIndex >= 0)
            {
                SceneManager.LoadScene(activeScene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(activeScene.name);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController controller;
        private Transform cameraTransform;
        private GameManager gameManager;
        private float verticalVelocity;
        private float pitch = 12f;
        private float nextDamageTime;
        private bool controlEnabled;

        private const float MoveSpeed = 6.4f;
        private const float JumpHeight = 1.5f;
        private const float Gravity = -24f;
        private const float MouseSensitivity = 2.2f;

        public void Initialize(CharacterController characterController, Transform playerCamera, GameManager manager)
        {
            controller = characterController;
            cameraTransform = playerCamera;
            gameManager = manager;
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
            if (!controlEnabled) return;

            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
            pitch = Mathf.Clamp(pitch - mouseY, -25f, 55f);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            Vector3 movement = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
            movement = Vector3.ClampMagnitude(movement, 1f) * MoveSpeed;

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (controller.isGrounded && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }
            verticalVelocity += Gravity * Time.deltaTime;
            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);

            if (transform.position.y < -5f)
            {
                transform.position = new Vector3(0f, 1.1f, -9.5f);
                verticalVelocity = 0f;
                TryTakeDamage(1);
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

        public void TryTakeDamage(int amount)
        {
            if (Time.time < nextDamageTime || gameManager.State != GameState.Playing) return;
            nextDamageTime = Time.time + 1f;
            gameManager.TakeDamage(amount);
        }
    }

    public sealed class Collectible : MonoBehaviour
    {
        private GameManager gameManager;
        private Transform visual;
        private float phase;
        private Vector3 baseLocalPosition;
        private bool collected;

        public void Initialize(GameManager manager, Transform visualTransform, float animationPhase)
        {
            gameManager = manager;
            visual = visualTransform;
            phase = animationPhase;
            baseLocalPosition = visual.localPosition;
        }

        private void Update()
        {
            if (visual == null) return;
            visual.Rotate(0f, 75f * Time.deltaTime, 0f, Space.World);
            visual.localPosition = baseLocalPosition + Vector3.up * (Mathf.Sin(Time.time * 2.4f + phase) * 0.16f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player") || gameManager.State != GameState.Playing) return;
            collected = true;
            gameManager.CollectCrystal();
            gameObject.SetActive(false);
        }
    }

    public sealed class Hazard : MonoBehaviour
    {
        private GameManager gameManager;
        private int damage;

        public void Initialize(GameManager manager, int damageAmount)
        {
            gameManager = manager;
            damage = damageAmount;
        }

        private void OnTriggerStay(Collider other)
        {
            if (gameManager.State != GameState.Playing || !other.CompareTag("Player")) return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) player.TryTakeDamage(damage);
        }
    }

    public sealed class ExitPortal : MonoBehaviour
    {
        private GameManager gameManager;
        private Transform energy;

        public void Initialize(GameManager manager, Transform energyVisual)
        {
            gameManager = manager;
            energy = energyVisual;
        }

        private void Update()
        {
            if (energy == null) return;
            energy.Rotate(0f, 0f, 35f * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.04f;
            energy.localScale = new Vector3(1.35f * pulse, 0.05f, 1.35f * pulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) gameManager.TryExit();
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
            musicSource.volume = 0.28f;
            effectsSource.volume = 0.72f;

            music = Resources.Load<AudioClip>("Audio/BGM");
            pickup = Resources.Load<AudioClip>("Audio/Pickup");
            hurt = Resources.Load<AudioClip>("Audio/Hurt");
            win = Resources.Load<AudioClip>("Audio/Win");
            lose = Resources.Load<AudioClip>("Audio/Lose");

            if (music == null) music = CreateTone("Placeholder BGM", 12f, 110f, 0.12f, true);
            if (pickup == null) pickup = CreateSweep("Placeholder Pickup", 0.28f, 620f, 1200f, 0.38f);
            if (hurt == null) hurt = CreateSweep("Placeholder Hurt", 0.34f, 180f, 70f, 0.48f);
            if (win == null) win = CreateSweep("Placeholder Win", 1.2f, 440f, 980f, 0.35f);
            if (lose == null) lose = CreateSweep("Placeholder Lose", 1.0f, 260f, 90f, 0.35f);
        }

        public void PlayMusic()
        {
            musicSource.clip = music;
            musicSource.Play();
        }

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
                if (layered) wave += Mathf.Sin(t * frequency * 1.5f * Mathf.PI * 2f) * 0.45f;
                float rhythm = 0.65f + Mathf.Sin(t * Mathf.PI * 2f * 0.5f) * 0.15f;
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
        private Text messageText;
        private Text endTitle;
        private Text endSummary;
        private Image damageFlash;
        private Coroutine messageRoutine;
        private Coroutine flashRoutine;
        private Font font;

        public void Build(Transform root, GameManager gameManager, Color cyan, Color magenta)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            damageFlash = CreateImage(root, "Damage Flash", new Color(1f, 0f, 0.1f, 0f), Vector2.zero, Vector2.one);
            damageFlash.raycastTarget = false;

            startPanel = CreatePanel(root, "Start Panel", new Color(0.015f, 0.02f, 0.07f, 0.94f));
            CreateText(startPanel.transform, "CRYSTAL ESCAPE", 74, cyan, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.63f), new Vector2(0.9f, 0.83f));
            CreateText(startPanel.transform, "FIVE CRYSTALS. THREE LIVES. ONE WAY OUT.", 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.12f, 0.53f), new Vector2(0.88f, 0.64f));
            CreateText(startPanel.transform, "WASD  MOVE     MOUSE  LOOK     SPACE  JUMP\nCollect all crystals, avoid the traps, then enter the portal.", 25, new Color(0.78f, 0.82f, 0.95f), TextAnchor.MiddleCenter, new Vector2(0.14f, 0.33f), new Vector2(0.86f, 0.52f));
            CreateButton(startPanel.transform, "START RUN", cyan, new Vector2(0.36f, 0.16f), new Vector2(0.64f, 0.27f), gameManager.StartGame);

            hudPanel = new GameObject("HUD Panel");
            hudPanel.transform.SetParent(root, false);
            RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
            Stretch(hudRect, Vector2.zero, Vector2.one);
            crystalText = CreateText(hudPanel.transform, "CRYSTALS  0 / 5", 30, cyan, TextAnchor.MiddleLeft, new Vector2(0.025f, 0.91f), new Vector2(0.36f, 0.985f));
            healthText = CreateText(hudPanel.transform, "HEALTH  3 / 3", 30, Color.white, TextAnchor.MiddleRight, new Vector2(0.64f, 0.91f), new Vector2(0.975f, 0.985f));
            messageText = CreateText(hudPanel.transform, "", 38, Color.white, TextAnchor.MiddleCenter, new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.83f));
            CreateText(hudPanel.transform, "WASD  MOVE   |   SPACE  JUMP   |   ESC  RELEASE MOUSE", 20, new Color(0.68f, 0.72f, 0.85f), TextAnchor.MiddleCenter, new Vector2(0.2f, 0.01f), new Vector2(0.8f, 0.07f));

            endPanel = CreatePanel(root, "End Panel", new Color(0.015f, 0.02f, 0.07f, 0.94f));
            endTitle = CreateText(endPanel.transform, "YOU ESCAPED!", 72, cyan, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.59f), new Vector2(0.9f, 0.8f));
            endSummary = CreateText(endPanel.transform, "", 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.2f, 0.42f), new Vector2(0.8f, 0.59f));
            CreateButton(endPanel.transform, "PLAY AGAIN", cyan, new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.36f), gameManager.RestartGame);
            CreateButton(endPanel.transform, "QUIT", new Color(0.28f, 0.3f, 0.42f), new Vector2(0.42f, 0.13f), new Vector2(0.58f, 0.21f), gameManager.QuitGame);

            startPanel.SetActive(true);
            hudPanel.SetActive(false);
            endPanel.SetActive(false);
            damageFlash.transform.SetAsLastSibling();
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
            endTitle.text = won ? "YOU ESCAPED!" : "GAME OVER";
            endTitle.color = won ? new Color(0.1f, 0.9f, 1f) : new Color(1f, 0.12f, 0.5f);
            TimeSpan time = TimeSpan.FromSeconds(elapsed);
            endSummary.text = "CRYSTALS  " + crystals + " / " + required + "\nTIME  " + time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
        }

        public void UpdateHud(int crystals, int required, int health, int maximumHealth)
        {
            crystalText.text = "CRYSTALS  " + crystals + " / " + required;
            healthText.text = "HEALTH  " + health + " / " + maximumHealth;
        }

        public void ShowMessage(string text, float duration)
        {
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(MessageRoutine(text, duration));
        }

        private IEnumerator MessageRoutine(string text, float duration)
        {
            messageText.text = text;
            yield return new WaitForSeconds(duration);
            messageText.text = string.Empty;
        }

        public void FlashDamage()
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float alpha = Mathf.Sin((t / 0.35f) * Mathf.PI) * 0.32f;
                damageFlash.color = new Color(1f, 0f, 0.1f, alpha);
                yield return null;
            }
            damageFlash.color = new Color(1f, 0f, 0.1f, 0f);
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            Image image = CreateImage(parent, name, color, Vector2.zero, Vector2.one);
            return image.gameObject;
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

        private Text CreateText(Transform parent, string value, int size, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
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
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            return text;
        }

        private void CreateButton(Transform parent, string label, Color color, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            Image image = CreateImage(parent, "Button - " + label, color, anchorMin, anchorMax);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            Text text = CreateText(image.transform, label, 30, new Color(0.02f, 0.03f, 0.08f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            text.fontStyle = FontStyle.Bold;
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

