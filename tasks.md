# Tasks

## Backlog

- Add `Password` to `User`
- Rename `OccurredAt` to `Timestamp` in `IDomainEvent`

## Todo

- [ ] Add `Nine.Contents.Domain` project in `Contents` folder
- [ ] Reference `Nine.SharedKernel` to `Nine.Contents.Domain`
- [ ] Add `Nine.Contents.Domain.Tests` project in `Contents` folder
- [ ] Reference `Nine.Contents.Domain` to `Nine.Contents.Domain.Tests`
- [ ] Add `Post` aggregate root in `Nine.Contents.Domain`
- [ ] Add `PostTests` in `Nine.Contents.Domain.Tests`

## Doing

## Done

- [x] Convert `UserFirstNameChangedDomainEventV1` constructor to a primary constructor
- [x] Use value objects in events
- [x] Use `FirstName` value object to `User`
- [x] Use `LastName` value object to `User`
- [x] Add `ChangeLastName` method to `User`
- [x] Replace `Create` method with a constructor in `User`
- [x] Add `Email` to `User`
- [x] Create `PhoneNumber` value object
- [x] Add `PhoneNumber` to `User`
- [x] Create `Username` value object
- [x] Add `Username` to `User`
- [x] Create `UserStates` enum
- [x] Add `State` property to `User`
- [x] Create `Name` value object in `Users`
- [x] Replace `FirstName` and `LastName` with `Name` in `User`
- [x] Add `CreateInstance` method to `User`
- [x] Rename `ChangeName` to `SetName` in `User`
- [x] Create `Nine.SharedKernel` project
- [x] Move `Abstractions` from `Nine.Domain` to `Nine.SharedKernel`
- [x] Create `Nine.Identities.Domain` project
- [x] Reference `Nine.SharedKernel` to `Nine.Identities.Domain`
- [x] Move `Users` from `Nine.Domain` to `Nine.Identities.Domain`
- [x] Change `Create` method access level from `internal` to `public` in `DomainEventId`
- [x] Remove `Nine.Domain` project 
- [x] Add `Nine.Identities.Domain.Tests` project in `Identities` folder
- [x] Reference `Nine.Identities.Domain` to `Nine.Identities.Domain.Tests`
- [x] Add `UserTests` class in `Nine.Identities.Domain.Tests`
