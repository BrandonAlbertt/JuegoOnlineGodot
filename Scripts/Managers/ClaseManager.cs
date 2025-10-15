using Godot;
using System;
using System.Collections.Generic;

public partial class ClaseManager : Node
{
    // 🔷 Singleton (única instancia global)
    public static ClaseManager Instance { get; private set; }
    // Diccionario con las clases del juego
    private Dictionary<string, ClaseInfo> _clases = new();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        SetProcess(false);

        InicializarClases();
        GD.Print("ClaseManager cargado y persistente");

    }

    private void InicializarClases()
    {
        _clases = new Dictionary<string, ClaseInfo>
        {
            { "Guerrero", new ClaseInfo(
                "Guerrero",
                "Experto en combate cuerp a cuerpo.",
                "Espadazo, Bloqueo",
                "Alto resistencia"
                )
            },
            { "Mago", new ClaseInfo(
                "Mago",
                "Dominio de la energía mágica.",
                "Bola de fuego, Escudo mágico",
                "Alto daño mágico"
                )
            },
            { "Arquero", new ClaseInfo(
                "Arquero",
                "Maestro del combate a distancia.",
                "Disparo preciso, Trampa",
                "Alta movilidad"
                )
            }
        };
    }

    // Obtener información de una clase por su nombre
    public ClaseInfo obtenerClase(string nombre)
    {
        if (_clases.ContainsKey(nombre))
        {
            return _clases[nombre];
        }

        GD.PrintErr($"Clase '{nombre}' no encontrada.");
        return null;
    }

    // Obtener todas las clases (por ejemplo para listarlas en un menu)
    public List<ClaseInfo> ObtenerTodasLasClases()
    {
        return new List<ClaseInfo>(_clases.Values);
    }

    // Estructura Interna que describe cada clase
    // esta es una clase (ClaseInfo) que contiene la informacion de cada clase
    // nombre, descripcion, habilidades y ventajas
    // es decir este es como un modelo de interno de como se organiza la informacion de la clase ClaseInfo
    // como un int que internamente guarda numero estnertos en este es una estructura que guarda varios datos
    // es decir es una estructura de datos personalizada como las clases trancicionales int, string, float, etc
    // pero esta es personalizada por nosotros
    /* == comentario tecnico == */
    /* == comentario tecnico == */
    // Clase de transferencia de datos (DTO) que encapsula los atributos
    // de una clase de personaje en un único objeto inmutable.
    // Implementa un patrón Value Object: la igualdad se basa en el contenido,
    // no en la referencia de memoria. Útil para desacoplar la capa de presentación
    // de la lógica de dominio y facilitar la serialización futura (JSON, XML).
    public class ClaseInfo
    {
        public string Nombre { get; }
        public string Descripcion { get; }
        public string Habilidades { get; }
        public string Ventajas { get; }

        public ClaseInfo(string nomb, string desc, string hab, string vent)
        {
            Nombre = nomb;
            Descripcion = desc;
            Habilidades = hab;
            Ventajas = vent;
        }
    }

}
