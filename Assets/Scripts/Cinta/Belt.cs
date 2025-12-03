using System.Collections;
using UnityEngine;

public class Belt : MonoBehaviour
{
    public BeltItem beltItem;
    public bool isSpaceTaken = false;

    private float speed = 30f;
    private float alturaCaja = 5.5f;

    public Vector3 GetSurfacePosition()
    {
        return transform.position + (transform.up * alturaCaja);
    }

    public IEnumerator MoverCajaHacia(Belt cintaDestino)
    {
        if (beltItem == null || beltItem.item == null) yield break;

        BeltItem itemMoviendose = this.beltItem;
        Transform cajaTransform = itemMoviendose.item.transform;

        this.beltItem = null;
        this.isSpaceTaken = false;

        if (cintaDestino == null)
        {
            Destroy(itemMoviendose.item);
            yield break;
        }

        cintaDestino.beltItem = itemMoviendose;
        cintaDestino.isSpaceTaken = true;

        Vector3 posicionDestino = cintaDestino.GetSurfacePosition();
        Quaternion rotacionDestino = cintaDestino.transform.rotation;

        while (cajaTransform != null && Vector3.Distance(cajaTransform.position, posicionDestino) > 0.05f)
        {
            cajaTransform.position = Vector3.MoveTowards(cajaTransform.position, posicionDestino, speed * Time.deltaTime);
            cajaTransform.rotation = Quaternion.Slerp(cajaTransform.rotation, rotacionDestino, speed * Time.deltaTime);
            yield return null;
        }

        if (cajaTransform != null)
        {
            cajaTransform.position = posicionDestino;
            cajaTransform.rotation = rotacionDestino;
        }
    }
}