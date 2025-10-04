Al-Kuraimi Bank Task Management System
  Task Management System for Systems Development Department
System Objective
Develop a Task Management System to assist the Systems Development in organizing work, tracking task execution, and improving productivity through effective task distribution and progress management.
________________________________________
1. Functional Requirements
Task Management
•	Ability to add new tasks with details such as title, description, priority, system, and creation date.
•	Assign tasks to employees based on job specialization and experience level.
•	Support task statuses (In Progress, Completed, Pending, Overdue).
•	Add comments and notes for each task to track execution details.
User and Permission Management
•	Create employee accounts with information such as name, email, and job role.
•	Define permissions based on roles (Manager, Support Staff, Task Administrator).
•	Implement secure login using JWT or OAuth authentication.
Notifications and Alerts
•	Send real-time notifications when a new task is assigned or its status is updated.
•	Alert employees as the due date approaches or if a task is delayed.
Reports and Analytics
•	Display reports on completed and pending tasks for each employee.
•	Analyse department performance through metrics like task completion rate and average execution time (optional).
________________________________________
2. Non-Functional Requirements
High Performance and Fast Response
•	Optimize database queries using Indexing to speed up task searches.
•	Support asynchronous processing for heavy operations.
Scalability and Maintainability
•	Design a modular architecture to ensure ease of future development.
•	Support integration with other systems such as ticketing systems or email.
Security and Protection
•	Secure data using password encryption and strong authentication techniques.
•	Prevent attacks such as SQL Injection and Cross-Site Scripting (XSS) (optional).
________________________________________
User-Friendly Interface
•	Design an interactive dashboard to display tasks in an organized and easy-to-navigate manner.
•	Support quick navigation and instant search to provide a comfortable user experience.
________________________________________
3. Proposed Technology Stack
•	Frontend: ASP.NET MVC
•	Backend: ASP.NET Core Web API
•	Database: SQL Server using Entity Framework
•	Database: MYSQL
•	Authentication and Security: JWT Authentication
•	Architecture: Monolithic

 
