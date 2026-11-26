# Gestión Odontológica — Sprint 1 (HU-18)

Backend en **ASP.NET Core Web API (.NET 8)** para la gestión de usuarios y el acceso de un sistema odontológico. La persistencia se realiza en **Oracle** exclusivamente mediante **procedimientos almacenados independientes** (sin SQL directo en los repositorios), sobre las tablas `USUARIOS` y `SOLICITUDES_PASSWORD`.

La solución incluye un **frontend independiente en Blazor WebAssembly (.NET 8)** que consume la API exclusivamente por HTTP/JSON. Es una solución aparte: no comparte proyectos, código ni referencias con el backend. El frontend incorpora pantalla de login, recuperación y restablecimiento de contraseña, guard de rutas y una sesión local con vigencia.

- `GestionOdontologica.Api/` — Web API REST y acceso a Oracle.
- `GestionOdontologica.Frontend/` — interfaz Blazor WebAssembly del Sprint 1 (HU-18).
- `GestionOdontologica.Frontend.sln` — solución independiente del frontend.

## Arquitectura

```text
GestionOdontologica.Api/
├── Controllers/
│   ├── UsuariosController.cs
│   └── AuthController.cs          # Login, recuperación y restablecimiento
├── Data/
│   ├── IOracleConnectionFactory.cs
│   ├── OracleConnectionFactory.cs
│   ├── IUsuarioRepository.cs
│   ├── UsuarioRepository.cs       # CRUD vía procedimientos sp_*
│   ├── IAuthRepository.cs
│   ├── AuthRepository.cs          # Login, bloqueo y tokens vía sp_*
│   └── OracleParameterExtensions.cs
├── DTOs/
│   ├── UsuarioDto.cs
│   ├── CreacionUsuarioDto.cs
│   ├── ActualizacionUsuarioDto.cs
│   ├── LoginRequestDto.cs / LoginResponseDto.cs
│   └── RecuperarPassword... / RestablecerPassword... / MensajeResponseDto.cs
├── Exceptions/
│   └── UsuarioDuplicadoException.cs
├── Infrastructure/
│   └── GlobalExceptionHandler.cs
├── Security/
│   └── PasswordHasher.cs          # BCrypt (hash y verificación en C#)
├── Properties/
│   └── launchSettings.json
├── Program.cs
├── appsettings.json
└── GestionOdontologica.Api.csproj

GestionOdontologica.Frontend/
├── Components/
│   ├── AuthShell.razor           # Marco visual compartido de las pantallas de acceso
│   ├── ConfirmModal.razor        # Confirmación flotante de baja y reactivación
│   ├── ModuloPendiente.razor
│   ├── RedirectToLogin.razor     # Redirección del guard de autorización
│   ├── ToastContainer.razor
│   ├── UsuarioForm.razor         # Formulario unificado de alta y edición (Master-Detail)
│   └── UsuariosTable.razor       # Tabla con filtros profesionales (tabs, roles) y acciones Blazor UI
├── Layout/
│   ├── EmptyLayout.razor         # Layout sin sidebar para login y recuperación
│   └── MainLayout.razor          # Sidebar, topbar, perfil real, expiración de sesión y toasts
├── Models/
│   ├── AuthModels.cs             # Contratos de autenticación y sesión
│   ├── AuthFormModels.cs         # Validaciones de login/recuperación/restablecimiento
│   ├── ActualizacionUsuarioDto.cs
│   ├── CreacionUsuarioDto.cs
│   ├── ProblemDetailsDto.cs
│   ├── UsuarioDto.cs
│   ├── UsuarioEdicionModel.cs
│   ├── UsuarioFormModel.cs
│   └── UsuarioVista.cs
├── Pages/
│   ├── Login.razor               # Inicio de sesión con returnUrl y Recordarme
│   ├── RecuperarPassword.razor   # Solicitud de recuperación
│   ├── RestablecerPassword.razor # Nueva contraseña con token
│   ├── Usuarios.razor            # Flujo completo de mantenimiento
│   ├── Inicio.razor / Roles.razor / Citas.razor / Pacientes.razor / Reportes.razor
│   └── Home.razor                # Redirección a /usuarios
├── Services/
│   ├── ApiException.cs
│   ├── ApiStatusService.cs
│   ├── AuthApiService.cs         # POST /api/auth/*
│   ├── SesionService.cs          # Sesión en localStorage con expiración
│   ├── SesionAuthenticationStateProvider.cs
│   ├── ToastService.cs
│   └── UsuariosApiService.cs     # GET/POST/PUT/DELETE /api/usuarios y reactivación
├── wwwroot/
│   ├── appsettings.json          # Api:BaseUrl
│   ├── css/app.css
│   └── index.html
├── App.razor
├── _Imports.razor
├── Program.cs
└── GestionOdontologica.Frontend.csproj

database/
└── sp_usuarios.sql               # Esquema, migraciones y procedimientos almacenados
```

Flujo de una petición:

```text
Frontend (Blazor WASM) → HTTP/JSON → UsuariosController/AuthController → Repository → sp_* → tablas Oracle
```

## Requisitos

- .NET 8 SDK (incluye las herramientas de Blazor WebAssembly)
- Oracle Database XE 21c o superior (o una instancia remota)
- Esquema con la tabla `USUARIOS` (incluye `INTENTOS_FALLIDOS`, `BLOQUEADO_HASTA` y `ULTIMO_ACCESO`), la tabla `SOLICITUDES_PASSWORD` y los procedimientos almacenados `sp_*` (ver `database/sp_usuarios.sql`)
- NuGet:
  - `Oracle.ManagedDataAccess.Core` 23.26.400
  - `BCrypt.Net-Next` 4.2.1
  - `Swashbuckle.AspNetCore` 6.6.2

## Configuración de Oracle

La cadena de conexión de producción se encuentra en `GestionOdontologica.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=SYSTEM;Password=CAMBIAR_ESTA_CLAVE;Data Source=localhost:1521/xe;Pooling=true;Connection Timeout=60;"
  }
}
```

Para Oracle XE 21c, el servicio suele ser `XEPDB1`:

```text
Data Source=localhost:1521/XEPDB1
```

Para no guardar la contraseña en el repositorio, se recomienda usar User Secrets en desarrollo:

```powershell
cd GestionOdontologica.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Oracle" "User Id=TU_USUARIO;Password=TU_CLAVE;Data Source=localhost:1521/XEPDB1"
```

También se puede sobrescribir con una variable de entorno:

```powershell
$env:ConnectionStrings__Oracle = "User Id=TU_USUARIO;Password=TU_CLAVE;Data Source=localhost:1521/XEPDB1"
```

> En producción se recomienda crear un usuario de aplicación con permisos mínimos, en lugar de usar `SYSTEM`.

### Ambientes: Development (QA) y Production

El proyecto `GestionOdontologica.Api` resuelve la cadena de conexión en este orden:

1. `ConnectionStrings:DefaultConnection` — definida en `appsettings.Development.json` (ambiente de QA).
2. `ConnectionStrings:Oracle` — definida en `appsettings.json` (producción, sin cambios).

`appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=bd_odontologia_qa;Password=Clave123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)))"
  }
}
```

`appsettings.json` (producción) conserva su propia cadena `Oracle`, por lo que al ejecutar con `ASPNETCORE_ENVIRONMENT=Production` no se toca el ambiente de QA.

Verificado en este equipo (ambos esquemas con la misma estructura):

| Entorno | Cadena usada | Pruebas realizadas |
|---|---|---|
| `Development` | `bd_odontologia_qa` @ `XEPDB1` | CRUD completo, login `200/401/403/423`, bloqueo a los 5 intentos, recuperación y restablecimiento de contraseña |
| `Production` | `SYSTEM` @ `xe` | CRUD completo, creación de los procedimientos `sp_*` y login |

> **Nota:** el repositorio usa la estructura actual de `USUARIOS` (`NOMBRE_COMPLETO`, `NOMBRE_USUARIO`, `CORREO`, `PASSWORD`, `ROL`, `ACTIVO`, `INTENTOS_FALLIDOS`, `BLOQUEADO_HASTA`, `ULTIMO_ACCESO`). Ambos esquemas de este equipo fueron alineados mediante `database/sp_usuarios.sql`.

### Base de datos de producción en este equipo

La cadena de `appsettings.json` apunta al esquema `SYSTEM` del servicio `xe`:

- Se desbloqueó la cuenta con `ALTER USER SYSTEM ACCOUNT UNLOCK;` (estaba en `ORA-28000`).
- La tabla `USUARIOS` de `SYSTEM` usa la misma estructura que QA, con las restricciones `PK_USUARIOS`, `UQ_USUARIOS_CORREO`, `UQ_USUARIOS_NOMBRE_USUARIO` y `CHK_USUARIOS_ACTIVO`, más la tabla `SOLICITUDES_PASSWORD` y los procedimientos `sp_*`.
- Al ejecutar la API con `--environment Production` se usa automáticamente esta cadena.

> **Seguridad:** la contraseña está en texto plano en `appsettings.json`. No subas ese archivo con credenciales reales a un repositorio compartido; usa User Secrets o `ConnectionStrings__Oracle` para esos entornos.

Si prefieres un usuario de aplicación con permisos mínimos en lugar de `SYSTEM`, crea un esquema en el PDB:

```sql
ALTER SESSION SET CONTAINER = XEPDB1;
CREATE USER odontocare IDENTIFIED BY "TU_CLAVE" DEFAULT TABLESPACE users QUOTA UNLIMITED ON users;
GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE TO odontocare;
```

Después ejecuta `database/sp_usuarios.sql` con ese usuario y apunta la cadena a `Data Source=localhost:1521/XEPDB1`. El script es idempotente: crea el esquema si falta, aplica las columnas nuevas y crea o actualiza los procedimientos almacenados.

## Ejecutar la API

```powershell
dotnet restore
dotnet run --project .\GestionOdontologica.Api\GestionOdontologica.Api.csproj
```

Swagger queda disponible en:

```text
http://localhost:5080/swagger
```

o, con el perfil HTTPS:

```text
https://localhost:7080/swagger
```

Para forzar el ambiente de producción sin cambiar archivos:

```powershell
dotnet run --project .\GestionOdontologica.Api\GestionOdontologica.Api.csproj --no-launch-profile --environment Production --urls http://localhost:5080
```

## Ejecutar el frontend

El frontend es una aplicación Blazor WebAssembly independiente. Se compila y ejecuta con el SDK de .NET, sin Node.js ni servidores estáticos externos:

```powershell
dotnet run --project .\GestionOdontologica.Frontend\GestionOdontologica.Frontend.csproj
```

La aplicación queda disponible en:

```text
http://localhost:5501/login
```

La raíz `/` redirige al login si no hay sesión o a `/usuarios` si ya iniciaste sesión. Las rutas internas (`/usuarios`, `/inicio`, `/roles`, etc.) están protegidas: sin sesión vigente redirigen a `/login?returnUrl=...`.

El perfil `http` de `Properties/launchSettings.json` ya usa el puerto **5501** (el 5500 suele estar ocupado por el listener de Oracle XE en algunos equipos).

La URL base de la API se configura en `GestionOdontologica.Frontend/wwwroot/appsettings.json`:

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5080/api/"
  }
}
```

Si ejecutas la API con el perfil HTTPS, usa `https://localhost:7080/api/`. Cuando la clave no existe o no es una URL absoluta, el cliente cae en `http://localhost:5080/api/` como valor por defecto.

Una vez autenticado, la interfaz carga el listado con `GET /api/usuarios`, crea usuarios con `POST /api/usuarios` y vuelve a consultar el listado automáticamente después de un `201 Created`. El mantenimiento completo se opera desde la misma pantalla: editar con `PUT /api/usuarios/{id}`, dar de baja con `DELETE /api/usuarios/{id}` y reactivar con `POST /api/usuarios/{id}/reactivar`. También incluye búsqueda local, filtros por estado y rol, validación con `DataAnnotations`, manejo de `400`/`404`/`409`/`500`, estados de carga, vacío, error y sin conexión.

Compilar solo el frontend:

```powershell
dotnet build .\GestionOdontologica.Frontend.sln
```

### CORS

La API permite los orígenes del frontend mediante la política `Frontend` registrada en `Program.cs` y configurada en `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5500",
      "http://127.0.0.1:5500",
      "http://localhost:5501",
      "http://127.0.0.1:5501"
    ]
  }
}
```

Si publicas la interfaz en otro host o puerto, agrega ese origen al arreglo y reinicia la API. `app.UseCors("Frontend")` debe ejecutarse antes de `MapControllers()`.

### Mantenimiento de usuarios en la interfaz

- **Operaciones completas:** la interfaz incluye el flujo integral de mantenimiento: alta (`POST`), consulta y búsqueda en tiempo real (`GET`), edición reutilizando el formulario de la izquierda (`PUT`), baja lógica con diálogo de confirmación (`DELETE`) y reactivación de cuentas (`POST /api/usuarios/{id}/reactivar`).
- **Edición:** al pulsar **Editar** en una fila, el formulario de la izquierda se rellena con los datos del usuario (`NOMBRE_COMPLETO`, `NOMBRE_USUARIO`, `CORREO`, `ROL`, `ACTIVO`). La contraseña es **opcional**: si se deja vacía se conserva la clave actual; si se escribe, se valida (8–72 caracteres) y se vuelve a hashear con BCrypt. El botón **Descartar cambios** cancela la edición.
- **Baja y reactivación:** **Eliminar** abre un diálogo de confirmación y ejecuta la baja lógica (`ACTIVO = 0`); el registro se conserva y puede restaurarse con **Activar**. Ambas acciones refrescan la tabla y muestran un toast.
- **Estado:** el formulario de edición permite alternar entre *Activo* e *Inactivo*. La tabla muestra el estado con indicadores y permite filtrar por estado y por rol.
- **Roles:** el rol (`Administrador`, `Odontólogo`, `Recepcionista`) se gestiona tanto en el alta como en la edición y se persiste en la columna `ROL` de la tabla `USUARIOS`. La tabla lo muestra con badges contextuales.
- **Autenticación:** el acceso requiere iniciar sesión. El perfil del sidebar muestra el nombre real, el rol y las iniciales del usuario autenticado; **Cerrar sesión** elimina la sesión local y vuelve al login. La sesión expira automáticamente y el guard de rutas impide abrir páginas internas sin autenticación.
- **Datos del formulario:** nombre completo, nombre de usuario, correo, contraseña y rol. `NOMBRE_USUARIO` y `CORREO` son únicos, por lo que un duplicado de cualquiera de los dos devuelve `409` y la interfaz lo resalta oportunamente.
- **Módulos pendientes:** Inicio, Roles, Citas, Pacientes y Reportes son rutas informativas (“Módulo en preparación”) hasta que existan sus HU.

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| `POST` | `/api/auth/login` | Valida credenciales y devuelve los datos públicos del usuario | `200`, `400`, `401`, `403`, `423`, `500` |
| `POST` | `/api/auth/recuperar-password` | Registra una solicitud de recuperación (respuesta genérica) | `200`, `400`, `500` |
| `POST` | `/api/auth/restablecer-password` | Restablece la contraseña con un token vigente | `200`, `400`, `500` |
| `GET` | `/api/usuarios` | Lista todos los usuarios, incluidos los inactivos | `200`, `500` |
| `GET` | `/api/usuarios/{id}` | Obtiene un usuario por ID | `200`, `404`, `500` |
| `POST` | `/api/usuarios` | Crea un usuario | `201`, `400`, `409`, `500` |
| `PUT` | `/api/usuarios/{id}` | Actualiza un usuario (incluye contraseña opcional) | `200`, `400`, `404`, `409`, `500` |
| `DELETE` | `/api/usuarios/{id}` | Baja lógica: `activo = 0` | `200`, `404`, `500` |
| `POST` | `/api/usuarios/{id}/reactivar` | Reactiva un usuario: `activo = 1` | `200`, `404`, `500` |

### Iniciar sesión

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "identificador": "aperez",
  "password": "ClaveSegura123",
  "recordarme": false
}
```

`identificador` acepta el nombre de usuario o el correo electrónico. Respuesta `200 OK`:

```json
{
  "idUsuario": 1,
  "nombreCompleto": "Ana María Pérez",
  "nombreUsuario": "aperez",
  "correo": "ana.perez@example.com",
  "rol": "Odontólogo",
  "expiraEn": "2026-10-27T10:45:00Z"
}
```

Respuestas de error:

| Estado | Cuándo | `detail` |
|---|---|---|
| `401` | Usuario o contraseña incorrectos | Mensaje genérico, sin revelar cuál falló |
| `403` | La cuenta está desactivada | Invita a contactar al administrador |
| `423` | La cuenta está bloqueada temporalmente | Indica los minutos restantes y envía `Retry-After` |

Política de bloqueo: tras **5 intentos fallidos** la cuenta se bloquea **15 minutos**. Un login exitoso o un restablecimiento de contraseña reinician el contador y eliminan el bloqueo.

### Recuperar contraseña

```http
POST /api/auth/recuperar-password
Content-Type: application/json
```

```json
{
  "correo": "ana.perez@example.com"
}
```

La respuesta es siempre `200` con un mensaje genérico, exista o no el correo, para no revelar qué cuentas están registradas. El token se genera en C# con `RandomNumberGenerator.GetHexString(32)`, se guarda **hasheado** (`STANDARD_HASH` SHA-256) en `SOLICITUDES_PASSWORD` y vence a los **30 minutos**. Solo se puede usar una vez y las solicitudes anteriores del mismo usuario se invalidan.

> **Desarrollo sin SMTP:** como el proyecto no tiene servidor de correo configurado, en ambiente `Development` la respuesta incluye `tokenDesarrollo` y `expiraEnMinutos`. El frontend muestra ese token y permite continuar el flujo. En `Production` esos campos siempre son `null`.

### Restablecer contraseña

```http
POST /api/auth/restablecer-password
Content-Type: application/json
```

```json
{
  "token": "0D658555A1CC9FAE5D46F1064AE7F022",
  "password": "NuevaClave789"
}
```

Si el token es inválido, expiró o ya se usó, devuelve `400`. Un restablecimiento exitoso también limpia `INTENTOS_FALLIDOS` y `BLOQUEADO_HASTA`.

### Sesión del frontend

- La sesión se guarda en `localStorage` bajo `odontocare.sesion.v1` con los datos públicos del usuario y `expiraEn`; **nunca** incluye contraseña, hash ni token.
- Duración: **8 horas**; con **Recordarme** se extiende a **7 días**.
- `SesionAuthenticationStateProvider` construye la identidad de Blazor y `AuthorizeRouteView` protege las rutas. Si no hay sesión, `RedirectToLogin` navega a `/login?returnUrl=...`.
- `MainLayout` programa el cierre automático al vencer la sesión y redirige a `/login?motivo=expirada`. Al cerrar sesión manualmente se usa `?motivo=sesion-cerrada`.
- No se emite JWT: la API valida las credenciales en el servidor y el cliente conserva solo datos no sensibles con vigencia local.

### Crear usuario

```http
POST /api/usuarios
Content-Type: application/json
```

```json
{
  "nombreCompleto": "Ana María Pérez",
  "nombreUsuario": "aperez",
  "correo": "ana.perez@example.com",
  "password": "ClaveSegura123",
  "rol": "Odontólogo"
}
```

Restricciones del payload:

- `nombreCompleto`: 3–150 caracteres.
- `nombreUsuario`: 3–50 caracteres, solo letras, números, punto, guion y guion bajo. Único.
- `correo`: formato de correo válido, máximo 150 caracteres. Único.
- `password`: 8–72 caracteres (BCrypt trunca a 72 bytes).
- `rol`: 3–50 caracteres.

Respuesta `201 Created`:

```json
{
  "idUsuario": 1,
  "nombreCompleto": "Ana María Pérez",
  "nombreUsuario": "aperez",
  "correo": "ana.perez@example.com",
  "rol": "Odontólogo",
  "activo": true
}
```

La cabecera `Location` apunta a `GET /api/usuarios/1`.

### Actualizar usuario

```http
PUT /api/usuarios/1
Content-Type: application/json
```

```json
{
  "nombreCompleto": "Ana María Pérez Gómez",
  "nombreUsuario": "aperez",
  "correo": "ana.perez@example.com",
  "rol": "Recepcionista",
  "activo": true
}
```

`rol` y `activo` son opcionales. Si se omiten, se conservan los valores actuales del usuario.

### Desactivar usuario

```http
DELETE /api/usuarios/1
```

Respuesta `200 OK`:

```json
{
  "idUsuario": 1,
  "activo": false,
  "mensaje": "Usuario desactivado correctamente."
}
```

### Reactivar usuario

```http
POST /api/usuarios/1/reactivar
Content-Type: application/json
```

Respuesta `200 OK`:

```json
{
  "idUsuario": 1,
  "activo": true,
  "mensaje": "Usuario reactivado correctamente."
}
```

La reactivación restaura el acceso (`ACTIVO = 1`) sin modificar los demás datos del usuario.

## Decisiones de implementación

- **Contraseñas:** se hashean con BCrypt (`work factor = 12`) en C# antes de llamar al procedimiento y el hash de 60 caracteres se guarda en `PASSWORD`. La verificación también se hace en C# porque Oracle no incluye BCrypt. El DTO de lectura no incluye la contraseña ni el hash.
- **Acceso a datos:** todo el CRUD y la autenticación pasan por procedimientos almacenados independientes (`sp_*`). No hay `SELECT`/`INSERT`/`UPDATE`/`DELETE` directos en los repositorios; solo llamadas a procedimientos con parámetros Oracle y cursores de salida (`SYS_REFCURSOR`).
- **Listado:** `sp_listar_usuarios` devuelve un cursor con todos los usuarios.
- **Consulta individual:** `sp_obtener_usuario` devuelve el usuario solicitado por `p_id_usuario`.
- **Alta:** `sp_insertar_usuario` inserta y devuelve el identificador generado; luego se consulta el usuario insertado para construir el DTO y la cabecera `Location`.
- **Actualización y baja:** `sp_actualizar_usuario`, `sp_desactivar_usuario` y `sp_activar_usuario` encapsulan los `UPDATE` y la lógica de nulos opcionales (`rol`, `activo`, `password`).
- **Login y bloqueo:** `sp_obtener_credenciales` entrega el hash, el estado, los intentos fallidos y los segundos de bloqueo; `sp_registrar_intento_fallido` incrementa el contador y aplica el bloqueo; `sp_registrar_acceso` limpia intentos y registra `ULTIMO_ACCESO`.
- **Recuperación:** `sp_solicitar_recuperacion` recibe el token ya generado en C# y guarda solo su hash SHA-256; `sp_restablecer_password` valida el token vigente y de un solo uso, actualiza la clave y limpia el bloqueo.
- **Duplicados:** Oracle devuelve `ORA-00001` por las restricciones únicas de `CORREO` o `NOMBRE_USUARIO`; el repositorio lo transforma en `UsuarioDuplicadoException` y el manejador global responde `409 Conflict` con un mensaje genérico (no revela cuál de los dos campos chocó).
- **Errores:** `GlobalExceptionHandler` devuelve `ProblemDetails` para errores de negocio, Oracle y errores inesperados, sin exponer detalles internos.
- **AutoCommit:** la conexión ODP.NET usa `AutoCommit = true` para que los procedimientos persistan al finalizar cada comando. Si en el futuro se requiere atomicidad entre varias operaciones, se debe usar una `OracleTransaction` y desactivar AutoCommit.
- **Cancelación:** todos los métodos aceptan `CancellationToken` y lo propagan a las operaciones asíncronas de Oracle.

## Base de datos

Todo el acceso a datos pasa por procedimientos almacenados independientes (`sp_*`). El script `database/sp_usuarios.sql` es idempotente y contiene:

1. Creación de `USUARIOS` y `SOLICITUDES_PASSWORD` si no existen.
2. Migración de columnas nuevas (`INTENTOS_FALLIDOS`, `BLOQUEADO_HASTA`, `ULTIMO_ACCESO`).
3. Índice `IX_SOLICITUDES_TOKEN` sobre `SOLICITUDES_PASSWORD(TOKEN_HASH)`.
4. Los 11 procedimientos almacenados `sp_*` con `CREATE OR REPLACE PROCEDURE`.
5. Eliminación del paquete anterior `PKG_USUARIOS` si todavía existe (migración).

Tablas:

```sql
CREATE TABLE usuarios (
    id_usuario          NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    nombre_completo     VARCHAR2(150) NOT NULL,
    nombre_usuario      VARCHAR2(50)  NOT NULL,
    correo              VARCHAR2(150) NOT NULL,
    password            VARCHAR2(255) NOT NULL,
    rol                 VARCHAR2(50)  DEFAULT 'Odontologo' NOT NULL,
    activo              NUMBER(1)     DEFAULT 1 NOT NULL,
    fecha_creacion      TIMESTAMP     DEFAULT CURRENT_TIMESTAMP NOT NULL,
    fecha_modificacion  TIMESTAMP     DEFAULT CURRENT_TIMESTAMP NOT NULL,
    intentos_fallidos   NUMBER(3)     DEFAULT 0 NOT NULL,
    bloqueado_hasta     TIMESTAMP,
    ultimo_acceso       TIMESTAMP,
    CONSTRAINT uq_usuarios_correo UNIQUE (correo),
    CONSTRAINT uq_usuarios_nombre_usuario UNIQUE (nombre_usuario),
    CONSTRAINT chk_usuarios_activo CHECK (activo IN (0, 1))
);

CREATE TABLE solicitudes_password (
    id_solicitud      NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    id_usuario        NUMBER NOT NULL,
    correo            VARCHAR2(150) NOT NULL,
    token_hash        RAW(32) NOT NULL,
    fecha_solicitud   TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    fecha_expiracion  TIMESTAMP NOT NULL,
    usado             NUMBER(1) DEFAULT 0 NOT NULL,
    fecha_uso         TIMESTAMP,
    CONSTRAINT fk_solicitudes_usuario
        FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
    CONSTRAINT ck_solicitudes_usado CHECK (usado IN (0, 1))
);
```

Procedimientos almacenados:

| Procedimiento | Uso |
|---|---|
| `sp_listar_usuarios` | Lista todos los usuarios (cursor) |
| `sp_obtener_usuario` | Obtiene un usuario por ID (cursor) |
| `sp_insertar_usuario` | Alta y retorno del ID generado |
| `sp_actualizar_usuario` | Edición con contraseña/rol/estado opcionales |
| `sp_desactivar_usuario` / `sp_activar_usuario` | Baja lógica y reactivación |
| `sp_obtener_credenciales` | Hash, estado, intentos y segundos de bloqueo |
| `sp_registrar_intento_fallido` | Incremento de intentos y bloqueo temporal |
| `sp_registrar_acceso` | Reinicio de intentos y registro de `ULTIMO_ACCESO` |
| `sp_solicitar_recuperacion` | Registro de token hasheado con vigencia |
| `sp_restablecer_password` | Validación del token y cambio de clave |

El usuario de conexión necesita `CREATE SESSION`, permisos DML sobre ambas tablas y `EXECUTE` sobre cada procedimiento:

```sql
GRANT CREATE SESSION TO TU_USUARIO;
GRANT SELECT, INSERT, UPDATE, DELETE ON TU_ESQUEMA.USUARIOS TO TU_USUARIO;
GRANT SELECT, INSERT, UPDATE, DELETE ON TU_ESQUEMA.SOLICITUDES_PASSWORD TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_listar_usuarios TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_obtener_usuario TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_insertar_usuario TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_actualizar_usuario TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_desactivar_usuario TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_activar_usuario TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_obtener_credenciales TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_registrar_intento_fallido TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_registrar_acceso TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_solicitar_recuperacion TO TU_USUARIO;
GRANT EXECUTE ON TU_ESQUEMA.sp_restablecer_password TO TU_USUARIO;
```

> Si el usuario de la API es el mismo dueño del esquema, los `GRANT` no son necesarios: Oracle permite ejecutar los procedimientos propios.

Para aplicar el script:

```powershell
$env:NLS_LANG = "AMERICAN_AMERICA.AL32UTF8"
Get-Content .\database\sp_usuarios.sql | & "C:\oraclexe\dbhomeXE\bin\sqlplus.exe" -S "TU_USUARIO/TU_CLAVE@localhost:1521/XEPDB1"
```

Al final debe aparecer `Procedimientos de usuarios y autenticación listos.` y puedes validar el estado con:

```sql
SELECT object_name, status
  FROM user_objects
 WHERE object_type = 'PROCEDURE'
   AND object_name LIKE 'SP_%'
 ORDER BY object_name;
```

Los 11 procedimientos deben quedar en estado `VALID`. Si el esquema venía del paquete anterior, este script lo elimina automáticamente.

## Pruebas rápidas

1. Ejecutar `database/sp_usuarios.sql` en el esquema de pruebas (crea tablas, migra columnas y compila los procedimientos `sp_*`).
2. Configurar la cadena de conexión (`DefaultConnection` en `appsettings.Development.json`).
3. Ejecutar `dotnet run --project .\GestionOdontologica.Api\GestionOdontologica.Api.csproj`.
4. Abrir Swagger. Si el esquema no tiene usuarios, crear uno con `POST /api/usuarios` (la contraseña se guarda hasheada). Luego probar `POST /api/auth/login`, los códigos `401/403/423`, la recuperación, el restablecimiento y el CRUD completo (`GET`, `PUT`, `DELETE`, `POST /{id}/reactivar`).
5. En otra terminal, ejecutar `dotnet run --project .\GestionOdontologica.Frontend\GestionOdontologica.Frontend.csproj`.
6. Abrir `http://localhost:5501/login` e iniciar sesión. Verificar las validaciones del formulario, el mensaje de credenciales incorrectas, el guard de rutas, el cierre de sesión, la recuperación con token de desarrollo, el alta con refresco automático, la edición sin cambiar contraseña, el cambio de clave, la baja con confirmación, la reactivación y los filtros por estado y rol.

## Posibles errores

| Error | Causa probable |
|---|---|
| `ORA-12514` | El servicio configurado en `Data Source` no existe. Prueba `xe` o `XEPDB1`. |
| `ORA-28000` | La cuenta está bloqueada (típico en `SYSTEM` tras varios intentos fallidos). Desbloquéala con `ALTER USER ... ACCOUNT UNLOCK;`. |
| `ORA-00942` | La tabla `USUARIOS`, `SOLICITUDES_PASSWORD` o los procedimientos `sp_*` no existen en el esquema del usuario conectado. Ejecuta `database/sp_usuarios.sql`. |
| `ORA-00904` | La tabla existe pero no tiene alguna de las columnas esperadas (`NOMBRE_COMPLETO`, `NOMBRE_USUARIO`, `CORREO`, `PASSWORD`, `ROL`, `ACTIVO`, `INTENTOS_FALLIDOS`, `BLOQUEADO_HASTA`, `ULTIMO_ACCESO`). Ejecuta el script para aplicar las migraciones. |
| `ORA-04063` | Un procedimiento `sp_*` está inválido. Recompílalo con `ALTER PROCEDURE sp_nombre COMPILE;` y revisa `USER_ERRORS`. |
| `ORA-00001` | `CORREO` o `NOMBRE_USUARIO` duplicado. La API lo devuelve como `409 Conflict`. |
| `ORA-01031` | El usuario no tiene permisos DML sobre las tablas o `EXECUTE` sobre los procedimientos `sp_*`. |
| Login `401` | Usuario/correo o contraseña incorrectos. El mensaje es genérico por seguridad. |
| Login `403` | La cuenta está desactivada; reactívala desde el mantenimiento de usuarios. |
| Login `423` | Se alcanzaron 5 intentos fallidos. Espera los minutos indicados o restablece la contraseña. |
| Frontend: «API sin conexión» | La API no está escuchando en la URL configurada (`http://localhost:5080/api/`). Inicia el backend. |
| Frontend: «API con error» + «Error de base de datos» | La API responde, pero Oracle rechaza la conexión. Revisa la cadena y el estado de la cuenta. |
| `address already in use` en el puerto 5501 | Ya hay otra instancia del frontend ejecutándose. Ciérrala con `Ctrl+C` o libera el puerto. |
