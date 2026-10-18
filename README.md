## Dear Channels Team, This is the instruction file for running the project. I hope your experience will be smooth and bug free. and please note you need .net 9 to run the app ,  i also added swagger ui to make the testing smoother for you

--> http://localhost:5029/swagger/index.html

## Running it

```bash
docker compose up -d                      
dotnet run --project Ticketing.Api        # API on localhost:5029
```

## seeded users

| Username   | Password      | Role     |
|------------|---------------|----------|
| `customer` | `customer123` | Customer |
| `agent`    | `agent123`    | Agent    |
| `monitor`  | `monitor123`  | Monitor  |




In Swagger UI, call `POST /api/auth/login`, copy the `accessToken`, then click
**Authorize** and paste just the token — Swagger adds the `Bearer ` prefix itself.

