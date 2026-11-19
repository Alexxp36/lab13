# Cómo comprobar el Laboratorio 13

Abre **Laboratorio13.sln** en Visual Studio. Entra a **Prueba → Explorador de pruebas** y pulsa **Ejecutar todas**. También puedes abrir PowerShell en `C:\net` y ejecutar:

```powershell
dotnet test .\Laboratorio13.sln --logger "console;verbosity=normal"
```

Las pruebas 1 al 4 comprueban el modelo y la configuración sin conectarse a la base de datos. Las pruebas 5 al 11 envían peticiones a la API y usan una base temporal en `.\SQLEXPRESS01` con autenticación de Windows. La suite crea esa base, carga datos de ejemplo y la elimina al terminar. La instancia debe estar en ejecución y tu cuenta debe poder crear bases. Puedes elegir otra instancia local con la variable `LAB13_TEST_SQL_SERVER`.

## Qué deberías ver

| Punto | Qué debe hacer el proyecto | Qué verifica la prueba |
| --- | --- | --- |
| 1 | Representar el diagrama en C#. | Los cuatro modelos se traducen a las tablas, columnas y tipos esperados. |
| 2 | Relacionar las tablas. | Estudiante → grado, matrícula → estudiante y matrícula → curso. |
| 3 | Exponer las tablas a EF Core. | El contexto entrega un `DbSet` para cada modelo. |
| 4 | Conectar con tu cuenta de Windows. | La configuración usa autenticación integrada y deja vacíos el usuario y la contraseña de SQL Server. |
| 5 | Crear el esquema mediante una migración. | SQL Server registra `InitialCreate` y aparecen las cuatro tablas. |
| 6 | Crear, consultar, editar y dar de baja un grado. | La API devuelve el grado editado. Tras la baja, la fila sigue en SQL Server con `IsActive = false` y el GET por ID devuelve `404`. |
| 7 | Crear, consultar, editar y dar de baja un curso. | La API guarda el cambio de créditos y aplica la misma baja lógica. |
| 8 | Buscar estudiantes por nombre, apellido y correo. | Los filtros se combinan. En la búsqueda de Ana, el orden por apellido es Vega, Rojas, Pérez. |
| 9 | Buscar por nombre y grado y devolver los datos del grado. | Ana Pérez va antes que Ana Vega porque su curso de mayor orden es Zoología. Ana Rojas aparece al final porque no tiene matrícula. |
| 10 | Buscar matrículas por curso. | Álgebra devuelve primero a Pérez y luego a Vega. Cada resultado incluye estudiante, curso y matrícula. |
| 11 | Buscar matrículas por grado. | Primero devuelve Álgebra/Pérez, Álgebra/Vega y Zoología/Pérez. Segundo devuelve Zoología/Soto. |

## Prueba manual del CRUD en Visual Studio

1. Inicia el proyecto **APIDemoLab** y abre **APIDemoLab.http**.
2. Envía `POST /api/grades`. Debes ver `201 Created` y un `gradeId`. Copia ese ID a la variable `@gradeId` del archivo.
3. Envía `GET /api/grades/{{gradeId}}`. Debes ver `200 OK` y los datos del grado.
4. Envía `PUT /api/grades/{{gradeId}}`. Debes ver `204 No Content`. Repite el GET para ver los datos actualizados.
5. Envía `DELETE /api/grades/{{gradeId}}` para dar de baja el grado de ejemplo. Debes ver `204 No Content`. Repite el GET para comprobar el `404 Not Found`.
6. Para probar cursos, repite el flujo con `/api/courses` y `@courseId`.

Las cuatro consultas del archivo HTTP devuelven `[]` cuando no hay filas que coincidan en la base del laboratorio. Para ver ejemplos con estudiantes, grados y matrículas, ejecuta la suite automatizada, que prepara sus propios datos.

El punto 12 es la entrega en Git y se comprueba con el repositorio publicado.
