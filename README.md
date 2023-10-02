# SmartCharging 
version: 18

## How to run the project

1. Create a `MySQL` container

`docker container run -d --name smart-charging-mysql-db -p 3306:3306 -e MYSQL_ROOT_PASSWORD=admin123456 mysql`

2. Apply migrations

`cd SmartCharging.Persistence` -> where `dbContext` is.
`dotnet ef --startup-project ../SmartCharging.API  database update`

## Project Description

* The business logic is in the `API` layer. There is a `GroupManager`.
* Entities and DTO objects can be found in the `Domain` layer.
* Database connection and relations are in the `Persistence` layer.
* In the `repository` layer, all CRUD operations are possible.
* Unfortunately at this stage, the `Service` layer is unnecessary.
* There are 39 unit tests in the `API.Test` layer to test all business logic in `GroupManager`.
## Tech Stack

.NET 7
EF Core 7
MySQL

### Libraries
Automapper ( If I had more time, would remove it entirely)
FluentValidation
Swashbuckle.AspNetCore (for swagger)
Xunit and Moq (For testing)


## Future Improvements

1. Dockerize the solution. Although I added `Dockerfile` and `docker-compose-yaml`, I had an error regarding the connection to the database and could not fix it.
2. Remove Automapper entirely
3. Simplify the project by removing the `Service` layer. The initial design was a bit different but current stage it adds another unnecessary abstraction.
4. Add more tests for the `Repository` layer. Integration tests should be added, too.
