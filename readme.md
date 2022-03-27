# Stowage Explorer
A simple project to explore the library [Stowage](https://github.com/aloneguid/stowage).

## Concept
A fictitious application needs to work with files and directories in three locations (i.e. Cloud1, LocalStorage and TempStorage) and on two separate platforms (i.e. Azure Blob Storage and local disk). The `Stowage` library is utilized to standardize these tasks.

## Project Structure
* SE.Domain - .NET 6 class library which contains all logic for the implementation.
* SE.ConsoleApp - .NET 6 console application used as a runtime to demonstrate the logic in the SE.Domain library.

## Blog Post
[Exploring Stowage](https://developingdane.com/exploring-stowage/)