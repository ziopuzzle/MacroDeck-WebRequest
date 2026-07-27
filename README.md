# MacroDeck-WebRequest
Web request plugin for MacroDeck

### Important Notes
- This is a plugin for [Macro Deck 2](https://github.com/Macro-Deck-App/Macro-Deck)(v2.15.0), it does NOT function as a standalone app!

## Features
Supported method: GET / POST / PUT / DELETE / PATCH

Supported variables

Default User-Agent: MacroDeck-WebRequest/1.0

## Variables
A variable is created after the request.

| Variable | Description | Type |
| --- | --- | --- |
| [Variable Name]_status<br>Default:wr_response_status | HTTP status code of last request | Integer |
| [Variable Name]_body<br>Default:wr_response_body | Response of last request | String |
