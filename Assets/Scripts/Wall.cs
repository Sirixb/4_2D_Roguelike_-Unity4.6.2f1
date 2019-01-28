using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	public Sprite dmgSprite;                    //Alternate sprite to display after Wall has been attacked by player.
	public int hp = 3;                          //hit points for the wall.
	public AudioClip chopSound1;
	public AudioClip chopSound2;

	private SpriteRenderer spriteRenderer;      //Store a component reference to the attached SpriteRenderer.

	void Awake ()
	{
		//Toma una referencia del componente SpriteRenderer.
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	//DamageWall es llamado cuando el jugador ataca el muro.
	public void DamageWall (int loss)
	{
		//Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
		SoundManager.instance.RandomizeSfx (chopSound1, chopSound2);
		
		//Establece a spriteRenderer con la clase sprite el daño de pared.
		spriteRenderer.sprite = dmgSprite;
		
		//Sustrae perdido de los puntos totales por golpe.
		hp -= loss;
		
		//Si los puntos de golpe son menores o iguales a cero:
		if(hp <= 0)
			//Desactive el gameObject.
			gameObject.SetActive (false);
	}
}
