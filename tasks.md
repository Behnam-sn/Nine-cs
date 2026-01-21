# Tasks

## Backlog

- Restructure the project to vertical slices
- Add `Nine.Content.Domain.Tests`
- Add `Password` to `User`
- Rename `OccurredAt` to `Timestamp` in `IDomainEvent`

## Todo

- [ ] Move `Users` from `Nine.Domain` to `Nine.Identities.Domain`
- [ ] Remove `Nine.Domain` project 
- [ ] Create `Post` aggregate root

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
