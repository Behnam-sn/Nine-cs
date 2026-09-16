Feature: Register and sign in
  Users create an identity with a password and receive an access token
  so they can call APIs as themselves.

Scenario: A new user registers, signs in, and reads their current identity
  Given a person with a unique email and password "Password1!"
  When they register
  Then registration succeeds
  When they sign in with the password grant
  Then they receive an access token
  When they request the current user
  Then the current user matches the registered identity

Scenario: Duplicate email is rejected
  Given a registered user
  When they try to register again with the same email
  Then registration is rejected because the email is already in use

Scenario: Weak password is rejected
  Given a person with a unique email and password "short"
  When they register
  Then registration is rejected as a bad request

Scenario: Wrong password is rejected
  Given a registered user
  When they sign in with password "WrongPass1!"
  Then sign in is rejected

Scenario: Current user requires a valid access token
  When someone requests the current user without a token
  Then the request is unauthorized

Scenario: A truncated access token is rejected
  Given a registered user who has signed in
  When they request the current user with only the token header
  Then the request is unauthorized
