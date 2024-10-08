# Library API
Frontend: https://github.com/SebastianFurmaniak02/LibraryApp
<br>
This repo is an API for Library project.
<br>
## What is done
- Connection to PSQL database using dapper - raw sql queries.
- Authorization and authentication - different users has different rights, the data is saved in JWT token.
- SQL injection protection on all endpoints.
- Use of database mechanisms - Master slave replication, transactions, triggers, prepared queries and stored procedures.
- Service that validates the booking time - if the booking is expired make the assortment available.
- Service that once per day subtract the one day from borrowing days left
- SSL connection database <--> backend.
