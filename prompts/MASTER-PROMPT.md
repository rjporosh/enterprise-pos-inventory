You are a Principal Enterprise Software Architect, Senior Product Designer, Senior UX Architect, Angular 22 Architect, React 19 Architect, and SaaS ERP Product Designer.

Your objective is to design and build a production-quality, enterprise-grade, multi-tenant SaaS Retail Management Platform.

This is NOT a demo, portfolio, or tutorial project.

The frontend architecture must be scalable enough to support thousands of businesses and millions of transactions in the future.

The application should be suitable for any retail business, including but not limited to:

- Clothing Store
- Boutique
- Burkha/Hijab Shop
- Super Shop
- Grocery Store
- Pharmacy
- Electronics Shop
- Mobile Shop
- Computer Shop
- Motor Parts Shop
- Auto Parts Shop
- Hardware Store
- Cosmetics Shop
- Furniture Store
- Book Store
- Sports Shop
- Gift Shop
- Pet Shop
- Bakery
- Restaurant (Future Module)
- Wholesale Business
- Distribution Company

The system must be designed as a configurable Multi-Tenant SaaS product where each business has its own:

- Company
- Branches
- Warehouses
- Products
- Employees
- Customers
- Suppliers
- Reports
- Settings
- Branding

No business-specific logic should be hardcoded.

Every feature should be configurable.

--------------------------------------------------
TECH STACK
--------------------------------------------------

POS Application

- Angular 22 (Latest)
- Standalone Components
- Angular Signals
- Angular Material
- TypeScript
- SCSS
- RxJS where appropriate
- Lazy Loading
- Feature-based Architecture
- Responsive Design
- Light & Dark Theme

Inventory & Back Office

- React 19 (Latest)
- TypeScript
- Material UI (MUI)
- React Router
- TanStack Query
- Zustand or Redux Toolkit
- SCSS
- Responsive Design
- Light & Dark Theme

--------------------------------------------------
IMPORTANT
--------------------------------------------------

Frontend UI only.

Do NOT create:

- Backend
- Authentication
- Authorization
- Database
- Express
- NestJS
- Spring Boot
- .NET
- Laravel

Use only:

- Mock Services
- Mock API Layer
- Fake JSON Data
- Interfaces
- Models

The application should be backend-ready.

--------------------------------------------------
DESIGN INSPIRATION
--------------------------------------------------

The UI quality should be comparable to enterprise software such as:

- Microsoft Dynamics 365
- SAP Business One
- Oracle NetSuite
- Odoo Enterprise
- Zoho Inventory
- Shopify POS
- Square POS
- Lightspeed Retail
- Vend POS
- QuickBooks Commerce

The design should feel modern, premium, clean, fast, and highly usable for cashiers, managers, warehouse staff, and business owners.

--------------------------------------------------
DESIGN SYSTEM
--------------------------------------------------

Create a complete design system including:

- Color Palette
- Typography
- Spacing System
- Elevation
- Icons
- Buttons
- Forms
- Cards
- Tables
- Chips
- Badges
- Dialogs
- Toast Notifications
- Loaders
- Skeleton Screens
- Empty States
- Error States
- Charts
- KPI Widgets
- Breadcrumbs
- Side Navigation
- Top Navigation
- User Menu
- Search UI
- Global Command Palette
- Responsive Grid System

--------------------------------------------------
ANGULAR POS MODULE
--------------------------------------------------

Design a complete POS interface including:

Dashboard

New Sale

Quick Sale

Barcode Scanner

Barcode Search

Product Grid

Product Search

Category Shortcuts

Cart

Customer Selection

Walk-in Customer

Discount

Coupon

Tax

Service Charge

Price Override

Quantity Update

Hold Sale

Resume Sale

Suspend Sale

Sales History

Returns

Exchange

Receipt Preview

Receipt Print

Kitchen Ticket (Future Ready)

Cash Drawer

Opening Cash

Closing Cash

Shift Closing

Multiple Payment Methods

Split Payment

Cash

Card

Mobile Banking

Gift Card

Store Credit

Due Payment

Refund

Offline Ready UI

Keyboard Shortcuts

--------------------------------------------------
REACT INVENTORY & BACK OFFICE MODULE
--------------------------------------------------

Dashboard

Products

Product Details

Variants

SKU Management

Barcode Management

Categories

Brands

Units

Attributes

Suppliers

Customers

Warehouses

Purchase Orders

Goods Receive Note

Stock Transfer

Stock Adjustment

Stock Count

Inventory Audit

Stock Ledger

Stock Movement

Inventory Valuation

Low Stock

Out of Stock

Dead Stock

Fast Moving Items

Slow Moving Items

Reorder Suggestions

Barcode Printing

Label Printing

Reports

Export

Dashboard Analytics

--------------------------------------------------
ADVANCED TABLE FEATURES
--------------------------------------------------

Every table should support:

Search

Filtering

Sorting

Column Resize

Column Reorder

Column Visibility

Pagination

Sticky Header

Row Selection

Bulk Actions

CSV Export

Excel Export

PDF Export (UI only)

Print

--------------------------------------------------
DASHBOARDS
--------------------------------------------------

Design professional dashboards containing:

Revenue

Sales

Profit

Expenses

Purchases

Orders

Returns

Inventory Value

Top Products

Top Customers

Top Categories

Warehouse Status

Recent Activities

Charts

KPIs

Widgets

--------------------------------------------------
MULTI-TENANT STRUCTURE
--------------------------------------------------

Design the UI to support:

Organization

Tenant

Branch

Warehouse

Store

Role

Permissions (UI placeholders only)

Business Settings

Branding

Currency

Timezone

Language

Tax Settings

Invoice Settings

Receipt Settings

--------------------------------------------------
REUSABLE COMPONENTS
--------------------------------------------------

Create reusable components for:

Data Table

Search Box

Filter Drawer

Status Badge

Metric Card

Stat Card

Charts

Dialogs

Drawer

Confirm Modal

Delete Modal

Product Card

Empty State

Loading State

Error State

Page Header

Action Toolbar

Breadcrumb

--------------------------------------------------
PROJECT STRUCTURE
--------------------------------------------------

Use enterprise-level folder structures for both Angular and React.

Include:

Components

Pages

Layouts

Shared

Core

Services

Interfaces

Models

Constants

Utilities

Routes

Guards (placeholder only)

Theme

Assets

Mock API

Sample JSON Data

--------------------------------------------------
DATA
--------------------------------------------------

Populate the application with realistic enterprise mock data including:

Products

Customers

Suppliers

Employees

Branches

Warehouses

Orders

Invoices

Payments

Purchase Orders

Inventory Movements

Reports

The application should feel like a real, production ERP system with realistic datasets rather than placeholder content.

--------------------------------------------------
QUALITY REQUIREMENTS
--------------------------------------------------

The code must be:

- Production-ready
- Clean Architecture
- Fully Componentized
- Scalable
- Maintainable
- Strongly Typed
- Responsive
- Accessible
- Reusable
- Consistent
- Enterprise-grade

Focus on delivering a premium user experience that can later be connected to a real backend without requiring major frontend restructuring.