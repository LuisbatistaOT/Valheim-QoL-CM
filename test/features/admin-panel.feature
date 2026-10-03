Feature: Admin panel for spec 001 version 0.001

  Scenario: Admin opens the panel
    Given the local player is an admin
    When the player presses the panel hotkey
    Then the admin panel is visible
    And the panel shows version "0.001"

  Scenario: Non-admin cannot open the panel
    Given the local player is not an admin
    When the player presses the panel hotkey
    Then the admin panel stays closed

  Scenario: Mode toggles stay independent
    Given God mode is on and Fly mode is off
    When the admin turns God mode off
    Then God mode is off
    And Fly mode is still off

  Scenario: Dead character rejects a mode toggle
    Given the admin character is dead
    And God mode is off
    When the admin turns God mode on
    Then the toggle is rejected
    And God mode stays off

  Scenario: Teleport uses the connected list
    Given "Ari" is a connected player
    When the admin brings themselves to "Ari"
    Then the admin moves to "Ari"
    And no name was typed

  Scenario: Spawn rejects a bad quantity
    Given the item "Wood" exists with max quality 1
    When the admin spawns "Wood" with quantity 0 and quality 1
    Then nothing is spawned
    And the panel reports an invalid quantity

  Scenario: Spawn uses the chosen quantity
    Given the item "Wood" exists with max quality 1
    When the admin spawns "Wood" with quantity 5 and quality 1
    Then 5 of "Wood" are spawned

  Scenario: Death removes the configured percent of each skill
    Given the skill-loss percent is 50
    And a skill level is 40
    When the player dies
    Then that skill level becomes 20
    And dropped gear is unchanged

  Scenario: Out-of-range percents are clamped
    Given a stored skill-loss percent of 140
    When skill loss is calculated
    Then the applied percent is 100

  Scenario: Grant admin prefers the connection id
    Given a connected player has Steam ID "76561198000000000"
    When an admin confirms grant admin for that player
    Then the world admin list contains "76561198000000000"
    And no id was typed

  Scenario: Missing connection id uses the typed fallback
    Given a connected player has no Steam ID
    When an admin confirms the typed id "76561198000000001"
    Then the world admin list contains "76561198000000001"

  Scenario: A bad typed id is refused
    When an admin confirms the typed id "not-a-steam-id"
    Then the world admin list is unchanged
