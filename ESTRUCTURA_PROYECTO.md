# Estructura del proyecto

Este documento es la referencia rapida del proyecto Godot. La idea es leer esto primero antes de tocar codigo, para entender el flujo general sin tener que revisar todos los archivos cada vez.

Nota para futuras ediciones: la parte de managers/autoloads explica los scripts que viven durante todo el juego y guardan datos compartidos. Antes de tocar login, usuarios, clases o multijugador, revisar primero esa seccion porque ahi estan los codigos base que conectan varias pantallas.

## Resumen

Proyecto Godot 4.5 con C#/.NET. El juego tiene por ahora una base de menu, login local con SQLite, seleccion de clase y multijugador por ENet.

- Escena principal configurada en `project.godot`: `Main.tscn`.
- La UI real del menu principal vive en `Scenes/UI/MainMenu.tscn`.
- La red usa `ENetMultiplayerPeer`.
- La persistencia usa SQLite en `user://jugadores.db`.
- Los sistemas globales estan registrados como autoloads.

## Carpetas principales

```text
Assets/
  Audio/
  Fonts/
  Models/
  Sprites/
    Soldado/
      Idle/
      Menu/
      run/
  Video/
    fondo_ligero.ogv

Config/
Prefabs/

Scenes/
  UI/
    MainMenu.tscn
    MultiplayerMenu.tscn
    CreateGame.tscn
    JoinGame.tscn
    SeleccionClase.tscn
  Networking/
    Lobby.tscn

Scripts/
  UI/
    MainMenu.cs
    MultiplayerMenu.cs
    CreateGame.cs
    JoinGame.cs
    SeleccionClase.cs
  Networking/
    NetworkManager.cs
    Lobby.cs
  Managers/
    DbManager.cs
    UserSession.cs
    ClaseManager.cs
```

## Configuracion Godot

Archivo: `project.godot`

Autoloads registrados:

- `NetworkManager`: `res://Scripts/Networking/NetworkManager.cs`
- `DbManager`: `res://Scripts/Managers/DbManager.cs`
- `UserSession`: `res://Scripts/Managers/UserSession.cs`
- `ClaseManager`: `res://Scripts/Managers/ClaseManager.cs`

Esto significa que esos scripts existen durante todo el juego y se acceden como singletons/globales.

## Dependencias C#

Archivo: `Nuevo Proyecto de Juego.csproj`

- SDK: `Godot.NET.Sdk/4.5.0`
- Target principal: `net8.0`
- Target Android: `net9.0`
- Paquete externo: `Microsoft.Data.Sqlite` version `9.0.9`

## Escenas y scripts

| Escena | Script | Funcion |
| --- | --- | --- |
| `Scenes/UI/MainMenu.tscn` | `Scripts/UI/MainMenu.cs` | Menu principal, login, crear usuarios, entrar a solo/multijugador. |
| `Scenes/UI/MultiplayerMenu.tscn` | `Scripts/UI/MultiplayerMenu.cs` | Pantalla para elegir crear sala, unirse o volver. |
| `Scenes/UI/CreateGame.tscn` | `Scripts/UI/CreateGame.cs` | Formulario para crear servidor con el usuario actual y puerto. |
| `Scenes/UI/JoinGame.tscn` | `Scripts/UI/JoinGame.cs` | Formulario para unirse con codigo Base64 de conexion. |
| `Scenes/Networking/Lobby.tscn` | `Scripts/Networking/Lobby.cs` | Sala de espera, lista de jugadores, listo/no listo, copiar codigo y seleccion de clase. |
| `Scenes/UI/SeleccionClase.tscn` | `Scripts/UI/SeleccionClase.cs` | Selecciona Guerrero, Mago o Arquero usando `ClaseManager`. |
| `Main.tscn` | Sin script | Escena 3D basica con luz y camara. |

La mayoria de escenas UI usan el video `res://Assets/Video/fondo_ligero.ogv` como fondo.

## Flujo de usuario

### Login local

1. `MainMenu.cs` abre `PopupLogin`.
2. `DbManager.ObtenerJugadores()` carga usuarios guardados en SQLite.
3. `DbManager.GuardarJugador(nombre)` crea o actualiza un usuario.
4. `UserSession.IniciarSession(nombre)` guarda el usuario activo.
5. Cuando hay sesion iniciada, se muestran los botones `Play` y `Multiplayer`.

### Modo solo

1. Desde `MainMenu`, boton `Play`.
2. Cambia a `Scenes/UI/SeleccionClase.tscn`.
3. `SeleccionClase.cs` consulta `ClaseManager`.
4. Si no hay conexion activa, al finalizar vuelve a `MainMenu`.

### Multijugador: crear sala

1. `MainMenu` -> `MultiplayerMenu`.
2. `MultiplayerMenu` -> `CreateGame`.
3. `CreateGame` toma el usuario de `UserSession` y el puerto del `SpinBox`.
4. Llama a `NetworkManager.Instance.CrearServidor(nombreJugador, puerto)`.
5. `NetworkManager` crea un `ENetMultiplayerPeer`, registra al host y cambia a `Lobby.tscn`.
6. `Lobby.cs` muestra IP local, puerto y un codigo Base64 con formato `ip:puerto`.

### Multijugador: unirse a sala

1. `MainMenu` -> `MultiplayerMenu`.
2. `MultiplayerMenu` -> `JoinGame`.
3. `JoinGame` lee el codigo Base64, lo decodifica a `ip:puerto`.
4. Llama a `NetworkManager.Instance.UnirseServidor(ip, puerto, nombreJugador)`.
5. Al conectarse, `NetworkManager.OnConectarAlServidor()` registra el jugador por RPC y cambia a `Lobby.tscn`.

### Lobby

`Lobby.cs` escucha dos senales del `NetworkManager`:

- `ListaJugadoresActualizada`
- `EstadosJugadoresActualizados`

Con esas senales refresca el `ItemList` de jugadores.

Colores actuales:

- Host: verde.
- Cliente listo: rojo.
- Cliente no listo: blanco.

El host ve el boton de iniciar partida. Los clientes ven el boton de listo/no listo.

## Sistemas globales

Estos son los scripts mas importantes para entender el proyecto. Estan registrados como autoloads en `project.godot`, por eso se cargan al iniciar el juego y se pueden usar desde distintas escenas.

Orden recomendado para leerlos:

1. `UserSession.cs`: saber que usuario esta usando el juego.
2. `DbManager.cs`: guardar y cargar usuarios en SQLite.
3. `ClaseManager.cs`: entregar la informacion de las clases.
4. `NetworkManager.cs`: crear/unirse a partidas y sincronizar jugadores.

Codigos necesarios para reconocerlos rapido:

```csharp
// UserSession: usuario activo en memoria.
UserSession.Instance.NombreUsuario;
UserSession.Instance.IniciarSession(nombre);
UserSession.Instance.CerrarSession();
UserSession.Instance.EstaLogueado();

// DbManager: usuarios guardados en SQLite.
DbManager.Instance.GuardarJugador(nombre);
DbManager.Instance.ObtenerJugadores();
DbManager.Instance.CargarPuntos(nombre);

// ClaseManager: datos de clases jugables.
ClaseManager.Instance.obtenerClase("Guerrero");
ClaseManager.Instance.ObtenerTodasLasClases();

// NetworkManager: conexion multijugador y lobby.
NetworkManager.Instance.CrearServidor(nombre, puerto);
NetworkManager.Instance.UnirseServidor(ip, puerto, nombre);
NetworkManager.Instance.CerrarConexion();
NetworkManager.Instance.HayConexionActiva();
```

### `NetworkManager.cs`

Sirve para controlar toda la red del juego: crear servidor, unirse como cliente, registrar jugadores, sincronizar la lista del lobby y manejar el estado listo/no listo.

Datos principales:

- `Peer`: conexion ENet actual.
- `ListaJugadores`: diccionario `id -> nombre`.
- `EstadosJugadores`: diccionario `id -> listo/no listo`.
- `JugadorNombre`: nombre local usado en red.
- `PuertoServidor`: puerto elegido por el host.

Metodos importantes:

- `CrearServidor(nombreJugador, puerto)`
- `UnirseServidor(ip, puerto, nombreJugador)`
- `RegistrarJugador(nombre)`
- `SincronizarListaJugadores(ids, nombres)`
- `CambiarEstadoJugador(estado)`
- `SincronizarEstadosJugadores(ids, estados)`
- `TodosClientesListos()`
- `CerrarConexion()`
- `HayConexionActiva()`
- `obtenerDireccionIPLocal()`
- `obtenerPuertoLocal()`

Senales:

- `ListaJugadoresActualizada`
- `EstadosJugadoresActualizados`

### `DbManager.cs`

Sirve para guardar usuarios y puntos en una base de datos local SQLite. Es usado por el menu principal para crear cuentas y cargar la lista de usuarios.

- Base de datos: `user://jugadores.db`
- Tabla: `jugadores`
- Columnas: `id`, `nombre`, `puntos`

Metodos importantes:

- `GuardarJugador(nombreJugador)`
- `CargarPuntos(nombreJugador)`
- `ObtenerJugadores()`

### `UserSession.cs`

Sirve para recordar que usuario esta logueado mientras el juego esta abierto. No guarda en disco; solo mantiene la sesion actual en memoria.

Metodos importantes:

- `IniciarSession(nombre)`
- `CerrarSession()`
- `EstaLogueado()`

Dato principal:

- `NombreUsuario`

### `ClaseManager.cs`

Sirve para tener centralizada la informacion de clases jugables. La pantalla `SeleccionClase` consulta este manager para mostrar descripcion, habilidades y ventajas.

Clases actuales:

- Guerrero
- Mago
- Arquero

Cada clase tiene:

- `Nombre`
- `Descripcion`
- `Habilidades`
- `Ventajas`

Metodos importantes:

- `obtenerClase(nombre)`
- `ObtenerTodasLasClases()`

## Rutas de nodos importantes

Estas rutas estan hardcodeadas en los scripts. Si cambias nombres o jerarquia en el editor, hay que actualizar el C#.

### MainMenu

- `CenterContainer/Panel/VBoxContainer/PlayButton`
- `CenterContainer/Panel/VBoxContainer/MultiplayerButton`
- `CenterContainer/Panel/VBoxContainer/OptionButton`
- `CenterContainer/Panel/VBoxContainer/ExitButton`
- `CenterContainer/Panel/HBoxContainer/BtnLogin`
- `CenterContainer/Panel/HBoxContainer/LabelNombre`
- `PopupLogin/CenterContainer/VBoxContainer/btnIniciarSesion`
- `PopupLogin/CenterContainer/VBoxContainer/btnAgregarCuenta`
- `PopupLogin/CenterContainer/VBoxContainer/btnCancelar`
- `PopupLogin/CenterContainer/VBoxContainer/OptionUsuarioButton`
- `PopupLogin/CenterContainer/VBoxContainer/LineEditNombre`

### Lobby

- `CenterContainer/Panel/VBoxContainer/PlayerList`
- `CenterContainer/Panel/VBoxContainer/HBoxContainer/textoInfo`
- `CenterContainer/Panel/VBoxContainer/HBoxContainer/codigoInfo`
- `CenterContainer/Panel/VBoxContainer/IniciarButton`
- `CenterContainer/Panel/VBoxContainer/SalirButton`
- `CenterContainer/Panel/VBoxContainer/ListoButton`
- `CenterContainer/Panel/VBoxContainer/SeleccionButton`

### SeleccionClase

- `HBoxContainer2/BtnSeleccionar`
- `HBoxContainer2/BtnVolver`
- `HBoxContainer/VBoxContainer/HBoxContainer/BtnGuerrero`
- `HBoxContainer/VBoxContainer/HBoxContainer/BtnMago`
- `HBoxContainer/VBoxContainer/HBoxContainer/BtnArquero`
- `HBoxContainer/VBoxContainer/LabelNombreClase`
- `HBoxContainer/VBoxContainer2/RichTextDescipcion`
- `HBoxContainer/VBoxContainer2/LabelHabilidades`
- `HBoxContainer/VBoxContainer2/LabelVentajas`

## Puntos delicados

- `project.godot` tiene autoloads indispensables. Si uno falta, menus/red/login pueden fallar.
- `NetworkManager` mezcla campos `static` con `Instance`; revisar con cuidado antes de cambiarlo.
- `JoinGame` actualmente usa el codigo Base64 como fuente real de IP/puerto. Los inputs separados de IP y puerto existen, pero no se usan en `OnUnirsePressed`.
- `Lobby.UpdatePlayerList()` asume que el host tiene ID `1`.
- `TodosClientesListos()` requiere al menos 2 jugadores.
- `SeleccionClase.ActualizarUI()` llama `NetworkManager.Instance.HayConexionActiva()`, por eso depende de que el autoload exista.
- Los scripts dependen mucho de rutas exactas de nodos.
- Hay comentarios y textos con caracteres mal codificados en algunos archivos; evitar mezclar codificaciones al editar.

## Como actualizar este documento

Actualizar este archivo cuando cambie cualquiera de estas cosas:

- Nueva escena importante.
- Nuevo autoload.
- Cambio de flujo entre menus.
- Cambio en red, login, base de datos o seleccion de clase.
- Cambio de rutas de nodos usadas por scripts.
- Nueva dependencia en el `.csproj`.
