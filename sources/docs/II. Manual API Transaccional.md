# 1. Descripción

Interfaz de tipo API REST que permite integrarse e interactuar a la solución con la plataforma YAPAGO.

---

# 2. Estructura de los endpoints

La estructura de los endpoints tendra el siguiente formato: **[Url base]/[Versión]/[Recurso]/[Acción]**

---

# 3. Detalle de entorno

| **Ambiente**          | **Base Url**                                                                    |
|-----------------------|---------------------------------------------------------------------------------|
| **QA**                | http://localhost:7265/api/swagger/ui#/                                          |
| **PRD**               |                                                                                 |

---

# 4. Definición de los endpoints

La interface se compone de 3 métodos:

## 4.1. Autenticar

**Método:** POST  
**URL:** /V1.0/seguridad/ObtenerToken  
**Headers:**  
- Content-Type: application/json  
- Accept: application/json 

**`Request`**
| #  | Parámetro    | Tipo   | Longitud | Formato | Descripción                 | Obligatorio |
|----|--------------|--------|----------|---------|-----------------------------|-------------|
| 1  | userType     | string | 50       | -       | Tipo de usuario.<br>`QA`:adminprofile<br>`PRD`:adminprofile| SI          |
| 2  | userProfile  | string | 50       | -       | Perfil asignado al usuario.<br>`QA`:U5u4r10.3x73rn0<br>`PRD`:ASBANC lo proporcionara| SI          |
| 3  | userPass     | string | 50       | -       | Contraseña del usuario.<br>`QA`:*C0n7r4s3n4.P4ra.3xt3rn0S<br>`PRD`:ASBANC lo proporcionara| SI          |

**`Response`**

| #  | Parámetro       | Tipo   | Longitud | Formato | Descripción                                |
|----|-----------------|--------|----------|---------|--------------------------------------------|
| 1  | token           | string | MAX       | -      | Token generado para el usuario autenticado. |
| 2  | expirationDate  | string | 24        | DD/MM/AAAA - hh:mm a. m./p. m.| Fecha y hora de expiración del token.|
| 3  | codResp         | string | 2         | -      | Código de respuesta del servicio.           |
| 4  | desResp         | string | 500       | -      | Descripción asociada al código de respuesta.|

## 4.2. Consulta

**Método:** POST  
**URL:** /V1.0/transac/consultar  
**Headers:**  
- Content-Type: application/json  
- Accept: application/json 
- Authorization: Bearer {token}

**`Request`**
| #  | Parámetro | Tipo   | Longitud | Formato | Descripción                  | Obligatorio |
|----|-----------|--------|----------|---------|------------------------------|-------------|
| 1  | empresa   | string | 3        | -       | Código de la empresa.        | SI          |
| 2  | servicio  | string | 5        | -       | Código del servicio.         | SI          |
| 3  | id        | string | 14       | -       | Identificador de la deuda.   | SI          |
| 4  | tipo      | string | 1        | -       | Tipo de operación. Por default enviar siempre `0`.| NO          |
| 5  | canal     | string | 2        | -       | Canal de origen de la transacción. | NO    |
| 6  | banco     | string | 4        | -       | Identificador del banco.     | NO          |

**`Response`**
| #  | Parámetro        | Tipo     | Longitud | Formato   | Descripción                              |
|----|------------------|----------|----------|-----------|------------------------------------------|
| 1  | cliente          | string   | 14       | -         | Nombre del cliente.                      |
| 2  | deudas           | array    | 10       | -         | Lista de deudas asociadas al cliente. La lista siempre esta ordenada de forma ascendente en base a la fecha de vencimiento.|
| 2.1| idDeuda          | string   | 12       | -         | Identificador único de la deuda.         |
| 2.2| servicio         | string   | 3        | -         | Código del servicio.                     |
| 2.3| documento        | string   | 16       | -         | Número de documento asociado a la deuda. |
| 2.4| descripcionDoc   | string   | 20       | -         | Descripción del documento.               |
| 2.5| fechaVencimiento | string   | 8        | ddMMaaaa  | Fecha de vencimiento de la deuda.        |
| 2.6| fechaEmision     | string   | 8        | ddMMaaaa  | Fecha de emisión del documento.          |
| 2.7| deuda            | string   | 15       | ############.##  | Importe total de la deuda.        |
| 2.8| pagoMinimo       | string   | 15       | ############.##  | Importe mínimo a pagar.           |
| 2.9| moneda           | string   | 1        | -         | Moneda de la deuda.<br> **PEN:** 1<br> **USD:** 2|
| 3  | codResp          | string   | 2        | -         | Código de respuesta del servicio.        |
| 4  | desResp          | string   | 100      | -         | Descripción asociada al código de respuesta.|

## 4.3. Pago

**Método:** POST  
**URL:** /V1.0/transac/pagar  
**Headers:**  
- Content-Type: application/json  
- Accept: application/json 
- Authorization: Bearer {token}

**`Request`**
| #  | Parámetro        | Tipo    | Longitud | Formato   | Descripción                                   | Obligatorio |
|----|------------------|---------|----------|-----------|-----------------------------------------------|-------------|
| 1  | fechaTxn         | string  | 8        | ddMMaaaa  | Fecha de la transacción de pago.              | SI          |
| 2  | horaTxn          | string  | 6        | HHmmss    | Hora de la transacción de pago.               | SI          |
| 3  | idCanal          | string  | 2        | -         | Identificador del canal de pago.              | SI          |
| 4  | idForma          | string  | 2        | -         | Identificador de la forma de pago.            | SI          |
| 5  | numeroOperacion  | string  | 12       | -         | Número de operación enviado por el banco.     | SI          |
| 6  | idConsulta       | string  | 16       | -         | Identificador de la consulta previa.          | SI          |
| 7  | servicio         | string  | 5        | -         | Código del servicio asociado.                 | SI          |
| 8  | numeroDocumento  | string  | 16       | -         | Número de documento de deuda.                 | SI          |
| 9  | importePagado    | decimal | 15       | ############.##  | Importe pagado por el cliente.         | SI          |
| 10 | moneda           | string  | 3        | -         | Moneda en la que se realiza el pago.          | SI          |
| 11 | idEmpresa        | string  | 3        | -         | Identificador de la empresa.                  | SI          |
| 12 | idBanco          | string  | 4        | -         | Identificador del banco.                      | SI          |
| 13 | voucher          | string  | 50       | -         | Número de comprobante o voucher.              | NO          |
| 14 | referenciaDeuda  | long    | -        | -         | Referencia a la deuda en el sistema.          | SI          |

**`Response`**
| #  | Parámetro    | Tipo   | Longitud | Formato | Descripción                                      |
|----|--------------|--------|----------|---------|--------------------------------------------------|
| 1  | cliente      | string | 30       | -       | Nombre del cliente.                              |
| 2  | operacionErp | string | 9        | -       | Número de operación generado en la solución.     |
| 3  | codResp      | string | 2        | -       | Código de respuesta del servicio.                |
| 4  | desResp      | string | 500      | -       | Descripción asociada al código de respuesta.     |

## 4.4. Reversa

**Método:** POST  
**URL:** /V1.0/transac/revertir  
**Headers:**  
- Content-Type: application/json  
- Accept: application/json 
- Authorization: Bearer {token}

**`Request`**
| #  | Parámetro        | Tipo   | Longitud | Formato   | Descripción                                   | Obligatorio |
|----|------------------|--------|----------|-----------|-----------------------------------------------|-------------|
| 1  | fechaTxn         | string | 8        | ddMMaaaa  | Fecha de la transacción de reversa.           | SI          |
| 2  | horaTxn          | string | 6        | HHmmss    | Hora de la transacción de reversa.            | SI          |
| 3  | idBanco          | string | 4        | -         | Identificador del banco.                      | SI          |
| 4  | idConsulta       | string | 16       | -         | Identificador de la consulta previa.          | SI          |
| 5  | idServicio       | string | 5        | -         | Código del servicio asociado.                 | SI          |
| 6  | tipoConsulta     | string | 1        | -         | Tipo de consulta a ejecutar.                  | SI          |
| 7  | numeroOperacion  | string | 12       | -         | Número único de operación.                    | SI          |
| 8  | numeroDocumento  | string | 16       | -         | Número de documento del cliente.              | SI          |
| 9  | idEmpresa        | string | 3        | -         | Identificador de la empresa.                  | SI          |
| 10 | voucher          | string | 50       | -         | Número de comprobante o voucher asociado.     | SI          |

**`Response`**
| #  | Parámetro    | Tipo   | Longitud | Formato | Descripción                                      |
|----|--------------|--------|----------|---------|--------------------------------------------------|
| 1  | cliente      | string | 30       | -       | Nombre del cliente.                              |
| 2  | operacionErp | string | 9        | -       | Número de operación generado en la solución.     |
| 3  | codResp      | string | 2        | -       | Código de respuesta del servicio.                |
| 4  | desResp      | string | 500      | -       | Descripción asociada al código de respuesta.     |

---

# 5. Reglas de negocio

- La llave de búsqueda se compone solo de números y letras sin ningún tipo de acentuación, ya que se podrían provocar malos funcionamientos en los canales de los bancos.
- El número de documento esta conformado solo por números, guión medio o letras sin ningún tipo de acentuación, ya que se podrían provocar malos funcionamientos en los canales de los bancos.
- La descripción de un documento solo puede estar formada por números, letras sin ningún tipo de acentuación, espacio en blanco o guión medio, ya que el uso de otros caracteres o enviarlo en blanco podría provocar malos funcionamientos en los canales de los bancos.
- Los valores que no son obligatorios pueden ser enviados en blanco.
- El método de consulta debe invocarse **una única vez**, no es necesario hacer el artificio de consulta escalonada por parte del proveedor.
- El parámetro del request **`idConsulta`** dentro de la consulta no debe considerar los **0 a la izquierda**. Por ejemplo:  
-Valor original ⇒ `98547859` | Valor enviado ⇒ `98547859`  
-Valor original ⇒ `04785124` | Valor enviado ⇒ `4785124`
- El parámetro del request **`tipoConsulta`** dentro de la consulta siempre será **0**.
- El parámetro **`idDeuda`**, presente en cada objeto del array **`deudas`** dentro del *response* del método de consulta, debe enviarse nuevamente en cada *request* a través del parámetro **`referenciaDeuda`** correspondiente a cada pago asociado. Este valor tiene únicamente carácter referencial, ya que la información de la tabla **deudas** es volátil y no persistente. 

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

---

# 6. Ejemplos de consumo

## 6.1. Autenticar

### 6.1.1. Request

```bash
curl -X POST "http://localhost:7265/api/V1.0/seguridad/ObtenerToken" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -d '{
           "userType": "permiso",
           "userProfile": "usuario",
           "userPass": "contrasenia"
         }'
```

### 6.1.2. Response - Autenticar conforme
```json
{
  "Token": "Token generado",
  "ExpirationDate": "29/09/2026-04:57:36 p. m.",
  "CodResp": "00",
  "DesResp": "Ok"
}
```

### 6.1.3. Response - Autenticar con error
```json
{
  "Token": "",
  "ExpirationDate": "",
  "CodResp": "99",
  "DesResp": "Descripcion error"
}
```

## 6.2. Consultar

### 6.2.1. Request
```bash
curl -X POST "http://localhost:7265/api/V1.0/transac/consultar" \
     -H "accept: application/json" \
     -H "Authorization: Bearer TokenGenerado" \
     -H "Content-Type: application/json" \
     -d '{
           "empresa": "801",
           "servicio": "001",
           "id": "46827821",
           "tipo": "0",
           "canal": "10",
           "banco": "1020"
         }'
```

### 6.2.2. Response - Consultar conforme (con deuda)
```json
{
  "cliente": "JOYO VARGAS ANA CARO6160",
  "deudas": [
    {
      "idDeuda": "2",
      "servicio": "001",
      "documento": "5675763",
      "descripcionDoc": "PRIMAS OHIO PEN",
      "fechaVencimiento": "20250804",
      "fechaEmision": "20250804",
      "deuda": "6419.00",
      "pagoMinimo": "6419.00",
      "moneda": "1"
    }
  ],
  "CodResp": "00",
  "DesResp": "Conforme"
}
```

### 6.2.3. Response - Consultar conforme (sin deuda)
```json
{
  "cliente": "JOYO VARGAS ANA CARO6160",
  "deudas": [],
  "CodResp": "22",
  "DesResp": "Sin deudas"
}
```

### 6.2.4. Response - Consultar conforme (no existe cliente)
```json
{
  "cliente": "",
  "deudas": [],
  "CodResp": "16",
  "DesResp": "Cliente no existe"
}
```

### 6.2.5. Response - Consultar con error
```json
{
  "cliente": "",
  "deudas": [],
  "CodResp": "99",
  "DesResp": "Descripcion error"
}
```

## 6.3. Pagar

### 6.3.1. Request
```bash
curl -X POST "http://localhost:7265/api/V1.0/transac/pagar" \
     -H "accept: application/json" \
     -H "Authorization: Bearer TokenGenerado" \
     -H "Content-Type: application/json" \
     -d '{
           "fechaTxn": "29092025",
           "horaTxn": "171100",
           "idCanal": "10",
           "idForma": "01",
           "numeroOperacion": "A123456789",
           "idConsulta": "46827821",
           "servicio": "001",
           "numeroDocumento": "5675763",
           "importePagado": 6419.00,
           "moneda": "1",
           "idEmpresa": "801",
           "idBanco": "1020",
           "voucher": "",
           "referenciaDeuda": 2
         }'
```

### 6.3.2. Response - Pagar conforme
```json
{
  "cliente": "JOYO VARGAS ANA CARO6160",
  "operacionErp": "3",
  "CodResp": "00",
  "DesResp": "Conforme"
}
```

### 6.3.3. Response - Pagar con error
```json
{
  "cliente": "",
  "operacionErp": "0",
  "CodResp": "99",
  "DesResp": "Descripcion error"
}
```

## 6.4. Revertir

### 6.4.1. Request
```bash
curl -X POST "http://localhost:7265/api/V1.0/transac/revertir" \
     -H "accept: application/json" \
     -H "Authorization: Bearer TokenGenerado" \
     -H "Content-Type: application/json" \
     -d '{
           "fechaTxn": "29092025",
           "horaTxn": "171528",
           "idBanco": "1020",
           "idConsulta": "46827821",
           "idServicio": "001",
           "tipoConsulta": "0",
           "numeroOperacion": "A123456789",
           "numeroDocumento": "5675763",
           "idEmpresa": "801",
           "voucher": ""
         }'
```

### 6.4.2. Response - Revertir conforme
```json
{
  "cliente": "JOYO VARGAS ANA CARO6160",
  "operacionErp": "4",
  "CodResp": "00",
  "DesResp": "Conforme"
}
```

### 6.4.3. Response - Revertir con error
```json
{
  "cliente": "",
  "operacionErp": "0",
  "CodResp": "99",
  "DesResp": "Descripcion error"
}
```

---

# 7. Anexos

## 7.1. Bancos

| Banco       | Valor |
|-------------|-------|
| BCP         | 1020  |
| BBVA        | 1023  |
| SCOTIABANK  | 1024  |
| INTERBANK   | 1022  |
| BANBIF      | 1021  |
| PICHINCHA   | 1025  |
| COMERCIO    | 1026  |

## 7.2. Formas de pago

| Forma                | Valor |
|----------------------|-------|
| Efectivo             | 01    |
| Tarjeta Débito       | 02    |
| Tarjeta de Crédito   | 03    |
| Cheque mismo banco   | 04    |
| Tarjeta Virtual      | 05    |
| Cargo en cuenta      | 06    |
| Cargo en cuenta y efectivo | 07 |
| Cheque de otro banco | 08    |

## 7.3. Canales de pago

| Canal                   | Valor |
|--------------------------|-------|
| Ventanilla              | 10    |
| POS (Solo SCOTIABANK)   | 14    |
| ATM                     | 20    |
| Monederos               | 30    |
| Agente Corresponsal     | 40    |
| Kiosko Multimedia       | 50    |
| Internet Banking        | 60    |
| IVR                     | 70    |
| Banca Celular           | 80    |
| Otros                   | 99    |

> **Nota:** No todos los canales aplican para todos los bancos; depende de lo que tenga habilitado el ente recaudador.

## 7.4. Códigos de respuesta

| Valor | Descripción                  |
|-------|------------------------------|
| 00    | TRANSACCIÓN PROCESADA OK     |
| 16    | CLIENTE NO EXISTE            |
| 22    | CLIENTE SIN DEUDA PENDIENTE  |
| 99    | ERROR DESCONOCIDO            |