using UnityEngine;
using System.Collections;

// La palabra clave abstract le permite crear clases y miembros de la clase que estén incompletos y deben ser implementadas en una clase derivada.
public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;           // Tiempo que le tomará al objeto moverse, en segundos.		
	public LayerMask blockingLayer;     	// Capa en la que la colisión será verificada y si podemos entrar en ella o no.    

	private BoxCollider2D boxCollider;      //El BoxCollider2D es un componenete añadido a este objeto.
	private Rigidbody2D rb2D;               //El Rigidbody2D es un componenete añadido a este objeto.
	private float inverseMoveTime;          //Usado para hacer el movimiento mas eficiente.
	
	
	// Protegido, funciones virtuales pueden ser anulados o sobreescritas por clases herederas.
	protected virtual void Start ()
	{
		//Obtiene una referencia del componente BoxCollider para este objeto.
		boxCollider = GetComponent <BoxCollider2D> ();
		
		//Obtiene una referencia del componente Rigidbody2D para este objeto.
		rb2D = GetComponent <Rigidbody2D> ();
		
		//Almacena el reciproco del tiempo de movimiento que podemos usar multiplicandolo en lugar de dividirlo, esto es mas eficiente.
		inverseMoveTime = 1f / moveTime;
	}
	
	//Mover devuelve verdadero si es capaz de moverse y falso en caso contrario.
	//Mover toma parametros de direccion x, direccion y RaycastHit2D para comprobar colisiones.
	protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
		//Almacena la posicion de inicio para moverse de, basado en la actual posicion del trasnform del objeto.
		Vector2 start = transform.position;//al hacer esta conversion a vector 2 se descarta el eje z.
		
		//Calcula la posicion final basado en la direccion de los parametros aprobados cuando llamamos a Move.
		Vector2 end = start + new Vector2 (xDir, yDir);
		
		//Deshabilita el boxCollider asi la Linecast(emision de linea) no golpea el objeto en su propio collider.
		boxCollider.enabled = false;
		
		//Emite una linea(Linecast) del punto de inicio al punto final comprobando collisiones en las capas de bloqueo.
		hit = Physics2D.Linecast (start, end, blockingLayer);
		
		//Rehabilita el boxCollider despues de linecast(emitir la linea)
		boxCollider.enabled = true;
		
		//Comprueba si cualquier cosa fue golpeada
		if(hit.transform == null)
		{
			//Si ninguna cosa fue golpeada, inicie la corutina SmoothMovement pasandole en el Vector 2 End como destino. 
			StartCoroutine (SmoothMovement (end));
			
			//Devuelve verdadero para decir que Move fue exitoso
			return true;
		}
		
		//Si algo fue afectado, retorne falso, Move no tuvo exito.
		return false;
	}
	
	
	//Co-routine para mover unidades de un espacio al siguiente ,toma un parametro end para especificar a donde moverse.
	protected IEnumerator SmoothMovement (Vector3 end)
	{
		//Calcula la distancia restante para moverse basado en la magnitud cuadrada de la diferencia entre la posicion actual y el parametro final(end).
		//Magnitud cuadrada es usada en lugar de magnitud porque es computacinalmente mas barato.
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		
		//Mientras que la distancia es mayor que una cantidad muy pequeña (Epsilon,casi cero)
		while(sqrRemainingDistance > float.Epsilon)
		{
			//Encuentre una nueva posicion proporcionalmente mas cerca del final(end), basado en el tiempo de movimiento(movetime).
			Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			
			//Llama MovePosition agregada al Rigidbody2D y lo mueve a la posicion calculada.
			rb2D.MovePosition (newPostion);
			
			//Recalclula la distancia restante despues de moverse.
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			//Retorna y hasta que el loop sqrRemainingDistance este suficientemente cerca de cero para terminar la funcion.
			yield return null;
		}
	}
	
	//Las palabra Reservada Virtual significa que AttemptMove(Intento mover) pueden ser anuladas o sobreescrita por la herencia de clases usando la palabra clave para sobreescribir (Override).
	//AttemptMove(Intento mover) toma un parametro generico T para especificar el tipo de componente que esperamos nuestra unidad interactue con algo si esta bloqueado(Jugador con enemigos, paredes con jugador).
	protected virtual void AttemptMove <T> (int xDir, int yDir)
		where T : Component
	{
		//Hit almacenara independientemente de nuestros (Linecast hits) golpes de emision de linea cuando Move es llamado.
		RaycastHit2D hit;
		
		//establece canMove en verdadero si se movio con exito, y falso si fallo.
		bool canMove = Move (xDir, yDir, out hit);
		
		//Chequea si nada fue alcanzado por Linecast
		if(hit.transform == null)
			//Si nada fue alcanzado, retorne y no ejecute codigo adicional.
			return;

		// Obtiene una referencia del componente para el tipo de componente tipo T asociado a el objeto que fue alcanzado.
		T hitComponent = hit.transform.GetComponent <T> ();

		//Si canMove es falso y hitComponent no es igual a null, significa que MovingObject esta bloqueado y fue afectado por alguna cosa que puede interactuar con el.
		if(!canMove && hitComponent != null)
			
			//Llama la funcion onCantMove y pasa el hitComponent como un parametro.
			OnCantMove (hitComponent);
	}
	
	
	//El modificador abstract indica que cosa se esta modificando y tuvo una perdida o una implementacion incompleta.
	//OnCantMove sera sobreescrito por funciones en las clases que heredan.
	protected abstract void OnCantMove <T> (T component)
		where T : Component;

}
