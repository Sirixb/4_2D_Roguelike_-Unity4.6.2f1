using UnityEngine;
using System; //nos permite usar el atributo serializable. serializable nos permite modificar como aparacen las variables en el inspector a modo de desplegable
using System.Collections.Generic;//Generic nos permite usar la lista
using Random= UnityEngine.Random;//necesitamos especificar esto porque hay una clase  llamada Random tanto en el sistema como en el motor de la unidad.

public class BoardManager : MonoBehaviour {
	// Usando Serializable nos permite incrustar una clase con subpropiedades  en el inspector.
	[Serializable]
	public class Count
	{
		public int minimun; // El valor mínimo para nuestra clase Count.
		public int maximun;// El valor máximo para nuestra clase Count.
		//incluiremos un constructor de asignacion para el contador asi podremos enviarle el valor de minimo y maximo cuando declaremos new count.
		public Count(int min, int max)
		{
			minimun=min;
			maximun= max;			
		}
	}
	//deliniamos las dimensiones el gameboard
	public int columns=8;// Número de columnas en nuestro tablero de juego.
	public int rows=8;// Número de filas en nuestro tablero de juego.
	//usamos Count para especificar un rango aleatorio
	public Count wallCount= new Count(5,9);	// límite Inferior y superior para nuestro número aleatorio de paredes por nivel.
	public Count foodCount= new Count(1,5);	// Inferior y límite superior para nuestro número aleatorio de los alimentos por nivel.
	public GameObject exit;// Prefab para desovar para la salida.
	public GameObject[] floorTiles;// Array de prefabricados de piso.
	public GameObject[] wallTiles;// Array de prefabricados de pared.
	public GameObject[] foodTiles;// Array de prefabricados de alimentos.
	public GameObject[] enemyTiles;// Array de prefabricados de enemigos.
	public GameObject[] outerWallTiles;// Array de prefabricados de paeredes exteriores.

	private Transform boardHolder;//Una variable para almacenar una referencia para el transform de nuestro Board object.
	private List <Vector3> gridPositions= new List<Vector3>();// Una lista de posibles ubicaciones para colocar las baldosas.

	//Limpia nuestra lista gridPositions y lo prepara para generar un nuevo tablero.
	void InitialiseList()
	{
		// Limpia nuestra lista gridPositions.
		gridPositions.Clear ();
		//Usamos dos loops anidaddos para rellenar nuestra lista en cada una de las posibles posiciones.
		for (int x=1; x < columns-1; x++)//largo del eje x horizontal // Recorrer eje x (columnas).
		{
			// Dentro de cada columna, haga un bucle a través del eje y (filas).
			for(int y=1; y < rows-1;y++)//largo del eje y vertical
			{
				// En cada índice añade un nuevo Vector3 a nuestra lista con las coordenadas X e Y de esa posición.
				gridPositions.Add(new Vector3(x,y,0f));
			}
		}
	}
	// Configura las paredes exteriores y el piso (fondo) del tablero de juego.
	void BoardSetup()
	{
		// Instancia el Board(tablero) y establece a boardHolder su transformación.
		boardHolder = new GameObject ("Board").transform;
		// Loop a lo largo del eje x, a partir de -1 (para llenar esquina) con baldosas de suelo o borde de pared exterior.
		for (int x = -1; x < columns+1; x++) 
		{
			// Loop lo largo del eje Y, a partir de -1 para colocar suelo o baldosas de pared exterior.
			for(int y = -1; y < columns+1; y++)
			{
				// Elija una baldosa al azar de nuestro array de prefabricados de baldosas de piso y preparese para crear instancias de ella.
				GameObject toInstantiate= floorTiles[Random.Range(0,floorTiles.Length)];
				// Comprobar si la posición actual está en el borde del tablero, de ser así elegir un prefabricado de pared exterior al azar de nuestro array de baldosas de pared exterior.
				if(x ==-1 || x== columns || y== -1 || y== rows)
					toInstantiate = outerWallTiles[Random.Range(0,outerWallTiles.Length)];

				// Instanciar el GameObject utilizando la instancia del prefabricado elegido para toInstantiarlo en el Vector3 correspondiente a la posición actual de la cuadrícula en bucle, convertirlo en un GameObject.
				GameObject instance= Instantiate(toInstantiate,new Vector3(x,y,0f),Quaternion.identity) as GameObject;
				// Establecer el padre de nuestro recién objeto instanciado para instanciarlo a boardHolder, esto es sólo por organización para evitar engordar la jerarquía.
				instance.transform.SetParent(boardHolder);
			}
		}
	}
	// RandomPosition devuelve una posición aleatoria de nuestra lista gridPositions(Posiciones de cuadricula).
	Vector3 RandomPosition()
	{
		// Declarar un entero randomIndex, establece su valor a un número aleatorio entre 0 y el número de elementos en nuestra lista gridPositions.
		int randomIndex = Random.Range (0, gridPositions.Count);
		// Declarar una variable de tipo Vector3 llamado randomPosition, establece su valor a la entrada en randomIndex de nuestra lista gridPositions.
		Vector3 randomPosition = gridPositions [randomIndex];
		// Eliminar la entrada en randomIndex de la lista para que no se puede volver a utilizar.
		gridPositions.RemoveAt (randomIndex);// para que no se escriban en el mismo lugar.
		// Devuelve la posición Vector3 seleccionado al azar.
		return randomPosition;
	}

	// LayoutObjectAtRandom(diseñar objeto al azar) acepta una array de objetos del juego para elegir a lo largo con un rango mínimo y máximo para el número de objetos a crear.
	void LayoutObjectAtRandom(GameObject[] tileArray, int minimun, int maximun)
	{
		// Elegir un número aleatorio de objetos para crear una instancia dentro de los límites mínimo y máximo.
		int objectCount = Random.Range(minimun, maximun + 1);
		// Crear instancias de objetos hasta que se alcance el límite objectCount elegido al azar
		for(int i=0; i < objectCount; i++)
		{
			// Elija una posición para randomPosition consiguiendo una posición aleatoria de nuestra lista de Vector3s disponibles almacenados en gridPosition.
			Vector3 randomPosition= RandomPosition();
			// Elija una baldosa al azar de tileArray "la cual puede contener enemigos, paredes o viveres" y asignarlo a tileChoice "que fue lo que escogio".
			GameObject tileChoice= tileArray[Random.Range(0,tileArray.Length)];
			// Instantiate tileChoice en la posición devuelta por RandomPosition sin cambio en la rotación
			Instantiate(tileChoice,randomPosition,Quaternion.identity);
		}
	}
	// SetupScene Inicializa nuestro nivel y llama a las funciones anteriores para diseñar el tablero de juego.
	public void SetupScene(int level)
	{
		// Crea las paredes exteriores y piso.
		BoardSetup ();
		// Resetear nuestra lista de gridpositions.
		InitialiseList ();
		// Crear una instancia de un número aleatorio de baldosas de pared sobre la base de mínimo y máximo, en posiciones aleatorias.
		LayoutObjectAtRandom (wallTiles, wallCount.minimun, wallCount.maximun);
		// Crear una instancia de un número aleatorio de baldosas de alimento a base de mínimo y máximo, en posiciones aleatorias.
		LayoutObjectAtRandom (foodTiles, foodCount.minimun, foodCount.maximun);
		// Determinar número de enemigos basados en el número nivel actual, basado en una progresión logarítmica
		int enemyCount = (int)Mathf.Log (level, 2f);
		// Crear una instancia de un número aleatorio de los enemigos a base de mínimo y máximo, en posiciones aleatorias.
		LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);
		// Crear una instancia de salida en la esquina superior derecha de nuestro tablero de juego
		Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);//en la posicion 7,7,0
	}
}
