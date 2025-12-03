using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public TuringManager turingManager;
    public GameObject cajaPrefab;

    public TMP_InputField inputValorA;
    public TMP_InputField inputValorB;

    private const int MAX_CAJAS = 9;

    public void CargarDatosEnCinta()
    {
        if (turingManager == null || cajaPrefab == null || inputValorA == null || inputValorB == null)
        {
            return;
        }

        LimpiarCintas();

        string unarioA = ConvertirDecimalAUnario(inputValorA.text);
        string unarioB = ConvertirDecimalAUnario(inputValorB.text);

        if (unarioA == "ERROR" || unarioB == "ERROR")
        {
            return;
        }

        string datosParaCinta = unarioA + "0" + unarioB;

        int indiceCinta = turingManager.indiceCabezalInicial;

        foreach (char simbolo in datosParaCinta)
        {
            if (indiceCinta >= turingManager.todasLasCintas.Count)
            {
                break;
            }

            Belt cintaObjetivo = turingManager.todasLasCintas[indiceCinta];

            if (simbolo == '1')
            {
                CrearCajaEn(cintaObjetivo);
            }

            indiceCinta++;
        }

        if (turingManager.todasLasCintas.Count > turingManager.indiceCabezalInicial)
        {
            turingManager.cintaActiva = turingManager.todasLasCintas[turingManager.indiceCabezalInicial];
        }
    }

    private string ConvertirDecimalAUnario(string textoNumero)
    {
        if (int.TryParse(textoNumero, out int numero) && numero >= 0)
        {
            if (numero > MAX_CAJAS)
            {
                numero = MAX_CAJAS;
            }

            return new string('1', numero);
        }
        return "ERROR";
    }

    private void CrearCajaEn(Belt cinta)
    {
        Vector3 pos = cinta.GetSurfacePosition();
        Quaternion rot = cinta.transform.rotation;

        GameObject nuevaCaja = Instantiate(cajaPrefab, pos, rot);

        BeltItem itemScript = nuevaCaja.GetComponent<BeltItem>();
        if (itemScript != null)
        {
            cinta.beltItem = itemScript;
            cinta.isSpaceTaken = true;
        }
    }

    private void LimpiarCintas()
    {
        foreach (var cinta in turingManager.todasLasCintas)
        {
            if (cinta.beltItem != null)
            {
                if (cinta.beltItem.item != null)
                    Destroy(cinta.beltItem.item);

                cinta.beltItem = null;
                cinta.isSpaceTaken = false;
            }
        }
    }
}