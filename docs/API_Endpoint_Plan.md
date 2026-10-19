# RaceDay API Endpoint Plan

> Copy your Part 1 API_Endpoint_Plan.md here before submission.

## Endpoints Summary

| # | Method | Route | Description | Role Required |
|---|--------|-------|-------------|---------------|
| 1 | POST | /api/auth/register | Register new user | Public |
| 2 | POST | /api/auth/login | Log in | Public |
| 3 | POST | /api/auth/logout | Log out | Authenticated |
| 4 | GET | /api/users/profile | Get own profile | Authenticated |
| 5 | PUT | /api/users/profile | Update own name | Authenticated |
| 6 | GET | /api/events | List events | Authenticated |
| 7 | GET | /api/events/{id} | Get single event | Authenticated |
| 8 | POST | /api/events | Create event | Organiser |
| 9 | PUT | /api/events/{id} | Update event | Organiser (owner) |
| 10 | DELETE | /api/events/{id} | Delete event | Organiser (owner) |
| 11 | GET | /api/categories | List categories | Authenticated |
| 12 | GET | /api/categories/{id} | Get category | Authenticated |
| 13 | POST | /api/categories | Create category | Organiser |
| 14 | PUT | /api/categories/{id} | Update category | Organiser |
| 15 | DELETE | /api/categories/{id} | Delete category | Organiser |
| 16 | GET | /api/eventcategories/event/{eventId} | Categories for event | Authenticated |
| 17 | POST | /api/eventcategories | Link category to event | Organiser (owner) |
| 18 | DELETE | /api/eventcategories/{id} | Unlink category | Organiser (owner) |
| 19 | POST | /api/enrolments | Enrol in category | Participant |
| 20 | GET | /api/enrolments/my | My enrolments | Participant |
| 21 | GET | /api/enrolments/event/{eventId} | Event enrolments | Authenticated |
| 22 | DELETE | /api/enrolments/{id} | Withdraw | Participant (own) |
| 23 | POST | /api/results | Capture result | Organiser (owner) |
| 24 | GET | /api/results/event/{eventId} | Event results | Authenticated |
| 25 | PUT | /api/results/{id} | Update result | Organiser (owner) |
