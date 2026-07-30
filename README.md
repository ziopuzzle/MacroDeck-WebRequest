# MacroDeck-WebRequest
Web request plugin for MacroDeck

### Important Notes
- This is a plugin for [Macro Deck 2](https://github.com/Macro-Deck-App/Macro-Deck)(v2.15.0), it does NOT function as a standalone app!

## Features
Supported method: GET, POST, PUT, DELETE, PATCH and other (manual input)

Supported variables (input and output)

Default User-Agent: MacroDeck-WebRequest/1.0

## Variables
A variable is created after the request.

| Variable | Description | Type |
| --- | --- | --- |
| wr_response_status | HTTP status code of last request<br>Note: if an error occurred during the last request, this value set to 0. | Integer |
| wr_response_body | Response of last request<br>Note: if an error occurred during the last request, this value set to error message. | String |
| [Variable Name]_status | HTTP status code of last success request | Integer |
| [Variable Name]_body | Response of last success request | String |
