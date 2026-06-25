# Design: GET /Api/count endpoint

## Summary

Add a read-only endpoint to `QuotesApiController` that returns the total number of quotes in the database.

## Endpoint

- **Route:** `GET /Api/count`
- **Auth:** none (public, consistent with the existing `GET /Api` and `GET /Api/random`)
- **Response:** `200 OK`, `Content-Type: application/json`
- **Response body:**
  ```json
  { "count": 42 }
  ```
  where `42` is the current total row count of the `Quote` table.

## Constraints

- Follow the existing `QuotesApiController` style — same route prefix, same return pattern as `GetAll` / `GetById` / `GetRandom`
- No new files, no new dependencies
- No auth attribute needed (existing public endpoints carry none)

## Acceptance Criteria

- [ ] `GET /Api/count` returns `200 OK`
- [ ] Response body is a JSON object with a single integer field `count`
- [ ] `count` equals the number of rows in the `Quote` table
- [ ] No authentication is required to call the endpoint
- [ ] Existing endpoints (`GET /Api`, `GET /Api/{id}`, `GET /Api/random`) are unaffected
