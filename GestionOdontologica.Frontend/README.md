# Frontend — Gestión de Usuarios (HU-18)

Interfaz de usuario para el acceso y la gestión de usuarios de la clínica odontológica, construida con **Blazor WebAssembly (.NET 8)**. Incluye inicio de sesión, recuperación y restablecimiento de contraseña, sesión con vigencia, guard de rutas, y el mantenimiento completo de usuarios (listar, crear, editar, dar de baja y reactivar).

Es una **solución independiente**: no referencia el proyecto de la API ni comparte código con él. La comunicación es exclusivamente HTTP/JSON contra los endpoints REST de `GestionOdontologica.Api`.

## Requisitos

- .NET 8 SDK (incluye las herramientas de Blazor WebAssembly).
- La Web API en ejecución, por defecto en `http://localhost:5080`.
- Oracle configurado en el backend para que `GET /api/usuarios` responda con datos.

No se necesita Node.js, `npm` ni un servidor estático adicional.

## Ejecutar

```powershell
dotnet run --project .\GestionOdontologica.Frontend\GestionOdontologica.Frontend.csproj
```

o, desde la carpeta del proyecto:

```powershell
cd GestionOdontologica.Frontend
dotnet run
```

La aplicación queda en:

```text
http://localhost:5501/login
```

La raíz `/` redirige a `/login` si no hay sesión o a `/inicio` si la sesión está vigente. Después de iniciar sesión siempre se abre `/inicio`. Las rutas internas están protegidas por `AuthorizeRouteView` y redirigen a `/login` cuando no hay autenticación.

Compilar sin ejecutar:

```powershell
dotnet build .\GestionOdontologica.Frontend.sln
```

## Configuración de la API

La URL base se lee de `wwwroot/appsettings.json`:

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5080/api/"
  }
}
```

- Si ejecutas la API con el perfil HTTPS, usa `https://localhost:7080/api/`.
- Si la clave no existe, el valor no es una URL absoluta o termina sin `/`, la aplicación normaliza la URL y, en el peor caso, usa `http://localhost:5080/api/`.
- El origen del frontend (`http://localhost:5501` y `http://127.0.0.1:5501`) ya está permitido en la política CORS `Frontend` de la API. Si cambias el puerto, agrégalo en `GestionOdontologica.Api/appsettings.json` (`Cors:AllowedOrigins`).

## Estructura

```text
GestionOdontologica.Frontend/
├── Components/
│   ├── AuthShell.razor           # Marco visual compartido de login/recuperación
│   ├── ConfirmModal.razor        # Diálogo modal de confirmación flotante (baja y activación)
│   ├── ModuloPendiente.razor     # Placeholder de módulos futuros
│   ├── RedirectToLogin.razor     # Redirección del guard de autorización
│   ├── ToastContainer.razor      # Notificaciones temporales
│   ├── UsuarioForm.razor         # Formulario unificado de alta y edición (Master-Detail)
│   └── UsuariosTable.razor       # Tabla con filtros profesionales (tabs, roles) y botones Blazor UI
├── Layout/
│   ├── EmptyLayout.razor         # Layout sin sidebar para login y recuperación
│   └── MainLayout.razor          # Sidebar, topbar, perfil real, expiración de sesión y toasts
├── Models/
│   ├── AuthModels.cs             # Contratos de login/recuperación y sesión
│   ├── AuthFormModels.cs         # Validaciones de login/recuperación/restablecimiento
│   ├── ActualizacionUsuarioDto.cs# Payload PUT para actualizar usuario
│   ├── CreacionUsuarioDto.cs
│   ├── ProblemDetailsDto.cs
│   ├── UsuarioDto.cs
│   ├── UsuarioEdicionModel.cs    # Modelo con validaciones para la edición
│   ├── UsuarioFormModel.cs       # Validaciones del formulario (clave obligatoria solo al crear)
│   └── UsuarioVista.cs           # Vista de la tabla (rol desde la API)
├── Pages/
│   ├── Login.razor               # Inicio de sesión (abre /inicio) y Recordarme
│   ├── RecuperarPassword.razor   # Solicitud de recuperación (token de desarrollo)
│   ├── RestablecerPassword.razor # Nueva contraseña con token
│   ├── Usuarios.razor            # Página principal del Sprint 1
│   ├── Home.razor                # Redirección a /usuarios
│   └── Inicio/Roles/Citas/Pacientes/Reportes.razor
├── Services/
│   ├── ApiException.cs           # Error con status HTTP + ProblemDetails
│   ├── ApiStatusService.cs       # Estado compartido de la conexión
│   ├── AuthApiService.cs         # POST /api/auth/login, recuperar y restablecer
│   ├── SesionService.cs          # Sesión en localStorage con expiración
│   ├── SesionAuthenticationStateProvider.cs # Identidad de Blazor y notificaciones
│   ├── ToastService.cs           # Cola de notificaciones
│   └── UsuariosApiService.cs     # GET/POST/PUT/DELETE y reactivación
├── wwwroot/
│   ├── appsettings.json
│   ├── css/app.css
│   └── index.html
├── App.razor                     # AuthorizeRouteView y guard de rutas
├── Program.cs
└── GestionOdontologica.Frontend.csproj
```

## Comportamiento

- **Acceso:** `/` redirige a `/login` sin sesión o a `/usuarios` con sesión vigente. El login valida formulario (usuario/correo y contraseña) y muestra mensajes claros para `401` (credenciales incorrectas), `403` (cuenta desactivada) y `423` (bloqueo temporal con minutos restantes).
- **Sesión:** al autenticarse se guarda en `localStorage` (`odontocare.sesion.v1`) solo los datos públicos del usuario y `expiraEn`. Dura 8 horas, o 7 días con **Recordarme**. El sidebar muestra nombre, rol e iniciales reales; **Cerrar sesión** borra la sesión y vuelve a `/login?motivo=sesion-cerrada`. Al vencer se redirige a `/login?motivo=expirada`.
- **Recuperación:** `/recuperar-password` envía el correo a `POST /api/auth/recuperar-password` y muestra siempre un mensaje genérico. La API genera un token de un solo uso (30 minutos) y lo envía por SMTP si `Email:Habilitado=true`; en `Development` también devuelve un token de demostración que la pantalla muestra con un enlace a `/restablecer-password?token=...`.
- **Restablecimiento:** `/restablecer-password` valida el token, la coincidencia y la política de contraseñas contra `POST /api/auth/restablecer-password`. El token queda inutilizable después del primer uso.
- **Política de contraseñas:** mínimo 8 y máximo 72 caracteres, sin letras iguales consecutivas (`aa`, `bb`) ni números iguales consecutivos (`11`, `22`). Se valida en el cliente y en la API antes de hashear con BCrypt.
- **Tema claro/oscuro:** el botón del topbar (y de las pantallas de autenticación) alterna el tema. La preferencia se guarda en `localStorage` (`odontocare.tema`), se aplica antes de cargar Blazor desde `index.html` y, si no existe, respeta `prefers-color-scheme` del sistema. En modo oscuro los títulos de tarjetas y formularios, las etiquetas de los campos y los encabezados de tabla usan tonos claros de alto contraste (`#F9FAFB`, `#E5E7EB`, `#F1F5F9`) y los encabezados de tabla van en `font-weight: 600`.
- **Caché del CSS:** `wwwroot/index.html` enlaza `css/app.css?v=X.Y`. Al modificar `app.css` hay que subir ese número (por ejemplo `?v=4.1`); si no, el navegador reutiliza la hoja de estilos anterior y la interfaz se ve sin estilos nuevos.
- **Paginación:** todas las tablas de datos (Usuarios, Inventario, Pacientes, Odontólogos, Roles y Permisos) usan el componente reutilizable `DataPagination` y el estado `Paginacion` (`Models/Paginacion.cs`). Permiten elegir 10, 25 o 50 filas por página, navegar entre páginas y reinician a la primera al cambiar búsqueda o filtros. La paginación es en cliente porque los listados se cargan completos desde la API.
- **Carga inicial:** `GET /api/usuarios` al entrar a `/usuarios`, con skeletons mientras se espera la respuesta.
- **Alta de usuario:** `EditForm` con validación por `DataAnnotations` (nombre completo, nombre de usuario, correo, contraseña y rol). Si el modelo es válido se envía `POST /api/usuarios`.
- **Refresco automático:** tras un `201 Created` se limpia el formulario, se muestra un toast y se vuelve a ejecutar el `GET` para reflejar el nuevo usuario y su rol.
- **Edición:** **Editar** en una fila rellena el formulario de la izquierda y lo cambia a modo edición (`PUT /api/usuarios/{id}`). La contraseña es opcional: vacía conserva la clave actual; con valor se valida con la política y se vuelve a hashear. **Descartar cambios** o **Cancelar** sale del modo edición.
- **Baja y reactivación:** **Eliminar** pide confirmación y ejecuta la baja lógica (`DELETE` → `activo = 0`). **Activar** restaura el acceso (`POST /api/usuarios/{id}/reactivar`). Ambas refrescan la tabla y muestran un toast.
- **Protección del Administrador:** los usuarios con rol `Administrador` (o `Administrador del Sistema`) no pueden desactivarse, eliminarse ni perder su rol. La tabla muestra **Protegido** en lugar de **Eliminar** y el formulario bloquea rol y estado; la API devuelve `409` si se intenta por otra vía.
- **Errores de la API:** `ProblemDetails` se traduce a mensajes en español. Los errores por campo (`errors`) se muestran debajo de cada input; `409` marca el nombre de usuario y el correo; `500` muestra el `detail` del backend.
- **Estados de la tabla:** carga, vacío, sin resultados de búsqueda, error con botón “Reintentar” y sin conexión.
- **Estado de la API:** el topbar y las tarjetas de resumen muestran “API conectada”, “API con error”, “API sin conexión” o “Conectando API…”.
- **Búsqueda local:** filtra por nombre, nombre de usuario, correo y rol sin llamar a la API.
- **Navegación:** sidebar con Inicio, Usuarios, Roles, Citas, Pacientes y Reportes. Las secciones sin HU muestran “Módulo en preparación”. El menú se colapsa en pantallas pequeñas.

## Roles

El rol es parte del contrato de la API: se envía en el `POST /api/usuarios` y el backend lo persiste en la columna `ROL` de la tabla `USUARIOS`. El `GET` lo devuelve en la propiedad `rol`, por lo que la tabla lo muestra sin almacenamiento local.

- Roles disponibles: `Administrador`, `Odontólogo`, `Recepcionista`.
- Rol por defecto: `Odontólogo`.
- La tabla no guarda roles en `localStorage`; el rol proviene siempre de la API. La sesión sí usa `localStorage`, pero solo para los datos públicos del usuario autenticado y su expiración.

## Operaciones de Mantenimiento de Usuarios

- **Alta (POST):** Formulario lateral con validación cliente/servidor y refresco automático.
- **Edición (PUT):** el formulario lateral (mismo componente del alta) se reutiliza para editar nombre completo, nombre de usuario, correo, rol y estado. La contraseña es opcional: si se omite se conserva la actual; si se escribe se valida con la política y se vuelve a hashear con BCrypt.
- **Baja lógica (DELETE):** Confirmación interactiva que marca el usuario como `activo = 0`. El Administrador está protegido y no puede darse de baja.
- **Reactivación (POST /reactivar):** Confirmación para restaurar el acceso al usuario (`activo = 1`).
- **Filtros avanzados:** Búsqueda textual en tiempo real, filtro por estado (Todos, Activos, Inactivos) y filtro por rol (Administrador, Odontólogo, Recepcionista).

## Limitaciones conocidas

- **Correo SMTP:** el envío real se configura en la sección `Email` de `GestionOdontologica.Api/appsettings.json` (`Habilitado`, `Host`, `Puerto`, `UsarSsl`, `Usuario`, `Password`, `Remitente`, `FrontendBaseUrl`). Con `Email:Habilitado=false` la API registra un aviso y no envía; en `Development` el token de demostración permite completar el flujo sin servidor de correo.
- **Sesión local:** no se emite JWT ni se firma la sesión; el cliente guarda solo datos públicos con `expiraEn`. La API valida las credenciales en el servidor y el guard de rutas evita el acceso casual a las páginas internas.
- **Campos del formulario:** nombre completo, nombre de usuario, correo, contraseña y rol. `NOMBRE_USUARIO` y `CORREO` son únicos en la tabla `USUARIOS`; un duplicado devuelve `409` y el formulario marca ambos campos.
