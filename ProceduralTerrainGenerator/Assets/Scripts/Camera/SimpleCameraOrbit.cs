using UnityEngine;
using UnityEngine.EventSystems; // Потрібно для перевірки UI

public class FreeLookCamer : MonoBehaviour
{
    [Header("Dependencies")]
    public TerrainGenerator terrainGenerator; // Посилання на ваш генератор ландшафту

    [Header("Pan Settings")]
    public float panSpeed = 1f; // Швидкість переміщення ландшафту

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeedMultiplier = 2f;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;
    public float smoothRotationTime = 0.05f;
    public bool invertY = false;

    [Header("Zoom Settings")]
    public float zoomSensitivity = 10f;
    public float minFov = 15f; // Мінімальний Field of View (більше зуму)
    public float maxFov = 90f; // Максимальний Field of View (менше зуму)

    // Приватні змінні для обертання
    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float rotationVelocityX = 0f;
    private float rotationVelocityY = 0f;

    void Start()
    {
        // Курсор розблокований за замовчуванням для роботи з UI
        UnlockCursor();

        // Ініціалізуємо початкові кути на основі поточної орієнтації камери
        Vector3 currentEuler = transform.eulerAngles;
        rotationY = currentEuler.y;
        rotationX = currentEuler.x;
        currentRotationY = rotationY;
        currentRotationX = rotationX;
    }

    void Update()
    {
        // Якщо курсор над UI, нічого не робимо.
        // Це дозволяє безперешкодно натискати кнопки та вводити текст.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // --- 1. Панорамування ландшафту (Ліва кнопка миші) ---
        if (Input.GetMouseButton(0))
        {
            // Перевіряємо, чи є посилання на генератор
            if (terrainGenerator != null)
            {
                // Змінюємо зміщення (offset) на основі руху миші
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                // Змінюємо offsetX/Y і одразу перегенеровуємо ландшафт
                terrainGenerator.offsetX -= mouseX * panSpeed;
                terrainGenerator.offsetY -= mouseY * panSpeed;
                terrainGenerator.GenerateTerrain();
            }
        }

        // --- 2. Обертання та переміщення камери (Права кнопка миші) ---
        HandleCameraLookAndMove();

        // --- 3. Керування масштабуванням (зумом) через колесо миші ---
        HandleZoom();

        // --- Додатково: вихід з режиму обертання клавішею Escape ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    private void HandleCameraLookAndMove()
    {
        // Коли натискаємо праву кнопку, блокуємо курсор для обертання
        if (Input.GetMouseButtonDown(1))
        {
            LockCursor();
        }

        // Поки права кнопка затиснута, обертаємо та рухаємо камеру
        if (Input.GetMouseButton(1))
        {
            // --- Керування обертанням ---
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            rotationY += mouseX;
            rotationX += (invertY ? mouseY : -mouseY);
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            currentRotationY = Mathf.SmoothDampAngle(currentRotationY, rotationY, ref rotationVelocityY, smoothRotationTime);
            currentRotationX = Mathf.SmoothDampAngle(currentRotationX, rotationX, ref rotationVelocityX, smoothRotationTime);

            transform.rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);

            // --- Керування переміщенням (WASDQE) ---
            float currentMoveSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentMoveSpeed *= sprintSpeedMultiplier;
            }

            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
            if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
            if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
            if (Input.GetKey(KeyCode.E)) moveDirection += transform.up;
            if (Input.GetKey(KeyCode.Q)) moveDirection -= transform.up;

            transform.position += moveDirection.normalized * currentMoveSpeed * Time.deltaTime;
        }

        // Коли відпускаємо праву кнопку, розблоковуємо курсор
        if (Input.GetMouseButtonUp(1))
        {
            UnlockCursor();
        }
    }

    private void HandleZoom()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            Camera camera = Camera.main;
            camera.fieldOfView -= scrollWheel * zoomSensitivity;
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, minFov, maxFov);
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}