# 1. Variables de entorno

- **`LoaderCron`:** Parámetro que permite establecer la frecuencia con la cual se ejecuta el timer de la función de carga de deudas. 
- **`GeneratorCron`:** Parámetro que permite establecer la frecuencia con la cual se ejecuta el timer de la función de generación de pagos.
- **`DbConf__ConnectionString`:** Parámetro que  permite establecer la cadena de conexión hacia la base de datos multicliente.
- **`SftpConfig__Server`:** Parámetro que permite establecer el servidor del SFTP a utilizarse.
- **`SftpConfig__User`:** Parámetro que permite establecer el usuario del SFTP a utilizarse.
- **`SftpConfig__Pass`:** Parámetro que permite establecer el Password del SFTP a utilizarse.
- **`SftpConfig__Root`:** Parámetro que permite establecer la carpeta raíz donde se establecerán las empresas dentro del SFTP a utilizarse. Por ejemplo: **/SFTP-NETSUITE-PROD/ReplicaAzure**
- **`JwtConfig__Claim`:** Parámetro que permite establecer el nivel de usuario solicitado por el método para ser consumido.
- **`JwtConfig__User`:** Parámetro que permite establecer el usuario necesario para autorizar la generación de un token JWT.
- **`JwtConfig__Password`:** Parámetro que permite establecer la contraseña necesaria para autorizar la generación de un token JWT.
- **`JwtConfig__SecretKey`:** Parámetro que define la clave secreta usada para firmar y validar el JWT. El valor se proporciona en formato Base64, por lo que debe decodificarse antes de ser utilizado.
- **`BlobConfig__ConnectionString`:** Parámetro que permite establecer la cadena de conexión para interactuar con el Table dentro del Azure Table Storage.
- **`BlobConfig__Table`:** Parámetro que permite establecer el nombre de la tabla dentro del Azure Table Storage donde se registrarán los logs de los diferentes procesos ejecutados por los componentes.
- **`BlobConfig__EnableLog`:** Parámetro que permite establecer si el log se encuentra activo o no. Los valores admitidos son: **[On|Off]**

---

# 2. Archivos de configuración

## 2.1. configurations.json

### 2.1.1. Parámetros

- **`Configurations`:** Parámetro raíz donde se tienen las configuraciones del sistema.
    - **`EmpresasConfig`:** Parámetro de tipo array object que contiene las configuraciones necesarias para una empresa. 
        - **`Nombre`:** Parámetro solo para uso referencial cuyo valor es el nombre de una empresa.
        - **`Codigo`:** Parámetro cuyo valor es el código de empresa asignado en el CORE de YAPAGO a una empresa.
        - **`Servicios`:** Array de valores de tipo string en el que se detallan los servicios que tiene una empresa.
    - **`JwtAlterConfig`:**
        - **`Audience`:** Audiencia permitida para el JWT. Actualmente no utilizado.
        - **`Issuer`:** Emisor del JWT
        - **`MinutesFactor`:** Parámetro que permite establecer el factor minutes para la formula del calculo de minutos para que caduque el JWT generado.
        - **`HoursFactor`:** Parámetro que permite establecer el factor horas para la formula del calculo de minutos para que caduque un JWT generado.
        - **`DaysFactor`:** Parámetro que permite establecer el factor días para la formula del calculo de minutos para que caduque un JWT generado.

### 2.1.2. Ejemplo de configuración

```json
{
  "Configurations": {
    "EmpresasConfig": [
      {
        "Nombre": "Empresa 01",
        "Codigo": "801",
        "Servicios": ["001", "002"]
      },
      {
        "Nombre": "Empresa 02",
        "Codigo": "500",
        "Servicios": [ "001", "002" ]
      }
    ],
    "JwtAlterConfig": {
      "Audience": "localhost",
      "Issuer": "asbanc",
      "MinutesFactor": "60",
      "HoursFactor": "24",
      "DaysFactor": "365"
    }
  }  
}
```