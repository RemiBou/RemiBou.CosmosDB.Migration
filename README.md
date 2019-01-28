[![Build Status](https://dev.azure.com/remibou/toss/_apis/build/status/RemiBou.RemiBou.CosmosDB.Migration?branchName=master)](https://dev.azure.com/remibou/toss/_build/latest?definitionId=7?branchName=master)

# RemiBou.CosmosDB.Migration
A package for managing versionning of your CosmosDB objects (stored procedure, trigger ...) and migrating your data as your schema changes.

This package is not published yet to nuget, but you can use the source code in your project if you find it interesting.

# How to use it

## Install Package

```
Install-Package RemiBou.CosmosDB.Migration -Version 0.0.0-CI-20190126-145359
```
## Setup

- Create a folder "CosmosDB" on your project root 
- Create a folder "Migrations" on the CosmosDB folder (this should be handled by the package in the short term)
- Add the following line to your csproj inside a ItemGroup tag
```xml
    <EmbeddedResource Include="CosmosDB\Migrations\**\*.js" />
```
 - Add the following line for launching the migrations
```cs
await new CosmosDBMigration(documentClient).MigrateAsync(this.GetType().Assembly);// add .Wait() if your are in a not in an async context like the Startup
```
- Each type of migration must respect a given convention :
	- Stored Procedure : must be a js file in a folder named "StoredProcedure" on the Migration folder, the name of the file without extension will be the name of the stored procedure. 
	- User Defined Function : must be a js file in a folder named "Function" on the Migration folder, the name of the file without extension will be the name of the function.
	- Trigger : must be a js file in a folder named "Trigger" on the Migration folder, the name of the file without extension will have to be structured this way : "{Pre/Post}-{Create/Update/Delete/Replace/All}-{Name}.js"

- When the migration is launched, all the migrations definitions will be read from the embedded ressources and upsert on the databases and collections. If the given Database or Collection doesn't exists it'll be created.

# Feature list

## Already done

- Create Database
- Create Collection
- Create / Update Stored procedure
- Create / Update Triggers
- Create / Update User Defined Functions

## To do

- Create nuget package
- Change ressource folder
- Automate mos of the setup with the package insllation
- Send custom migration written in C#
- Delete Database / Collection / Stored Procedure / Trigger / UDF
- Bulk Operation defined by js file
- Custom Migration strategy
- Validate all the migrations before applying them
- Log
- create DB and collection without SP/UDF/Trigger
- ....
