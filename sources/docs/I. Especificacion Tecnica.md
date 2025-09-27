# 1. Descripción

Solución que permite a las empresas cargar información sobre deudas pendientes en un repositorio multiempresa, con el fin de disponibilizarla a través de los canales de los bancos y habilitar la recepción de los pagos asociados a cada una de ellas.

# 2. Características y Requerimientos

- El lenguaje de desarrollo utilizado es C# sobre .NET 8.
- Utiliza el modelo **`dotnet-isolated`**.
- La carga de información se realiza mediante un archivo cuyo formato es definido por ASBANC.
- Los pagos se reportan mediante un archivo cuyo formato es definido por ASBANC.
- La empresa debe disponer de cuentas de recaudación asociadas a cada servicio en cada banco con el que desee operar.

# 3. Componentes de la solución

- **Servicio de carga:** Función encargada de leer archivos desde una carpeta en SFTP y, una vez procesados, dejarlos en otra carpeta dentro del mismo repositorio. El intervalo de ejecución es parametrizable en minutos.  
- **API transaccional:** Interfaz que permite la integración de la solución con la plataforma **YAPAGO**.  
- **Servicio generador:** Función responsable de generar los archivos de pagos correspondientes a un rango de tiempo determinado. El intervalo de ejecución también es parametrizable en minutos.  
- **Tabla de logs:** Componente **Table** de **Azure Storage** donde se registran los logs de las peticiones realizadas al API transaccional, así como los resultados de los servicios de carga de deudas y de generación de pagos.  
- **SFTP:** Componente utilizado como punto de intercambio. Un tercero deposita en él los archivos que son procesados por el servicio de carga, y desde este mismo repositorio puede recuperar los archivos de pagos generados por el servicio generador.
- **Base de datos:** Componente SQL Azure DB en el que se centralizará la información cargada por las empresas.  

---

# 4. Consideraciones generales

Deben tenerse en consideración lo siguiente:

- El parámetro **`idDeuda`** que se encuentra dentro de cada uno de los objetos del array `deudas` del **response del método de consulta**, debe reenviarse en cada request en el parámetro **`referenciaDeuda`** de cada request de pago asociado a cada objeto. Este valor es solo referencial, ya que la data de la tabla **deudas** es volátil.

- La solución está compuesta por **3 funciones**, las cuales se despliegan de manera conjunta al publicar el componente en **Azure**. Esto se debe a que, por la propia naturaleza de un servicio **Azure Function**, no es posible realizar una publicación parcial.  

- El parámetro **`idDeuda`**, presente en cada objeto del array **`deudas`** dentro del *response* del método de consulta, debe enviarse nuevamente en cada *request* a través del parámetro **`referenciaDeuda`** correspondiente a cada pago asociado. Este valor tiene únicamente carácter referencial, ya que la información de la tabla **deudas** es volátil y no persistente. 

---

**`Response Consulta`**

```json
{
  "cliente": "Juan Perez",
  "deudas": [
    {
      "idDeuda": "12",
      "servicio": "001",
      "documento": "F00000001",
      "descripcionDoc": "Factura",
      "fechaVencimiento": "10052025",
      "fechaEmision": "10052025",
      "deuda": "100.99",
      "pagoMinimo": "100.99",
      "moneda": "1"
    }
  ],
  "codResp": "00",
  "desResp": "Ok"
}
```

---

**`Request del pago`**

```json
{ 
  "fechaTxn": "20092025", 
  "horaTxn": "180520", 
  "idCanal": "10", 
  "idForma": "01", 
  "numeroOperacion": "A0154785", 
  "idConsulta": "12345678", 
  "servicio": "001", 
  "numeroDocumento": "F00000001", 
  "importePagado": 100.99, 
  "moneda": "1", 
  "idEmpresa": "801", 
  "idBanco": "1020", 
  "voucher": "", 
  "referenciaDeuda": 12 
}
```

- El método de consulta debe invocarse **una única vez**, no es necesario hacer el artificio de consulta escalonada por parte del proveedor.  

- El parámetro del request **`idConsulta`** dentro de la consulta no debe considerar los **0 a la izquierda**. Por ejemplo:  
-Valor original ⇒ `98547859` | Valor enviado ⇒ `98547859`  
-Valor original ⇒ `04785124` | Valor enviado ⇒ `4785124`  

- El parámetro del request **`tipoConsulta`** dentro de la consulta siempre será **0**.

---

# 5. Proceso de lectura y carga de deudas

## 5.1. Definición

El proceso de lectura permite que cada empresa cargue sus deudas en la base de datos multicliente. Una vez registradas, estas deudas quedan disponibles para ser consultadas y utilizadas en la ejecución de transacciones a través de los diferentes canales bancarios.

## 5.2. Características

- Este proceso se ejecuta automáticamente con una frecuencia configurable mediante una variable de entorno en Azure, la cual aplica de forma uniforme a todas las empresas.
- El único formato de archivo soportado es .TXT.
- Se ha mantenido la estructura original del encabezado y del detalle de deudas para asegurar compatibilidad con los archivos generados por clientes de EXPRESS en caso de migración.
- El intercambio de archivos se realiza a través de un repositorio SFTP que centraliza la carga y descarga de la información.

## 5.3. Ciclo de vida

El siguiente diagrama muestra el ciclo de vida del proceso de carga de deudas dentro de AZURE:

![Flujo lectura](https://github.com/AsbancPE/YaPagoMulticlient/raw/main/sources/imgs/FlujoLectura.jpeg)

## 5.4. Estructura de carpetas

| **Componente**       | **Descripción**                                                                 | **Path**                                   |
|-----------------------|---------------------------------------------------------------------------------|--------------------------------------------|
| **Empresa**           | Raíz de la carpeta asignada a la empresa por ASBANC.                           | `[root]/[código empresa]`                  |
| **Deudas**            | Carpeta raíz destinada al proceso de carga.                                    | `[root]/[código empresa]/Deudas`           |
| **Deudas - Pending**  | Carpeta en la que se deposita el archivo para su lectura.                      | `[root]/[código empresa]/Deudas/Pending`   |
| **Deudas - Complete** | Carpeta a la que se mueve el archivo después de procesarse.                    | `[root]/[código empresa]/Deudas/Complete`  |

## 5.5. Estructura del archivo

Un archivo de carga se compone de 2 partes

### 5.5.1. Cabecera de archivo

#### a. Descripción

La primera fila del archivo contiene información general sobre el propósito del mismo y el detalle de las deudas incluidas.

#### b. Estructura

La cabecera del archivo se estructura de la siguiente manera:

| #   | Campo                  | Tipo | Long. | Inicio       | Fin        | Descripción |
|-----|------------------------|------|-------|--------------|------------|-------------|
| 01  | **Nombre del Archivo** | AN   | 21    | 1            | 21         | Nombre que identifica al archivo.<br><br>Formato: "DEEEEEDDMMAAASSS.TXT"<br><br>**Donde:**<br>- **D**: Constante que identifica el tipo de archivo de deudas.<br>- **EEEEE**: Código de Empresa en FTR, entregado por Asbanc. Alineado a la derecha con ceros a la izquierda.<br>Ejemplo: `730` → `00730`.<br>- **DDMMAA**: Fecha de generación del archivo.<br>- **SSS**: Número secuencial dentro de la fecha de generación.<br><br>**Ejemplo:** `D0073022102021001.TXT` indica el primer archivo generado el 22 de octubre de 2021. Se pueden generar varios archivos por día. |
| 02  | **Tipo Actualización** | AN   | 1     | 22           | 22         | Tipo de actualización de datos.<br><br>- `R` = Reemplazo total de deudas. Elimina todas las deudas de la BD y carga nuevamente toda la información.<br>- `N` = Archivo de novedades. Solo actualiza deudas según el tipo de actualización de cada registro del detalle. |
| 03  | **Código Empresa**     | N    | 07    | 23           | 29         | Valor constante = `0000730`. |
| 04  | **Fecha Transmisión**  | N    | 08    | 30           | 37         | En formato `DDMMAAAA`. |
| 05  | **Hora Transmisión**   | N    | 06    | 38           | 43         | En formato `HHMMSS` (HH24). |
| 06  | **Número Registros**   | N    | 09    | 44           | 52         | Cantidad total de deudas enviadas (en el detalle). |
| 07  | **Importe Total Soles**| N    | 15    | 53           | 67         | Importe total en moneda **Soles**, igual a la suma de todas las deudas enviadas.<br>Formato `(15,2)`. |
| 08  | **Importe Total Dólares**| N  | 15    | 68           | 82         | Importe total en moneda **Dólares**, igual a la suma de todas las deudas enviadas.<br>Formato `(15,2)`. |
| 09  | **Filler**             | AN   | 120   | 83           | 202        | Disponible para uso futuro. |
| 10  | **Mac**                | N    | 08    | 203          | 210        | Código de autenticidad del archivo (para uso futuro).<br>Indicar `00000000`. |

### 5.5.2. Detalle deudas

#### a. Descripción

Todos los registros a partir de la segunda fila del archivo, donde en cada fila se detalla la información relacionada a una deuda.

#### b. Estructura

El detalle de cada deuda se estructura de la siguiente manera:

| #   | Campo              | Tipo | Long.    | Inicio   | Fin      | Descripción |
|-----|--------------------|------|----------|----------|----------|-------------|
| 01  | Tipo Actualización | AN   | 1        | 1        | 1        | Tipo de actualización de registro:<br> - **N** = Nuevo registro (insertar)<br> - **A** = Actualizar registro (se busca la deuda y se actualizan importes: deuda, mora, gastos administrativos)<br> - **D** = Eliminar registro (se elimina la deuda de la BD) |
| 02  | Código Cliente     | AN   | 14       | 2        | 15       | Identificador único de cliente en la empresa. Cadena alfanumérica alineada a la izquierda. Se completa con espacios en blanco a la derecha si no cubre la longitud. Solo admite letras y números (sin acentos ni “Ñ”). |
| 03  | Nombre Cliente     | AN   | 30       | 16       | 45       | Nombre del cliente. Cadena alfanumérica alineada a la izquierda. Se completa con espacios en blanco a la derecha si no cubre la longitud. Solo admite letras, números, punto y espacios (sin acentos ni “Ñ”). |
| 04  | Código Producto    | AN   | 3        | 46       | 48       | Código del producto o servicio:<br> - **001** = Recaudación fact. soles<br> - **002** = Recaudación fact. dólares<br> - **003** = Recaudación proformas soles<br> - **004** = Recaudación proformas dólares |
| 05  | NumDocumento       | AN   | 16       | 49       | 64       | Número del documento de la deuda. Cadena alfanumérica alineada a la izquierda, completada con espacios en blanco si no cubre la longitud. Solo admite letras, números y guion medio (sin acentos ni “Ñ”). Para un mismo cliente no pueden repetirse documentos y deben enviarse en orden ascendente. |
| 06  | DescDocumento      | AN   | 20       | 65       | 84       | Descripción del documento de la deuda. |
| 07  | Fecha Vencimiento  | AN   | 8        | 85       | 92       | Fecha de vencimiento de la deuda en formato **DDMMYYYY**. |
| 08  | Fecha Emisión      | AN   | 8        | 93       | 100      | Fecha de emisión de la cuponera en formato **DDMMYYYY**. |
| 09  | Deuda              | N    | 12       | 101      | 112      | Monto de deuda a pagar (sin incluir mora). Formato numérico **(12,2)**. |
| 10  | Mora               | N    | 12       | 113      | 124      | Importe de la mora. Formato numérico **(12,2)**. |
| 11  | GastosAdm          | N    | 12       | 125      | 136      | Gastos administrativos. Formato numérico **(12,2)**. |
| 12  | Pago Mínimo        | N    | 12       | 137      | 148      | Debe ser mayor que **0** y menor o igual que la **Deuda Total** (Deuda + Mora + GastosAdm). Si no se acepta pago parcial, este importe debe ser igual a la Deuda Total.<br>Formato numérico **(12,2)**. |
| 13  | Periodo            | N    | 2        | 149      | 150      | Indica el periodo al que pertenece la deuda. Si no aplica, se envía **00**. |
| 14  | Año                | N    | 4        | 151      | 154      | Indica el año de la deuda. |
| 15  | Cuota              | N    | 2        | 155      | 156      | Número de cuota de la deuda. Si no aplica, se envía **00**. |
| 16  | MonedaDoc          | AN   | 1        | 157      | 157      | Indica la moneda en la que están expresados los importes del documento:<br> - **1** = Nuevos Soles<br> - **2** = Dólares Americanos |
| 17  | Número DNI         | AN   | 8        | 158      | 165      | Número de DNI del cliente (opcional). Cadena numérica fija de 8 dígitos que siempre empieza con **1** o **2**. Si no aplica, se completa con espacios en blanco. |
| 18  | Número RUC         | AN   | 11       | 166      | 176      | Número de RUC del cliente (opcional). Cadena numérica fija de 11 dígitos. Puede empezar con **0**. Si no aplica, se completa con espacios en blanco. |
| 19  | Identificador 04   | AN   | 14       | 177      | 190      | Identificador alternativo de búsqueda (opcional). Cadena alfanumérica alineada a la izquierda. Se completa con espacios en blanco si no cubre la longitud. Solo admite letras y números (sin acentos ni “Ñ”). |
| 20  | Filler             | AN   | 20       | 191      | 210      | Campo libre para uso futuro o particular de cada empresa. |

## 5.6. Reglas del proceso

- Un archivo de tipo "R" en el registro de cabecera, solo puede tener registros de tipo "N". Todo registro que no sea de este tipo es considerado error y es omitido del procesamiento.
- A menos que el primer registro no tenga errores, el proceso de carga no es total, es decir si un registro falla se procede con el siguiente hasta que se procesen todas las filas dentro del documento.
- Como resultado de realizar el proceso de carga se generan 2 archivos:
    - **Archivo renombrado:** El archivo con la deudas, luego de procesarse se toma de la carpeta "Pending" y se mueve a "Complete" renombrandolo con la siguiente estructura: 
	<br>**input_[nombre archivo]_[ddMMyyyyhhmmssfff].TXT**
	<br>Por ejemplo: **input_D0050010092025003_17092025052048308.TXT**	
    - **Resultados:** Dentro de la carpeta complete se genera un archivo con extension ".json" que contiene un resumen del procesamiento realizado y las lineas observadas, con el detalle de por que fueron observadas. El nombre del archivo tiene la siguiente estructura dependiendo de si se llegan a procesar registros o no:
	<br>Si se procesan registros => **resume_[nombre archivo]_[ddMMyyyyhhmmssfff].json**
	<br>Por ejemplo: **resume_D0050010092025003_17092025052048308.json**
	<br>Si no se procesan registros => **ERROR_[nombre archivo]_[ddMMyyyyhhmmssfff].json**
	<br>Por ejemplo: **ERROR_D0050010092025003_17092025052048308.json**

**`EJEMPLO 01 - Se procesaron registros`**
```json
{
  "ErrorDetails": [
    {
      "Row": "A72173635      ARIAS AGUILAR ROUVIE6161      0015676032         PRIMAS OHIO PEN     040820250408202500000001801700000000000000000000000000000001801708202500172173635           6161                              F",
      "Results": [
        "El tipo de registro no aplica en archivo de reemplazo"
      ]
    },
    {
      "Row": "D20033970      MALDONADO ALVARADO R6157      0015672910         PRIMAS OHIO PEN     010820250108202500000016410000000000000000000000000000000016410008202500120033970           6157                              F",
      "Results": [
        "El tipo de registro no aplica en archivo de reemplazo"
      ]
    },
    {
      "Row": "N70068426      VASQUEZ DIAZ THALIA 4520      0014182865         PRIMAS OHIO PEN     120420251204202500000000000000000000000000000000000000000000000004202500170068426           4520                              F",
      "Results": [
        "Un monto es invalido"
      ]
    }
  ],
  "FileName": "/SFTP-NETSUITE-PROD/ReplicaAzure/801/Deudas/Pending/D0050010092025003.TXT",
  "FileType": "Replace",
  "TotalRecords": "2433",
  "SuccessRecords": "2430",
  "ErrorRecords": "3",
  "StartExec": "2025-09-17T17:20:46.2756674",
  "EndExec": "2025-09-17T17:20:48.3086419",
  "Duration": "2.03297 seg"
}
```

**`EJEMPLO 02 - Problemas antes de realizar el procesamiento de deudas`**
```json
{
  "ErrorDetails": [
    {
      "Row": "Generic",
      "Results": [
        "El codigo de la carpeta no se asocia a una empresa dentro de la base de datos"
      ]
    }
  ],
  "FileName": "NoApply",
  "FileType": "NoApply",
  "TotalRecords": "0",
  "SuccessRecords": "0",
  "ErrorRecords": "0",
  "StartExec": "2025-09-25T15:48:59.0613738",
  "EndExec": "2025-09-25T15:48:59.0620929",
  "Duration": "0.00072 seg"
}
```

---

# 6. Proceso de generación de pagos

## 6.1. Definición

Proceso en el que la solución genera el detalle de las transacciones y lo pone a disposición de los clientes para su lectura y descarga.

## 6.2. Características

- Este proceso se realiza de forma automática con una frecuencia configurable "X" dentro de las variables de entorno en AZURE. La variable que controla el tiempo de ejecución es la misma para todas las empresas.
- El formato soportado para los archivos es ".TXT".

## 6.3. Ciclo de vida

El siguiente diagrama muestra el ciclo de vida del proceso de generación del detalle de transacciones dentro de AZURE:

![Flujo de generación](https://github.com/AsbancPE/YaPagoMulticlient/blob/main/sources/imgs/FlujoGeneracion.jpeg?raw=true)

## 6.4. Estructura de carpetas

| **Componente**        | **Descripción**                                                                 | **Path**                                   |
|-----------------------|---------------------------------------------------------------------------------|--------------------------------------------|
| **Empresa**           | Raíz de la carpeta asignada a la empresa por ASBANC.                            | `[root]/[código empresa]`                  |
| **Pagos**             | Carpeta destinada al proceso de generación. En esta carpeta se depositan los archivos para que la empresa pueda procesarlos. | `[root]/[código empresa]/Pagos` |

## 6.5. Reglas del proceso

- Como resultado de realizar el proceso de generación, se genera 2 archivos:
	- **Archivo transaccional:** Archivo con extensión ".TXT" que contiene el detalle de las transacciones, cuyo nombre tiene la siguiente estructura:
	<br>**[Código de empresa]_[ddMMyyyyhhmmssfff].TXT**
	<br>Por ejemplo: **801_17092025052048308.TXT**
	- **Resumen transaccional:** Archivo con extensión ".json" que contiene el resumen del proceso de generación, cuyo nombre tiene la siguiente estructura:
	<br>**[Código de empresa]_[ddMMyyyyhhmmssfff].json**
	<br>Por ejemplo: **801_17092025052048308.json**

## 6.6. Estructura del archivo

La estructura del archivo de pagos es la siguiente:

| #   | Campo                        | Tipo | Long.    | Inicio   | Fin      | Descripción |
|-----|------------------------------|------|----------|----------|----------|-------------|
| 01  | Tipo de registro             | AN   | 2        | 1        | 2        | Constante: **DD** |
| 02  | Código de identificación del cliente | AN | 14 | 27 | 40 | Código único del cliente. |
| 03  | Nombre Cliente               | AN   | 30       | 41       | 70       | Nombre del cliente. |
| 04  | Código Producto              | AN   | 3        | 71       | 73       | Código del producto o servicio:<br> - **001** = Recaudación fact. soles<br> - **002** = Recaudación fact. dólares<br> - **003** = Recaudación proformas soles<br> - **004** = Recaudación proformas dólares |
| 05  | Número de documento          | AN   | 16       | 74       | 89       | Número del documento pagado. |
| 06  | Fecha de vencimiento         | N    | 8        | 90       | 97       | Fecha de vencimiento de la deuda. Formato **DDMMAAAA**.<br>Ejemplo: `02122014`. |
| 07  | Fecha de pago                | N    | 8        | 98       | 105      | Fecha de la transacción en el Banco. Formato **DDMMAAAA**.<br>Ejemplo: `30112014`. |
| 08  | Hora de pago                 | N    | 6        | 106      | 111      | Hora de la transacción en el Banco. Formato **HHmmss** (24h).<br>Ejemplo: `141030` (2:10:30 PM). |
| 09  | Importe pagado               | N    | 15       | 112      | 126      | Importe pagado por el cliente, sin coma ni punto decimal.<br>Ejemplo: `000000000001550`. |
| 10  | Agencia                      | N    | 15       | 127      | 141      | Código de la sucursal y/o agencia del Banco donde se realizó el pago. |
| 11  | Dirección Agencia            | N    | 40       | 142      | 181      | Dirección de la sucursal y/o agencia del Banco donde se realizó el pago. |
| 12  | Número de operación          | N    | 12       | 182      | 193      | Número de la operación bancaria que aparece en el archivo de conciliación. No siempre corresponde al número impreso en el voucher del Banco. |
| 13  | Canal de pago                | AN   | 2        | 194      | 195      | Código del canal de recaudación:<br> - **10** = Ventanilla<br> - **20** = ATM<br> - **30** = Monederos<br> - **40** = Agente Corresponsal<br> - **50** = Kiosko Multimedia<br> - **60** = Internet Banking<br> - **70** = IVR<br> - **80** = Banca Celular<br> - **99** = Otros |
| 14  | Forma de pago                | AN   | 2        | 196      | 197      | Forma de pago utilizada:<br> - **01** = Efectivo<br> - **02** = Tarjeta de Débito<br> - **03** = Tarjeta de Crédito<br> - **04** = Cheque |
| 15  | Fecha de pago contable       | N    | 8        | 174      | 181      | Fecha contable de la transacción en el FTR. Puede diferir de la fecha registrada por el banco.<br>Formato **DDMMAAAA**.<br>Ejemplo: `30112014`. |
| 16  | Estado transacción           | N    | 1        | 182      | 182      | Estado de la transacción:<br> - **P** = Pagado<br> - **A** = Anulado/Extornado |
| 17  | Código de banco              | N    | 4        | 182      | 182      | Código del banco desde el que se realizó la transacción |
| 18  | Cuenta banco                 | N    | 20       | 182      | 182      | Número de cuenta asociada al servicio desde el cual se realizo la transacción |
| 19  | Voucher                      | N    | 20       | 182      | 182      | Numero de voucher enviado desde el canal del banco. Actualmente solo en uso por BBVA y en algunos canales. |
| 20  | Filler (libre)               | N    | 30       | 183      | 212      | Campo reservado para uso futuro. |