🏥 Examen Parcial: Sistema Médico — Gestión de Consultas y Pacientes

Stack: ASP.NET Core MVC (.NET 8) + Identity + EF Core (MySQL o SQLite) + Redis (sesiones/cache) + Razor Views
Infraestructura: Render.com (Web Service)
Cache/Sesiones: Redis
Control de versiones: GitHub (ramas por módulo + PRs hacia main)

🚀 Descripción

Sistema médico en entorno web que permite la gestión de pacientes, médicos y consultas.
Los usuarios pueden autenticarse, registrar y visualizar citas, mientras que los administradores gestionan los datos clínicos y horarios.

⚕️ Funcionalidades principales

Autenticación: Login seguro con Identity para médicos y administradores.

Gestión de pacientes: registro, edición, eliminación y listado.

Gestión de médicos: creación, actualización y asignación de especialidades.

Consultas médicas: programación, visualización y validaciones de horario.

Validaciones:

Fecha de consulta ≥ fecha actual.

No se permiten dos consultas para el mismo médico en el mismo horario.

Paciente no puede tener dos consultas simultáneas.

Panel administrativo: control general de consultas, pacientes y médicos.

📂 Ramas del proyecto
Rama	Descripción
feature/base-dominio	Proyecto base con modelos principales (Paciente, Médico, Consulta)
feature/pacientes	CRUD de pacientes
feature/medicos	CRUD de médicos y especialidades
feature/consultas	Registro y validación de consultas médicas
feature/sesion-redis	Configuración de sesiones y cache con Redis
feature/panel-admin	Panel de administración con reportes
deploy/render	Configuración de despliegue en Render
🧱 Estructura del proyecto

El proyecto está dividido en capas para mantener la separación de responsabilidades:

Controllers: manejo de peticiones HTTP y lógica de flujo.

Models: definición de entidades y relaciones (Paciente, Médico, Consulta).

Data: configuración de Entity Framework Core y acceso a datos.

Views: vistas Razor para la interfaz de usuario.

wwwroot: archivos estáticos (CSS, JS, imágenes).

⚙️ Configuración

Variables de entorno principales:

ASPNETCORE_ENVIRONMENT=Production

ASPNETCORE_URLS=https://pruebasprogramacion.onrender.com

ConnectionStrings__DefaultConnection

Redis__ConnectionString

Cuenta del Administrador (por defecto):

Email: elliot@ecorp.com
Password: elliot@ecorp.com

🐳 Dockerfile

El proyecto cuenta con un Dockerfile personalizado que:

Realiza el build de la aplicación ASP.NET Core (.NET 8).

Publica la aplicación en una imagen ligera.

La deja lista para despliegue en Render o Railway.