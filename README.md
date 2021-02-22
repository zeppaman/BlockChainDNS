# Blockchain DNS

... TBD

## How it works

... TBD

[Protocol specifications](README.md)

## RUN THE DEMO
This project contains a demo ready to go. It is a self contained example using:

- a custom DNS as backed
- unit test for proving the producer side of the protocol
- unit test for reading data
- a web application to test keys and shows data

You just need to type `docker-compose up` for starting the project, anyway you have to enter data manually for testing it. With the following test case execution you will have data to test.

```bash
docker-compose up
# in BlockChainDNS.UnitTest folder
# Generate keys
dotnet test  --filter GenerateKeys 

# Show generated keys
ls \bin\Debug\netcoreapp3.1 


# Write Decript key
dotnet test --filter WriteDecryptKey

# Write sample data
dotnet test --filter CRUD

# This will output data records like {key}.1.bockchaindns.io

```

Now your data is set and the environment running, just take the URL from the console and use it into the web page, that is located at `http://localhost:2080`