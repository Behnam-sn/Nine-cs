# Social Media Platform (DDD + CQRS + Event Sourcing)

This project is a **social media application backend** built with **C#/.NET**, designed using **Domain-Driven Design (DDD)**, **Hexagonal Architecture (Ports & Adapters)**, **CQRS**, and **Event Sourcing**.

The goal of this project is to build a **scalable, maintainable, and evolvable system** that models a real-world social media domain while keeping business logic cleanly separated from infrastructure concerns.

---

## 🧠 Architectural Overview

This project follows these core architectural principles:

- **Domain-Driven Design (DDD)**  
  Clear bounded contexts, aggregates, and domain events

- **Hexagonal Architecture (Ports & Adapters)**  
  Domain and application layers are independent of frameworks

- **CQRS (Command Query Responsibility Segregation)**  
  Separate write (command) and read (query) models

- **Event Sourcing**  
  Aggregates are persisted as event streams, not state snapshots

- **Event-Driven Architecture**  
  Domain and integration events are used to propagate changes

---

## 🧩 Bounded Contexts

The system is divided into the following bounded contexts:

- **Identity** – users, profiles, account state
- **SocialGraph** – follow relationships and privacy rules
- **Content** – posts, media, visibility
- **Interaction** – likes, reactions, comments
- **Feed** – timeline generation and ranking
- **Notification** – user notifications
- **Moderation** – reports and content safety

Each bounded context owns:
- Its aggregates
- Its domain events
- Its read models
