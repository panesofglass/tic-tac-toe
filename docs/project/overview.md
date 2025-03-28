# Project Overview

This project implements a classic Tic-Tac-Toe game using the datastar hypermedia framework. The goal is to demonstrate how interactive web applications can be built with minimal JavaScript by leveraging datastar's declarative data binding approach and server-rendering techniques.

## Core Design Principles

1. Hypermedia-Driven Architecture
   - Application state and interactions managed through HTML attributes
   - Server-rendered HTML with progressive enhancement
   - Real-time updates via server-sent events

2. Minimal JavaScript
   - Uses datastar.js for declarative data binding
   - No custom JavaScript required
   - Progressive enhancement approach

3. Clean Architecture
   - Separation of game logic (Engine) from web presentation
   - Immutable game state management
   - Event-sourced move history

## Technology Stack

- .NET 8 with MinimalAPI and RazorSlices
- datastar for hypermedia-driven interactions
- Server-sent events (SSE) for real-time updates
