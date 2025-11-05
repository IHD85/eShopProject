Test Catalog API
a)Opret brand:
POST http://localhost:8083/api/CatalogBrand
{
  "brand": "Apple"
}

(b) Opret type:
POST http://localhost:8083/api/CatalogType
{
  "type": "Laptop"
}


(c) Opret produkt:
POST http://localhost:8083/api/Catalog
{
  "catalogBrandId": 1,
  "catalogTypeId": 1,
  "name": "MacBook Pro 14",
  "description": "M3 Pro 16GB RAM",
  "price": 18999.99,
  "pictureUri": "macbook.png",
  "availableStock": 10
}

du kan finde i browser:FX
gateway: http://localhost:8080/catalog/api/Catalog
local: http://localhost:8083/api/Catalog
___________________________________________________________________________________


Test Basket API

(a) Tilføj produkt til basket
POST http://localhost:8081/api/Basket
{
  "customerId": "cust-1001",
  "items": [
    { "productId": 1, "productName": "Keyboard", "price": 300, "quantity": 1 }
  ]
}


(b) Hent basket:
http://localhost:8081/api/Basket/cust-1001
((((((Dette publicerer BasketCheckedOutIntegrationEvent til RabbitMQ.
Order.API vil fange eventet og gemme ordren i OrderDb.))))

du kan finde i browser:
gateway: http://localhost:8080/basket/api/Basket/cust-9999 
local: http://localhost:8081/api/Basket/cust-9999
gateway:http://localhost:8080/basket/api/Basket/cust-9991/checkout

_____________________________________________________________________________________

Tjek at ordren blev oprettet
http://localhost:8082/api/Order
_________________________________________________________________________________________
Test via Gateway (YARP)

http://localhost:8080/basket/api/Basket/cust-1001
______________________________________________________________________________________
Overvåg RabbitMQ

http://localhost:15672

Login: guest / guest

___________________________________________________________________________________________
EF-migrations (kun første gang lokalt)
catalog:
dotnet ef migrations add InitialCreate -p eShop.Catalog.Infrastructure -s eShop.Catalog.API -o eShop.Catalog.Infrastructure/Migrations

order:
dotnet ef migrations add InitialCreate -p eShop.Order.Infrastructure -s eShop.Order.API -o eShop.Order.Infrastructure/Migrations

Identity:
dotnet ef migrations add InitialCreate -p eShop.Identity.Infrastructure -s eShop.Identity.API -o eShop.Identity.Infrastructure/Migrations


-p = projektet hvor DbContext ligger (Infrastructure)

-s = startup projektet (API)

-o = hvor migrations skal gemmes
_____________________________________________________________________________________________________

docker compose up -d --build  ##føste gang

docker compose up -d 
docker compose down      ## stop docker
_____________________________________________________________________________________

identity test gateway postman post
http://localhost:8080/identity/api/auth/register
http://localhost:8080/identity/api/auth/register-admin
http://localhost:8080/identity/api/auth/login

lokal: http://localhost:8084/api/auth/register-admin

test token(jwt)
https://www.jwt.io/
________________________________________________________________________________
Info :
Når Basket → RabbitMQ → Order modtager basket.checkedout:

Order gemmes i database.

Order sender derefter et OrderCreatedIntegrationEvent via _eventBus.Publish().

Catalog.API, som allerede har Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>(), modtager det.

Catalog.API reducerer lageret (AvailableStock -= Quantity).
______________________________________________________________________
så test i postman:
Test af hele eShop-flowet

1.Opret Admin:
POST http://localhost:8080/identity/api/auth/register-admin
Body:
{
  "username": "admin1",
  "password": "Admin123!"
}
Kopier den JWT-token du får tilbage. Den skal bruges i de næste kald som Authorization: Bearer <token>



2.Tilføj produkt i Catalog
POST http://localhost:8080/catalog/api/catalog
Headers:
Authorization: Bearer <admin-token>
Content-Type: application/json
men husk brand og type at add
Body:
{
  "catalogBrandId": 1,
  "catalogTypeId": 1,
  "name": "MacBook Pro",
  "description": "16GB RAM, M3 chip",
  "price": 17999,
  "availableStock": 10,
  "pictureUri": "macbook.png"
}


3. Opret almindelig bruger (kunde)
-POST http://localhost:8080/identity/api/auth/register
{
  "username": "ihab",
  "password": "User123!"
}

-Login som bruger for at få token
POST http://localhost:8080/identity/api/auth/login
{
  "username": "ihab",
  "password": "User123!"
}

Gem den JWT-token. Den bruges til Basket-kald.

3. Tilføj varer til kurv og gennemfør checkout
POST http://localhost:8080/basket/api/Basket
Headers:
Authorization: Bearer <user-token>
Content-Type: application/json
Body:
{
  "customerId": "cust-9991",
  "items": [
    {
      "productId": 1,
      "productName": "MacBook Pro",
      "price": 17999,
      "quantity": 2
    }
  ]
}


Checkout
POST http://localhost:8080/basket/api/Basket/cust-9991/checkout
{
  "customerId": "cust-9991"
}

4.Verificér asynkron kommunikation
Basket.API        sender basket.checkedout event
Order.API         modtager event, opretter ny ordre og sender OrderCreatedIntegrationEvent
Catalog.API       modtager event og opdaterer lageret (AvailableStock -= Quantity)


5.Tjek databaser i DBeaver eller psql

 Database           Tabel                                   Indhold                      
 ------------  ----------------------------------  ---------------------------- 
 CatalogDb          CatalogItems                            Lager opdateret              
 OrderDb            Orders, OrderItems                      Ny ordre oprettet            
 BasketDb           tom (fordi Redis bruges til cache)                               
 IdentityDb         Users                                   Admin og brugere registreret 
