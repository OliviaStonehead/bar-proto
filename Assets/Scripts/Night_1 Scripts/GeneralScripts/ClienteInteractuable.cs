using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Header("Configuración")]
    public string nombreCliente;
    public string dialogoPedido = "Hola, traeme una cerveza.";
    public string dialogoGracias = "Gracias, Lucas. Dejala ahí.";

    [Header("UI")]
    public GameObject indicadorVioleta;

    private Act1Manager manager;

    private void Awake()
    {
        manager = FindObjectOfType<Act1Manager>();
    }

    private void Start()
    {
        // Al iniciar partida, todos los clientes arrancan esperando atención.
        // Esto evita que queden estados raros entre pruebas.
        estadoActual = EstadoCliente.EsperandoAtencion;
    }

    public void Interact()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<Act1Manager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("[ClienteInteractuable] No se encontró Act1Manager en la escena.");
            return;
        }

        switch (estadoActual)
        {
            case EstadoCliente.EsperandoAtencion:
                TomarPedido();
                break;

            case EstadoCliente.EsperandoPedido:
                EntregarPedido();
                break;

            case EstadoCliente.Atendido:
                break;
        }
    }

    void TomarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        // =========================
        // MARIELA
        // =========================
        if (clienteNormalizado == "mariela")
        {
            // Mariela NO puede pedir antes de que Carlos haya sido atendido.
            if (manager.clientesAtendidosTotal < 1)
            {
                manager.MostrarDialogo("Lucas: Primero debería atender al cliente de la barra.");
                Debug.Log("[ClienteInteractuable] Mariela bloqueada porque Carlos todavía no fue atendido.");
                return;
            }

            // Si Carlos ya fue atendido, ahora sí Mariela hace su pedido especial.
            manager.RegistrarPedidoMarielaHoney();

            estadoActual = EstadoCliente.EsperandoPedido;

            Debug.Log("[ClienteInteractuable] Pedido de Mariela tomado.");
            return;
        }

        // =========================
        // CARLOS
        // =========================
        if (clienteNormalizado == "carlos")
        {
            // Si Carlos ya fue atendido, no vuelve a pedir.
            if (manager.clientesAtendidosTotal >= 1)
            {
                manager.MostrarDialogo("Carlos: Gracias, maestro.");
                return;
            }

            // Registramos oficialmente el pedido en Act1Manager
            manager.carlosPidioCerveza = true;

            manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);

            estadoActual = EstadoCliente.EsperandoPedido;

            if (manager.indicadorCervezas != null)
            {
                manager.indicadorCervezas.SetActive(true);
            }

            Debug.Log("[ClienteInteractuable] Carlos pidió cerveza. carlosPidioCerveza = TRUE.");
            return;
        }

        // =========================
        // OTROS CLIENTES
        // =========================
        manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;

        Debug.Log("[ClienteInteractuable] Pedido tomado.");
    }

    void EntregarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        // =========================
    // CARLOS
    // =========================
    if (clienteNormalizado == "carlos")
    {
        if (!manager.carlosPidioCerveza)
        {
            manager.MostrarDialogo("Carlos: Primero dejame pedirte bien...");
            return;
        }

        // Validación crítica con el Manager
        if (!manager.TienePedidoEntregable())
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
            return;
        }

        // Si pasa la validación, entregamos el pedido
        manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
        estadoActual = EstadoCliente.Atendido;

        if (indicadorVioleta != null)
        {
            indicadorVioleta.SetActive(false);
        }

        manager.ClienteCompletado();
        Debug.Log("[ClienteInteractuable] Carlos atendido con éxito.");
        return;
    }

    // =========================
    // MARIELA
    // =========================
    if (clienteNormalizado == "mariela")
    {
        if (manager.clientesAtendidosTotal < 1)
        {
            estadoActual = EstadoCliente.EsperandoAtencion;
            manager.marielaPidioHoney = false;
            manager.MostrarDialogo("Lucas: Primero debería atender al cliente de la barra.");
            return;
        }

        if (!manager.marielaPidioHoney)
        {
            manager.RegistrarPedidoMarielaHoney();
            estadoActual = EstadoCliente.EsperandoPedido;
            return;
        }

        // Validación crítica con el Manager
        if (!manager.TienePedidoEntregable())
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
            return;
        }

        manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
        estadoActual = EstadoCliente.Atendido;

        if (indicadorVioleta != null)
        {
            indicadorVioleta.SetActive(false);
        }

        manager.ClienteCompletado();
        Debug.Log("[ClienteInteractuable] Mariela atendida con éxito.");
        return;
    }

        // =========================
        // OTROS CLIENTES
        // =========================
        if (manager.TienePedidoEntregable())
        {
            manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);

            estadoActual = EstadoCliente.Atendido;

            if (indicadorVioleta != null)
            {
                indicadorVioleta.SetActive(false);
            }

            manager.ClienteCompletado();
        }
        else
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
        }
    }

    public string GetDescription()
    {
        if (estadoActual == EstadoCliente.EsperandoAtencion)
        {
            return "Presiona [E] para tomar pedido";
        }

        if (estadoActual == EstadoCliente.EsperandoPedido)
        {
            return "Presiona [E] para entregar pedido";
        }

        return "";
    }

    public bool CanInteract()
    {
        return estadoActual != EstadoCliente.Atendido;
    }
}

public enum EstadoCliente
{
    EsperandoAtencion,
    EsperandoPedido,
    Atendido
}