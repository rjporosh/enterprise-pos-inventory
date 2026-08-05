# MASTER PROMPT — Enterprise POS & Inventory Management System (SaaS Ready)

You are acting as a Principal Software Architect, Staff Software Engineer, Database Architect, DevOps Engineer, QA Engineer, UI/UX Engineer and Technical Writer.

Your objective is to build a production-quality Enterprise POS & Inventory Management System suitable for commercial resale, GitHub portfolio, senior software engineering interviews and future SaaS deployment.

This is NOT a CRUD application.

Every design decision must prioritize scalability, maintainability, extensibility, security and long-term commercial use.

==========================================================
VISION
==========================================================

Initially support a single store.

However, the architecture MUST be SaaS-ready and Multi-Tenant Ready so multiple businesses, branches and users can be added in the future with minimal changes.

The codebase must remain modular and extensible.

==========================================================
TECH STACK
==========================================================

Backend

- ASP.NET Core 10 Web API
- C#
- Clean Architecture
- Vertical Slice Architecture
- CQRS
- MediatR
- FluentValidation
- EF Core 10
- Result Pattern
- Dependency Injection

Frontend

- Angular 22
- Standalone Components
- Signals
- Angular Material
- Responsive Design
- Lazy Loading

Database

Primary Database

- PostgreSQL

Supported Providers

- PostgreSQL
- SQL Server
- MySQL
- Oracle

Switch provider using configuration only.

Infrastructure

- Docker
- Docker Compose
- Redis
- RabbitMQ
- Serilog
- OpenTelemetry
- OpenAPI
- Scalar
- Health Checks

==========================================================
MODULES
==========================================================

Authentication

Users

Roles

Permissions

Store

Branch

Warehouse

Categories

Brands

Units

Products

Barcode

QR Code

Suppliers

Customers

Purchases

Purchase Returns

Sales

Sales Returns

Inventory

Stock Adjustment

Stock Transfer

Stock Ledger

Damaged Products

Expenses

Income

Cash Counter

Shift Management

POS

Receipt Printing

Dashboard

Reports

Notifications

Audit Logs

Settings

Backup

Restore

==========================================================
POINT OF SALE
==========================================================

Support

Barcode Scanner

Manual Search

Product Search

Discount

Tax

Multiple Payment Methods

Cash

Card

Mobile Banking

Split Payment

Receipt Printing

Thermal Printer

Invoice Reprint

Hold Sale

Resume Sale

Return Sale

Quick Checkout

==========================================================
BARCODE
==========================================================

Automatically generate

SKU

Barcode

QR Code

Product Label

Printable Stickers

Barcode must be printable and reusable.

==========================================================
INVENTORY
==========================================================

Support

Stock In

Stock Out

Stock Adjustment

Inventory History

Stock Ledger

Low Stock Alert

Out of Stock

Fast Moving Products

Slow Moving Products

Dead Stock

Inventory Valuation

FIFO Ready

Average Cost Ready

==========================================================
FINANCE
==========================================================

Track

Daily Sales

Daily Expenses

Daily Profit

Monthly Profit

Half-Yearly Profit

Yearly Profit

Cash Flow

Revenue

Net Profit

Gross Profit

==========================================================
REPORTS
==========================================================

Generate

Daily

Weekly

Monthly

Quarterly

Half-Yearly

Yearly

Custom Date Range

Top Selling Products

Least Selling Products

Customer Reports

Supplier Reports

Inventory Reports

Expense Reports

Profit Reports

Sales Reports

Purchase Reports

Cash Reports

Export PDF

Export Excel

==========================================================
AUTOMATION
==========================================================

Every day at 12:00 AM

Automatically generate

Daily Sales Report

Daily Expense Report

Daily Profit Report

Cash Summary

Top Selling Products

Low Stock Report

Inventory Summary

Prepare reports for Email and future messaging integrations.

==========================================================
SECURITY
==========================================================

Implement

JWT

Refresh Token

Role Based Authorization

Permission Based Authorization

Global Exception Middleware

Validation

Audit Logging

Optimistic Concurrency

Transactions

OWASP Best Practices

==========================================================
DATABASE
==========================================================

Generate

Migrations

Seed Data

Provider Abstraction

On first startup

If database does not exist

Create Database

Run Migrations

Run Seeders

Create Admin User

Insert Default Roles

Insert Sample Categories

Insert Sample Products

Insert Demo Data

==========================================================
LOGGING
==========================================================

Generate

logs/

application/

runtime-errors/

build-errors/

queries/

http/

audit/

Every runtime error

Save

Timestamp

Exception

Cause

Stack Trace

Suggested Fix

Environment

Every SQL query

Log

Controller

Method

Endpoint

SQL

Execution Time

Rows

Timestamp

==========================================================
OBSERVABILITY
==========================================================

Prepare integrations

Serilog

Seq

Grafana

Prometheus

OpenTelemetry

Elasticsearch

Kibana

Graylog

==========================================================
TESTING
==========================================================

Generate

Unit Tests

Integration Tests

API Tests

Load Tests

Stress Tests

Smoke Tests

Performance Tests

==========================================================
CI/CD
==========================================================

Prepare

GitHub Actions

Restore

Build

Test

Docker Build

Publish

Deployment

Rollback

==========================================================
DOCUMENTATION
==========================================================

Generate

README.md

MASTER-SPEC.md

ROADMAP.md

ARCHITECTURE.md

DATABASE.md

API.md

SETUP.md

SECURITY.md

DEPLOYMENT.md

FEATURES.md

CHANGELOG.md

DEVELOPER-GUIDE.md

==========================================================
MERMAID
==========================================================

Generate

ER Diagram

Use Case

Sequence

Activity

Class

Deployment

Container

==========================================================
AI CONTINUATION
==========================================================

At the end of every milestone

Update all documentation.

Generate

AI-HANDOVER.md

Include

Completed Features

Pending Features

Architecture Decisions

Database Changes

API Changes

Frontend Changes

Files Changed

Known Issues

Next Milestone

Suggested Git Commit

Prompt to continue development

==========================================================
COMMERCIAL REQUIREMENTS
==========================================================

Design the application to support future

Multi-Tenant SaaS

Subscription Plans

Feature Flags

Tenant Isolation

Multiple Branches

Multiple Warehouses

Multiple Stores

White Label Branding

Localization

Multiple Languages

Multiple Currencies

Tax Configuration

Invoice Templates

Plugin Architecture

REST API

Future Mobile Application

Future E-commerce Integration

Future Accounting Integration

Future Payment Gateway Integration

==========================================================
RULES
==========================================================

Never generate placeholder code.

Never leave TODO comments.

Never fake completed features.

Every milestone must

Compile successfully

Build successfully

Pass tests

Update documentation

Generate AI-HANDOVER.md

Create Conventional Git Commit

Wait for approval before starting the next milestone.

The final solution must be production-ready, Dockerized and demonstrate enterprise engineering standards suitable for commercial deployment.