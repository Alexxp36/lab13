# Laboratorio 13 · Avance Code First

Proyecto ASP.NET Core Web API con controladores y OpenAPI. Usa .NET 9, el SDK instalado en el equipo.

1. Abre **APIDemoLab.csproj** en Visual Studio y revisa las carpetas Controllers, Models, DTO/Request y DTO/Response.
2. Revisa los modelos Course, Grade, Student y Enrollment. Las claves y tipos de columna siguen el diagrama del PDF. Course y Grade tienen IsActive para su futura baja lógica.
3. Revisa ApplicationDbContext. Registra las cuatro entidades y las relaciones Student → Grade, Enrollment → Student y Enrollment → Course.
4. En appsettings.json, DefaultConnection apunta a la instancia local SQLEXPRESS01 y a la base Laboratorio13Avance mediante autenticación integrada de Windows. Program.cs registra el contexto con UseSqlServer.
5. En una terminal situada en la carpeta del proyecto, restaura los paquetes y aplica la migración.

   ```powershell
   dotnet restore
   dotnet tool restore
   dotnet build
   dotnet ef database update
   ```

6. En SQL Server Management Studio, conecta a la instancia SQLEXPRESS01 con autenticación de Windows y comprueba las cuatro tablas y el historial __EFMigrationsHistory.
7. Ejecuta el perfil APIDemoLab para abrir Swagger UI en https://localhost:7135/swagger. Expande una ruta y usa **Try it out → Execute** para ver la respuesta. El documento OpenAPI se puede consultar en https://localhost:7135/openapi/v1.json.

## Controller · Puntos 6 a 11

| Punto | Ruta | Comportamiento |
| --- | --- | --- |
| 6–7 | `/api/grades` y `/api/courses` | GET y POST en la colección. GET, PUT y DELETE por ID. DELETE marca `IsActive = false` y el registro deja de aparecer en el CRUD. |
| 8 | `GET /api/students` | Filtros `firstName`, `lastName` y `email`. Orden descendente por apellido. |
| 9 | `GET /api/students/by-grade` | Filtros `firstName` y `grade`. Devuelve el estudiante con su grado. Orden descendente por nombre del curso. |
| 10 | `GET /api/enrollments/by-course` | Filtro `course`. Devuelve estudiante, curso y matrícula. |
| 11 | `GET /api/enrollments/by-grade` | Filtro `grade`. Devuelve estudiante, curso y matrícula. |

Los filtros de texto son opcionales y admiten coincidencias parciales. En el punto 9, cada estudiante aparece una vez y se ordena por el primer curso de su lista en orden alfabético descendente. Los estudiantes sin matrícula aparecen al final. Las rutas de matrículas ordenan por curso y después por apellido, ambos en orden ascendente, y conservan el historial.

Usa **APIDemoLab.http** para probar los endpoints. Las consultas de estudiantes y matrículas requieren datos en esas tablas para devolver filas. El punto 12 corresponde a la entrega en Git.

La [guía de pruebas](../APIDemoLab.Tests/GUIA-PRUEBAS.md) explica qué debe hacer cada punto y cómo verlo en Visual Studio.

[Comandos de EF Core](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
