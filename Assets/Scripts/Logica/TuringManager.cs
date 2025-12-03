using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TuringManager : MonoBehaviour
{
    public List<Belt> todasLasCintas = new List<Belt>();
    public Belt cintaActiva;

    [Header("Recursos")]
    public GameObject cajaPrefab;
    [Tooltip("Arraste aquí un Cubo o Cilindro para que actúe como pistón")]
    public GameObject paloPrefab;

    [Tooltip("Índice de la cinta donde empieza el cabezal (ej: 18)")]
    public int indiceCabezalInicial = 18;

    private bool isGlobalMoving = false;
    public float cooldownMovimiento = 1.2f;

    public enum TipoOperacion { Suma, Resta }

    [Header("Estado de la Máquina")]
    public TipoOperacion operacionSeleccionada;
    public string estadoActual = "Q0";
    public bool maquinaCorriendo = false;

    private Dictionary<(string, char), (string, char, char)> tablaDeReglas;

    private void Awake()
    {
        var cintasEncontradas = FindObjectsOfType<Belt>();
        todasLasCintas = cintasEncontradas.OrderBy(c => c.transform.position.x).ToList();
        AsignarCabezal(indiceCabezalInicial);
    }

    public void BotonIniciarSuma()
    {
        DetenerYReiniciar();
        operacionSeleccionada = TipoOperacion.Suma;
        EjecutarMaquina();
    }

    public void BotonIniciarResta()
    {
        DetenerYReiniciar();
        operacionSeleccionada = TipoOperacion.Resta;
        EjecutarMaquina();
    }

    public void BotonDetenerMaquina()
    {
        DetenerYReiniciar();
    }

    private void DetenerYReiniciar()
    {
        maquinaCorriendo = false;
        StopAllCoroutines();
        isGlobalMoving = false;
        estadoActual = "Q0";
    }

    private void EjecutarMaquina()
    {
        InicializarReglas();
        maquinaCorriendo = true;
        StartCoroutine(CicloDeEjecucion());
    }

    private IEnumerator CicloDeEjecucion()
    {
        while (maquinaCorriendo)
        {
            char simboloLeido = LeerSimbolo();

            if (tablaDeReglas.TryGetValue((estadoActual, simboloLeido), out var accion))
            {
                string nuevoEstado = accion.Item1;
                char simboloEscribir = accion.Item2;
                char movimiento = accion.Item3;

                if (simboloLeido != simboloEscribir)
                {
                    EscribirSimbolo(simboloEscribir);
                    yield return new WaitForSeconds(2.0f);
                }

                if (movimiento == 'R') MoverCintaHaciaIzquierda();
                else if (movimiento == 'L') MoverCintaHaciaDerecha();

                estadoActual = nuevoEstado;

                yield return new WaitForSeconds(cooldownMovimiento);

                if (cintaActiva == null)
                {
                    maquinaCorriendo = false;
                }
            }
            else
            {
                maquinaCorriendo = false;
            }
        }
    }

    private void InicializarReglas()
    {
        tablaDeReglas = new Dictionary<(string, char), (string, char, char)>();

        if (operacionSeleccionada == TipoOperacion.Suma)
        {
            tablaDeReglas.Add(("Q0", '0'), ("Q0", '0', 'R'));
            tablaDeReglas.Add(("Q0", '1'), ("Q4", '1', 'R'));
            tablaDeReglas.Add(("Q1", '0'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q1", '1'), ("Q2", '1', 'L'));
            tablaDeReglas.Add(("Q2", '0'), ("Q3", '1', 'R'));
            tablaDeReglas.Add(("Q2", '1'), ("Q2", '1', 'L'));
            tablaDeReglas.Add(("Q3", '1'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q3", '0'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q4", '0'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q4", '1'), ("Q4", '1', 'R'));
        }
        else if (operacionSeleccionada == TipoOperacion.Resta)
        {
            tablaDeReglas.Add(("Q0", '0'), ("Q0", '0', 'R'));
            tablaDeReglas.Add(("Q0", '1'), ("Q3", '1', 'R'));
            tablaDeReglas.Add(("Q1", '0'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q1", '1'), ("Q2", '0', 'L'));
            tablaDeReglas.Add(("Q2", '0'), ("Q2", '0', 'L'));
            tablaDeReglas.Add(("Q2", '1'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q3", '0'), ("Q1", '0', 'R'));
            tablaDeReglas.Add(("Q3", '1'), ("Q3", '1', 'R'));
        }
    }

    public void AsignarCabezal(int indice)
    {
        if (indice >= 0 && indice < todasLasCintas.Count)
            cintaActiva = todasLasCintas[indice];
    }

    public char LeerSimbolo()
    {
        if (cintaActiva == null) return 'E';
        return (cintaActiva.beltItem != null) ? '1' : '0';
    }

    public void EscribirSimbolo(char simbolo)
    {
        if (cintaActiva == null) return;
        if (simbolo == '1' && cintaActiva.beltItem == null)
        {
            StartCoroutine(AnimarInsercion());
        }
        else if (simbolo == '0' && cintaActiva.beltItem != null)
        {
            DestruirCajaFisica();
        }
    }

    private IEnumerator AnimarInsercion()
    {
        if (cajaPrefab == null) yield break;

        float duracion = 1.5f;

        Vector3 direccionEmpuje = cintaActiva.transform.right;

        Vector3 posCentroCinta = cintaActiva.GetSurfacePosition();
        Quaternion rotCinta = cintaActiva.transform.rotation;

        Vector3 posInicialCaja = posCentroCinta - (direccionEmpuje * 15.0f);
        GameObject nuevaCaja = Instantiate(cajaPrefab, posInicialCaja, rotCinta);

        BeltItem itemScript = nuevaCaja.GetComponent<BeltItem>();
        if (itemScript != null)
        {
            cintaActiva.beltItem = itemScript;
            cintaActiva.isSpaceTaken = true;
        }

        Vector3 posInicialPalo = posInicialCaja - (direccionEmpuje * 5.0f);
        posInicialPalo += Vector3.up * 2.0f;

        GameObject palo = Instantiate(paloPrefab, posInicialPalo, Quaternion.identity);
        palo.transform.localScale = new Vector3(10f, 10f, 10f);

        Quaternion rotPalo = Quaternion.LookRotation(direccionEmpuje);
        palo.transform.rotation = rotPalo;

        Vector3 posFinalPalo = posCentroCinta - (direccionEmpuje * 3.0f) + (Vector3.up * 2.0f);
        Vector3 posFinalCaja = posCentroCinta;

        float tiempo = 0;
        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            float tSmooth = t * t * (3f - 2f * t);

            palo.transform.position = Vector3.Lerp(posInicialPalo, posFinalPalo, tSmooth);

            if (nuevaCaja != null)
            {
                nuevaCaja.transform.position = Vector3.Lerp(posInicialCaja, posFinalCaja, tSmooth);
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        if (nuevaCaja != null) nuevaCaja.transform.position = posFinalCaja;
        if (palo != null) Destroy(palo);
    }

    private void DestruirCajaFisica()
    {
        if (cintaActiva.beltItem != null)
        {
            GameObject cajaVisual = null;
            if (cintaActiva.beltItem.item != null) cajaVisual = cintaActiva.beltItem.item;

            cintaActiva.beltItem = null;
            cintaActiva.isSpaceTaken = false;

            if (cajaVisual != null)
            {
                if (paloPrefab != null)
                {
                    StartCoroutine(AnimarEmpujeSalida(cajaVisual));
                }
                else
                {
                    Destroy(cajaVisual);
                }
            }
        }
    }

    private IEnumerator AnimarEmpujeSalida(GameObject caja)
    {
        float duracion = 1.5f;
        Vector3 direccionSalida = -cintaActiva.transform.right;

        Vector3 pivotSuperior = caja.transform.position + (Vector3.up * 12.0f);

        GameObject palo = Instantiate(paloPrefab, caja.transform.position, Quaternion.identity);
        palo.transform.localScale = new Vector3(10f, 10f, 10f);

        Quaternion rotFinal = Quaternion.LookRotation(direccionSalida);
        Quaternion rotInicial = Quaternion.LookRotation(Vector3.down);

        float halfHeight = 10.0f;

        Vector3 posInicialCaja = caja.transform.position;
        Vector3 posFinalCaja = posInicialCaja + (direccionSalida * 15.0f);

        float tiempo = 0;
        while (tiempo < duracion)
        {
            if (caja == null) break;

            float t = tiempo / duracion;
            float tSmooth = Mathf.Sin(t * Mathf.PI * 0.5f);

            Quaternion rotActual = Quaternion.Slerp(rotInicial, rotFinal, tSmooth);
            palo.transform.rotation = rotActual;
            palo.transform.position = pivotSuperior - (rotActual * Vector3.up * halfHeight);

            if (tSmooth > 0.3f)
            {
                float tCaja = (tSmooth - 0.3f) / 0.7f;
                caja.transform.position = Vector3.Lerp(posInicialCaja, posFinalCaja, tCaja);
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        if (caja != null) Destroy(caja);
        if (palo != null) Destroy(palo);
    }

    public void MoverCintaHaciaIzquierda() { if (!isGlobalMoving) StartCoroutine(ProcesoMoverIzquierda()); }
    public void MoverCintaHaciaDerecha() { if (!isGlobalMoving) StartCoroutine(ProcesoMoverDerecha()); }

    private IEnumerator ProcesoMoverIzquierda()
    {
        isGlobalMoving = true;
        for (int i = 0; i < todasLasCintas.Count; i++)
        {
            Belt actual = todasLasCintas[i];
            Belt destino = (i > 0) ? todasLasCintas[i - 1] : null;
            if (actual.beltItem != null) actual.StartCoroutine(actual.MoverCajaHacia(destino));
        }
        yield return new WaitForSeconds(cooldownMovimiento);
        isGlobalMoving = false;
    }

    private IEnumerator ProcesoMoverDerecha()
    {
        isGlobalMoving = true;
        for (int i = todasLasCintas.Count - 1; i >= 0; i--)
        {
            Belt actual = todasLasCintas[i];
            Belt destino = (i < todasLasCintas.Count - 1) ? todasLasCintas[i + 1] : null;
            if (actual.beltItem != null) actual.StartCoroutine(actual.MoverCajaHacia(destino));
        }
        yield return new WaitForSeconds(cooldownMovimiento);
        isGlobalMoving = false;
    }

    public void BotonEscribirUno() => EscribirSimbolo('1');
    public void BotonEscribirCero() => EscribirSimbolo('0');
}