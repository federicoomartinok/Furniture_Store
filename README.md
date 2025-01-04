# Furniture Store API

## Descripción

**Furniture Store API** es una API RESTful desarrollada en C# como parte de un curso en Udemy. Este proyecto tiene como objetivo practicar el desarrollo de aplicaciones backend utilizando ASP.NET Core y SQL Server. La API permite gestionar productos, pedidos e inventario de una tienda de muebles.

## Características

- **Gestión de Productos**: Crear, leer, actualizar y eliminar información de productos, como nombre, descripción, precio y stock disponible.
- **Gestión de Pedidos**: Registrar nuevos pedidos, consultar el estado de pedidos existentes y actualizar su información.
- **Gestión de Inventario**: Monitorear y actualizar el inventario de productos en tiempo real.

## Tecnologías Utilizadas

- **Lenguaje**: C#
- **Framework**: ASP.NET Core
- **Base de Datos**: SQL Server

## Instalación

1. **Clonar el Repositorio**:

   ```bash
   git clone https://github.com/federicoomartinok/Furniture_Store.git

   Restaurar Dependencias:

   Navega al directorio del proyecto y restaura las dependencias:

   cd Furniture_Store
   dotnet restore

   Configurar la Base de Datos:

   Asegúrate de tener una instancia de SQL Server en funcionamiento.
   Configura la cadena de conexión en el archivo appsettings.json según tus credenciales y configuración de la base de datos.

   Aplicar Migraciones:

   Aplica las migraciones para crear la base de datos y las tablas necesarias:

   dotnet ef database update

   Ejecutar la Aplicación:

   Inicia la aplicación con el siguiente comando:

   dotnet run

   La API estará disponible en https://localhost:5001 por defecto.

## Uso

La API expone los siguientes endpoints:

- **Productos**:
  - `GET /api/products`: Obtiene la lista de todos los productos.
  - `GET /api/products/{id}`: Obtiene un producto por su ID.
  - `POST /api/products`: Crea un nuevo producto.
  - `PUT /api/products/{id}`: Actualiza un producto existente.
  - `DELETE /api/products/{id}`: Elimina un producto.

- **Pedidos**:
  - `GET /api/orders`: Obtiene la lista de todos los pedidos.
  - `GET /api/orders/{id}`: Obtiene un pedido por su ID.
  - `POST /api/orders`: Crea un nuevo pedido.
  - `PUT /api/orders/{id}`: Actualiza un pedido existente.
  - `DELETE /api/orders/{id}`: Elimina un pedido.

- **Inventario**:
  - `GET /api/inventory`: Obtiene el estado actual del inventario.
  - `PUT /api/inventory/{productId}`: Actualiza el stock de un producto.

Para más detalles sobre cada endpoint y ejemplos de uso, consulta la documentación generada automáticamente con Swagger en https://localhost:5001/swagger.


