using UnityEngine;

public class PuntoSuministro : MonoBehaviour, IInteractable
{
    [Header("Tipo de Suministro")]
    [Tooltip("Si está activo, usa la lógica del minijuego de cerveza. Si está desactivado, entrega directamente el item (Heladera).")]
    [SerializeField] private bool esBarra = true;

    [Header("Referencias de Canilla (Solo si esBarra = true)")]
    [SerializeField] private ServicioCerveza servicioCerveza;

    [Header("Referencias de Heladera/Estación (Solo si esBarra = false)")]
    [SerializeField] private ItemSO itemAEntregar; // Arrastrar acá el ItemSO_Honey
    [SerializeField] private AudioSource sonidoSuministro;

    private void Awake()
    {
        if (esBarra && servicioCerveza == null)
            servicioCerveza = GetComponentInChildren<ServicioCerveza>();
    }

    public bool CanInteract()
    {
        // ==========================================
        // CASO 1: ES LA BARRA (Canilla Cerveza)
        // ==========================================
        if (esBarra)
        {
            if (servicioCerveza == null) return false;

            // 1. Si ya hay un vaso en la canilla (listo para servir)
            if (servicioCerveza.VasoEnCanilla)
                return true;

            // 2. Si el jugador tiene el vaso vacío en la mano
            if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
            {
                ItemSO itemEnMano = ControladorMano3D.Instance.ObtenerItemActual();
                
                if (itemEnMano != null && servicioCerveza.ItemVasoVacio != null)
                {
                    if (itemEnMano == servicioCerveza.ItemVasoVacio || 
                        itemEnMano.nombreItem == servicioCerveza.ItemVasoVacio.nombreItem)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // ==========================================
        // CASO 2: ES LA HELADERA (Honey / Directo)
        // ==========================================
        
        // Se puede interactuar únicamente si la mano está libre para agarrar el item
        if (ControladorMano3D.Instance != null && !ControladorMano3D.Instance.TieneManoOcupada())
        {
            return true;
        }

        return false;
    }

    public string GetDescription()
    {
        if (esBarra)
        {
            if (servicioCerveza != null && servicioCerveza.VasoEnCanilla)
            {
                return "Presiona [E] para servir cerveza";
            }

            return "Presiona [E] para colocar el vaso";
        }

        return itemAEntregar != null 
            ? $"Presiona [E] para agarrar {itemAEntregar.nombreItem}" 
            : "Presiona [E] para interactuar";
    }

    public void Interact()
    {
        // ==========================================
        // CASO 1: ES LA BARRA
        // ==========================================
        if (esBarra)
        {
            if (servicioCerveza == null) return;

            // Ya hay un vaso abajo -> Servimos
            if (servicioCerveza.VasoEnCanilla)
            {
                servicioCerveza.Servir();
                return;
            }

            // Tenemos el vaso en la mano -> Lo colocamos
            if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
            {
                servicioCerveza.ColocarVaso();
            }

            return;
        }

        // ==========================================
        // CASO 2: ES LA HELADERA
        // ==========================================
        if (ControladorMano3D.Instance != null && !ControladorMano3D.Instance.TieneManoOcupada())
        {
            if (sonidoSuministro != null)
            {
                sonidoSuministro.Play();
            }

            if (itemAEntregar != null)
            {
                ControladorMano3D.Instance.EquiparItem(itemAEntregar);
            }
        }
    }
}