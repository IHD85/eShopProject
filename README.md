
# eShop Projekt - Microservices.

****Gruppe****: Ihab, Seymen, Nour og Adil.

---

### Opsætning

Ingen manuel opsætning kræves udover at man kører Docker compose filen.

Man skal dog sørge for at port `5432`, `7163`, `6379`, `15672` og `5672` ikke er i brug.

Disse er bevidst exposed (dog vil man i reel produktion ikke expose alle disse).

---

  

### Build

Før man kører compose filen er det vigtigt at builde projektet.
```powershell

docker compose build

```

---

  

### Start


```powershell

docker compose up

```

---

  

## Gateway (YARP)

Projektet gør brug af en Gateway, hvilket betyder at man ikke direkte kan tilgå de forskellige services.

Gatewayen er udviklet med ****YARP****.

  

Services som kan tilgås via Gatewayen er: ****Basket****, ****Catalog****, ****Order**** og ****Identity****.

Frontendappen i monolitten kan kun tilgå services via gateway.

  

---

  

## Basket Service Endpoints (Med Gateway)

  

### POST: Opret Basket for en kunde med Customer Id

****Localhost URL:****
http://localhost:7163/basket/Basket

  

****Eksempel JSON request body:****

```json

{
"customerId": "cust-1001",
  "items": [
    {
      "productId": 1,
      "productName": "Keyboard",
      "price": 300,
      "quantity": 1
    }

  ]
}

```

  

Dette tilføjer basket til en Redis database (da baskets er midlertidige).

  

---

  

### GET: Hent en basket for én kunde

****Localhost URL:****

http://localhost:7163/basket/Basket/cust-1001

  

Dette henter basket data for kunden med id `cust-1001`.

  

---

  

### POST: Basket checkout for kunden

****Localhost URL:****

http://localhost:7163/basket/Basket/cust-1001/checkout

  

Ingen JSON body kræves.

Dette vil checkoute kunden med id `cust-1001` og sende en besked til ****RabbitMQ****, som ****Order Service**** er subscribed til.

Derudover slettes basket fra Redis.

  

---

  

## Catalog Service Endpoints (Med Gateway)

  

### POST: Opret ny Catalog Brand

****Localhost URL:****

http://localhost:7163/catalog/catalog-brands/add?brandName=ExampleBrand

 

****Eksempel:****

Ingen body – brand oprettes via query param.

  

---

  

### GET: Hent alle Catalog Brands

****Localhost URL:****

http://localhost:7163/catalog/catalog-brands

  

---

  

### GET: Hent alle Catalog Types

****Localhost URL:****

http://localhost:7163/catalog/catalog-types

---
 

### GET: Tilføj Catalog Type

****Localhost URL:****

http://localhost:7163/catalog/catalog-types/add?typeName=ExampleType

---

  

### GET: Hent alle Catalog Items

****Localhost URL:****

http://localhost:7163/catalog/catalog-items

  

Understøtter: `pageSize`, `pageIndex`, `catalogBrandId`, `catalogTypeId`.

  

---

  

### POST: Opret Catalog Item

****Localhost URL:****

http://localhost:7163/catalog/catalog-items

  

****Eksempel JSON request body:****

```json

{
"name": "SuperFed Hovedtelefon",
"description": "Luksus lyd",
"price": 1999.95,
"pictureUri": "http://example.com/img.png",
"catalogTypeId": 1,
"catalogBrandId": 1
}

```

  

---

  

### GET: Hent Catalog Item med ID

****Localhost URL:****

http://localhost:7163/catalog/catalog-items/1

  

---

  

### DELETE: Slet Catalog Item

****Localhost URL:****

http://localhost:7163/catalog/catalog-items/1

  

---

  

### PUT: Opdater Catalog Item

****Localhost URL:****

http://localhost:7163/catalog/catalog-items

  

****Eksempel JSON request body:****

```json

{
"id": 1,
"name": "Opdateret Hovedtelefon",
"description": "Endnu bedre luksus lyd",
"price": 2100.00,
"pictureUri": "http://example.com/img.png",
"catalogTypeId": 1,
"catalogBrandId": 1
}

```
