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

lokal: http://localhost:8084/api/auth/register-admin

test token(jwt)
https://www.jwt.io/# eShopProject