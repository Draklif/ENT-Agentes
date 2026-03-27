using System;

using UnityEngine;

public class ClimateEventsManager : MonoBehaviour
{

    //Instancia global para poder llamar esta clase desde otros scripts
    public static ClimateEventsManager Instance { get; private set; } //solo puede asignarle valores desde esta clase pero se puede leer desde cualquier clase fuera

    // Tipos de clima (despejado, lluvia y tormenta)
    public enum ClimateState { Clear, Rain, Storm } //Enumera los climas del 0 al 2 siendo 0 - Despejado, 1 - Lluvia y 2 - tormenta
    public ClimateState CurrentClimate { get; private set; } = ClimateState.Clear; //Inicializa en despejado. Se modifica unicamente desde esta clase, se puede leer desde otras



    //Tiempo que va a durar cada clima
    public float minTime = 5f;
    public float maxTime = 10f;

    public float timer;//Temporizador que va a controlar el tiempo de cuando cambia el clima
   public ClimateState InspectorClimateState; // Muestra clima actual


    // Efectos visuales de los climas para sensación de ejecución (variables)
    public ParticleSystem rainParticles;
    public ParticleSystem stormParticles;
    public UnityEngine.UI.Image lightningImage;
    private Coroutine lightningCoroutine; // Guarda la referencia de la coroutine del rayo para poder detenerla cuando el clima cambie.

    void Awake() //funcion que se inicia cuando se carga la escena
    {
        if (Instance == null) //Si no existe la instancia
        {
            Instance = this; // Se asigna esta instancia a Instance para que cualquier otro
            //script pueda acceder a ella.
        }
        else
        {
            Destroy(gameObject); // Evita tener dos instancias si ya existe una
        }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = UnityEngine.Random.Range(minTime, maxTime);//Inicializa el timer con un valor random de entre 5 a 10 secundios
        SetClimate(ClimateState.Clear); //Esta linea lo que hace es que siempre el clima empezara definido como despejado
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime; //Reduce el temporizador según el tiempo transcurrido

        // Cuando el temporizador  para cambiar al clima llega a 0 el clima va a cambiar
        if (timer <= 0f)
        {
            NextClimate(); //Llama a la funcion de cambio de clima

            // Se reinicia el timer con un valor entre 5 segundos a 10 segundos
            timer = UnityEngine.Random.Range(minTime, maxTime);
        }
    }


    // Función donde se ejecutara la logica del cambio climatico
    void NextClimate()
    {


        int randomClimate = UnityEngine.Random.Range(0, 3); // Nos bota un numero del 0 al 2
        SetClimate((ClimateState)randomClimate); //Segun el numero que sale se escoge el clima al que va a pasar. Llama a la enum del inicio y si sale 0 despejado 1 lluvia y 2 tormenta.
    }

    void SetClimate(ClimateState newClimate)
    {
        CurrentClimate = newClimate; //Aqui se va a guardar el clima que se va a ejecutar. 

        InspectorClimateState = newClimate; // Se actualiza la parte que informa en el inspector el estado del clima

        // Detiene cualquier coroutine de rayo en curso para que no haya rayos en el  clima despejado y lluvia
        if (lightningCoroutine != null)
        {
            StopCoroutine(lightningCoroutine);
            lightningCoroutine = null;
        }



        // Los efectos del clima anterior se borrarar de la pantalla
        if (rainParticles != null) //Si hay lluvia la desactiva
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (stormParticles != null) //Si hay tormenta la desactiva
            stormParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (lightningImage != null)
        {
            lightningImage.color = new Color(1, 1, 1, 0); // hace invisible el rayo con la opacidad a 0
            if (lightningCoroutine != null)
            {
                StopCoroutine(lightningCoroutine); // Detener coroutine anterior
                lightningCoroutine = null;
            }
        }

        // Un switch para activar un nuevo clima
        switch (newClimate)
        {
            case ClimateState.Clear: //Si se recibe de climateState el numero 0, el clima se despeja
                Debug.Log("Clima despejado");
                break;

            case ClimateState.Rain: //Si se recibe de climateState el numero 1, el clima es lluvia
                Debug.Log("Lluvia");
                if (rainParticles != null)
                    rainParticles.Play(); // Salen las particulas de lluvia
                break;

            case ClimateState.Storm:
                Debug.Log("Tormenta");  //Si se recibe de climateState el numero 0, el clima es tormenta
                if (stormParticles != null)
                    stormParticles.Play(); //Hace que salgan las cosas para simular la tormenta

                if (lightningImage != null)
                   lightningCoroutine = StartCoroutine(LightningFlash());  //Activa flashes para simular rayos
                break;
        }


    }


    // Funcion para el efecto de rayo como un parpadeo rapido en pantalla
    System.Collections.IEnumerator LightningFlash() //Crea una funcion que se ejecuta con pausas mientras sigue funcionando la simulación.
    {
        while (CurrentClimate == ClimateState.Storm) //Un while que funciona si el clima es tormenta ----> Seria algo tipo; mientras el clima sea tormenta ejecuta lo de down
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f)); // Está linea lo que nos permite que haya un tiempo tipo como de recarga para que vuelva a salir el efecto, esta configuarada
            //entre 2 a 5 segundos

            if (lightningImage != null) //Verifica que la imagen exista
            {
                lightningImage.color = new Color(1, 1, 1, 0.8f); //Activa el falsh  del rayo haciendolo visible en 0.8 de opacidad
                yield return new WaitForSeconds(0.1f); //Espera 0.1 segundos para que sea muy rapido el efecto tipo un destello
                lightningImage.color = new Color(1, 1, 1, 0); // Desactiva el efecto haciendolo invisible con 0 de opacidad
            }
        }
    }


    // Funcion para reducir la vision del conejo y el depredador segund el clima
    public float GetVisionMultiplier()
    {
        switch (CurrentClimate) //Segun el clima 
        {
            case ClimateState.Rain://Reduce la vision un 20%
                return 0.8f;

            case ClimateState.Storm: //Reduce la visión un 50% 
                return 0.5f;

            default:
                return 1f; //No reduce la vision
        }
    }

}