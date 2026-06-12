# 🗺️ Mapa Visual del Proyecto: Flujo, Variables y Red

Este documento presenta de forma gráfica y rápida la arquitectura del juego. Está diseñado con diagramas de flujo y mapas de comunicación estructurados por colores para entender el sistema de inmediato.

---

## 🎨 Leyenda de Colores de los Diagramas

* 🟩 **Verde:** Autoloads / Singletons Globales (Viven en todo el juego y guardan variables clave).
* 🟦 **Azul:** Escenas de la Interfaz (UI) y Controladores Locales.
* 🟪 **Morado:** Base de Datos local SQLite (Persistencia).
* 🟧 **Naranja:** Red, Multijugador y RPCs.
* 🟨 **Amarillo:** Señales / Eventos (Notificaciones de cambio).

---

## 🗺️ 1. Mapa de Comunicación y Variables Globales

Este diagrama muestra cómo interactúan las pantallas (UI) con los Gestores Globales y qué variables transfieren información.

```mermaid
flowchart TD
    %% Estilos de Nodos
    classDef global fill:#4CAF50,stroke:#388E3C,stroke-width:2px,color:#fff;
    classDef ui fill:#2196F3,stroke:#1976D2,stroke-width:2px,color:#fff;
    classDef db fill:#9C27B0,stroke:#7B1FA2,stroke-width:2px,color:#fff;
    classDef net fill:#FF9800,stroke:#F57C00,stroke-width:2px,color:#fff;

    %% Nodos
    subgraph UI_Scenes [Escenas de la Interfaz]
        Menu[MainMenu.cs]:::ui
        SelClase[SeleccionClase.cs]:::ui
        LobbyUI[Lobby.cs]:::ui
        CrearJuego[CreateGame.cs]:::ui
        UnirseJuego[JoinGame.cs]:::ui
    end

    subgraph Autoloads [Singletons Globales]
        DB[DbManager.cs]:::global
        Session[UserSession.cs]:::global
        ClaseM[ClaseManager.cs]:::global
        NetM[NetworkManager.cs]:::net
    end

    SQLite[(user://jugadores.db)]:::db

    %% Flujos de Información
    Menu -->|1. Consulta usuarios| DB
    Menu -->|2. Inicia sesión| Session
    DB <-->|Lee y escribe jugadores| SQLite

    SelClase -->|Consulta clases disponibles| ClaseM
    SelClase -->|Verifica si está en online| NetM

    CrearJuego -->|Llama a crear servidor| NetM
    UnirseJuego -->|Llama a unirse a IP/Puerto| NetM

    LobbyUI -->|Pregunta si todos están listos| NetM
    LobbyUI -->|Lee lista y estados de jugadores| NetM

    %% Aplicar clases
    class DB,Session,ClaseM global;
    class Menu,SelClase,LobbyUI,CrearJuego,UnirseJuego ui;
    class NetM net;
```

---

## ⚡ 2. Mapa de Señales y Eventos (Comunicación Reactiva)

Las señales notifican a la interfaz que algo cambió para que esta se redibuje sola.

```mermaid
flowchart LR
    classDef net fill:#FF9800,stroke:#F57C00,stroke-width:2px,color:#fff;
    classDef signal fill:#FFEB3B,stroke:#FBC02D,stroke-width:2px,color:#000;
    classDef ui fill:#2196F3,stroke:#1976D2,stroke-width:2px,color:#fff;

    NetM[NetworkManager.cs]:::net
    
    %% Señales
    S1{{"ListaJugadoresActualizada"}}:::signal
    S2{{"EstadosJugadoresActualizados"}}:::signal

    LobbyUI[Lobby.cs]:::ui

    %% Conexiones
    NetM -->|Emite al cambiar jugadores| S1
    NetM -->|Emite al cambiar listos| S2

    S1 -->|Dispara la función| F1["UpdatePlayerList()"]
    S2 -->|Dispara la función| F1

    F1 -->|Actualiza colores en| ListUI["ItemList (UI)"]:::ui
```

### 🎨 Colores de Jugadores en el Lobby:
* 🟢 **Verde:** Host (Siempre listo, ID de peer `1`).
* 🔴 **Rojo:** Cliente **Listo**.
* ⚪ **Blanco:** Cliente **No Listo**.

---

## 🟪 3. Flujo de Datos: Base de Datos SQLite

Así fluyen los datos cuando te registras o inicias sesión.

```mermaid
flowchart TD
    classDef ui fill:#2196F3,stroke:#1976D2,stroke-width:2px,color:#fff;
    classDef global fill:#4CAF50,stroke:#388E3C,stroke-width:2px,color:#fff;
    classDef db fill:#9C27B0,stroke:#7B1FA2,stroke-width:2px,color:#fff;

    Input["Escribe Nombre (LineEdit)"]:::ui -->|OnAgregarCuentaPressed| DB[DbManager.Instance]:::global
    DB -->|SQL: INSERT INTO jugadores| SQLite[(user://jugadores.db)]:::db
    
    Dropdown["OptionButton (UI)"]:::ui -->|OnLoginPressed| DB
    DB -->|SQL: SELECT nombre| SQLite
    DB -->|Retorna lista de nombres| Dropdown

    Select["Iniciar Sesión"]:::ui -->|UserSession.IniciarSession| ActiveUser["UserSession.NombreUsuario"]:::global
```

---

## 🟧 4. El Sistema Online (ENet + RPCs)

Las funciones marcadas con `[Rpc]` son métodos que se ejecutan en otras computadoras a través de la red.

```mermaid
sequenceDiagram
    autonumber
    rect rgb(33, 150, 243)
        Note over Cliente, Host: Conexión Inicial
    end
    Cliente->>Host: Conexión física por ENet (IP + Puerto)
    Host-->>Cliente: Acepta Conexión (PeerConnected)

    rect rgb(255, 152, 0)
        Note over Cliente, Host: Registro de Datos
    end
    Cliente->>Host: RPCId(1): RegistrarJugador(nombre)
    Note over Host: Guarda jugador en su diccionario local
    Host->>Cliente: RPC: SincronizarListaJugadores(ids, nombres)
    Note over Cliente: Actualiza su propia lista local

    rect rgb(76, 175, 80)
        Note over Cliente, Host: Lógica del Lobby (Listo / No Listo)
    end
    Cliente->>Host: RPC: CambiarEstadoJugador(true)
    Host->>Cliente: RPC: SincronizarEstadosJugadores(ids, estados)
    Note over Host: Verifica: ¿Todos listos? (TodosClientesListos())
    Host->>Cliente: Cambia de escena e inicia partida
```

### 📑 Resumen Rápido de Variables de Red:
* **`NetworkManager.ListaJugadores`**: Diccionario `<int, string>` que guarda `ID_Jugador -> Nombre`.
* **`NetworkManager.EstadosJugadores`**: Diccionario `<int, bool>` que guarda `ID_Jugador -> Listo(True)/NoListo(False)`.
* **`NetworkManager.JugadorNombre`**: Variable de texto con el nombre de tu usuario local en la partida.
* **`NetworkManager.PuertoServidor`**: Puerto en el que está escuchando el host.
