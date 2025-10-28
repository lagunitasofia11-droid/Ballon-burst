using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    
    // Sección de referencias a los jugadores
    
    [Header("Jugador 1 (Lu)")]
    public Lu player1_lu1;   
    public Lu player1_lu2;  

    [Header("Jugador 2 (Bowser)")]
    public Bowser player2_bowser1; 
    public Bowser player2_bowser2; 

   
    // Configuración de tiempo del juego

    public float startDelay = 4f;      
    public float totalGameTime = 20f;  

    private float gameTimer = 0f;      
    private bool gameStarted = false;  

   
    // Inicio del juego
   

    void Start()
    {
        
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(startDelay); 

        
        player1_lu1?.EnablePressing();
        player1_lu2?.EnablePressing();
        player2_bowser1?.EnablePressing();
        player2_bowser2?.EnablePressing();

     
        gameStarted = true;
        gameTimer = 0f;
        Debug.Log("[GameController] Juego iniciado");
    }

    
    

    void Update()
    {
        if (!gameStarted) return; 

        gameTimer += Time.deltaTime; 

       
        bool player1Won = player1_lu1.HasWon() || player1_lu2.HasWon();
        bool player2Won = player2_bowser1.HasWon() || player2_bowser2.HasWon();

        if (player1Won || player2Won || gameTimer >= totalGameTime)
        {
            EndGame(player1Won, player2Won);
        }
    }

   
    // Lógica para terminar el juego

    void EndGame(bool player1Won, bool player2Won)
    {
        gameStarted = false; 

       
        player1_lu1?.DisablePressing();
        player1_lu2?.DisablePressing();
        player2_bowser1?.DisablePressing();
        player2_bowser2?.DisablePressing();

        float player1Progress = Mathf.Max(player1_lu1.GetFillProgress(), player1_lu2.GetFillProgress());
        float player2Progress = Mathf.Max(player2_bowser1.GetFillProgress(), player2_bowser2.GetFillProgress());

       
        // Determinar ganadores 
        
        if (player1Won && !player2Won)
        {
           
            Debug.Log("[GameController] Ganó Jugador 1 (Lu)!");
            player1_lu1?.SetVictoryState(true);
            player1_lu2?.SetVictoryState(true);
            player2_bowser1?.SetSadState();
            player2_bowser2?.SetSadState();
        }
        else if (!player1Won && player2Won)
        {
           
            Debug.Log("[GameController] Ganó Jugador 2 (Bowser)!");
            player2_bowser1?.SetVictoryState(true);
            player2_bowser2?.SetVictoryState(true);
            player1_lu1?.SetSadState();
            player1_lu2?.SetSadState();
        }
        else
        {
           
            if (player1Progress > player2Progress)
            {
                Debug.Log("[GameController] Ganó Jugador 1 (Lu) por mayor inflado!");
                player1_lu1?.SetVictoryState(false);
                player1_lu2?.SetVictoryState(false);
                player2_bowser1?.SetSadState();
                player2_bowser2?.SetSadState();
            }
            else if (player2Progress > player1Progress)
            {
                Debug.Log("[GameController] Ganó Jugador 2 (Bowser) por mayor inflado!");
                player2_bowser1?.SetVictoryState(false);
                player2_bowser2?.SetVictoryState(false);
                player1_lu1?.SetSadState();
                player1_lu2?.SetSadState();
            }
        }
    }
}
