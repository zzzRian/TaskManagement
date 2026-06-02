# Task Management System — ASP.NET Core MVC (.NET 8) + MySQL

Sistema de gestión de proyectos y tareas con RBAC (Admin / Cliente),
Kanban, Calendario, Reportes (PDF / Excel) y Auditoría.

## Tecnologías
- ASP.NET Core MVC (.NET 8) + Razor Views
- Entity Framework Core 8 + Pomelo MySQL provider
- ASP.NET Core Identity (RBAC con roles + permisos)
- Bootstrap 5, jQuery, AJAX, Chart.js, FullCalendar
- ClosedXML (Excel), QuestPDF (PDF)

## Requisitos
- .NET 8 SDK
- MySQL Server 8.x + MySQL Workbench
- (Opcional) Visual Studio Code con extensión C#

## Configuración
Edita `appsettings.json` y ajusta la cadena de conexión:

```
"DefaultConnection": "Server=localhost;Port=3306;Database=TaskManagementDb;User=root;Password=root;"
```

## Ejecutar

```bash
dotnet restore
dotnet tool install --global dotnet-ef       # solo la primera vez
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Al iniciar, se crean automáticamente:
- Roles: `Admin`, `Client`
- Permisos del sistema (RBAC)
- Usuario admin: **admin@admin.com / Admin123**
- Usuario cliente demo: **client@client.com / Client123**

## Estructura

```
Controllers/    AccountController, DashboardController, UsersController, RolesController,
                ProjectsController, TasksController, KanbanController, CalendarController,
                ReportsController, NotificationsController, AuditController, ProfileController
Models/Entities ApplicationUser, ApplicationRole, Permission, RolePermission,
                Project, ProjectMember, TaskItem, TaskComment, TaskAttachment,
                Notification, AuditLog
Models/ViewModels Login, Register, User, Project, Task, Dashboard, Report, Profile
Services/       Auth, User, Role, Project, Task, Kanban, Calendar, Report, Notification, Audit
Repositories/   GenericRepository, UnitOfWork, UserRepository, ProjectRepository, TaskRepository
Middlewares/    ExceptionMiddleware, AuditMiddleware
Data/           ApplicationDbContext, DbSeeder
Views/          Razor views por controlador + _Layout
```

## Roles

### Administrador
Acceso completo: dashboard, usuarios, roles/permisos, proyectos, tareas,
kanban global, calendario global, reportes, auditoría, configuración.

### Cliente
Acceso restringido: dashboard personal, sus tareas, kanban personal,
calendario personal, reportes simplificados, perfil.

El Cliente **no** puede crear/editar/eliminar proyectos, gestionar usuarios,
roles, auditoría, ni ver tareas de otros usuarios.

## Script SQL inicial
Ver `database.sql`. EF Core también creará el esquema automáticamente con `database update`.

Miesntras se desarrolla de manera local
ApplicationDbContextFactory.cs
string conn =
    "Server=127.0.0.1;Port=3306;Database=TaskManagementDb;User=root;Password=MyNewPass12;";