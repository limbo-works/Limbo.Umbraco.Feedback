# Endpoints






## Add a new entry

Adds a new feedback entry with the details from the request body.

If you have implemented a custom feedback plugin, this will trigger the [`OnEntryAdding`](./plugins.md#onentryadding) method to be called while the feedback entry is still being processed, and the [`OnEntryAdded`](./plugins.md#onentryadded) method after the entry has been processed and added to the database.

By default, the `name`, `email` and `comment` properties in the request body are optional. But your custom feedback plugin may change this behaviour - eg. by rejecting all entries that don't have a comment. Or require all three properties to be specified if the user submits a feedback entry with a negative rating. If you're doing something like this, your frontend should ideally do the same validation for a better user experience.

```yml endpoint
method: POST
url: /api/feedback
accepts: application/json
parameters:
- type: string
  in: header
  name: content-type
  description: "Should be <code>application/json</code>."
- type: object
  in: body
  properties:
  - type: string
    format: uuid
    name: siteKey
    required: true
    description: "The GUID key of the site."
    example: "16f104a1-9458-4676-8a96-c9f552595b87"
  - type: string
    format: uuid
    name: pageKey
    required: true
    description: "The GUID key of the page."
    example: "cb3edc1b-3988-47d8-9d95-f58537b1df08"
  - type: string
    format: uuid
    name: rating
    required: true
    description: "The GUID key of the rating."
    example: "00000000-0000-0000-0001-000000002000"
  - type: string
    name: name
    description: "The name of the user."
    example: "John Doe"
  - type: string
    name: email
    description: "The email address of the user."
    example: "john-doe@example.com"
  - type: string
    name: comment
    description: "The user's comment."
    example: "Hello World"
responses:
- status: 201
  content-type: application/json
  description: "Created"
  body: 
    type: object
    properties:
    - type: string
      format: uuid
      name: key
      description: "The GUID key of the added feedback entry."
    example: >
      {
        "key": "bdbc8a5d-c134-41b9-9e08-ed78a36419af"
      }
- status: 400
  content-type: application/json
  description: "Validation failed"
  body: 
    type: object
    properties:
    - type: string
      name: message
      description: "A messsage about why the request was cancelled."
    example: >
      {
        "message": "The feedback submission was rejected by the server."
      }
- status: 429
  content-type: application/json
  description: "Too Many Requests"
  body: 
    type: object
    properties:
    - type: string
      name: message
      description: "An error message that may be shown to the user."
    example: >
      {
        "message": "The request was aborted due to too many requests."
      }
- status: 500
  content-type: application/json
  description: "Server Error"
  body: 
    type: object
    properties:
    - type: string
      name: message
      description: "A messsage about why the request failed."
    example: >
      {
        "message": "The feedback submission could not be saved due to an error on the server."
      }
```

## Update an entry

The feedback module allows you to update a feedback entry after it has been created. Among other things, this is to handle a scenario where the user's choice of rating is save first, and then the entry is later updated with additional information if the user chooses to specify these as well.

Similar to when adding a new entry, if you have custom feedback plugin, updating an existing entry will trigger the [`OnEntryUpdating`](./plugins.md#onentryupdating) method to be called while the feedback entry is still being processed, and the [`OnEntryUpdated`](./plugins.md#onentryupdated) method after the entry has been processed and updated in the database.

By default the feedback module doesn't apply any restriction to when and who can update an entry as long as the entry's GUID key is known. If you need this kind of restriction, it would be up to you to handle this through a custom feedback plugin.

```yml endpoint
method: PATCH
url: /api/feedback/{key}
accepts: application/json
parameters:
- type: string
  format: uuid
  in: path
  name: key
  description: "The GUID key of the feedback entry to update."
- type: object
  in: body
  properties:
  - type: string
    format: uuid
    name: siteKey
    required: true
    description: "The GUID key of the site."
    example: "16f104a1-9458-4676-8a96-c9f552595b87"
  - type: string
    format: uuid
    name: pageKey
    required: true
    description: "The GUID key of the page."
    example: "cb3edc1b-3988-47d8-9d95-f58537b1df08"
  - type: string
    name: name
    description: "The name of the user."
    example: "John Doe"
  - type: string
    name: email
    description: "The email address of the user."
    example: "john-doe@example.com"
  - type: string
    name: comment
    description: "The user's comment."
    example: "Hello World"
responses:
- status: 200
  content-type: application/json
  description: "OK"
  body: 
    type: object
    properties:
    - type: string
      format: uuid
      name: key
      description: "The GUID key of the added feedback entry."
    example: >
      {
        "key": "bdbc8a5d-c134-41b9-9e08-ed78a36419af"
      }
- status: 400
  content-type: application/json
  description: "Validation failed"
  body: 
    type: object
    properties:
    - type: string
      format: uuid
      name: key
      message: "An error message that may be shown to the user."
    example: >
      {
        "message": "The feedback submission was rejected by the server."
      }
- status: 404
  content-type: application/json
  description: "Not Found"
  body: 
    type: object
    properties:
    - type: string
      format: uuid
      name: key
      message: "An error message that may be shown to the user."
    example: >
      {
        "message": "An entry with the specified key could not be found."
      }
- status: 429
  content-type: application/json
  description: "Too Many Requests"
  body: 
    type: object
    properties:
    - type: string
      format: uuid
      name: key
      message: "An error message that may be shown to the user."
    example: >
      {
        "message": "The request was aborted due to too many requests."
      }
```
